// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarReferenceTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Tests for the rendered grammar references.
    /// </summary>
    [TestFixture]
    public class GrammarReferenceTests
    {
        private GrammarParser parser = null!;
        private GrammarReference reference = null!;

        [SetUp]
        public void SetUp()
        {
            this.parser = new GrammarParser();
            this.reference = new GrammarReference();
        }

        [Test]
        public void Groups_by_clause_and_links_the_element()
        {
            var text = this.reference.Render(
                "SysML", "2026-05", this.parser.Parse(GrammarParserTests.Grammar));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("## 8.2.2.5.1 Packages"));
                Assert.That(text, Does.Contain("produces [Package](../metamodel/elements/Package.md)"));
                Assert.That(text, Does.Contain("clause `8.2.2.5.1`"));
                Assert.That(text, Does.Contain("```kebnf"));
                Assert.That(text, Does.Contain("productions: 4"));
            });
        }

        [Test]
        public void Lists_the_features_a_production_populates()
        {
            var text = this.reference.Render(
                "SysML", "2026-05", this.parser.Parse(GrammarParserTests.Grammar));

            Assert.That(text, Does.Contain("features `body =`, `language =`"));
        }

        [Test]
        public void Productions_with_no_clause_land_under_Lexical()
        {
            var productions = this.parser.Parse("Foo =\n    'foo'\n");

            var text = this.reference.Render("KerML", "2026-05", productions);

            Assert.That(Occurrences(text, "## Lexical"), Is.EqualTo(1));
        }

        [Test]
        public void Graphical_images_are_linked_at_the_release_tag()
        {
            var text = this.reference.RenderGraphical(
                "2026-05",
                "Systems-Modeling/SysML-v2-Release",
                this.parser.ParseGraphical(GraphicalGrammarParserTests.Graphical));

            Assert.That(text, Does.Contain(
                "![general-compartment](https://raw.githubusercontent.com/Systems-Modeling/"
                + "SysML-v2-Release/2026-05/bnf/images/general-compartment.svg)"));
        }

        [Test]
        public void Graphical_groups_by_clause_and_counts_images()
        {
            var text = this.reference.RenderGraphical(
                "2026-05", "owner/repo", this.parser.ParseGraphical(GraphicalGrammarParserTests.Graphical));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("## 8.2.3.2 Elements and Relationships Graphical Notation"));
                Assert.That(text, Does.Contain("## 8.2.3.3 Dependencies Graphical Notation"));
                Assert.That(text, Does.Contain("withImages: 2"));
                Assert.That(text, Does.Contain("productions: 5"));
            });
        }

        [Test]
        public void Rendering_is_deterministic()
        {
            var productions = this.parser.Parse(GrammarParserTests.Grammar);

            Assert.That(
                this.reference.Render("SysML", "2026-05", productions),
                Is.EqualTo(this.reference.Render("SysML", "2026-05", productions)));
        }

        [Test]
        public void The_reference_carries_no_carriage_returns()
        {
            // The committed knowledge base is pinned to LF by .gitattributes; emitting CRLF here would
            // show up as a whole-file diff on a Windows machine.
            var text = this.reference.Render(
                "SysML", "2026-05", this.parser.Parse(GrammarParserTests.Grammar));

            Assert.That(text, Does.Not.Contain("\r"));
        }

        private static int Occurrences(string text, string value)
        {
            var count = 0;

            for (var index = text.IndexOf(value, System.StringComparison.Ordinal);
                 index >= 0;
                 index = text.IndexOf(value, index + value.Length, System.StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }
    }
}
