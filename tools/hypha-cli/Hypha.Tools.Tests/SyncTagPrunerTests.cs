// ------------------------------------------------------------------------------------------------
// <copyright file="SyncTagPrunerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.IO;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Sync;

    /// <summary>
    /// Suite of tests for <see cref="SyncTagPruner"/>: the on-disk half of what <c>hypha sync</c> does
    /// when a newer release replaces the one it previously added itself.
    /// </summary>
    /// <remarks>
    /// Real <see cref="VersionManifestFile"/> I/O against a temp directory, the same style
    /// <c>ReleaseWindowEvictorTests</c> already uses - a mocked <see cref="IKnowledgeLayout"/> would
    /// only be testing that the mock was called, not that the manifest ends up correct.
    /// </remarks>
    [TestFixture]
    public class SyncTagPrunerTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-sync-prune-{Guid.NewGuid():N}"));

            this.layout = new KnowledgeLayout(this.workspace);
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
        public void The_previous_locally_added_release_is_deleted_and_dropped_from_the_manifest()
        {
            this.Seed("2026-05");
            this.Seed("2026-04");
            this.Seed("2026-06");
            WriteManifest(this.layout, "2026-06", "2026-04", "2026-05", "2026-06");

            SyncTagPruner.Prune(this.layout, "2026-05", committedBaselineTags: ["2026-04"]);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.False);
                Assert.That(this.layout.Knowledge("2026-05").Exists, Is.False);
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.True);
                Assert.That(this.layout.ReleaseSources("2026-06").Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-06", "2026-04" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-06"));
            });
        }

        [Test]
        public void A_committed_baseline_release_is_never_pruned()
        {
            this.Seed("2026-05");
            this.Seed("2026-04");
            WriteManifest(this.layout, "2026-05", "2026-04", "2026-05");

            Assert.That(
                () => SyncTagPruner.Prune(this.layout, "2026-04", committedBaselineTags: ["2026-05", "2026-04"]),
                Throws.InvalidOperationException);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.True);
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05", "2026-04" }));
        }

        [Test]
        public void The_manifests_default_release_is_never_pruned()
        {
            // Structurally unreachable from SyncCommand (it always fetches with MakeDefault: true
            // before pruning), asserted here anyway as the same defence-in-depth ReleaseWindowEvictor
            // has for the same mistake.
            this.Seed("2026-05");
            WriteManifest(this.layout, "2026-05", "2026-05");

            Assert.That(
                () => SyncTagPruner.Prune(this.layout, "2026-05", committedBaselineTags: null),
                Throws.InvalidOperationException);

            Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
        }

        [Test]
        public void Pruning_a_release_that_was_never_actually_written_does_not_throw()
        {
            this.Seed("2026-06");
            WriteManifest(this.layout, "2026-06", "2026-05", "2026-06");

            Assert.That(
                () => SyncTagPruner.Prune(this.layout, "2026-05", committedBaselineTags: null),
                Throws.Nothing);

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-06" }));
        }

        [Test]
        public void Pruning_with_no_manifest_at_all_does_nothing()
        {
            Assert.That(
                () => SyncTagPruner.Prune(this.layout, "2026-05", committedBaselineTags: null),
                Throws.Nothing);
        }

        /// <summary>Seeds a real, if empty, release: a manifest entry plus dummy source/knowledge files.</summary>
        private void Seed(string tag)
        {
            var xmi = this.layout.Xmi(tag);
            xmi.Create();
            File.WriteAllText(Path.Combine(xmi.FullName, "KerML.uml"), "<xmi/>");

            var metamodel = this.layout.Metamodel(tag);
            metamodel.Create();
            File.WriteAllText(Path.Combine(metamodel.FullName, "index.json"), "{}");
        }

        private static void WriteManifest(KnowledgeLayout layout, string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledVersion(
                    tag,
                    new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                    new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(
                VersionManifest.Build(defaultTag, versions), layout.VersionManifest.FullName);
        }
    }
}

