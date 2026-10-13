// ------------------------------------------------------------------------------------------------
// <copyright file="ApiRemoverTests.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Suite of tests for <see cref="ApiRemover"/>: removing exactly one API version, on request, under
    /// the same rules as a language release.
    /// </summary>
    [TestFixture]
    public class ApiRemoverTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout knowledge = null!;
        private ApiLayout layout = null!;
        private ApiRemover remover = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-remove-{Guid.NewGuid():N}"));

            this.knowledge = new KnowledgeLayout(this.workspace);
            this.layout = new ApiLayout(this.knowledge);
            this.remover = new ApiRemover(this.layout, NullLogger<ApiRemover>.Instance);
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
            File.WriteAllText(this.layout.PimModel(tag).FullName, "<xmi/>");

            var pim = this.layout.Pim(tag);
            pim.Create();
            File.WriteAllText(Path.Combine(pim.FullName, "index.json"), "{}");
        }

        private void WriteManifest(string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledApiVersion(
                    tag,
                    new UpstreamReference(ApiUpstream.ServicesRepository, "abc"),
                    new UpstreamReference(Upstream.ReleaseRepository, "def"),
                    new UpstreamReference(Upstream.PilotRepository, "ghi"),
                    ApiUpstream.PimDocument));

            ApiVersionManifestFile.Write(
                ApiVersionManifest.Build(defaultTag, versions), this.layout.VersionManifest.FullName);
        }

        [Test]
        public async Task A_non_default_version_is_removed_from_disk_and_the_manifest()
        {
            this.Seed("2026-04");
            this.Seed("2025-09");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            await this.remover.RemoveAsync("2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.Sources("2025-09").Exists, Is.False);
                Assert.That(this.layout.Knowledge("2025-09").Exists, Is.False);
                Assert.That(this.layout.Sources("2026-04").Exists, Is.True);
                Assert.That(this.layout.Knowledge("2026-04").Exists, Is.True);
            });

            var manifest = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest!.GetTags(), Is.EqualTo(new[] { "2026-04" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-04"));
            });
        }

        [Test]
        public void The_current_default_is_refused()
        {
            this.Seed("2026-04");
            this.Seed("2025-09");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => this.remover.RemoveAsync("2026-04"));

            Assert.That(exception!.Message, Does.Contain("hypha api use"));

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.Sources("2026-04").Exists, Is.True);
                Assert.That(this.layout.Sources("2025-09").Exists, Is.True);
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04", "2025-09" }));
            });
        }

        [Test]
        public void The_current_default_is_refused_even_with_force_while_another_could_take_its_place()
        {
            // --force is for the last version installed; it is not a way to leave a manifest whose
            // default is gone.
            this.Seed("2026-04");
            this.Seed("2025-09");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            Assert.ThrowsAsync<InvalidOperationException>(() => this.remover.RemoveAsync("2026-04", force: true));

            Assert.That(this.layout.Sources("2026-04").Exists, Is.True);
        }

        [Test]
        public void An_uninstalled_tag_is_refused()
        {
            this.Seed("2026-04");
            this.WriteManifest("2026-04", "2026-04");

            var exception = Assert.ThrowsAsync<ArgumentException>(() => this.remover.RemoveAsync("2026-05"));

            Assert.That(exception!.Message, Does.Contain("'2026-05' is not installed"));
        }

        [Test]
        public void The_only_installed_version_is_refused_without_force()
        {
            this.Seed("2026-04");
            this.WriteManifest("2026-04", "2026-04");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => this.remover.RemoveAsync("2026-04"));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("--force"));
                Assert.That(this.layout.Sources("2026-04").Exists, Is.True);
                Assert.That(this.layout.VersionManifest.Exists, Is.True);
            });
        }

        [Test]
        public async Task Force_removes_the_last_version_and_deletes_the_manifest()
        {
            this.Seed("2026-04");
            this.WriteManifest("2026-04", "2026-04");

            await this.remover.RemoveAsync("2026-04", force: true);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.Sources("2026-04").Exists, Is.False);
                Assert.That(this.layout.Knowledge("2026-04").Exists, Is.False);
                Assert.That(this.layout.VersionManifest.Exists, Is.False);
                Assert.That(this.layout.InstalledTags, Is.Empty);
            });
        }

        [Test]
        public void Removing_without_a_manifest_is_refused()
        {
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => this.remover.RemoveAsync("2026-04"));

            Assert.That(exception!.Message, Does.Contain("nothing is installed to remove"));
        }

        [Test]
        public async Task A_version_that_was_never_actually_written_still_drops_from_the_manifest()
        {
            // The manifest can outlive the folders if something deleted them out of band.
            this.Seed("2026-04");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            await this.remover.RemoveAsync("2025-09");

            Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04" }));
        }

        [Test]
        public async Task Removing_an_api_version_leaves_the_language_release_of_the_same_tag_alone()
        {
            // The same tag names a language release and an API version; only one of them was asked for.
            this.Seed("2026-04");
            this.Seed("2025-09");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            var releaseSources = this.knowledge.Xmi("2025-09");
            releaseSources.Create();
            File.WriteAllText(Path.Combine(releaseSources.FullName, "KerML.uml"), "<xmi/>");

            var releaseKnowledge = this.knowledge.Metamodel("2025-09");
            releaseKnowledge.Create();
            File.WriteAllText(Path.Combine(releaseKnowledge.FullName, "index.json"), "{}");

            await this.remover.RemoveAsync("2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(this.knowledge.ReleaseSources("2025-09").Exists, Is.True);
                Assert.That(this.knowledge.Knowledge("2025-09").Exists, Is.True);
                Assert.That(File.Exists(Path.Combine(releaseSources.FullName, "KerML.uml")), Is.True);
            });
        }

        [Test]
        public void A_cancelled_removal_changes_nothing()
        {
            this.Seed("2026-04");
            this.Seed("2025-09");
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            using var source = new CancellationTokenSource();
            source.Cancel();

            Assert.That(
                () => this.remover.RemoveAsync("2025-09", cancellationToken: source.Token),
                Throws.InstanceOf<OperationCanceledException>());

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.Sources("2025-09").Exists, Is.True);
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04", "2025-09" }));
            });
        }

        [Test]
        public void A_blank_tag_is_refused()
        {
            Assert.That(() => this.remover.RemoveAsync(" "), Throws.ArgumentException);
        }
    }
}
