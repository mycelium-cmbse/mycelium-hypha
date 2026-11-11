// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordSchemaTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json;

    using Hypha.Knowledge.Keywords;

    // knowledge/keywords.schema.json is hand-written; this keeps it and the generated document in step.
    [TestFixture]
    public class KeywordSchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            this.schema = JsonDocument.Parse(File.ReadAllText(Repository.Layout!.KeywordsSchema.FullName, Encoding.UTF8));

            var entries = new SortedDictionary<string, KeywordEntry>(StringComparer.Ordinal)
            {
                ["ref"] = new(KeywordEntry.Reserved, "sysml", ["BasicUsagePrefix"], [new KeywordMetaclass("PartUsage", KeywordMetaclass.Helper, ["PartUsage"], ["BasicUsagePrefix"])]),
            };
            var document = new KeywordDocument(
                KeywordDocument.CurrentSchemaVersion,
                "2026-05",
                new SortedDictionary<string, KeywordGrammar>(StringComparer.Ordinal) { ["sysml"] = new(new KeywordCounts(1, 0, 0), entries) });

            this.produced = JsonDocument.Parse(KeywordFile.Render(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        [Test]
        public void The_document_has_exactly_the_required_top_level_keys() =>
            Assert.That(Keys(this.produced.RootElement), Is.EqualTo(Required(this.schema.RootElement)));

        [Test]
        public void A_grammar_and_its_counts_match_the_schema()
        {
            var grammar = this.produced.RootElement.GetProperty("grammars").GetProperty("sysml");
            var definition = Definition(this.schema, "grammar");

            Assert.Multiple(() =>
            {
                Assert.That(Keys(grammar), Is.EqualTo(Required(definition)));
                Assert.That(
                    Keys(grammar.GetProperty("counts")),
                    Is.EqualTo(Required(definition.GetProperty("properties").GetProperty("counts"))));
            });
        }

        [Test]
        public void An_entry_and_its_metaclass_match_the_schema()
        {
            var entry = this.produced.RootElement.GetProperty("grammars").GetProperty("sysml").GetProperty("entries").GetProperty("ref");

            Assert.Multiple(() =>
            {
                Assert.That(Keys(entry), Is.EqualTo(Required(Definition(this.schema, "entry"))));
                Assert.That(Keys(entry.GetProperty("metaclasses")[0]), Is.EqualTo(Required(Definition(this.schema, "metaclass"))));
            });
        }

        [Test]
        public void The_kinds_and_the_ways_a_metaclass_is_reached_are_the_ones_the_schema_allows()
        {
            static IEnumerable<string> Allowed(JsonElement definition, string property) =>
                definition.GetProperty("properties").GetProperty(property).GetProperty("enum").EnumerateArray().Select(item => item.GetString()!);

            Assert.Multiple(() =>
            {
                Assert.That(
                    Allowed(Definition(this.schema, "entry"), "kind"),
                    Is.EquivalentTo(new[] { KeywordEntry.Reserved, KeywordEntry.Contextual, KeywordEntry.Operator }));
                Assert.That(
                    Allowed(Definition(this.schema, "metaclass"), "via"),
                    Is.EquivalentTo(new[] { KeywordMetaclass.Direct, KeywordMetaclass.Helper }));
            });
        }

        private static JsonElement Definition(JsonDocument schema, string name) =>
            schema.RootElement.GetProperty("$defs").GetProperty(name);

        private static IEnumerable<string> Keys(JsonElement element) =>
            element.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal);

        private static IEnumerable<string> Required(JsonElement element) =>
            element.GetProperty("required").EnumerateArray().Select(item => item.GetString()!).Order(StringComparer.Ordinal);
    }
}
