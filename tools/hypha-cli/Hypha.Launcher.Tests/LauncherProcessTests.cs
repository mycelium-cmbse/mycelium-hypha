// ------------------------------------------------------------------------------------------------
// <copyright file="LauncherProcessTests.cs" company="Starion Group S.A.">
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;

    // Starts hypha-launch as a client does, over standard input and output, with HYPHA_BIN pointing at the
    // hypha this solution built, since no released hypha has serve yet.
    [TestFixture]
    public class LauncherProcessTests
    {
        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp() =>
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-launcher-{Guid.NewGuid():N}"));

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [Test]
        public async Task The_client_talks_to_hypha_serve_through_the_launcher()
        {
            var knowledge = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "knowledge-root"));

            var (initialize, tools, standardError) = await this.Handshake(
                new Dictionary<string, string> { ["HYPHA_BIN"] = TestHypha.Executable().FullName },
                ["--repository-root", knowledge.FullName]);

            Assert.Multiple(() =>
            {
                Assert.That((string?)initialize["result"]?["serverInfo"]?["name"], Is.EqualTo("hypha"), standardError);
                Assert.That((string?)initialize["result"]?["instructions"], Does.Contain("NORMATIVE"), "the real server's instructions");
                Assert.That(tools["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Does.Contain("hypha_metaclass"));
            });
        }

        [Test]
        public async Task Without_a_hypha_to_start_the_client_gets_hypha_status_saying_why()
        {
            var (initialize, tools, standardError) = await this.Handshake(
                new Dictionary<string, string> { ["HYPHA_BIN"] = Path.Combine(this.workspace.FullName, "missing", "hypha") },
                []);

            Assert.Multiple(() =>
            {
                Assert.That((string?)initialize["result"]?["instructions"], Does.Contain("HYPHA_BIN").And.Contain("does not exist"));
                Assert.That(tools["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Is.EqualTo(new[] { "hypha_status" }));
                Assert.That(standardError, Does.Contain("hypha-launch:"), "the reason is logged to standard error too");
            });
        }

        [Test]
        public async Task A_hypha_that_cannot_be_started_falls_back_to_hypha_status_saying_why()
        {
            var unstartable = new FileInfo(Path.Combine(this.workspace.FullName, OperatingSystem.IsWindows() ? "hypha.exe" : "hypha"));
            File.WriteAllText(unstartable.FullName, "not a program");

            var (initialize, tools, _) = await this.Handshake(new Dictionary<string, string> { ["HYPHA_BIN"] = unstartable.FullName }, []);

            Assert.Multiple(() =>
            {
                Assert.That((string?)initialize["result"]?["instructions"], Does.Contain("could not be started").And.Contain("HYPHA_BIN"));
                Assert.That(tools["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Is.EqualTo(new[] { "hypha_status" }));
            });
        }

        [Test]
        public async Task The_launcher_reports_the_hypha_version_its_package_names()
        {
            // A package of its own: the launcher beside a manifest, and a HYPHA_HOME it cannot use, so that it
            // answers itself without reaching the network.
            var bin = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "plugin", "bin"));
            foreach (var file in new[] { "hypha-launch.dll", "hypha-launch.runtimeconfig.json", "hypha-launch.deps.json" })
            {
                File.Copy(Path.Combine(AppContext.BaseDirectory, file), Path.Combine(bin.FullName, file));
            }

            File.WriteAllText(Path.Combine(bin.FullName, LaunchManifest.FileName), """{"hyphaVersion": "1.6.3"}""");

            var (initialize, _, _) = await this.Handshake(
                new Dictionary<string, string> { ["HYPHA_BIN"] = string.Empty, ["HYPHA_HOME"] = "relative/home" },
                [],
                Path.Combine(bin.FullName, "hypha-launch.dll"));

            Assert.Multiple(() =>
            {
                Assert.That((string?)initialize["result"]?["serverInfo"]?["version"], Is.EqualTo("1.6.3"));
                Assert.That((string?)initialize["result"]?["instructions"], Does.Contain("HYPHA_HOME must be an absolute path"));
            });
        }

        private async Task<(JsonNode Initialize, JsonNode Tools, string StandardError)> Handshake(
            IReadOnlyDictionary<string, string> environment, string[] arguments, string? launcher = null)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = this.workspace.FullName,
            };
            start.ArgumentList.Add(launcher ?? Path.Combine(AppContext.BaseDirectory, "hypha-launch.dll"));
            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            foreach (var (name, value) in environment)
            {
                start.Environment[name] = value;
            }

            using var launch = Process.Start(start)!;
            var standardError = launch.StandardError.ReadToEndAsync();

            await launch.StandardInput.WriteLineAsync(
                """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}""");
            await launch.StandardInput.FlushAsync();
            var initialize = await Response(launch, 1);

            await launch.StandardInput.WriteLineAsync("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
            await launch.StandardInput.WriteLineAsync("""{"jsonrpc":"2.0","id":2,"method":"tools/list"}""");
            await launch.StandardInput.FlushAsync();
            var tools = await Response(launch, 2);

            launch.StandardInput.Close();
            await launch.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(1));

            Assert.That(launch.ExitCode, Is.Zero, "the launcher exits as its server does, once the client has gone");

            return (initialize, tools, await standardError);
        }

        private static async Task<JsonNode> Response(Process launch, int id)
        {
            while (await launch.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromMinutes(1)) is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line) && JsonNode.Parse(line) is { } message && (int?)message["id"] == id)
                {
                    return message;
                }
            }

            throw new InvalidOperationException($"the launcher closed standard output before answering request {id}");
        }
    }
}
