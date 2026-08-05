// ------------------------------------------------------------------------------------------------
// <copyright file="ReservedKeywordTests.cs" company="Starion Group S.A.">
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
    /// Tests for reading the <c>RESERVED_KEYWORD</c> production out of a grammar.
    /// </summary>
    [TestFixture]
    public class ReservedKeywordTests
    {
        private const string Bnf =
            """

            // Clause 8.2.2.1.2 Lexical Structure

            RESERVED_KEYWORD =
                'about' | 'abstract' | 'accept' | 'part' | 'def'
                | 'attribute' | 'use' | 'case'

            DEFINED_BY  = ':'   | 'defined' 'by'
            """;

        private IGrammarParser parser = null!;

        [SetUp]
        public void SetUp() => this.parser = new GrammarParser();

        [Test]
        public void Reads_the_keywords_from_the_grammar()
        {
            Assert.That(this.parser.ReservedKeywords(Bnf), Is.EqualTo(new[]
            {
                "about", "abstract", "accept", "attribute", "case", "def", "part", "use",
            }));
        }

        [Test]
        public void Tolerates_a_grammar_without_the_production() =>
            Assert.That(this.parser.ReservedKeywords("SOMETHING_ELSE = 'x'"), Is.Empty);

        [Test]
        public void Stops_at_the_next_production()
        {
            // 'defined' and 'by' belong to DEFINED_BY, not to the keyword list.
            Assert.That(this.parser.ReservedKeywords(Bnf), Does.Not.Contain("defined"));
        }

        [Test]
        public void Stops_at_a_blank_line()
        {
            var bnf = "RESERVED_KEYWORD =\n    'part'\n\n    'stray'\n";

            Assert.That(this.parser.ReservedKeywords(bnf), Is.EqualTo(new[] { "part" }));
        }

        [Test]
        public void Keywords_are_deduplicated_and_ordered()
        {
            var bnf = "RESERVED_KEYWORD =\n    'part' | 'about' | 'part'\n";

            Assert.That(this.parser.ReservedKeywords(bnf), Is.EqualTo(new[] { "about", "part" }));
        }
    }
}
