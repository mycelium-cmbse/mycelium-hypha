// ------------------------------------------------------------------------------------------------
// <copyright file="ApiLayoutTests.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Tests for where the API inputs and the knowledge generated from them live.
    /// </summary>
    [TestFixture]
    public class ApiLayoutTests
    {
        private DirectoryInfo workspace = null!;
        private IApiLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"api-layout-{Guid.NewGuid():N}"));

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

        private string Relative(FileSystemInfo path) =>
            Path.GetRelativePath(this.workspace.FullName, path.FullName).Replace('\\', '/');

        [Test]
        public void Places_the_per_version_inputs_under_sources_api()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(this.layout.Sources("2026-04")), Is.EqualTo("sources/api/2026-04"));
                Assert.That(
                    this.Relative(this.layout.OpenApi("2026-04")),
                    Is.EqualTo("sources/api/2026-04/openapi/openapi.json"));
                Assert.That(this.Relative(this.layout.Xmi("2026-04")), Is.EqualTo("sources/api/2026-04/xmi"));
                Assert.That(
                    this.Relative(this.layout.PimModel("2026-04")),
                    Is.EqualTo("sources/api/2026-04/xmi/Systems-Modeling-API.xmi"));
                Assert.That(
                    this.Relative(this.layout.Specifications("2026-04")), Is.EqualTo("sources/api/2026-04/specs"));
            });
        }

        [Test]
        public void Places_the_generated_knowledge_under_knowledge_api()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(this.layout.Knowledge("2026-04")), Is.EqualTo("knowledge/api/2026-04"));
                Assert.That(this.Relative(this.layout.Pim("2026-04")), Is.EqualTo("knowledge/api/2026-04/pim"));
                Assert.That(
                    this.Relative(this.layout.PimGraph("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/pim/metamodel.json"));
                Assert.That(
                    this.Relative(this.layout.OpenApiKnowledge("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/openapi"));
                Assert.That(
                    this.Relative(this.layout.OpenApiIndex("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/openapi/index.json"));
                Assert.That(
                    this.Relative(this.layout.SpecificationText("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/spec"));
                Assert.That(
                    this.Relative(this.layout.SpecificationCatalog("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/spec/index.json"));
                Assert.That(
                    this.Relative(this.layout.CrossReferences("2026-04")),
                    Is.EqualTo("knowledge/api/2026-04/cross-references.json"));
            });
        }

        [Test]
        public void The_manifest_is_kept_apart_from_the_language_releases_manifest()
        {
            // The two axes are installed, switched and removed independently.
            var knowledge = new KnowledgeLayout(this.workspace);

            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(this.layout.VersionManifest), Is.EqualTo("knowledge/api/versions.json"));
                Assert.That(
                    this.layout.VersionManifest.FullName, Is.Not.EqualTo(knowledge.VersionManifest.FullName));
            });
        }

        [Test]
        public void The_primitive_types_are_the_ones_the_language_releases_share()
        {
            Assert.That(
                this.Relative(this.layout.SharedPrimitiveTypes), Is.EqualTo("sources/PrimitiveTypes.xmi"));
        }

        [Test]
        public void An_api_version_never_collides_with_a_language_release_of_the_same_tag()
        {
            var knowledge = new KnowledgeLayout(this.workspace);

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.layout.Sources("2026-04").FullName,
                    Is.Not.EqualTo(knowledge.ReleaseSources("2026-04").FullName));
                Assert.That(
                    this.layout.Knowledge("2026-04").FullName,
                    Is.Not.EqualTo(knowledge.Knowledge("2026-04").FullName));
            });
        }

        [Test]
        public void Inputs_are_read_from_the_repository_and_output_is_written_to_the_output_root()
        {
            // What lets `hypha api generate --output` write a scratch copy without touching the checkout.
            var output = new DirectoryInfo(Path.Combine(this.workspace.FullName, "elsewhere"));
            var redirected = new ApiLayout(new KnowledgeLayout(this.workspace, output));

            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(redirected.Sources("2026-04")), Is.EqualTo("sources/api/2026-04"));
                Assert.That(
                    this.Relative(redirected.PimModel("2026-04")),
                    Is.EqualTo("sources/api/2026-04/xmi/Systems-Modeling-API.xmi"));
                Assert.That(
                    this.Relative(redirected.SharedPrimitiveTypes), Is.EqualTo("sources/PrimitiveTypes.xmi"));
                Assert.That(
                    this.Relative(redirected.VersionManifest),
                    Is.EqualTo("knowledge/api/versions.json"),
                    "the manifest says what is installed, which is a property of the checkout");

                Assert.That(
                    this.Relative(redirected.Knowledge("2026-04")), Is.EqualTo("elsewhere/knowledge/api/2026-04"));
                Assert.That(
                    this.Relative(redirected.Pim("2026-04")), Is.EqualTo("elsewhere/knowledge/api/2026-04/pim"));
                Assert.That(
                    this.Relative(redirected.PimGraph("2026-04")),
                    Is.EqualTo("elsewhere/knowledge/api/2026-04/pim/metamodel.json"));
                Assert.That(
                    this.Relative(redirected.OpenApiIndex("2026-04")),
                    Is.EqualTo("elsewhere/knowledge/api/2026-04/openapi/index.json"));
                Assert.That(
                    this.Relative(redirected.SpecificationCatalog("2026-04")),
                    Is.EqualTo("elsewhere/knowledge/api/2026-04/spec/index.json"));
                Assert.That(
                    this.Relative(redirected.CrossReferences("2026-04")),
                    Is.EqualTo("elsewhere/knowledge/api/2026-04/cross-references.json"));
            });
        }

        [Test]
        public void A_blank_tag_is_refused_rather_than_silently_pointing_at_the_api_folder()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => this.layout.Knowledge(" "));
                Assert.Throws<ArgumentException>(() => this.layout.Sources(string.Empty));
                Assert.Throws<ArgumentException>(() => this.layout.Xmi(" "));
                Assert.Throws<ArgumentException>(() => this.layout.PimModel(string.Empty));
                Assert.Throws<ArgumentException>(() => this.layout.Pim(" "));
            });
        }

        [Test]
        public void Without_a_manifest_there_are_no_installed_versions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.Empty);
                Assert.That(this.layout.DefaultTag, Is.Null);
            });
        }

        [Test]
        public void The_installed_versions_come_from_the_manifest_and_are_re_read()
        {
            // Re-read rather than cached: installing rewrites the manifest, and a cached list would be
            // a stale answer for the rest of the run.
            Assert.That(this.layout.InstalledTags, Is.Empty);

            this.WriteManifest("2025-09", "2025-09", "2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04", "2025-09" }));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2025-09"));
            });

            this.WriteManifest("2026-04", "2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04" }));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-04"));
            });
        }

        [Test]
        public void An_installed_language_release_is_not_an_installed_api_version()
        {
            var knowledge = new KnowledgeLayout(this.workspace);

            VersionManifestFile.Write(
                VersionManifest.Build(
                    "2026-05",
                    [
                        new InstalledVersion(
                            "2026-05",
                            new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                            new UpstreamReference(Upstream.PilotRepository, "def")),
                    ]),
                knowledge.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.Empty);
                Assert.That(this.layout.DefaultTag, Is.Null);
            });
        }

        [Test]
        public void A_missing_knowledge_layout_is_refused()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new ApiLayout(null!));
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
    }
}
