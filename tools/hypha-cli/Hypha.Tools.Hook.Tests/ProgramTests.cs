// ------------------------------------------------------------------------------------------------
// <copyright file="ProgramTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class ProgramTests
    {
        private const string PreviousPluginRoot = "__unset__";

        private string? previousEnvironmentValue;
        private DirectoryInfo workspace = null!;
        private DirectoryInfo cacheRoot = null!;

        [SetUp]
        public void SetUp()
        {
            this.previousEnvironmentValue =
                Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT") ?? PreviousPluginRoot;

            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-hook-program-{Guid.NewGuid():N}"));

            // Never the real %LOCALAPPDATA%\mycelium-hypha: a machine that has actually used this hook
            // before may already have the test's chosen version/RID cached there, which would make
            // CliProvisioner.EnsureAsync's IsCached check skip the network the test means to exercise.
            this.cacheRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-hook-cache-{Guid.NewGuid():N}"));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(
                "CLAUDE_PLUGIN_ROOT",
                this.previousEnvironmentValue == PreviousPluginRoot ? null : this.previousEnvironmentValue);

            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }

            this.cacheRoot.Refresh();
            if (this.cacheRoot.Exists)
            {
                this.cacheRoot.Delete(recursive: true);
            }
        }

        private string ResolveIsolatedCacheFolder(Environment.SpecialFolder folder) =>
            folder == Environment.SpecialFolder.LocalApplicationData
                ? this.cacheRoot.FullName
                : throw new InvalidOperationException("unexpected folder requested");

        [Test]
        public void ResolvePluginRoot_uses_the_environment_variable_when_set()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);

            Assert.That(Program.ResolvePluginRoot().FullName, Is.EqualTo(this.workspace.FullName));
        }

        [Test]
        public void ResolvePluginRoot_falls_back_to_walking_up_from_the_executable_when_unset()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", null);

            // hooks/native/<rid>/hypha-hook sits three levels below the plugin root; this only asserts
            // the walk-up actually happens (a real DirectoryInfo three levels above the test binary),
            // not any specific path, since that depends on where the test itself runs from.
            var expected = new DirectoryInfo(AppContext.BaseDirectory).Parent?.Parent?.Parent;

            Assert.That(Program.ResolvePluginRoot().FullName, Is.EqualTo(expected!.FullName));
        }

        [Test]
        public void ReadPinnedCliVersion_is_null_when_there_is_no_plugin_manifest()
        {
            Assert.That(Program.ReadPinnedCliVersion(this.workspace), Is.Null);
        }

        [Test]
        public void ReadPinnedCliVersion_reads_the_pinned_version()
        {
            WritePluginManifest(this.workspace, """{"hyphaCliVersion": "1.2.0"}""");

            Assert.That(Program.ReadPinnedCliVersion(this.workspace), Is.EqualTo("1.2.0"));
        }

        [Test]
        public void ReadPinnedCliVersion_is_null_when_the_field_is_blank()
        {
            WritePluginManifest(this.workspace, """{"hyphaCliVersion": "  "}""");

            Assert.That(Program.ReadPinnedCliVersion(this.workspace), Is.Null);
        }

        [Test]
        public void ReadPinnedCliVersion_is_null_for_unreadable_json_rather_than_throwing()
        {
            WritePluginManifest(this.workspace, "not json at all");

            Assert.That(Program.ReadPinnedCliVersion(this.workspace), Is.Null);
        }

        [Test]
        public void ParseCheckResult_parses_a_valid_line_of_json()
        {
            var result = Program.ParseCheckResult(
                """{"installedTags":["2026-05"],"defaultTag":"2026-05","availableOnline":["2026-06","2026-05"]}""");

            Assert.Multiple(() =>
            {
                Assert.That(result!.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
                Assert.That(result.DefaultTag, Is.EqualTo("2026-05"));
                Assert.That(result.AvailableOnline, Is.EqualTo(new[] { "2026-06", "2026-05" }));
            });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ParseCheckResult_is_null_for_empty_or_missing_output(string? output)
        {
            Assert.That(Program.ParseCheckResult(output), Is.Null);
        }

        [Test]
        public void ParseCheckResult_is_null_for_unreadable_json_rather_than_throwing()
        {
            Assert.That(Program.ParseCheckResult("not json at all"), Is.Null);
        }

        [Test]
        public async Task RunAsync_with_no_plugin_manifest_returns_without_any_network_call()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);

            var handler = new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

            await Program.RunAsync(new HttpClient(handler), this.ResolveIsolatedCacheFolder);

            Assert.That(handler.Requests, Is.EqualTo(0));
        }

        [Test]
        public async Task RunAsync_provisions_nothing_further_when_the_release_cannot_be_reached()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);
            WritePluginManifest(this.workspace, """{"hyphaCliVersion": "1.2.0"}""");

            var handler = new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            // Must not throw: CliProvisioner.EnsureAsync catches the failure internally, and RunAsync
            // simply returns once it sees no executable came back, never reaching CheckRunner at all.
            Assert.That(
                async () => await Program.RunAsync(
                    new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") },
                    this.ResolveIsolatedCacheFolder),
                Throws.Nothing);

            Assert.That(handler.Requests, Is.EqualTo(1));
        }

        private static void WritePluginManifest(DirectoryInfo pluginRoot, string json)
        {
            var manifestDirectory = Directory.CreateDirectory(Path.Combine(pluginRoot.FullName, ".claude-plugin"));
            File.WriteAllText(Path.Combine(manifestDirectory.FullName, "plugin.json"), json);
        }

        private sealed class CountingHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

            public CountingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => this.respond = respond;

            public int Requests { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Requests++;

                return Task.FromResult(this.respond(request));
            }
        }
    }
}
