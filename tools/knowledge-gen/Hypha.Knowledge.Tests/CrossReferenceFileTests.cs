// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceFileTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.CrossReferences;

    [TestFixture]
    public class CrossReferenceFileTests
    {
        private DirectoryInfo scratch = null!;

        [SetUp]
        public void SetUp() =>
            this.scratch = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-xref-file-{Guid.NewGuid():N}"));

        [TearDown]
        public void TearDown() => this.scratch.Delete(recursive: true);

        [Test]
        public void A_written_document_reads_back_unchanged_with_its_title_matched_edges()
        {
            // The generator reads the previous file to carry the title matches forward when the PDFs
            // are absent; a read that binds nothing would drop them silently.
            var original = new CrossReferenceBuilder().Build(
                CrossReferenceBuilderTests.Inputs(
                    productions: CrossReferenceBuilderTests.PerGrammar("sysml", "PartUsage", "PartUsage"),
                    examples: new Dictionary<string, IReadOnlyList<string>>(
                        StringComparer.Ordinal)
                    {
                        ["PartUsage"] = ["examples/Vehicle.md"],
                    }) with
                {
                    GrammarClauses = CrossReferenceBuilderTests.PerGrammar("kerml", "Feature", "8.3.6.10"),
                    GrammarDocuments = new Dictionary<string, string>(StringComparer.Ordinal) { ["kerml"] = "kerml" },
                });

            var path = Path.Combine(this.scratch.FullName, "cross-references.json");
            CrossReferenceFile.Write(original, path);

            var read = CrossReferenceFile.ReadIfPresent(path);

            var edges = original.Entries.Values.SelectMany(entry => entry.Clauses).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(edges.Select(edge => edge.Method), Does.Contain(ClauseEdge.ExactTitleMatch));
                Assert.That(edges.Select(edge => edge.Method), Does.Contain(ClauseEdge.GrammarClause));

                Assert.That(read, Is.Not.Null);
                Assert.That(CrossReferenceFile.Render(read!), Is.EqualTo(CrossReferenceFile.Render(original)));
                Assert.That(
                    CrossReferenceFile.TitleMatchedEdges(read),
                    Is.EqualTo(CrossReferenceFile.TitleMatchedEdges(original)).And.Not.Empty);
            });
        }

        [Test]
        public void A_missing_file_reads_as_null()
        {
            var missing = Path.Combine(this.scratch.FullName, "cross-references.json");

            Assert.That(CrossReferenceFile.ReadIfPresent(missing), Is.Null);
        }
    }
}
