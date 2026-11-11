// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordIndexerTests.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Keywords;

    // The keyword table of #205, on a small grammar shaped like the real ones: lexical productions before
    // RESERVED_KEYWORD, fragments typed by a supertype, a selector that hands over to a subtype, and
    // assigned calls that build other elements.
    [TestFixture]
    public class KeywordIndexerTests
    {
        private const string Grammar =
            """
            // Clause 1 Lexical

            NAME =
                'x' 'y'

            RESERVED_KEYWORD =
                'part' | 'ref' | 'unused'

            RESERVED_SYMBOL =
                ':>>' | ';'

            REDEFINES = ':>>' | 'redefines'

            // Clause 2 Parts

            PartUsage =
                UsagePrefix 'part' Usage ';'

            UsagePrefix : Usage =
                ( isReference ?= 'ref' )?

            Usage =
                Identification Redefinition?

            Redefinition : Feature =
                REDEFINES ownedRelationship += OwnedRedefinition

            OwnedRedefinition : Redefinition =
                redefinedFeature = [QualifiedName] 'hidden'

            Identification : Element =
                ( '<' declaredShortName = NAME '>' )?
                // a 'commented' literal is not syntax

            Selector : Usage =
                PartUsage | 'select'

            Value : Feature =
                value = ( Expression | Literal )

            Expression =
                'expr'

            Literal =
                'lit'
            """;

        private static readonly Dictionary<string, IReadOnlySet<string>> Ancestors = new(StringComparer.Ordinal)
        {
            ["Element"] = Set(),
            ["Feature"] = Set("Element"),
            ["Usage"] = Set("Feature", "Element"),
            ["PartUsage"] = Set("Usage", "Feature", "Element"),
            ["Redefinition"] = Set("Element"),
            ["Expression"] = Set("Feature", "Element"),
        };

        private readonly GrammarParser parser = new();

        private KeywordGrammar table = null!;

        [SetUp]
        public void SetUp() => this.table = this.Build(Grammar);

        [Test]
        public void Literals_are_reserved_contextual_or_operators_and_lexical_and_punctuation_are_left_out()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.table.Entries.Keys,
                    Is.EqualTo(new[] { ":>>", "<", ">", "expr", "hidden", "lit", "part", "redefines", "ref", "select", "unused" }));
                Assert.That(
                    this.table.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Kind),
                    Is.EquivalentTo(new Dictionary<string, string>
                    {
                        [":>>"] = "operator",
                        ["<"] = "operator",
                        [">"] = "operator",
                        ["expr"] = "contextual",
                        ["hidden"] = "contextual",
                        ["lit"] = "contextual",
                        ["part"] = "reserved",
                        ["redefines"] = "contextual",
                        ["ref"] = "reserved",
                        ["select"] = "contextual",
                        ["unused"] = "reserved",
                    }));
                Assert.That(this.table.Counts, Is.EqualTo(new KeywordCounts(Reserved: 3, Contextual: 5, Operators: 3)));
            });
        }

        [Test]
        public void A_keyword_written_by_a_production_building_the_metaclass_is_direct()
        {
            var part = this.table.Entries["part"];

            Assert.Multiple(() =>
            {
                Assert.That(part.Productions, Is.EqualTo(new[] { "PartUsage" }));
                Assert.That(part.Metaclasses, Is.EqualTo(new[] { Metaclass("PartUsage", "direct", ["PartUsage"], ["PartUsage"]) }).Using<KeywordMetaclass>(Same));
            });
        }

        [Test]
        public void A_fragment_typed_by_a_supertype_carries_its_keyword_to_the_caller()
        {
            Assert.That(
                this.table.Entries["ref"].Metaclasses,
                Is.EqualTo(new[]
                {
                    Metaclass("PartUsage", "helper", ["PartUsage"], ["UsagePrefix"]),
                    Metaclass("Usage", "direct", ["UsagePrefix"], ["UsagePrefix"]),
                }).Using<KeywordMetaclass>(Same));
        }

        [Test]
        public void Untyped_helpers_are_followed_transitively()
        {
            Assert.That(
                this.table.Entries[":>>"],
                Is.EqualTo(new KeywordEntry(
                    "operator",
                    ["REDEFINES"],
                    [
                        Metaclass("Feature", "helper", ["Redefinition"], ["REDEFINES"]),
                        Metaclass("PartUsage", "helper", ["PartUsage"], ["REDEFINES"]),
                        Metaclass("Usage", "helper", ["Usage"], ["REDEFINES"]),
                    ])).Using<KeywordEntry>(Same));
        }

        [Test]
        public void A_selector_that_hands_over_to_a_subtype_does_not_take_its_keywords()
        {
            Assert.Multiple(() =>
            {
                // Selector : Usage = PartUsage | 'select': a usage written as 'part' is a part usage.
                Assert.That(this.table.Entries["part"].Metaclasses.Select(metaclass => metaclass.Name), Is.EqualTo(new[] { "PartUsage" }));
                Assert.That(
                    this.table.Entries["select"].Metaclasses,
                    Is.EqualTo(new[] { Metaclass("Usage", "direct", ["Selector"], ["Selector"]) }).Using<KeywordMetaclass>(Same));
            });
        }

        [Test]
        public void An_assigned_call_or_group_and_a_cross_reference_build_another_element()
        {
            Assert.Multiple(() =>
            {
                // ownedRelationship += OwnedRedefinition: 'hidden' belongs to the redefinition only.
                Assert.That(
                    this.table.Entries["hidden"].Metaclasses,
                    Is.EqualTo(new[] { Metaclass("Redefinition", "direct", ["OwnedRedefinition"], ["OwnedRedefinition"]) }).Using<KeywordMetaclass>(Same));

                // value = ( Expression | Literal ): neither is the value's own syntax.
                Assert.That(
                    this.table.Entries["expr"].Metaclasses,
                    Is.EqualTo(new[] { Metaclass("Expression", "direct", ["Expression"], ["Expression"]) }).Using<KeywordMetaclass>(Same));
                Assert.That(this.table.Entries["lit"], Is.EqualTo(new KeywordEntry("contextual", ["Literal"], [])).Using<KeywordEntry>(Same));
            });
        }

        [Test]
        public void An_operator_in_an_optional_group_is_reached_and_a_commented_literal_is_not()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.table.Entries["<"].Metaclasses.Select(metaclass => $"{metaclass.Name} {metaclass.Via}"),
                    Is.EqualTo(new[] { "Element direct", "PartUsage helper", "Usage helper" }));
                Assert.That(this.table.Entries.Keys, Has.None.EqualTo("commented"));
            });
        }

        [Test]
        public void A_call_after_an_assigned_keyword_or_cross_reference_is_bare()
        {
            // As in the 2026-05 MergeNode ("isComposite ?= 'merge' UsageDeclaration") and FeatureTyping
            // ("typedFeature = [QualifiedName] TYPED_BY ...").
            var table = this.Build(
                """
                Node : Usage =
                    isComposite ?= 'merge' Helper

                Typing : Feature =
                    type = [QualifiedName] Helper

                Helper =
                    ':>>'
                """);

            Assert.That(
                table.Entries[":>>"].Metaclasses,
                Is.EqualTo(new[]
                {
                    Metaclass("Feature", "helper", ["Typing"], ["Helper"]),
                    Metaclass("Usage", "helper", ["Node"], ["Helper"]),
                }).Using<KeywordMetaclass>(Same));
        }

        [Test]
        public void A_cross_reference_is_not_a_call_into_the_production_it_names()
        {
            var table = this.Build(
                """
                Redefinition =
                    redefinedFeature = [QualifiedName]

                QualifiedName =
                    NAME ( '::' NAME )*
                """);

            Assert.That(table.Entries["::"], Is.EqualTo(new KeywordEntry("operator", ["QualifiedName"], [])).Using<KeywordEntry>(Same));
        }

        [Test]
        public void A_call_into_a_production_building_the_same_metaclass_is_followed_and_direct_wins()
        {
            // PartBody is visited first and writes 'body' directly; PartUsage then reaches it as a helper.
            var table = this.Build(
                """
                PartBody : PartUsage =
                    'body'

                PartUsage =
                    'part' PartBody
                """);

            Assert.That(
                table.Entries["body"].Metaclasses,
                Is.EqualTo(new[] { Metaclass("PartUsage", "direct", ["PartBody", "PartUsage"], ["PartBody"]) }).Using<KeywordMetaclass>(Same));
        }

        [Test]
        public void An_assigned_group_ends_at_its_own_closing_parenthesis()
        {
            var table = this.Build(
                """
                Feature =
                    value = ( ( Expression ) | Literal ) Tail

                Expression =
                    'expr'

                Literal =
                    'lit'

                Tail =
                    'tail'
                """);

            Assert.Multiple(() =>
            {
                Assert.That(
                    table.Entries["tail"].Metaclasses,
                    Is.EqualTo(new[] { Metaclass("Feature", "helper", ["Feature"], ["Tail"]) }).Using<KeywordMetaclass>(Same));
                Assert.That(table.Entries["lit"].Metaclasses, Is.Empty);
            });
        }

        [Test]
        public void A_reserved_keyword_no_production_writes_is_listed_with_nothing()
        {
            Assert.That(this.table.Entries["unused"], Is.EqualTo(new KeywordEntry("reserved", [], [])).Using<KeywordEntry>(Same));
        }

        [Test]
        public void A_recursive_grammar_terminates()
        {
            var table = this.Build(
                """
                Feature =
                    Helper 'a'

                Helper =
                    Feature 'b' Helper
                """);

            Assert.Multiple(() =>
            {
                Assert.That(table.Entries["a"].Metaclasses, Is.EqualTo(new[] { Metaclass("Feature", "direct", ["Feature"], ["Feature"]) }).Using<KeywordMetaclass>(Same));
                Assert.That(table.Entries["b"].Metaclasses, Is.EqualTo(new[] { Metaclass("Feature", "helper", ["Feature"], ["Helper"]) }).Using<KeywordMetaclass>(Same));
            });
        }

        [Test]
        public void Without_a_reserved_keyword_production_nothing_is_lexical_and_a_later_production_replaces_an_earlier_one()
        {
            var table = this.Build(
                """
                Feature =
                    'old'

                Feature =
                    'new'
                """);

            Assert.That(
                table.Entries.Select(entry => $"{entry.Key} {entry.Value.Kind} {string.Join(",", entry.Value.Productions)} {entry.Value.Metaclasses.Count}"),
                Is.EqualTo(new[] { "new contextual Feature 1" }));
        }

        [Test]
        public void The_indexer_needs_every_argument()
        {
            var indexer = new KeywordIndexer();
            var productions = this.parser.Parse(Grammar);

            Assert.Multiple(() =>
            {
                Assert.That(() => indexer.Build(null!, [], Ancestors), Throws.ArgumentNullException);
                Assert.That(() => indexer.Build(productions, null!, Ancestors), Throws.ArgumentNullException);
                Assert.That(() => indexer.Build(productions, [], null!), Throws.ArgumentNullException);
            });
        }

        private static IReadOnlySet<string> Set(params string[] names) => names.ToHashSet(StringComparer.Ordinal);

        private static KeywordMetaclass Metaclass(string name, string via, string[] productions, string[] through) =>
            new(name, via, productions, through);

        // The records hold lists, which compare by reference, so entries are compared field by field.
        private static bool Same(KeywordEntry expected, KeywordEntry actual) =>
            expected.Kind == actual.Kind
            && expected.Productions.SequenceEqual(actual.Productions)
            && expected.Metaclasses.Count == actual.Metaclasses.Count
            && expected.Metaclasses.Zip(actual.Metaclasses).All(pair => Same(pair.First, pair.Second));

        private static bool Same(KeywordMetaclass expected, KeywordMetaclass actual) =>
            expected.Name == actual.Name
            && expected.Via == actual.Via
            && expected.Productions.SequenceEqual(actual.Productions)
            && expected.Through.SequenceEqual(actual.Through);

        private KeywordGrammar Build(string grammar) =>
            new KeywordIndexer().Build(this.parser.Parse(grammar), this.parser.ReservedKeywords(grammar), Ancestors);
    }
}
