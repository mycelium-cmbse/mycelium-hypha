// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionStatusTests.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    // Over a real ApiLayout in a temporary folder, so what "generated" means is pinned against the
    // paths the API generators actually write to.
    [TestFixture]
    public class ApiVersionStatusTests
    {
        private DirectoryInfo workspace = null!;
        private ApiLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"api-status-{Guid.NewGuid():N}"));

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
        public void ReadAll_reports_every_installed_version_in_manifest_order()
        {
            this.Install("2026-04", "2026-04", "2025-02");
            this.GenerateCore("2026-04");
            this.layout.Sources("2026-04").Create();

            var versions = ApiVersionStatus.ReadAll(this.layout);

            Assert.Multiple(() =>
            {
                Assert.That(versions.Select(version => version.Tag), Is.EqualTo(new[] { "2026-04", "2025-02" }));
                Assert.That(versions[0].IsDefault, Is.True);
                Assert.That(versions[0].SourcesFetched, Is.True);
                Assert.That(versions[0].Artifacts.Core, Is.True);
                Assert.That(versions[1].IsDefault, Is.False);
                Assert.That(versions[1].SourcesFetched, Is.False);
                Assert.That(versions[1].Artifacts.Core, Is.False);
            });
        }

        [Test]
        public void ReadAll_with_nothing_installed_is_empty()
        {
            Assert.That(ApiVersionStatus.ReadAll(this.layout), Is.Empty);
        }

        [Test]
        public void Read_reports_one_version_and_whether_it_is_the_default()
        {
            this.Install("2026-04", "2026-04", "2025-02");

            Assert.Multiple(() =>
            {
                Assert.That(ApiVersionStatus.Read(this.layout, "2026-04").IsDefault, Is.True);
                Assert.That(ApiVersionStatus.Read(this.layout, "2025-02").IsDefault, Is.False);
            });
        }

        [Test]
        public void Read_of_a_version_that_is_not_installed_reads_as_nothing_present()
        {
            var status = ApiVersionStatus.Read(this.layout, "2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(status.IsDefault, Is.False);
                Assert.That(status.SourcesFetched, Is.False);
                Assert.That(status.Artifacts, Is.EqualTo(new ApiArtifacts(false, false, false, false)));
            });
        }

        [Test]
        public void Each_artifact_is_read_from_its_own_file()
        {
            this.Install("2026-04", "2026-04");
            Write(this.layout.PimGraph("2026-04"));
            Write(this.layout.SpecificationCatalog("2026-04"));

            var artifacts = ApiVersionStatus.Read(this.layout, "2026-04").Artifacts;

            Assert.Multiple(() =>
            {
                Assert.That(artifacts.Pim, Is.True);
                Assert.That(artifacts.OpenApi, Is.False);
                Assert.That(artifacts.CrossReferences, Is.False);
                Assert.That(artifacts.Specification, Is.True);
                Assert.That(artifacts.Core, Is.False, "the specification text is not part of the core");
            });
        }

        [TestCase("pim")]
        [TestCase("openapi")]
        [TestCase("cross-references")]
        public void An_empty_file_is_not_generated(string artifact)
        {
            // An interrupted write leaves an empty file behind; it must not read as a usable version.
            this.Install("2026-04", "2026-04");
            this.GenerateCore("2026-04");
            var file = artifact switch
            {
                "pim" => this.layout.PimGraph("2026-04"),
                "openapi" => this.layout.OpenApiIndex("2026-04"),
                _ => this.layout.CrossReferences("2026-04"),
            };
            File.WriteAllText(file.FullName, string.Empty);

            Assert.That(ApiVersionStatus.Read(this.layout, "2026-04").Artifacts.Core, Is.False);
        }

        [Test]
        public void An_empty_specification_catalog_is_not_extracted()
        {
            this.Install("2026-04", "2026-04");
            var catalog = this.layout.SpecificationCatalog("2026-04");
            catalog.Directory!.Create();
            File.WriteAllText(catalog.FullName, string.Empty);

            Assert.That(ApiVersionStatus.Read(this.layout, "2026-04").Artifacts.Specification, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void A_blank_tag_is_refused(string? tag)
        {
            Assert.That(() => ApiVersionStatus.Read(this.layout, tag!), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void A_missing_layout_is_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => ApiVersionStatus.Read(null!, "2026-04"), Throws.ArgumentNullException);
                Assert.That(() => ApiVersionStatus.ReadAll(null!), Throws.ArgumentNullException);
            });
        }

        private static void Write(FileInfo file)
        {
            file.Directory!.Create();
            File.WriteAllText(file.FullName, "{}");
        }

        private void GenerateCore(string tag)
        {
            Write(this.layout.PimGraph(tag));
            Write(this.layout.OpenApiIndex(tag));
            Write(this.layout.CrossReferences(tag));
        }

        private void Install(string defaultTag, params string[] tags)
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
