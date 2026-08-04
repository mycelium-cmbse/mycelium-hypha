// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarLinksTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Tests for the grammar-to-metamodel joins, and above all for the guards that keep them honest.
    /// </summary>
    [TestFixture]
    public class GrammarLinksTests
    {
        private GrammarParser parser = null!;
        private GrammarLinks links = null!;

        [SetUp]
        public void SetUp()
        {
            this.parser = new GrammarParser();
            this.links = new GrammarLinks();
        }

        private static HashSet<string> Metaclasses(params string[] names) => [.. names];

        private IReadOnlyList<Production> Parse(string grammar) => this.parser.Parse(grammar);

        [Test]
        public void Metaclass_links_prefer_the_declared_type_over_the_name()
        {
            var result = this.links.MetaclassLinks(
                this.Parse(GrammarParserTests.Grammar), Metaclasses("Package", "TextualRepresentation"));

            Assert.Multiple(() =>
            {
                // PackageDeclaration is typed ": Package", so it links to Package rather than to a
                // metaclass of its own name.
                Assert.That(result["Package"], Is.EqualTo(new[] { "Package", "PackageDeclaration" }));
                Assert.That(result["TextualRepresentation"], Is.EqualTo(new[] { "TextualRepresentation" }));
            });
        }

        [Test]
        public void Metaclass_links_never_invent_an_element()
        {
            var result = this.links.MetaclassLinks(
                this.Parse(GrammarParserTests.Grammar), Metaclasses("Package"));

            Assert.That(
                result.Keys, Is.EqualTo(new[] { "Package" }),
                "only metaclasses the metamodel actually has may appear");
        }

        [Test]
        public void Clause_links_use_the_grammars_own_attribution()
        {
            var result = this.links.ClauseLinks(
                this.Parse(GrammarParserTests.Grammar), Metaclasses("Package", "TextualRepresentation"));

            Assert.Multiple(() =>
            {
                Assert.That(result["TextualRepresentation"], Is.EqualTo(new[] { "8.2.2.4" }));
                Assert.That(result["Package"], Is.EqualTo(new[] { "8.2.2.5.1" }));
            });
        }

        [Test]
        public void Clause_links_are_ordered_numerically()
        {
            // 8.3.10 follows 8.3.2 - ordinal ordering would put it between 8.3.1 and 8.3.2.
            var grammar =
                """
                // Clause 8.3.10 Later

                Package =
                    'package'

                // Clause 8.3.2 Earlier

                PackageBody : Package =
                    '{' '}'
                """;

            var result = this.links.ClauseLinks(this.Parse(grammar), Metaclasses("Package"));

            Assert.That(result["Package"], Is.EqualTo(new[] { "8.3.2", "8.3.10" }));
        }

        [Test]
        public void Feature_links_follow_untyped_helper_productions()
        {
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    PartUsageDeclaration

                PartUsageDeclaration =
                    'part' declaredName = NAME

                OtherUsage : Feature =
                    isReadOnly ?= 'readonly'
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage", "Feature"));

            Assert.Multiple(() =>
            {
                // The assignment lives in the untyped helper, but belongs to the element the caller builds.
                Assert.That(
                    result["PartUsage"].Select(link => (link.Feature, link.Operator)),
                    Is.EqualTo(new[] { ("declaredName", "=") }));
                Assert.That(
                    result["PartUsage"][0].Productions, Is.EqualTo(new[] { "PartUsageDeclaration" }));
            });
        }

        [Test]
        public void Feature_links_follow_helpers_transitively()
        {
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    PartUsageDeclaration

                PartUsageDeclaration =
                    'part' PartUsageName

                PartUsageName =
                    declaredName = NAME
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage"));

            Assert.Multiple(() =>
            {
                // Two hops: PartUsage -> PartUsageDeclaration -> PartUsageName.
                Assert.That(
                    result["PartUsage"].Select(link => link.Feature), Is.EqualTo(new[] { "declaredName" }));
                Assert.That(result["PartUsage"][0].Productions, Is.EqualTo(new[] { "PartUsageName" }));
            });
        }

        [Test]
        public void Feature_links_do_not_follow_a_helper_shared_by_two_elements()
        {
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    SharedDeclaration

                ItemUsage =
                    SharedDeclaration

                SharedDeclaration =
                    declaredName = NAME
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage", "ItemUsage"));

            // The helper is reachable from both, so attributing it to either would be a guess. This is
            // the guard that keeps the real count at 301 assignments rather than a larger, wronger one.
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Feature_links_survive_a_self_referencing_grammar()
        {
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    Nested

                Nested =
                    Nested | declaredName = NAME
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage"));

            Assert.That(
                result["PartUsage"].Select(link => link.Feature), Is.EqualTo(new[] { "declaredName" }));
        }

        [Test]
        public void Feature_links_do_not_follow_a_typed_production()
        {
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    OtherUsage

                OtherUsage : Feature =
                    isReadOnly ?= 'readonly'
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage", "Feature"));

            Assert.Multiple(() =>
            {
                // OtherUsage declares its own metaclass, so its assignment must not be attributed to
                // PartUsage.
                Assert.That(result, Does.Not.ContainKey("PartUsage"));
                Assert.That(result["Feature"][0].Feature, Is.EqualTo("isReadOnly"));
            });
        }

        [Test]
        public void A_reference_inside_a_quoted_literal_is_not_a_reference()
        {
            // Keywords are quoted, and some of them are capitalised. Reading one as a production would
            // make an unrelated helper look shared.
            var grammar =
                """
                // Clause 1 Parts

                PartUsage =
                    'Helper' Helper

                ItemUsage =
                    'Helper'

                Helper =
                    declaredName = NAME
                """;

            var result = this.links.FeatureLinks(this.Parse(grammar), Metaclasses("PartUsage", "ItemUsage"));

            Assert.That(result["PartUsage"][0].Productions, Is.EqualTo(new[] { "Helper" }));
        }
    }
}
