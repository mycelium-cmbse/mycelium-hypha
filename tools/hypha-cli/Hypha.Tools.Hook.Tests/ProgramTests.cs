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

    /// <summary>
    /// Suite of tests for the hook entry point's internal pieces.
    /// </summary>
    /// <remarks>
    /// <see cref="Program.RunAsync"/> takes its <see cref="HttpClient"/> as a parameter specifically so
    /// it can be driven end-to-end here against a stub transport, rather than only through
    /// <see cref="Program.Main"/> (which always builds a real one) - real detached-process spawning at
    /// the very end of a fully successful run is still left to manual verification, per
    /// <c>tools/hypha-cli/README.md</c>.
    /// </remarks>
    [TestFixture]
    public class ProgramTests
    {
        private const string PreviousPluginRoot = "__unset__";

        private string? previousEnvironmentValue;
        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp()
        {
            this.previousEnvironmentValue =
                Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT") ?? PreviousPluginRoot;

            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-hook-program-{Guid.NewGuid():N}"));
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
        }

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
        public void ReadStatus_is_null_when_there_is_no_status_file()
        {
            var cache = new CacheLayout(this.workspace, "abc123");

            Assert.That(Program.ReadStatus(cache), Is.Null);
        }

        [Test]
        public void ReadStatus_reads_a_real_status_file()
        {
            var cache = new CacheLayout(this.workspace, "abc123");
            cache.StatusFile.Directory!.Create();
            File.WriteAllText(
                cache.StatusFile.FullName,
                """{"schemaVersion": "1.0.0", "phase": "fetching", "targetTag": "2026-06"}""");

            var status = Program.ReadStatus(cache);

            Assert.Multiple(() =>
            {
                Assert.That(status!.Phase, Is.EqualTo("fetching"));
                Assert.That(status.TargetTag, Is.EqualTo("2026-06"));
            });
        }

        [Test]
        public void ReadStatus_is_null_for_unreadable_json_rather_than_throwing()
        {
            var cache = new CacheLayout(this.workspace, "abc123");
            cache.StatusFile.Directory!.Create();
            File.WriteAllText(cache.StatusFile.FullName, "not json");

            Assert.That(Program.ReadStatus(cache), Is.Null);
        }

        [Test]
        public async Task RunAsync_with_no_plugin_manifest_returns_without_any_network_call()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);

            var handler = new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

            await Program.RunAsync(new HttpClient(handler));

            Assert.That(handler.Requests, Is.EqualTo(0));
        }

        [Test]
        public async Task RunAsync_reports_nothing_and_provisions_nothing_when_the_release_cannot_be_reached()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);
            WritePluginManifest(this.workspace, """{"hyphaCliVersion": "1.2.0"}""");

            var handler = new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            // Must not throw: EnsureAsync catches the failure internally, and RunAsync simply returns.
            Assert.That(
                async () => await Program.RunAsync(new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") }),
                Throws.Nothing);

            Assert.That(handler.Requests, Is.EqualTo(1));
        }

        [Test]
        public async Task RunAsync_prints_a_summary_when_a_previous_run_left_progress_behind()
        {
            Environment.SetEnvironmentVariable("CLAUDE_PLUGIN_ROOT", this.workspace.FullName);
            WritePluginManifest(this.workspace, """{"hyphaCliVersion": "1.2.0"}""");

            var cache = new CacheLayout(
                CacheLayout.ResolveRoot(), CacheLayout.ComputeInstallKey(this.workspace.FullName));
            cache.StatusFile.Directory!.Create();
            File.WriteAllText(
                cache.StatusFile.FullName,
                """{"schemaVersion": "1.0.0", "phase": "fetching", "targetTag": "2026-06", "fetch": {"kind": "textual", "done": 1, "total": 2}}""");

            try
            {
                var handler = new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
                var originalOut = Console.Out;
                var writer = new StringWriter();
                Console.SetOut(writer);

                try
                {
                    await Program.RunAsync(
                        new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") });
                }
                finally
                {
                    Console.SetOut(originalOut);
                }

                Assert.That(writer.ToString(), Does.Contain("2026-06").And.Contains("textual"));
            }
            finally
            {
                cache.StateDirectory.Refresh();
                if (cache.StateDirectory.Exists)
                {
                    cache.StateDirectory.Delete(recursive: true);
                }
            }
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
