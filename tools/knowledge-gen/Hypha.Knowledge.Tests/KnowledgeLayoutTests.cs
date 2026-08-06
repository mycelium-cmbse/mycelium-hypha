// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeLayoutTests.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Tests for the one place that knows where things live.
    /// </summary>
    [TestFixture]
    public class KnowledgeLayoutTests
    {
        private DirectoryInfo workspace = null!;
        private IKnowledgeLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"layout-{Guid.NewGuid():N}"));

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

        private string Relative(FileSystemInfo path) =>
            Path.GetRelativePath(this.workspace.FullName, path.FullName).Replace('\\', '/');

        [Test]
        public void Places_the_per_release_inputs_under_sources()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(this.layout.Xmi("2026-05")), Is.EqualTo("sources/2026-05/xmi"));
                Assert.That(
                    this.Relative(this.layout.TextualSources("2026-05")), Is.EqualTo("sources/2026-05/textual"));
                Assert.That(
                    this.Relative(this.layout.Bnf("2026-05")), Is.EqualTo("sources/2026-05/textual/bnf"));
                Assert.That(
                    this.Relative(this.layout.Specifications("2026-05")), Is.EqualTo("sources/2026-05/specs"));
            });
        }

        [Test]
        public void Places_the_generated_knowledge_under_knowledge()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Relative(this.layout.Knowledge("2026-05")), Is.EqualTo("knowledge/2026-05"));
                Assert.That(
                    this.Relative(this.layout.Metamodel("2026-05")), Is.EqualTo("knowledge/2026-05/metamodel"));
                Assert.That(
                    this.Relative(this.layout.MetamodelIndex("2026-05")),
                    Is.EqualTo("knowledge/2026-05/metamodel/index.json"));
                Assert.That(
                    this.Relative(this.layout.TextualNotation("2026-05")),
                    Is.EqualTo("knowledge/2026-05/textual-notation"));
                Assert.That(
                    this.Relative(this.layout.Examples("2026-05")),
                    Is.EqualTo("knowledge/2026-05/textual-notation/examples"));
                Assert.That(
                    this.Relative(this.layout.CrossReferences("2026-05")),
                    Is.EqualTo("knowledge/2026-05/cross-references.json"));
            });
        }

        [Test]
        public void The_shared_and_per_release_inputs_are_kept_apart()
        {
            // PrimitiveTypes.xmi is the OMG UML primitives library: published by neither upstream and
            // identical for every release, so it sits above the tag folders rather than inside one.
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.Relative(this.layout.SharedPrimitiveTypes), Is.EqualTo("sources/PrimitiveTypes.xmi"));
                Assert.That(this.Relative(this.layout.VersionManifest), Is.EqualTo("knowledge/versions.json"));
                Assert.That(
                    this.Relative(this.layout.CrossReferenceSchema),
                    Is.EqualTo("knowledge/cross-references.schema.json"));
            });
        }

        [Test]
        public void The_specification_catalog_is_per_release_and_per_document() =>
            Assert.That(
                this.Relative(this.layout.SpecificationCatalog("2026-05", "kerml")),
                Is.EqualTo("knowledge/2026-05/spec/kerml/index.json"));

        [Test]
        public void A_grammar_is_named_within_its_release()
        {
            Assert.That(
                this.Relative(this.layout.Grammar("2026-05", "KerML-textual-bnf.kebnf")),
                Is.EqualTo("sources/2026-05/textual/bnf/KerML-textual-bnf.kebnf"));
        }

        [Test]
        public void A_blank_tag_is_refused_rather_than_silently_pointing_at_the_root()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => this.layout.Knowledge(" "));
                Assert.Throws<ArgumentException>(() => this.layout.Xmi(string.Empty));
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
            // Re-read rather than cached: generation rewrites the manifest, and a cached list would be
            // a stale answer for the rest of the run.
            Assert.That(this.layout.InstalledTags, Is.Empty);

            Releases.VersionManifestFile.Write(
                Releases.VersionManifest.Build(
                    "2026-05",
                    [
                        new Releases.InstalledVersion(
                            "2026-05",
                            new Releases.UpstreamReference(Releases.Upstream.ReleaseRepository, "abc"),
                            new Releases.UpstreamReference(Releases.Upstream.PilotRepository, "def")),
                    ]),
                this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-05" }));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public void Discovery_finds_a_folder_holding_both_trees()
        {
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "sources"));
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "knowledge"));

            var nested = Directory.CreateDirectory(
                Path.Combine(this.workspace.FullName, "tools", "somewhere", "bin"));

            Assert.That(KnowledgeLayout.Discover(nested)?.Root.FullName, Is.EqualTo(this.workspace.FullName));
        }

        [Test]
        public void Discovery_needs_both_trees_not_just_one()
        {
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "knowledge"));

            var found = KnowledgeLayout.Discover(this.workspace);

            // A folder with only one of the two is not a checkout of this repository; walking on is
            // the right answer, and null when nothing above it qualifies either.
            Assert.That(found?.Root.FullName, Is.Not.EqualTo(this.workspace.FullName));
        }

        [Test]
        public void This_test_run_is_inside_the_repository() =>
            Assert.That(
                Repository.Layout, Is.Not.Null,
                "the generation fixtures rely on discovery working from the test binary");
    }
}
