// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseStatusTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;

    /// <summary>Suite of tests for <see cref="ReleaseStatus"/>.</summary>
    [TestFixture]
    public class ReleaseStatusTests
    {
        /// <summary>The scratch repository root each test creates and deletes again.</summary>
        private DirectoryInfo root = null!;

        /// <summary>The knowledge layout over <see cref="root"/> that the release status is read from.</summary>
        private KnowledgeLayout layout = null!;

        /// <summary>Creates an empty scratch repository root and the layout over it.</summary>
        [SetUp]
        public void SetUp()
        {
            this.root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-status-" + Guid.NewGuid().ToString("N")));
            this.root.Create();
            this.layout = new KnowledgeLayout(this.root);
        }

        /// <summary>Deletes the scratch repository root, if a test left it behind.</summary>
        [TearDown]
        public void TearDown()
        {
            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [Test]
        public void Nothing_installed_is_an_empty_list()
        {
            Assert.That(ReleaseStatus.ReadAll(this.layout), Is.Empty);
        }

        [Test]
        public void A_fetched_but_ungenerated_release_has_no_artifacts()
        {
            KnowledgeLookupTests.Install(this.layout, "2026-09");
            this.layout.Xmi("2026-09").Create();

            var status = ReleaseStatus.Read(this.layout, "2026-09");

            Assert.Multiple(() =>
            {
                Assert.That(status.Tag, Is.EqualTo("2026-09"));
                Assert.That(status.IsDefault, Is.True);
                Assert.That(status.SourcesFetched, Is.True);
                Assert.That(status.Artifacts, Is.EqualTo(new ReleaseArtifacts(false, false, false, false, false)));
                Assert.That(status.Artifacts.Core, Is.False);
            });
        }

        [Test]
        public void A_generated_release_without_specifications_is_core_complete()
        {
            KnowledgeLookupTests.Install(this.layout, "2026-09", "2026-05");
            this.GenerateCore("2026-05");

            var status = ReleaseStatus.Read(this.layout, "2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(status.IsDefault, Is.False);
                Assert.That(status.SourcesFetched, Is.False);
                Assert.That(status.Artifacts.Core, Is.True);
                Assert.That(status.Artifacts.Specification, Is.False);
            });
        }

        [Test]
        public void The_specification_text_needs_both_documents()
        {
            KnowledgeLookupTests.Install(this.layout, "2026-09");
            Touch(this.layout.SpecificationCatalog("2026-09", "kerml"));

            Assert.That(ReleaseStatus.Read(this.layout, "2026-09").Artifacts.Specification, Is.False);

            Touch(this.layout.SpecificationCatalog("2026-09", "sysml2"));

            Assert.That(ReleaseStatus.Read(this.layout, "2026-09").Artifacts.Specification, Is.True);
        }

        [Test]
        public void An_empty_file_does_not_count_as_generated()
        {
            KnowledgeLookupTests.Install(this.layout, "2026-09");
            var index = this.layout.MetamodelIndex("2026-09");
            index.Directory!.Create();
            File.WriteAllText(index.FullName, string.Empty);

            Assert.That(ReleaseStatus.Read(this.layout, "2026-09").Artifacts.Metamodel, Is.False);
        }

        [Test]
        public void Every_installed_release_is_read_in_manifest_order()
        {
            KnowledgeLookupTests.Install(this.layout, "2026-05", "2026-09");

            Assert.That(ReleaseStatus.ReadAll(this.layout).Select(status => status.Tag), Is.EqualTo(new[] { "2026-09", "2026-05" }));
        }

        /// <summary>
        /// Writes a minimal non-empty file (an empty JSON object), creating its folder first, so the release status
        /// counts it as generated.
        /// </summary>
        /// <param name="file">The file to create or overwrite.</param>
        private static void Touch(FileInfo file)
        {
            file.Directory!.Create();
            File.WriteAllText(file.FullName, "{}");
        }

        /// <summary>
        /// Simulates a generated release without specification text by touching the metamodel index, the
        /// textual-notation index, the model-library index and the cross-references.
        /// </summary>
        /// <param name="tag">The release whose core artifacts are created.</param>
        private void GenerateCore(string tag)
        {
            Touch(this.layout.MetamodelIndex(tag));
            Touch(new FileInfo(Path.Combine(this.layout.TextualNotation(tag).FullName, "index.md")));
            Touch(this.layout.ModelLibraryIndex(tag));
            Touch(this.layout.CrossReferences(tag));
        }
    }
}
