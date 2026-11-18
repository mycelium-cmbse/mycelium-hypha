// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Launcher.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Launcher.Provisioning;

    // Each way a download of hypha can be refused, through Launch as the launcher downloads it, ending in
    // the reason the fallback server's hypha_status gives the client.
    [TestFixture]
    public class DownloadFailureReasonTests
    {
        private const string Version = "1.6.0";
        private const string Rid = "win-x64";
        private const string AssetName = $"hypha-{Version}-{Rid}.zip";
        private const string DownloadUrl = "https://downloads.example.test/" + AssetName;

        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp() =>
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-download-reason-{Guid.NewGuid():N}"));

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [TestCase("no asset", $"the release tools-v{Version} has no {AssetName}")]
        [TestCase("no checksum", $"the release tools-v{Version} publishes no checksum for {AssetName}")]
        [TestCase("checksum mismatch", $"{AssetName} did not match the checksum the release tools-v{Version} publishes")]
        public async Task Each_reason_a_download_is_refused_is_what_hypha_status_says(string failing, string reason)
        {
            var target = await this.Launcher(Respond(failing)).FindAsync(this.Package(), Rid);
            var status = FallbackServer.Answer(
                """{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"hypha_status","arguments":{}}}""",
                target.Failure!,
                Version)!;

            Assert.Multiple(() =>
            {
                Assert.That(target.Executable, Is.Null);
                Assert.That(target.Failure, Does.Contain($"hypha {Version} for {Rid} could not be downloaded: {reason}."));
                Assert.That((string?)status["result"]!["content"]![0]!["text"], Does.Contain(reason));
            });
        }

        private static Func<HttpRequestMessage, HttpResponseMessage> Respond(string failing) => failing switch
        {
            "no asset" => _ => Json(new
            {
                body = $"| `hypha-{Version}-osx-arm64.zip` | {new string('0', 64)} |",
                assets = new[] { new { name = $"hypha-{Version}-osx-arm64.zip", browser_download_url = "https://downloads.example.test/other.zip" } },
            }),
            "no checksum" => _ => Json(new
            {
                body = "the checksum table is missing entirely",
                assets = new[] { new { name = AssetName, browser_download_url = DownloadUrl } },
            }),
            "checksum mismatch" => request => request.RequestUri!.ToString().Contains("releases/tags", StringComparison.Ordinal)
                ? Json(new
                {
                    body = $"| `{AssetName}` | {new string('0', 64)} |",
                    assets = new[] { new { name = AssetName, browser_download_url = DownloadUrl } },
                })
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Zip()) },
            _ => throw new ArgumentOutOfRangeException(nameof(failing), failing, null),
        };

        private static HttpResponseMessage Json(object body) =>
            new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };

        private static byte[] Zip()
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                using var writer = new StreamWriter(archive.CreateEntry("hypha.exe").Open(), Encoding.UTF8);
                writer.Write("not what the checksum promises");
            }

            return stream.ToArray();
        }

        private DirectoryInfo Package()
        {
            var bin = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "plugin", "bin"));
            File.WriteAllText(Path.Combine(bin.FullName, LaunchManifest.FileName), $$"""{"hyphaVersion": "{{Version}}"}""");

            return bin;
        }

        private Launch Launcher(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            var environment = new Dictionary<string, string?> { [HomeFolder.EnvironmentVariable] = Path.Combine(this.workspace.FullName, "home") };
            var client = new HttpClient(new Stub(respond)) { BaseAddress = new Uri("https://api.github.com/") };

            return new Launch(name => environment.GetValueOrDefault(name), _ => Path.Combine(this.workspace.FullName, "local"), new CliProvisioner(client));
        }

        private sealed class Stub(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(respond(request));
        }
    }
}
