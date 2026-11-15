// ------------------------------------------------------------------------------------------------
// <copyright file="MigrationProcessTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Migration;
    using Hypha.Knowledge.Releases;

    // The one-time copy into an empty HYPHA_HOME, through a real hypha process, as Program.Run starts it
    // before a verb. Every folder the process reads or writes is a scratch one: HYPHA_HOME, the Claude
    // configuration folder (CLAUDE_CONFIG_DIR) and, off Windows, the editors' settings (XDG_CONFIG_HOME).
    // On Windows the editors' settings are the roaming application data folder, which a process cannot be
    // pointed away from; the migration only reads it, and this test asserts only on the plugin's copy.
    [TestFixture]
    public class MigrationProcessTests
    {
        private const string Tag = "2026-05";

        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp() => this.workspace = Directory.CreateTempSubdirectory("hypha-migration-");

        [TearDown]
        public void TearDown() => this.workspace.Delete(recursive: true);

        [Test]
        public async Task The_first_verb_against_an_empty_home_copies_in_what_the_plugin_installed_and_answers_from_it()
        {
            var home = this.Folder("home");
            var claude = this.Folder("claude");
            var plugin = new KnowledgeLayout(new DirectoryInfo(Path.Combine(claude.FullName, "plugins", "cache", "marketplace", LegacyInstallLocator.PluginName, "1.5.0")));
            new TemporaryInstall(plugin).Release(Tag);

            var (exitCode, output, error) = await this.Run(["list"], home, claude);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, error);
                Assert.That(output, Does.Contain(Tag), "list answers from the copy");
                Assert.That(new KnowledgeLayout(home).InstalledTags, Does.Contain(Tag), "the release is recorded in HYPHA_HOME");
                Assert.That(plugin.InstalledTags, Does.Contain(Tag), "the plugin's own install is left as it was");
            });
        }

        [Test]
        public async Task A_home_that_already_holds_a_release_is_not_copied_into()
        {
            var home = this.Folder("home");
            new TemporaryInstall(new KnowledgeLayout(home)).Release("2026-04");
            var claude = this.Folder("claude");
            var plugin = new KnowledgeLayout(new DirectoryInfo(Path.Combine(claude.FullName, "plugins", "cache", "marketplace", LegacyInstallLocator.PluginName, "1.5.0")));
            new TemporaryInstall(plugin).Release(Tag);

            var (exitCode, _, error) = await this.Run(["list"], home, claude);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, error);
                Assert.That(new KnowledgeLayout(home).InstalledTags, Is.EqualTo(new[] { "2026-04" }));
            });
        }

        private DirectoryInfo Folder(string name) => Directory.CreateDirectory(Path.Combine(this.workspace.FullName, name));

        private async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments, DirectoryInfo home, DirectoryInfo claude)
        {
            // Outside any checkout, so the knowledge folder is HYPHA_HOME's.
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = this.Folder("elsewhere").FullName,
            };

            start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "hypha.dll"));
            foreach (var argument in arguments.Append("--no-logo"))
            {
                start.ArgumentList.Add(argument);
            }

            start.Environment[HyphaHome.EnvironmentVariable] = home.FullName;
            start.Environment["CLAUDE_CONFIG_DIR"] = claude.FullName;
            start.Environment["XDG_CONFIG_HOME"] = this.Folder("config").FullName;

            using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(1));

            return (process.ExitCode, await output, await error);
        }

        // A release in the current format, as a plugin installed it: the golden metamodel and a manifest.
        private sealed class TemporaryInstall(KnowledgeLayout layout)
        {
            public void Release(string tag)
            {
                var golden = Path.Combine(
                    KnowledgeLayout.Discover()!.Root.FullName, "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel");
                Directory.CreateDirectory(layout.Metamodel(tag).FullName);
                File.Copy(Path.Combine(golden, "metamodel.json"), layout.MetamodelGraph(tag).FullName);
                File.Copy(Path.Combine(golden, "index.json"), layout.MetamodelIndex(tag).FullName);

                var versions = new List<InstalledVersion>
                {
                    new(tag, new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                        new UpstreamReference(Upstream.PilotRepository, "def")),
                };
                VersionManifestFile.Write(
                    VersionManifest.Build(tag, versions), layout.VersionManifest.FullName);
            }
        }
    }
}
