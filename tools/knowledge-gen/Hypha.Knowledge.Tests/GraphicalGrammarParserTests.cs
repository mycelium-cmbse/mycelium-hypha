// ------------------------------------------------------------------------------------------------
// <copyright file="GraphicalGrammarParserTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Linq;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Tests for the graphical (<c>.kgbnf</c>) parser, on text in the shape upstream really uses.
    /// </summary>
    [TestFixture]
    public class GraphicalGrammarParserTests
    {
        internal const string Graphical =
            """

            // Part 2 - Systems Modeling Language (SysML)

            // Clause 8.2.3.2 Elements and Relationships Graphical Notation

            element =
                 dependencies-and-annotations-element
               | general-element

            compartment =| general-compartment

            general-compartment =
                  <img src="images/general-compartment.svg" width="408.0">
                  general-view

            ellipsis-at-lower-left-corner = '...'

            // Note. An element inside a textual compartment is selected by graying out a substring.

            // Clause 8.2.3.3 Dependencies Graphical Notation

            binary-dependency =
                  <img src="images/binary-dependency.svg" width="200.0">
            """;

        private IGrammarParser parser = null!;

        [SetUp]
        public void SetUp() => this.parser = new GrammarParser();

        [Test]
        public void Parses_kebab_case_production_names()
        {
            var names = this.parser.ParseGraphical(Graphical).Select(production => production.Name);

            Assert.That(names, Is.EqualTo(new[]
            {
                "element",
                "compartment",
                "general-compartment",
                "ellipsis-at-lower-left-corner",
                "binary-dependency",
            }));
        }

        [Test]
        public void Tolerates_the_leading_alternation_bar()
        {
            // "compartment =| general-compartment" occurs upstream and must not be dropped.
            var byName = GrammarParserTests.ByName(this.parser.ParseGraphical(Graphical));

            Assert.Multiple(() =>
            {
                Assert.That(byName, Does.ContainKey("compartment"));
                Assert.That(byName["compartment"].Body, Does.Contain("general-compartment"));
            });
        }

        [Test]
        public void Captures_image_references()
        {
            var byName = GrammarParserTests.ByName(this.parser.ParseGraphical(Graphical));

            Assert.Multiple(() =>
            {
                Assert.That(
                    byName["general-compartment"].Images,
                    Is.EqualTo(new[] { "images/general-compartment.svg" }));
                Assert.That(byName["element"].Images, Is.Empty);
            });
        }

        [Test]
        public void Attributes_productions_to_their_clause()
        {
            var byName = GrammarParserTests.ByName(this.parser.ParseGraphical(Graphical));

            Assert.Multiple(() =>
            {
                Assert.That(byName["element"].Clause, Is.EqualTo("8.2.3.2"));
                Assert.That(byName["binary-dependency"].Clause, Is.EqualTo("8.2.3.3"));
            });
        }

        [Test]
        public void Note_comments_do_not_start_a_production_or_leak_into_bodies()
        {
            var byName = GrammarParserTests.ByName(this.parser.ParseGraphical(Graphical));

            Assert.Multiple(() =>
            {
                Assert.That(byName["ellipsis-at-lower-left-corner"].Body, Does.Not.Contain("Note."));
                Assert.That(byName.Keys.Where(name => name.StartsWith("note")), Is.Empty);
            });
        }

        [Test]
        public void Graphical_productions_declare_no_metaclass()
        {
            // The graphical grammar has no ": Metaclass" form; nothing should claim otherwise.
            Assert.That(
                this.parser.ParseGraphical(Graphical).All(production => production.Produces is null),
                Is.True);
        }

        [Test]
        public void Repeated_images_are_listed_once_and_ordered()
        {
            var productions = this.parser.ParseGraphical(
                """
                // Clause 1 Notation

                view =
                      <img src="images/b.svg">
                      <img src="images/a.svg">
                      <img src="images/b.svg">
                """);

            Assert.That(productions[0].Images, Is.EqualTo(new[] { "images/a.svg", "images/b.svg" }));
        }
    }
}
