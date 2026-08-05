// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceGenerationTests.cs" company="Starion Group S.A.">
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
    using System.Text;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/cross-references.json</c> for every installed release.
    /// </summary>
    /// <remarks>
    /// This is the one generator that used to need the OMG PDFs. It no longer does: the grammar-stated
    /// clause edges are parsed from the committed <c>.kebnf</c>, and the title-matched ones are
    /// recovered from the committed document when the specification catalog is absent. A contributor
    /// without the PDFs can therefore regenerate this file and get the same bytes back.
    /// </remarks>
    [TestFixture]
    public class CrossReferenceGenerationTests
    {
        /// <summary>The grammar name in the sources, its key in the output, and its document.</summary>
        private static readonly (string File, string Key, string Document)[] Grammars =
        [
            ("KerML", "kerml", "kerml"),
            ("SysML", "sysml", "sysml2"),
        ];

        private IGrammarParser parser = null!;
        private IGrammarLinks links = null!;
        private IKnowledgeReader reader = null!;
        private ICrossReferenceBuilder builder = null!;

        [SetUp]
        public void SetUp()
        {
            this.parser = new GrammarParser();
            this.links = new GrammarLinks();
            this.reader = new KnowledgeReader();
            this.builder = new CrossReferenceBuilder();
        }

        [Test]
        public void Regenerates_the_cross_references()
        {
            var generated = 0;

            foreach (var tag in Repository.Layout?.InstalledTags ?? [])
            {
                var knowledge = Repository.Layout!.Knowledge(tag);
                var metamodelIndex = new FileInfo(
                    Path.Combine(knowledge.FullName, "metamodel", "index.json"));

                if (!metamodelIndex.Exists || !Repository.Layout!.Bnf(tag).Exists)
                {
                    continue;
                }

                var document = this.builder.Build(this.Inputs(tag, knowledge, metamodelIndex));
                var written = CrossReferenceFile.Write(
                    document, Path.Combine(knowledge.FullName, CrossReferenceDocument.FileName));

                var text = File.ReadAllText(written.FullName, Encoding.UTF8);

                Assert.Multiple(() =>
                {
                    // The licensing guarantee: an edge records a clause number, never the OMG wording
                    // it points at.
                    Assert.That(text, Does.Not.Contain("\"title\""));
                    Assert.That(document.Counts.Elements, Is.EqualTo(document.Entries.Count));
                    Assert.That(document.Counts.WithClauses, Is.GreaterThan(0));
                    Assert.That(document.Counts.WithExamples, Is.GreaterThan(0));
                    Assert.That(document.Counts.StatedClauseEdges, Is.GreaterThan(0));
                });

                TestContext.Out.WriteLine(
                    $"{tag}: {document.Counts.Elements} elements, "
                    + $"{document.Counts.ClauseEdges} clause edges ({document.Counts.StatedClauseEdges} stated), "
                    + $"{document.Counts.GrammarEdges} grammar, {document.Counts.FeatureEdges} feature, "
                    + $"{document.Counts.ExampleEdges} example");

                generated++;
            }

            if (generated == 0)
            {
                Assert.Ignore("no release has both a metamodel index and a fetched grammar");
            }
        }

        [Test]
        public void The_same_document_comes_out_with_and_without_the_specification_catalog()
        {
            // The claim that makes this generatable from committed sources alone. Building with the
            // catalog, then rebuilding with no catalog but the previous output's title matches, must
            // give the same bytes - otherwise a contributor without the PDFs would silently drop
            // edges the moment they regenerate.
            var tag = Repository.Layout is null
                ? null
                : Repository.Layout!.InstalledTags.FirstOrDefault(candidate =>
                    SpecificationIndexes(Repository.Layout!.Knowledge(candidate))
                        .All(index => index.Value.Exists));

            if (tag is null)
            {
                Assert.Ignore("no release has an extracted specification catalog");
                return;
            }

            var knowledge = Repository.Layout!.Knowledge(tag);
            var metamodelIndex = new FileInfo(Path.Combine(knowledge.FullName, "metamodel", "index.json"));

            var withCatalog = this.builder.Build(this.Inputs(tag, knowledge, metamodelIndex));

            var withoutCatalog = this.builder.Build(this.Inputs(tag, knowledge, metamodelIndex) with
            {
                ClauseTitles = new Dictionary<string, IReadOnlyDictionary<string, string>>(),
                CarriedClauseEdges = CrossReferenceFile.TitleMatchedEdges(withCatalog),
            });

            Assert.That(
                CrossReferenceFile.Render(withoutCatalog), Is.EqualTo(CrossReferenceFile.Render(withCatalog)));
        }

        private static Dictionary<string, FileInfo> SpecificationIndexes(DirectoryInfo knowledge) =>
            new(StringComparer.Ordinal)
            {
                ["kerml"] = new(Path.Combine(knowledge.FullName, "spec", "kerml", "index.json")),
                ["sysml2"] = new(Path.Combine(knowledge.FullName, "spec", "sysml2", "index.json")),
            };

        private CrossReferenceInputs Inputs(string tag, DirectoryInfo knowledge, FileInfo metamodelIndex)
        {
            var (elements, modelVersionUri) = this.reader.ReadElements(metamodelIndex);
            var names = elements.Select(element => element.Name).ToHashSet(StringComparer.Ordinal);

            var parsed = new Dictionary<string, IReadOnlyList<Production>>(StringComparer.Ordinal);

            foreach (var (file, key, _) in Grammars)
            {
                var path = Path.Combine(
                    Repository.Layout!.Bnf(tag).FullName, $"{file}-textual-bnf.kebnf");

                parsed[key] = this.parser.Parse(File.ReadAllText(path, Encoding.UTF8));
            }

            var documents = new Dictionary<string, SpecificationDocument>(StringComparer.Ordinal);
            var clauseTitles = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
            var carried = CrossReferenceFile.TitleMatchedEdges(
                CrossReferenceFile.ReadIfPresent(
                    Path.Combine(knowledge.FullName, CrossReferenceDocument.FileName)));

            foreach (var (key, index) in SpecificationIndexes(knowledge))
            {
                if (!index.Exists)
                {
                    continue;
                }

                var (titles, document) = this.reader.ReadClauseTitles(index);
                clauseTitles[key] = titles;
                documents[key] = document;
            }

            // Without the catalog the document metadata is not readable either, so it is carried
            // forward alongside the edges - it is two names and two version numbers, already committed.
            if (clauseTitles.Count == 0)
            {
                documents = CarriedDocuments(knowledge);
            }

            return new CrossReferenceInputs
            {
                Elements = elements,
                ModelVersionUri = modelVersionUri,
                Documents = documents,
                ClauseTitles = clauseTitles,
                CarriedClauseEdges = clauseTitles.Count == 0
                    ? carried
                    : new Dictionary<string, IReadOnlyList<ClauseEdge>>(),
                Productions = Project(parsed, value => this.links.MetaclassLinks(value, names)),
                GrammarClauses = Project(parsed, value => this.links.ClauseLinks(value, names)),
                GrammarFeatures = Project(parsed, value => this.links.FeatureLinks(value, names)),
                GrammarDocuments = Grammars.ToDictionary(
                    grammar => grammar.Key, grammar => grammar.Document, StringComparer.Ordinal),
                Examples = this.reader.ReadExamples(
                    new DirectoryInfo(
                        Path.Combine(knowledge.FullName, "textual-notation", "examples")),
                    "textual-notation/examples/"),
            };
        }

        private static Dictionary<string, SpecificationDocument> CarriedDocuments(DirectoryInfo knowledge)
        {
            var previous = CrossReferenceFile.ReadIfPresent(
                Path.Combine(knowledge.FullName, CrossReferenceDocument.FileName));

            return previous is null
                ? new Dictionary<string, SpecificationDocument>(StringComparer.Ordinal)
                : new Dictionary<string, SpecificationDocument>(
                    previous.Documents.ToDictionary(entry => entry.Key, entry => entry.Value),
                    StringComparer.Ordinal);
        }

        private static Dictionary<string, TResult> Project<TResult>(
            Dictionary<string, IReadOnlyList<Production>> parsed,
            Func<IReadOnlyList<Production>, TResult> select) =>
            parsed.ToDictionary(entry => entry.Key, entry => select(entry.Value), StringComparer.Ordinal);
    }
}
