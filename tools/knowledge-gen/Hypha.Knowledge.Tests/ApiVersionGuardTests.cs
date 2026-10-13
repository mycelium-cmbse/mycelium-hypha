// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionGuardTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Suite of tests for which tags may be offered, installed and removed as an API version.
    /// </summary>
    /// <remarks>
    /// The PIM model is one fixed OMG document, dated 2025-02. A tag older than that would pair it with
    /// an earlier OpenAPI document and specification, and several such tags do not carry the files a
    /// fetch reads at all.
    /// </remarks>
    [TestFixture]
    public class ApiVersionGuardTests
    {
        private static readonly string[] Tags = ["2026-04", "2025-02", "2025-01", "2024-12", "2022-02"];

        private DirectoryInfo workspace = null!;
        private ApiLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-guard-{Guid.NewGuid():N}"));
            this.layout = new ApiLayout(new KnowledgeLayout(this.workspace));
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
        public void A_tag_older_than_the_pim_model_is_not_offered()
        {
            Assert.That(ApiCatalog.Available(Tags, Tags, Tags), Is.EqualTo(new[] { "2026-04", "2025-02" }));
        }

        [TestCase("2026-04", true)]
        [TestCase("2025-02", true)]
        [TestCase("2025-02.1", true)]
        [TestCase("2025-01", false)]
        [TestCase("2022-02", false)]
        [TestCase("main", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void A_tag_is_offerable_when_it_is_version_shaped_and_not_older_than_the_earliest(string? tag, bool expected)
        {
            Assert.That(ApiCatalog.IsOfferable(tag), Is.EqualTo(expected));
        }

        [TestCase("2025-01")]
        [TestCase("2022-02")]
        public void Installing_a_tag_older_than_the_earliest_is_refused_before_anything_is_fetched(string tag)
        {
            var fetcher = new Mock<IReleaseFetcher>(MockBehavior.Strict);
            var documents = new Mock<IDocumentDownloader>(MockBehavior.Strict);

            var installer = new ApiInstaller(
                fetcher.Object,
                documents.Object,
                Mock.Of<ICommitResolver>(),
                this.layout,
                new HyphaKnowledgeOptions(),
                NullLogger<ApiInstaller>.Instance);

            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => installer.InstallAsync(new ApiInstallRequest { Tag = tag }));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain(ApiCatalog.EarliestVersion));
                Assert.That(this.layout.VersionManifest.Exists, Is.False);
            });
        }

        [TestCase("..")]
        [TestCase("../..")]
        [TestCase("main")]
        public void Removing_something_that_is_not_a_version_is_refused_whatever_the_manifest_says(string tag)
        {
            // The tag becomes a folder that is deleted recursively: a hand-edited manifest must not be
            // able to point that at sources/ or knowledge/ themselves.
            var manifest = this.layout.VersionManifest;
            manifest.Directory!.Create();
            File.WriteAllText(
                manifest.FullName,
                $$"""
                {
                  "schemaVersion": "1.0.0",
                  "default": "2026-04",
                  "versions": [
                    { "tag": "2026-04", "services": { "repo": "a/b", "commit": "1" }, "release": { "repo": "a/c", "commit": "2" }, "pilot": { "repo": "a/d", "commit": "3" }, "pim": "x" }
                  ]
                }
                """);
            var bystander = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "sources", "2026-05"));

            var remover = new ApiRemover(this.layout, NullLogger<ApiRemover>.Instance);

            Assert.Multiple(() =>
            {
                Assert.That(() => remover.RemoveAsync(tag, force: true), Throws.ArgumentException);

                bystander.Refresh();
                Assert.That(bystander.Exists, Is.True);
                Assert.That(manifest.Exists, Is.True);
            });
        }
    }
}
