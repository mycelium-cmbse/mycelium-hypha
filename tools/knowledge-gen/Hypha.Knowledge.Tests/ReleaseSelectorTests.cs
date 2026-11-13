// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSelectorTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    [TestFixture]
    public class ReleaseSelectorTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private ReleaseSelector selector = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-select-{Guid.NewGuid():N}"));
            this.layout = new KnowledgeLayout(this.workspace);
            this.selector = new ReleaseSelector(this.layout);
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
        public void An_installed_release_becomes_the_default()
        {
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            var selection = this.selector.Use("2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(selection, Is.EqualTo(new ReleaseSelection("2026-04", "2026-05")));
                Assert.That(selection.Changed, Is.True);
                Assert.That(VersionManifestFile.Read(this.layout.VersionManifest.FullName).Default, Is.EqualTo("2026-04"));
                Assert.That(VersionManifestFile.Read(this.layout.VersionManifest.FullName).GetTags(), Is.EquivalentTo(new[] { "2026-05", "2026-04" }));
            });
        }

        [Test]
        public void The_current_default_is_left_alone_without_rewriting_the_manifest()
        {
            this.WriteManifest("2026-05", "2026-05", "2026-04");
            var manifest = this.layout.VersionManifest;
            var written = DateTime.UtcNow.AddDays(-1);
            File.SetLastWriteTimeUtc(manifest.FullName, written);

            var selection = this.selector.Use("2026-05");

            manifest.Refresh();
            Assert.Multiple(() =>
            {
                Assert.That(selection.Changed, Is.False);
                Assert.That(selection.Previous, Is.EqualTo("2026-05"));
                Assert.That(manifest.LastWriteTimeUtc, Is.EqualTo(written));
            });
        }

        [Test]
        public void A_release_that_is_not_installed_is_refused_naming_the_installed_ones()
        {
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => this.selector.Use("2026-06"),
                    Throws.ArgumentException.With.Message.EqualTo("'2026-06' is not installed; the installed releases are 2026-05, 2026-04"));
                Assert.That(VersionManifestFile.Read(this.layout.VersionManifest.FullName).Default, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public void With_nothing_installed_it_says_so()
        {
            Assert.That(
                () => this.selector.Use("2026-05"),
                Throws.ArgumentException.With.Message.EqualTo("'2026-05' is not installed, and nothing is installed yet"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void A_blank_tag_is_refused(string? tag)
        {
            Assert.That(() => this.selector.Use(tag!), Throws.InstanceOf<ArgumentException>());
        }

        private void WriteManifest(string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledVersion(
                    tag,
                    new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                    new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(VersionManifest.Build(defaultTag, versions), this.layout.VersionManifest.FullName);
        }
    }
}
