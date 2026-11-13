// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeMigrationTests.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Migration;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    [TestFixture]
    public class KnowledgeMigrationTests
    {
        private DirectoryInfo workspace = null!;
        private DirectoryInfo home = null!;
        private KnowledgeLayout homeLayout = null!;
        private List<LegacyInstall> installs = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-migrate-{Guid.NewGuid():N}"));
            this.home = new DirectoryInfo(Path.Combine(this.workspace.FullName, "home"));
            this.homeLayout = new KnowledgeLayout(this.home);
            this.installs = [];
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
        public async Task An_empty_home_gets_every_release_and_API_version_the_clients_installed()
        {
            var plugin = this.Install("plugin", "Claude plugin 2.1.2", "2026-05", "2026-02", "2026-05");
            var code = this.Install("vscode", "VS Code", "2026-09.1", "2026-09.1");
            this.InstallApi(code, "2026-04");
            File.WriteAllText(Path.Combine(plugin.FullName, "sources", ReleaseInputs.SharedXmi), "<xmi/>");
            this.Older(plugin);

            var result = await this.Migration().MigrateOnceAsync();

            var manifest = VersionManifestFile.Read(this.homeLayout.VersionManifest.FullName);
            var api = ApiVersionManifestFile.ReadIfPresent(new ApiLayout(this.homeLayout).VersionManifest.FullName)!;
            Assert.Multiple(() =>
            {
                Assert.That(result.Skipped, Is.Null);
                Assert.That(
                    result.Copied.Select(copied => $"{copied.Kind} {copied.Tag} {copied.From}"),
                    Is.EqualTo(new[] { "release 2026-09.1 VS Code", "api 2026-04 VS Code", "release 2026-05 Claude plugin 2.1.2", "release 2026-02 Claude plugin 2.1.2" }));
                Assert.That(manifest.GetTags(), Is.EquivalentTo(new[] { "2026-09.1", "2026-05", "2026-02" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-09.1"), "the default is the one the most recently changed client used");
                Assert.That(api.GetTags(), Is.EqualTo(new[] { "2026-04" }));
                Assert.That(File.ReadAllText(Path.Combine(this.homeLayout.Knowledge("2026-05").FullName, "metamodel", "page.md")), Is.EqualTo("2026-05 from plugin"));
                Assert.That(File.ReadAllText(Path.Combine(this.homeLayout.ReleaseSources("2026-02").FullName, "xmi", "KerML.uml")), Is.EqualTo("2026-02 from plugin"));
                Assert.That(File.Exists(Path.Combine(new ApiLayout(this.homeLayout).Knowledge("2026-04").FullName, "pim", "page.md")), Is.True);
                Assert.That(this.homeLayout.SharedPrimitiveTypes.Exists, Is.True);
                Assert.That(File.Exists(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName)), Is.True);
                Assert.That(this.home.EnumerateDirectories("*" + KnowledgeMigration.StagingSuffix, SearchOption.AllDirectories), Is.Empty);
                Assert.That(File.Exists(Path.Combine(this.home.FullName, ReleaseOperationLock.FileName)), Is.False, "the lock is released");
            });
        }

        [Test]
        public async Task A_release_both_clients_installed_comes_from_the_one_changed_last()
        {
            var plugin = this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            this.Install("vscode", "VS Code", "2026-05", "2026-05");
            this.Older(plugin);

            var result = await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Copied.Select(copied => copied.From), Is.EqualTo(new[] { "VS Code" }));
                Assert.That(File.ReadAllText(Path.Combine(this.homeLayout.Knowledge("2026-05").FullName, "metamodel", "page.md")), Is.EqualTo("2026-05 from vscode"));
            });
        }

        [Test]
        public async Task Knowledge_read_from_anywhere_but_HYPHA_HOME_is_left_alone()
        {
            this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            var checkout = new KnowledgeLayout(Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "checkout")));

            var result = await this.Migration(checkout).MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Skipped, Does.Contain("not read from HYPHA_HOME"));
                Assert.That(this.home.Exists, Is.False);
            });
        }

        [Test]
        public async Task It_runs_once()
        {
            this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            var migration = this.Migration();
            await migration.MigrateOnceAsync();
            this.Install("vscode", "VS Code", "2026-09.1", "2026-09.1");

            var again = await migration.MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(again.Skipped, Does.Contain("already ran"));
                Assert.That(this.homeLayout.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
            });
        }

        [Test]
        public async Task A_home_that_already_has_a_release_is_left_alone_and_never_asked_again()
        {
            this.Install("plugin", "Claude plugin", "2026-02", "2026-02");
            WriteManifest(this.homeLayout, "2026-05", "2026-05");

            var result = await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Skipped, Does.Contain("already has a release"));
                Assert.That(this.homeLayout.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
                Assert.That(File.Exists(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName)), Is.True);
            });
        }

        [Test]
        public async Task With_no_client_install_nothing_is_recorded_so_a_later_one_is_still_found()
        {
            var result = await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Copied, Is.Empty);
                Assert.That(result.Skipped, Is.Null);
                Assert.That(File.Exists(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName)), Is.False);
            });
        }

        [Test]
        public async Task A_lock_another_hypha_holds_skips_the_migration_until_next_time()
        {
            this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            this.home.Create();
            File.WriteAllText(Path.Combine(this.home.FullName, ReleaseOperationLock.FileName), $"{{\"pid\":{Environment.ProcessId},\"token\":\"other\"}}");

            var result = await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Skipped, Does.Contain("another hypha is changing releases"));
                Assert.That(this.homeLayout.InstalledTags, Is.Empty);
                Assert.That(File.Exists(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName)), Is.False);
            });
        }

        [Test]
        public async Task A_client_whose_manifest_cannot_be_read_is_passed_over()
        {
            var broken = this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            File.WriteAllText(new KnowledgeLayout(broken).VersionManifest.FullName, "{ not json");
            this.Install("vscode", "VS Code", "2026-09.1", "2026-09.1");

            var result = await this.Migration().MigrateOnceAsync();

            Assert.That(result.Copied.Select(copied => copied.Tag), Is.EqualTo(new[] { "2026-09.1" }));
        }

        [Test]
        public async Task A_release_listed_without_its_knowledge_is_not_copied()
        {
            var plugin = this.Install("plugin", "Claude plugin", "2026-05", "2026-02", "2026-05");
            new KnowledgeLayout(plugin).Knowledge("2026-02").Delete(recursive: true);

            var result = await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.Copied.Select(copied => copied.Tag), Is.EqualTo(new[] { "2026-05" }));
                Assert.That(this.homeLayout.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
            });
        }

        [Test]
        public async Task A_folder_left_from_an_interrupted_copy_is_replaced()
        {
            this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            var leftover = Directory.CreateDirectory(this.homeLayout.Knowledge("2026-05").FullName + KnowledgeMigration.StagingSuffix);
            File.WriteAllText(Path.Combine(leftover.FullName, "stale.md"), "half");
            Directory.CreateDirectory(this.homeLayout.Knowledge("2026-05").FullName);
            File.WriteAllText(Path.Combine(this.homeLayout.Knowledge("2026-05").FullName, "orphan.md"), "not in any manifest");

            await this.Migration().MigrateOnceAsync();

            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(leftover.FullName), Is.False);
                Assert.That(File.Exists(Path.Combine(this.homeLayout.Knowledge("2026-05").FullName, "orphan.md")), Is.False);
                Assert.That(File.Exists(Path.Combine(this.homeLayout.Knowledge("2026-05").FullName, "metamodel", "page.md")), Is.True);
            });
        }

        [Test]
        public void A_cancelled_migration_installs_nothing_and_is_tried_again()
        {
            this.Install("plugin", "Claude plugin", "2026-05", "2026-05");
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.Multiple(() =>
            {
                Assert.That(() => this.Migration().MigrateOnceAsync(cancelled.Token), Throws.InstanceOf<OperationCanceledException>());
                Assert.That(File.Exists(this.homeLayout.VersionManifest.FullName), Is.False);
                Assert.That(File.Exists(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName)), Is.False);
            });
        }

        [Test]
        public async Task On_request_it_adds_what_home_lacks_and_keeps_its_default_even_after_a_migration_ran()
        {
            this.Install("plugin", "Claude plugin", "2026-09.1", "2026-02", "2026-09.1");
            WriteManifest(this.homeLayout, "2026-05", "2026-05");
            File.WriteAllText(Path.Combine(this.home.FullName, KnowledgeMigration.MarkerFileName), "ran");

            var result = await this.Migration().MigrateAsync();

            var manifest = VersionManifestFile.Read(this.homeLayout.VersionManifest.FullName);
            Assert.Multiple(() =>
            {
                Assert.That(result.Copied.Select(copied => copied.Tag), Is.EquivalentTo(new[] { "2026-09.1", "2026-02" }));
                Assert.That(manifest.GetTags(), Is.EquivalentTo(new[] { "2026-09.1", "2026-05", "2026-02" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public async Task On_request_it_copies_from_the_folders_named()
        {
            var elsewhere = this.Install("elsewhere", "unused", "2026-05", "2026-05");
            this.installs.Clear();

            var result = await this.Migration().MigrateAsync([elsewhere]);

            Assert.Multiple(() =>
            {
                Assert.That(result.Copied.Select(copied => (copied.Tag, copied.From)), Is.EqualTo(new[] { ("2026-05", elsewhere.FullName) }));
                Assert.That(this.homeLayout.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
            });
        }

        [Test]
        public async Task HYPHA_HOME_is_never_copied_into_itself()
        {
            WriteManifest(this.homeLayout, "2026-05", "2026-05");
            this.installs.Add(new LegacyInstall(this.home, "itself"));

            var result = await this.Migration().MigrateAsync();

            Assert.That(result.Copied, Is.Empty);
        }

        private static void WriteManifest(KnowledgeLayout layout, string defaultTag, params string[] tags)
        {
            foreach (var tag in tags)
            {
                Seed(Path.Combine(layout.Knowledge(tag).FullName, "metamodel", "page.md"), $"{tag} from {layout.Root.Name}");
                Seed(Path.Combine(layout.ReleaseSources(tag).FullName, "xmi", "KerML.uml"), $"{tag} from {layout.Root.Name}");
            }

            VersionManifestFile.Write(
                VersionManifest.Build(defaultTag, tags.Select(tag => new InstalledVersion(tag, Reference(Upstream.ReleaseRepository), Reference(Upstream.PilotRepository)))),
                layout.VersionManifest.FullName);
        }

        private static UpstreamReference Reference(string repository) => new(repository, "abc");

        private static void Seed(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        private DirectoryInfo Install(string folder, string origin, string defaultTag, params string[] tags)
        {
            var root = new DirectoryInfo(Path.Combine(this.workspace.FullName, folder));
            WriteManifest(new KnowledgeLayout(root), defaultTag, tags);
            this.installs.Add(new LegacyInstall(root, origin));

            return root;
        }

        private void InstallApi(DirectoryInfo root, string version)
        {
            var api = new ApiLayout(new KnowledgeLayout(root));
            Seed(Path.Combine(api.Knowledge(version).FullName, "pim", "page.md"), version);
            Seed(Path.Combine(api.Sources(version).FullName, "openapi", "openapi.json"), "{}");
            ApiVersionManifestFile.Write(
                ApiVersionManifest.Build(version, [new InstalledApiVersion(version, Reference("services"), Reference(Upstream.ReleaseRepository), Reference(Upstream.PilotRepository), "pim")]),
                api.VersionManifest.FullName);
        }

        private void Older(DirectoryInfo root) =>
            File.SetLastWriteTimeUtc(new KnowledgeLayout(root).VersionManifest.FullName, DateTime.UtcNow.AddDays(-7));

        private KnowledgeMigration Migration(IKnowledgeLayout? layout = null) =>
            new(
                layout ?? this.homeLayout,
                this.home,
                new Installs(this.installs),
                new SystemProcessProbe(),
                NullLogger<ReleaseOperationLock>.Instance,
                NullLogger<KnowledgeMigration>.Instance);

        private sealed class Installs(List<LegacyInstall> installs) : ILegacyInstallLocator
        {
            public IReadOnlyList<LegacyInstall> Locate() => installs.ToList();
        }
    }
}
