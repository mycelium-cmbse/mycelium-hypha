// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseRemoverTests.cs" company="Starion Group S.A.">
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
    /// Suite of tests for <see cref="ReleaseRemover"/>: removing exactly one release, on request.
    /// </summary>
    [TestFixture]
    public class ReleaseRemoverTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private ReleaseRemover remover = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-remove-{Guid.NewGuid():N}"));

            this.layout = new KnowledgeLayout(this.workspace);
            this.remover = new ReleaseRemover(this.layout, NullLogger<ReleaseRemover>.Instance);
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
        public async Task A_non_default_release_is_removed_from_disk_and_the_manifest()
        {
            this.Seed("2026-05");
            this.Seed("2026-04");
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            await this.remover.RemoveAsync("2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.False);
                Assert.That(this.layout.Knowledge("2026-04").Exists, Is.False);
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public void The_current_default_is_refused()
        {
            this.Seed("2026-05");
            this.Seed("2026-04");
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => this.remover.RemoveAsync("2026-05"));

            Assert.That(exception!.Message, Does.Contain("hypha use"));

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.True);
                Assert.That(this.layout.ReleaseSources("2026-04").Exists, Is.True);
            });
        }

        [Test]
        public void An_uninstalled_tag_is_refused()
        {
            this.Seed("2026-05");
            this.WriteManifest("2026-05", "2026-05");

            Assert.ThrowsAsync<ArgumentException>(() => this.remover.RemoveAsync("2026-06"));
        }

        [Test]
        public void The_only_installed_release_is_refused_without_force()
        {
            this.Seed("2026-05");
            this.WriteManifest("2026-05", "2026-05");

            Assert.ThrowsAsync<InvalidOperationException>(() => this.remover.RemoveAsync("2026-05"));
        }

        [Test]
        public async Task Force_removes_the_last_release_and_deletes_the_manifest()
        {
            this.Seed("2026-05");
            this.WriteManifest("2026-05", "2026-05");

            await this.remover.RemoveAsync("2026-05", force: true);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.ReleaseSources("2026-05").Exists, Is.False);
                Assert.That(this.layout.VersionManifest.Exists, Is.False);
                Assert.That(this.layout.InstalledTags, Is.Empty);
            });
        }

        [Test]
        public void Removing_without_a_manifest_is_refused()
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => this.remover.RemoveAsync("2026-05"));
        }

        [Test]
        public async Task A_release_that_was_never_actually_written_still_drops_from_the_manifest()
        {
            // The manifest can outlive the folders if something deleted them out of band.
            this.Seed("2026-05");
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            await this.remover.RemoveAsync("2026-04");

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05" }));
        }
    }
}
