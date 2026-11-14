// ------------------------------------------------------------------------------------------------
// <copyright file="CliProvisionerEnsureAsyncTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Launcher.Provisioning;

    [TestFixture]
    public class CliProvisionerEnsureAsyncTests
    {
        private const string Version = "1.2.0";
        private const string Rid = "linux-x64";
        private const string AssetName = $"hypha-{Version}-{Rid}.zip";

        private DirectoryInfo workspace = null!;
        private CacheLayout cache = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-launch-ensure-{Guid.NewGuid():N}"));

            this.cache = new CacheLayout(this.workspace, "abc123");
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Already_cached_returns_immediately_without_any_network_call()
        {
            var executable = this.cache.CliExecutable(Version, Rid);
            executable.Directory!.Create();
            File.WriteAllText(executable.FullName, "already here");
            File.WriteAllText(this.cache.OkMarker(Version, Rid).FullName, "deadbeef");

            var handler = new StubHandler(_ => throw new InvalidOperationException("should not be called"));
            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.That(result!.FullName, Is.EqualTo(executable.FullName));
        }

        [Test]
        public async Task A_valid_release_and_archive_are_downloaded_verified_and_extracted()
        {
            var zipBytes = BuildZip("hypha", "pretend binary content");
            var sha256 = Sha256Hex(zipBytes);
            var downloadUrl = "https://downloads.example.test/" + AssetName;

            var handler = new StubHandler(request =>
                request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, sha256)
                    : ZipResponse(zipBytes));

            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(File.ReadAllText(result!.FullName), Is.EqualTo("pretend binary content"));
                Assert.That(CliProvisioner.IsCached(this.cache, Version, Rid), Is.True);
                Assert.That(
                    File.Exists(Path.Combine(this.cache.BinDirectory(Version, Rid).FullName, AssetName + ".download")),
                    Is.False,
                    "the downloaded archive is cleaned up once extracted");
            });
        }

        [Test]
        public async Task A_second_call_after_success_makes_no_further_network_calls()
        {
            var zipBytes = BuildZip("hypha", "content");
            var sha256 = Sha256Hex(zipBytes);
            var downloadUrl = "https://downloads.example.test/" + AssetName;
            var requests = 0;

            var handler = new StubHandler(request =>
            {
                requests++;

                return request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, sha256)
                    : ZipResponse(zipBytes);
            });

            var provisioner = new CliProvisioner(CreateClient(handler));

            await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);
            var requestsAfterFirstCall = requests;

            await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.That(requests, Is.EqualTo(requestsAfterFirstCall), "the second call should hit IsCached, not the network");
        }

        [Test]
        public async Task An_unreachable_release_endpoint_is_provisioned_as_null_and_logged()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(this.cache.DownloadErrorLog.Exists, Is.True);
                Assert.That(File.ReadAllText(this.cache.DownloadErrorLog.FullName), Does.Contain("HttpRequestException"));
            });
        }

        [Test]
        public async Task No_matching_asset_in_the_release_is_provisioned_as_null()
        {
            var handler = new StubHandler(_ => JsonResponse(new
            {
                body = "no checksum table here",
                assets = new[] { new { name = "hypha-1.2.0-osx-arm64.zip", browser_download_url = "https://example.test/other.zip" } },
            }));

            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task A_matching_asset_with_no_checksum_row_is_provisioned_as_null()
        {
            var downloadUrl = "https://downloads.example.test/" + AssetName;

            var handler = new StubHandler(_ => JsonResponse(new
            {
                body = "the checksum table is missing entirely",
                assets = new[] { new { name = AssetName, browser_download_url = downloadUrl } },
            }));

            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task A_checksum_mismatch_is_provisioned_as_null_and_the_partial_download_is_removed()
        {
            var zipBytes = BuildZip("hypha", "tampered content, does not match the advertised checksum");
            var wrongSha256 = new string('0', 64);
            var downloadUrl = "https://downloads.example.test/" + AssetName;

            var handler = new StubHandler(request =>
                request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, wrongSha256)
                    : ZipResponse(zipBytes));

            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(CliProvisioner.IsCached(this.cache, Version, Rid), Is.False);
                Assert.That(
                    File.Exists(Path.Combine(this.cache.BinDirectory(Version, Rid).FullName, AssetName + ".download")),
                    Is.False,
                    "a failed checksum must not leave the unverified archive behind");
            });
        }

        [Test]
        public async Task A_folder_the_download_cannot_write_to_is_a_failure_not_a_crash()
        {
            var handler = new StubHandler(_ => throw new UnauthorizedAccessException("Access to the path is denied."));
            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(provisioner.LastFailure, Does.Contain("Access to the path is denied"));
            });
        }

        [Test]
        public async Task Two_launchers_provisioning_at_once_both_get_the_one_verified_copy()
        {
            var zipBytes = BuildZip("hypha", "the one copy");
            var sha256 = Sha256Hex(zipBytes);
            var downloadUrl = "https://downloads.example.test/" + AssetName;

            HttpResponseMessage Respond(HttpRequestMessage request)
            {
                // Slow enough that both downloads are under way at once.
                Thread.Sleep(200);

                return request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, sha256)
                    : ZipResponse(zipBytes);
            }

            var first = new CliProvisioner(CreateClient(new StubHandler(Respond)));
            var second = new CliProvisioner(CreateClient(new StubHandler(Respond)));

            var results = await Task.WhenAll(
                Task.Run(() => first.EnsureAsync(this.cache, Version, Rid, CancellationToken.None)),
                Task.Run(() => second.EnsureAsync(this.cache, Version, Rid, CancellationToken.None)));

            Assert.Multiple(() =>
            {
                Assert.That(results.Select(result => result?.FullName), Is.All.EqualTo(this.cache.CliExecutable(Version, Rid).FullName));
                Assert.That(File.ReadAllText(results[0]!.FullName), Is.EqualTo("the one copy"));
                Assert.That(CliProvisioner.IsCached(this.cache, Version, Rid), Is.True);
                Assert.That(first.LastFailure, Is.Null);
                Assert.That(second.LastFailure, Is.Null);
                Assert.That(
                    this.cache.BinDirectory(Version, Rid).Parent!.EnumerateDirectories("*" + CliProvisioner.StagingSuffix),
                    Is.Empty,
                    "neither download leaves its temporary folder behind");
            });
        }

        [Test]
        public async Task A_folder_left_without_its_marker_is_replaced_by_the_verified_download()
        {
            var leftover = this.cache.BinDirectory(Version, Rid);
            leftover.Create();
            File.WriteAllText(Path.Combine(leftover.FullName, "hypha"), "half an old download");
            var zipBytes = BuildZip("hypha", "verified");
            var downloadUrl = "https://downloads.example.test/" + AssetName;
            var handler = new StubHandler(request =>
                request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, Sha256Hex(zipBytes))
                    : ZipResponse(zipBytes));

            var result = await new CliProvisioner(CreateClient(handler)).EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(result!.FullName), Is.EqualTo("verified"));
                Assert.That(CliProvisioner.IsCached(this.cache, Version, Rid), Is.True);
            });
        }

        [Test]
        public async Task A_temporary_folder_abandoned_an_hour_ago_is_cleared_and_a_recent_one_is_left()
        {
            var bin = this.cache.BinDirectory(Version, Rid);
            bin.Parent!.Create();
            var abandoned = Directory.CreateDirectory($"{bin.FullName}.old{CliProvisioner.StagingSuffix}");
            Directory.SetLastWriteTimeUtc(abandoned.FullName, DateTime.UtcNow.AddHours(-2));
            var recent = Directory.CreateDirectory($"{bin.FullName}.busy{CliProvisioner.StagingSuffix}");
            var zipBytes = BuildZip("hypha", "content");
            var downloadUrl = "https://downloads.example.test/" + AssetName;
            var handler = new StubHandler(request =>
                request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, Sha256Hex(zipBytes))
                    : ZipResponse(zipBytes));

            await new CliProvisioner(CreateClient(handler)).EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(abandoned.FullName), Is.False);
                Assert.That(Directory.Exists(recent.FullName), Is.True, "another launcher may still be downloading into it");
            });
        }

        [Test]
        public async Task An_archive_without_the_executable_is_not_cached_and_says_so()
        {
            var zipBytes = BuildZip("readme.txt", "no hypha in here");
            var downloadUrl = "https://downloads.example.test/" + AssetName;
            var handler = new StubHandler(request =>
                request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                    ? ReleaseResponse(downloadUrl, Sha256Hex(zipBytes))
                    : ZipResponse(zipBytes));
            var provisioner = new CliProvisioner(CreateClient(handler));

            var result = await provisioner.EnsureAsync(this.cache, Version, Rid, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(provisioner.LastFailure, Does.Contain("holds no hypha"));
                Assert.That(CliProvisioner.IsCached(this.cache, Version, Rid), Is.False);
            });
        }

        private static HttpClient CreateClient(HttpMessageHandler handler) =>
            new(handler) { BaseAddress = new Uri("https://api.github.com/") };

        private static HttpResponseMessage ReleaseResponse(string downloadUrl, string sha256) =>
            JsonResponse(new
            {
                body = $"| `{AssetName}` | {sha256} |",
                assets = new[] { new { name = AssetName, browser_download_url = downloadUrl } },
            });

        private static HttpResponseMessage ZipResponse(byte[] zipBytes) =>
            new(HttpStatusCode.OK) { Content = new ByteArrayContent(zipBytes) };

        private static HttpResponseMessage JsonResponse(object body) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
            };

        private static byte[] BuildZip(string entryName, string content)
        {
            using var stream = new MemoryStream();

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                writer.Write(content);
            }

            return stream.ToArray();
        }

        private static string Sha256Hex(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => this.respond = respond;

            public List<string> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests.Add(request.RequestUri!.ToString());

                return Task.FromResult(this.respond(request));
            }
        }
    }
}
