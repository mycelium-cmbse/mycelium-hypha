// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseWindowEvictorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Tests for shrinking the installed releases back to the rolling window.
    /// </summary>
    [TestFixture]
    public class ReleaseWindowEvictorTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private ReleaseWindowEvictor evictor = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-evict-{Guid.NewGuid():N}"));

            this.layout = new KnowledgeLayout(this.workspace);
            this.evictor = new ReleaseWindowEvictor(this.layout, NullLogger<ReleaseWindowEvictor>.Instance);
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

        private void WriteManifest(string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledVersion(
                    tag,
                    new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                    new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(
                VersionManifest.Build(defaultTag, versions), this.layout.VersionManifest.FullName);
        }

        [Test]
        public async Task The_oldest_release_beyond_keep_is_deleted_from_disk_and_the_manifest()
        {
            this.Seed("2026-04");
            this.Seed("2026-05");
            this.Seed("2026-06");
            this.WriteManifest("2026-06", "2026-04", "2026-05", "2026-06");

            var evicted = await this.evictor.EvictAsync(keep: 2);

            Assert.Multiple(() =>
            {
                Assert.That(evicted, Is.EqualTo(new[] { "2026-04" }));
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.False);
                Assert.That(this.layout.Knowledge("2026-04").Exists, Is.False);
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
                Assert.That(this.layout.ReleaseSources("2026-06").Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-06", "2026-05" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-06"));
            });
        }

        [Test]
        public async Task Nothing_is_evicted_when_the_window_is_not_over_size()
        {
            this.Seed("2026-05");
            this.WriteManifest("2026-05", "2026-05");

            var evicted = await this.evictor.EvictAsync(keep: 2);

            Assert.Multiple(() =>
            {
                Assert.That(evicted, Is.Empty);
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
            });
        }

        [Test]
        public async Task Evicting_a_release_that_was_never_actually_fetched_does_not_throw()
        {
            // The manifest can outlive the folders if something deleted them out of band; eviction
            // should still succeed and update the manifest rather than fail on a missing directory.
            this.WriteManifest("2026-06", "2026-04", "2026-05", "2026-06");

            var evicted = await this.evictor.EvictAsync(keep: 2);

            Assert.That(evicted, Is.EqualTo(new[] { "2026-04" }));
        }

        [Test]
        public void Evicting_the_default_release_is_refused_and_nothing_is_deleted()
        {
            // Unreachable in a normal run (the default always tracks the newest tag), but a stale or
            // hand-edited manifest could still ask for it - refuse loudly rather than leave every
            // skill pointing at a folder that no longer exists.
            this.Seed("2026-04");
            this.Seed("2026-05");
            this.WriteManifest("2026-04", "2026-04", "2026-05");

            Assert.That(() => this.evictor.EvictAsync(keep: 1), Throws.InvalidOperationException);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.True);
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05", "2026-04" }));
        }

        [Test]
        public void Evicting_without_a_manifest_is_refused()
        {
            Assert.That(() => this.evictor.EvictAsync(keep: 2), Throws.InvalidOperationException);
        }
    }
}
