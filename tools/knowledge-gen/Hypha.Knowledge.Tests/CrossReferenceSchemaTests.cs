// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceSchemaTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.CrossReferences;

    /// <summary>
    /// Keeps the hand-written <c>cross-references.schema.json</c> honest without pulling in a
    /// JSON-Schema validator.
    /// </summary>
    /// <remarks>
    /// Checks the shape the builder actually produces against the <c>required</c> lists the schema
    /// declares, so adding a field to one and not the other fails here.
    /// </remarks>
    [TestFixture]
    public class CrossReferenceSchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            var schemaFile = Repository.Layout!.CrossReferenceSchema;

            this.schema = JsonDocument.Parse(File.ReadAllText(schemaFile.FullName, Encoding.UTF8));

            var document = new CrossReferenceBuilder().Build(
                CrossReferenceBuilderTests.Inputs(
                    productions: CrossReferenceBuilderTests.PerGrammar("sysml", "PartUsage", "PartUsage"),
                    examples: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["textual-notation/examples/part-definitions.md"] = ["PartUsage"],
                    }));

            this.produced = JsonDocument.Parse(CrossReferenceFile.Render(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        private static IEnumerable<string> Keys(JsonElement element) =>
            element.EnumerateObject().Select(property => property.Name).Order(System.StringComparer.Ordinal);

        private static IEnumerable<string> Required(JsonElement element) =>
            element.GetProperty("required").EnumerateArray()
                .Select(item => item.GetString()!)
                .Order(System.StringComparer.Ordinal);

        private JsonElement Definition(string name) =>
            this.schema.RootElement.GetProperty("$defs").GetProperty(name);

        private JsonElement Entry => this.produced.RootElement.GetProperty("entries").GetProperty("PartUsage");

        [Test]
        public void The_document_has_exactly_the_required_top_level_keys() =>
            Assert.That(Keys(this.produced.RootElement), Is.EqualTo(Required(this.schema.RootElement)));

        [Test]
        public void The_counts_block_matches_the_schema() =>
            Assert.That(
                Keys(this.produced.RootElement.GetProperty("counts")),
                Is.EqualTo(Required(this.schema.RootElement.GetProperty("properties").GetProperty("counts"))));

        [Test]
        public void An_entry_matches_the_schema() =>
            Assert.That(Keys(this.Entry), Is.EqualTo(Required(this.Definition("entry"))));

        [Test]
        public void Every_edge_shape_matches_the_schema()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    Keys(this.Entry.GetProperty("clauses")[0]), Is.EqualTo(Required(this.Definition("clauseEdge"))));
                Assert.That(
                    Keys(this.Entry.GetProperty("grammar")[0]), Is.EqualTo(Required(this.Definition("grammarEdge"))));
                Assert.That(
                    Keys(this.Entry.GetProperty("examples")[0]), Is.EqualTo(Required(this.Definition("exampleEdge"))));
            });
        }

        [Test]
        public void The_provenance_tiers_are_exactly_the_ones_the_schema_enumerates()
        {
            var declared = this.Definition("provenance").GetProperty("enum")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .Order(System.StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(Keys(this.produced.RootElement.GetProperty("provenanceTiers")), Is.EqualTo(declared));
                Assert.That(
                    this.Entry.GetProperty("clauses")[0].GetProperty("provenance").GetString(),
                    Is.AnyOf(Provenance.Normative, Provenance.Model, Provenance.Derived));
            });
        }

        [Test]
        public void The_feature_edge_shape_matches_the_schema()
        {
            // The synthetic entry above has no feature assignments, so build one that does rather than
            // leaving this shape unchecked.
            var document = new CrossReferenceBuilder().Build(CrossReferenceBuilderTests.Inputs() with
            {
                GrammarFeatures = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<Grammar.FeatureLink>>>
                {
                    ["kerml"] = new Dictionary<string, IReadOnlyList<Grammar.FeatureLink>>
                    {
                        ["Feature"] = [new Grammar.FeatureLink("declaredName", "=", ["FeatureDeclaration"])],
                    },
                },
            });

            using var rendered = JsonDocument.Parse(CrossReferenceFile.Render(document));

            var edge = rendered.RootElement
                .GetProperty("entries").GetProperty("Feature").GetProperty("features")[0];

            Assert.That(Keys(edge), Is.EqualTo(Required(this.Definition("featureEdge"))));
        }
    }
}
