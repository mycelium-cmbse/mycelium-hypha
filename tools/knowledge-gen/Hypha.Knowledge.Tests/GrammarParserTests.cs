// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarParserTests.cs" company="Starion Group S.A.">
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
    /// Tests for the textual BNF parser, on grammar text in the shape the <c>.kebnf</c> files use.
    /// </summary>
    [TestFixture]
    public class GrammarParserTests
    {
        /// <summary>A slice of the real grammar: a clause comment, a typed production, and each operator.</summary>
        internal const string Grammar =
            """

            // Clause 8.2.2.4 Annotations

            TextualRepresentation =
                ( 'rep' Identification )?
                'language' language = STRING_VALUE body = REGULAR_COMMENT

            // Clause 8.2.2.5.1 Packages

            Package =
                ( ownedRelationship += PrefixMetadataMember )*
                PackageDeclaration PackageBody

            PackageDeclaration : Package =
                'package' Identification

            LibraryPackage =
                ( isStandard ?= 'standard' ) 'library'
                PackageDeclaration
            """;

        private IGrammarParser parser = null!;

        [SetUp]
        public void SetUp() => this.parser = new GrammarParser();

        internal static Dictionary<string, Production> ByName(IEnumerable<Production> productions) =>
            productions.ToDictionary(production => production.Name);

        [Test]
        public void Parses_every_production()
        {
            var names = this.parser.Parse(Grammar).Select(production => production.Name);

            Assert.That(names, Is.EqualTo(new[]
            {
                "TextualRepresentation", "Package", "PackageDeclaration", "LibraryPackage",
            }));
        }

        [Test]
        public void Captures_the_declared_metaclass()
        {
            var byName = ByName(this.parser.Parse(Grammar));

            Assert.Multiple(() =>
            {
                Assert.That(byName["PackageDeclaration"].Produces, Is.EqualTo("Package"));
                Assert.That(
                    byName["Package"].Produces, Is.Null, "an untyped production declares no metaclass");
            });
        }

        [Test]
        public void Attributes_each_production_to_its_clause()
        {
            var byName = ByName(this.parser.Parse(Grammar));

            Assert.Multiple(() =>
            {
                Assert.That(byName["TextualRepresentation"].Clause, Is.EqualTo("8.2.2.4"));
                Assert.That(byName["TextualRepresentation"].ClauseTitle, Is.EqualTo("Annotations"));
                Assert.That(byName["PackageDeclaration"].Clause, Is.EqualTo("8.2.2.5.1"));
            });
        }

        [Test]
        public void Extracts_the_features_a_production_populates()
        {
            var byName = ByName(this.parser.Parse(Grammar));

            Assert.Multiple(() =>
            {
                Assert.That(
                    byName["TextualRepresentation"].Features,
                    Is.EqualTo(new[]
                    {
                        new FeatureAssignment("body", "="), new FeatureAssignment("language", "="),
                    }));
                Assert.That(
                    byName["Package"].Features,
                    Is.EqualTo(new[] { new FeatureAssignment("ownedRelationship", "+=") }));
                Assert.That(
                    byName["LibraryPackage"].Features,
                    Is.EqualTo(new[] { new FeatureAssignment("isStandard", "?=") }),
                    "?= is a boolean flag");
            });
        }

        [Test]
        public void The_production_header_is_not_read_as_an_assignment()
        {
            // "PackageDeclaration : Package =" ends in "=", so scanning from column 0 would report the
            // production's own name as a feature it populates.
            var byName = ByName(this.parser.Parse(Grammar));

            Assert.That(byName["PackageDeclaration"].Features, Is.Empty);
        }

        [Test]
        public void Body_keeps_the_production_verbatim()
        {
            var byName = ByName(this.parser.Parse(Grammar));

            Assert.That(
                byName["PackageDeclaration"].Body,
                Is.EqualTo("PackageDeclaration : Package =\n    'package' Identification"));
        }

        [Test]
        public void Tolerates_a_grammar_with_no_clause_comments()
        {
            var productions = this.parser.Parse("Foo =\n    'foo'\n");

            Assert.Multiple(() =>
            {
                Assert.That(productions[0].Clause, Is.Null);
                Assert.That(productions[0].Features, Is.Empty);
            });
        }

        [Test]
        public void Carriage_returns_never_reach_the_body()
        {
            // The sources are LF upstream, but a Windows checkout with the wrong attributes is not
            // exotic - and a stray \r would land inside the fenced code block of the reference.
            var productions = this.parser.Parse("// Clause 1 Parts\r\n\r\nFoo =\r\n    'foo'\r\n");

            Assert.Multiple(() =>
            {
                Assert.That(productions[0].Body, Does.Not.Contain("\r"));
                Assert.That(productions[0].Clause, Is.EqualTo("1"));
                Assert.That(productions[0].ClauseTitle, Is.EqualTo("Parts"));
            });
        }

        [Test]
        public void A_clause_number_with_no_title_leaves_the_title_empty()
        {
            var productions = this.parser.Parse("// Clause 8.2\nFoo =\n    'foo'\n");

            Assert.Multiple(() =>
            {
                Assert.That(productions[0].Clause, Is.EqualTo("8.2"));
                Assert.That(productions[0].ClauseTitle, Is.Empty);
            });
        }

        [Test]
        public void An_equality_test_is_not_an_assignment()
        {
            // The negative lookahead exists so "==" is never read as an assignment operator.
            var productions = this.parser.Parse("Foo =\n    'foo' ( bar == baz )\n");

            Assert.That(productions[0].Features, Is.Empty);
        }
    }
}
