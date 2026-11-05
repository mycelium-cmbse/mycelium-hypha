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
        /// <summary>The scratch cache root injected in place of the real <c>%LOCALAPPDATA%</c> toolchain cache.</summary>
        private DirectoryInfo cacheRoot = null!;

        /// <summary>The <c>uv</c> release target of the platform the tests run on, whose asset the stubs serve.</summary>
        private string target = null!;

        /// <summary>Creates an empty scratch cache root and resolves the current platform's <c>uv</c> release target.</summary>
        [SetUp]
        public void SetUp()
        {
            this.cacheRoot = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-uv-tests-" + Guid.NewGuid().ToString("N")));
            this.cacheRoot.Create();

            // Whatever platform the test happens to run on - the stub responds to that target's asset.
            this.target = UvPlatform.CurrentTarget
                ?? throw new InvalidOperationException("this platform has no uv release target");
        }

        /// <summary>Deletes the scratch cache root, if a test left it behind.</summary>
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
        public void Leaves_uv_its_default_locations_unless_it_owns_the_toolchain()
        {
            using var client = new HttpClient();
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot);

            Assert.That(provisioner.Environment, Is.Empty);
        }

        [Test]
        public void Keeps_uv_s_cache_and_python_under_a_toolchain_root_it_owns()
        {
            using var client = new HttpClient();
            var provisioner = new UvProvisioner(client, NullLogger<UvProvisioner>.Instance, this.cacheRoot, ownsToolchain: true);

            Assert.That(
                provisioner.Environment,
                Is.EquivalentTo(new Dictionary<string, string>
                {
                    [UvProvisioner.UvCacheVariable] = Path.Combine(this.cacheRoot.FullName, "uv-cache"),
                    [UvProvisioner.UvPythonInstallVariable] = Path.Combine(this.cacheRoot.FullName, "python"),
                }));
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
            Assert.That(executable.FullName, Is.EqualTo(provisioner.Executable(UvProvisioner.PinnedVersion, this.target).FullName));
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

        /// <summary>
        /// Builds an in-memory <c>uv</c> release archive holding a fake executable, in the format the target ships: a zip
        /// on Windows, a gzipped tar elsewhere.
        /// </summary>
        /// <param name="target">The <c>uv</c> release target that decides the archive format and the executable's name.</param>
        /// <returns>The bytes of the archive.</returns>
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

        /// <summary>
        /// A stubbed HTTP transport that answers every request from a canned response function and records each
        /// requested URI, so a test can tell whether the provisioner reached the network at all.
        /// </summary>
        private sealed class StubHandler : HttpMessageHandler
        {
            /// <summary>The function producing the response to each request.</summary>
            private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

            /// <summary>Initializes a new instance of the <see cref="StubHandler"/> class.</summary>
            /// <param name="respond">The function producing the response to each request.</param>
            private StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                this.respond = respond;
            }

            /// <summary>Gets the URI of every request received so far, in the order they arrived.</summary>
            public List<string> Requests { get; } = [];

            /// <summary>Creates a handler serving the release, the archive and its matching SHA-256 checksum.</summary>
            /// <param name="target">The <c>uv</c> release target whose asset is served.</param>
            /// <param name="archive">The archive served as the release asset.</param>
            /// <returns>A handler under which provisioning succeeds.</returns>
            public static StubHandler ForSuccessfulDownload(string target, byte[] archive) =>
                ForDownload(target, archive, Convert.ToHexStringLower(SHA256.HashData(archive)));

            /// <summary>Creates a handler serving the release and the archive, but a checksum that does not match it.</summary>
            /// <param name="target">The <c>uv</c> release target whose asset is served.</param>
            /// <param name="archive">The archive served as the release asset.</param>
            /// <returns>A handler under which checksum verification fails.</returns>
            public static StubHandler ForCorruptedChecksum(string target, byte[] archive) =>
                ForDownload(target, archive, new string('0', 64));

            /// <summary>Creates a handler answering every request with 404 Not Found.</summary>
            /// <returns>A handler under which the pinned release does not exist.</returns>
            public static StubHandler ForNotFound() =>
                new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            /// <summary>Creates a handler serving a release that lists no assets at all.</summary>
            /// <returns>A handler under which the release exists but has no asset for the current target.</returns>
            public static StubHandler ForReleaseWithNoMatchingAsset() => new(request =>
                request.RequestUri!.ToString().Contains("/releases/tags/", StringComparison.Ordinal)
                    ? Json("""{"assets":[]}""")
                    : new HttpResponseMessage(HttpStatusCode.NotFound));

            /// <summary>Creates a handler that throws on any request, for tests in which no network call may happen.</summary>
            /// <returns>A handler that fails the test's request instead of answering it.</returns>
            public static StubHandler ThatNeverResponds() =>
                new(_ => throw new InvalidOperationException("no network call was expected"));

            /// <summary>
            /// Creates a handler serving the release metadata – listing the archive and its <c>.sha256</c> file – the
            /// archive itself and the given checksum; any other URI is not found.
            /// </summary>
            /// <param name="target">The <c>uv</c> release target whose asset name the release lists.</param>
            /// <param name="archive">The archive served at the archive download URL.</param>
            /// <param name="checksumHex">The lowercase hexadecimal SHA-256 served at the checksum download URL.</param>
            /// <returns>A handler serving that release.</returns>
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

            /// <summary>Wraps a JSON body in a 200 OK response.</summary>
            /// <param name="body">The JSON text of the response.</param>
            /// <returns>The successful response carrying <paramref name="body"/>.</returns>
            private static HttpResponseMessage Json(string body) =>
                new(HttpStatusCode.OK) { Content = new StringContent(body) };

            /// <summary>Records the request's URI and answers it from the response function.</summary>
            /// <param name="request">The request the provisioner sends.</param>
            /// <param name="cancellationToken">Unused: the response is produced synchronously.</param>
            /// <returns>A completed task carrying the canned response.</returns>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Add(request.RequestUri!.ToString());

                return Task.FromResult(this.respond(request));
            }
        }
    }
}
