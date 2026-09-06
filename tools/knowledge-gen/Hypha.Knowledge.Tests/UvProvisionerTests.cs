// ------------------------------------------------------------------------------------------------
// <copyright file="UvProvisionerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Formats.Tar;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Tests for provisioning <c>uv</c>, against a stubbed transport and an injected cache root - no
    /// live calls, no touching the real <c>%LOCALAPPDATA%</c>.
    /// </summary>
    [TestFixture]
    public class UvProvisionerTests
    {
        private DirectoryInfo cacheRoot = null!;
        private string target = null!;

        [SetUp]
        public void SetUp()
        {
            this.cacheRoot = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), "hypha-uv-tests-" + Guid.NewGuid().ToString("N")));
            this.cacheRoot.Create();

            // Whatever platform the test happens to run on - the stub responds to that target's asset.
            this.target = UvPlatform.CurrentTarget
                ?? throw new InvalidOperationException("this platform has no uv release target");
        }

        [TearDown]
        public void TearDown()
        {
            this.cacheRoot.Refresh();
            if (this.cacheRoot.Exists)
            {
                this.cacheRoot.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Downloads_verifies_and_caches_uv_on_first_use()
        {
            var archive = BuildArchive(this.target);
            var handler = StubHandler.ForSuccessfulDownload(this.target, archive);
            using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };

            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            Assert.That(executable!.Exists, Is.True);
            Assert.That(
                executable.FullName,
                Is.EqualTo(provisioner.Executable(UvProvisioner.PinnedVersion, this.target).FullName));
            Assert.That(provisioner.OkMarker(UvProvisioner.PinnedVersion, this.target).Exists, Is.True);
        }

        [Test]
        public async Task A_second_call_never_touches_the_network()
        {
            var archive = BuildArchive(this.target);
            var handler = StubHandler.ForSuccessfulDownload(this.target, archive);
            using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);
            await provisioner.EnsureAsync();
            var requestsAfterFirstCall = handler.Requests.Count;

            await provisioner.EnsureAsync();

            Assert.That(handler.Requests, Has.Count.EqualTo(requestsAfterFirstCall));
        }

        [Test]
        public async Task An_already_cached_build_is_returned_without_any_network_call()
        {
            var archive = BuildArchive(this.target);
            var handler = StubHandler.ForSuccessfulDownload(this.target, archive);
            using var seedingClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };
            await new UvProvisioner(seedingClient, NullLogger<UvProvisioner>.Instance, this.cacheRoot).EnsureAsync();

            var freshHandler = StubHandler.ThatNeverResponds();
            using var freshClient = new HttpClient(freshHandler) { BaseAddress = new Uri("https://api.github.com/") };
            var provisioner = new UvProvisioner(freshClient, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            Assert.That(freshHandler.Requests, Is.Empty);
        }

        [Test]
        public async Task A_checksum_mismatch_is_rejected_and_nothing_is_cached()
        {
            var archive = BuildArchive(this.target);
            var handler = StubHandler.ForCorruptedChecksum(this.target, archive);
            using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Null);
            Assert.That(provisioner.OkMarker(UvProvisioner.PinnedVersion, this.target).Exists, Is.False);
        }

        [Test]
        public async Task A_missing_release_never_throws_and_returns_null()
        {
            using var client = new HttpClient(StubHandler.ForNotFound())
            {
                BaseAddress = new Uri("https://api.github.com/"),
            };
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            FileInfo? executable = null;
            Assert.That(async () => executable = await provisioner.EnsureAsync(), Throws.Nothing);
            Assert.That(executable, Is.Null);
        }

        [Test]
        public async Task A_release_missing_the_expected_asset_returns_null()
        {
            using var client = new HttpClient(StubHandler.ForReleaseWithNoMatchingAsset())
            {
                BaseAddress = new Uri("https://api.github.com/"),
            };
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Null);
        }

        private static byte[] BuildArchive(string target)
        {
            var executableName = UvPlatform.ExecutableName(target);
            var content = "fake uv binary"u8.ToArray();

            using var buffer = new MemoryStream();

            if (UvPlatform.IsZip(target))
            {
                using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = zip.CreateEntry(executableName);
                    using var entryStream = entry.Open();
                    entryStream.Write(content);
                }
            }
            else
            {
                using (var gzip = new GZipStream(buffer, CompressionMode.Compress, leaveOpen: true))
                using (var writer = new TarWriter(gzip, leaveOpen: true))
                {
                    var entry = new PaxTarEntry(TarEntryType.RegularFile, executableName)
                    {
                        DataStream = new MemoryStream(content),
                    };
                    writer.WriteEntry(entry);
                }
            }

            return buffer.ToArray();
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

            private StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                this.respond = respond;
            }

            public List<string> Requests { get; } = [];

            public static StubHandler ForSuccessfulDownload(string target, byte[] archive) =>
                ForDownload(target, archive, Convert.ToHexStringLower(SHA256.HashData(archive)));

            public static StubHandler ForCorruptedChecksum(string target, byte[] archive) =>
                ForDownload(target, archive, new string('0', 64));

            public static StubHandler ForNotFound() =>
                new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            public static StubHandler ForReleaseWithNoMatchingAsset() => new(request =>
                request.RequestUri!.ToString().Contains("/releases/tags/", StringComparison.Ordinal)
                    ? Json("""{"assets":[]}""")
                    : new HttpResponseMessage(HttpStatusCode.NotFound));

            public static StubHandler ThatNeverResponds() =>
                new(_ => throw new InvalidOperationException("no network call was expected"));

            private static StubHandler ForDownload(string target, byte[] archive, string checksumHex)
            {
                var assetName = UvPlatform.AssetName(target);
                const string ArchiveUrl = "https://example.test/archive";
                const string ChecksumUrl = "https://example.test/checksum";

                return new StubHandler(request =>
                {
                    var uri = request.RequestUri!.ToString();

                    if (uri.Contains("/releases/tags/", StringComparison.Ordinal))
                    {
                        return Json($$"""
                            {"assets":[
                                {"name":"{{assetName}}","browser_download_url":"{{ArchiveUrl}}"},
                                {"name":"{{assetName}}.sha256","browser_download_url":"{{ChecksumUrl}}"}
                            ]}
                            """);
                    }

                    if (uri == ChecksumUrl)
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent($"{checksumHex} *{assetName}"),
                        };
                    }

                    if (uri == ArchiveUrl)
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(archive),
                        };
                    }

                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                });
            }

            private static HttpResponseMessage Json(string body) =>
                new(HttpStatusCode.OK) { Content = new StringContent(body) };

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Add(request.RequestUri!.ToString());

                return Task.FromResult(this.respond(request));
            }
        }
    }
}
