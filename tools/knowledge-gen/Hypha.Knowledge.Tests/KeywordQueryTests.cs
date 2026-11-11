// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordQueryTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;

    // The keyword query of #205, over the fixture's keywords.json (see QueryFixture.Keywords).
    [TestFixture]
    public class KeywordQueryTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void A_keyword_of_one_grammar_names_its_productions_and_metaclasses()
        {
            var result = this.fixture.Query.Keyword("part");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom}"), Is.EqualTo(new[] { "sysml reserved sysml" }));
                Assert.That(result.Value![0].Productions, Is.EqualTo(new[] { "PartUsage" }));
                Assert.That(
                    result.Value![0].Metaclasses.Select(metaclass => $"{metaclass.Name} {metaclass.Via}"),
                    Is.EqualTo(new[] { "PartUsage direct" }));
                Assert.That(result.Grounding.Tier, Is.EqualTo(Provenance.Derived));
            });
        }

        [Test]
        public void A_keyword_written_through_a_helper_names_the_metaclass_and_the_helper()
        {
            var metaclasses = this.fixture.Query.Keyword("ref", grammar: "sysml").Value!.Single().Metaclasses;

            Assert.That(
                metaclasses.Select(metaclass => $"{metaclass.Name} {metaclass.Via} {string.Join(",", metaclass.Productions)} {string.Join(",", metaclass.Through)}"),
                Is.EqualTo(new[] { "PartUsage helper PartUsage UsagePrefix", "Usage direct UsagePrefix UsagePrefix" }));
        }

        [Test]
        public void A_keyword_in_both_grammars_has_one_use_per_grammar_unless_one_is_asked_for()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Keyword(":>>").Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom} {string.Join(",", use.Productions)}"),
                    Is.EqualTo(new[] { "kerml operator kerml FeatureDeclaration", "sysml operator sysml PartUsage" }));
                Assert.That(
                    this.fixture.Query.Keyword(":>>", grammar: "kerml").Value!.Select(use => use.Grammar),
                    Is.EqualTo(new[] { "kerml" }));
            });
        }

        [Test]
        public void A_keyword_only_KerML_expressions_write_names_their_syntax_in_SysML()
        {
            var result = this.fixture.Query.Keyword("and", grammar: "sysml");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom}"), Is.EqualTo(new[] { "sysml reserved kerml" }));
                Assert.That(result.Value![0].Productions, Is.EqualTo(new[] { "OperatorExpression" }));
                Assert.That(
                    result.Value![0].Metaclasses.Select(metaclass => $"{metaclass.Name} {metaclass.Via}"),
                    Is.EqualTo(new[] { "Expression direct" }));
                Assert.That(
                    this.fixture.Query.Keyword("+").Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom}"),
                    Is.EqualTo(new[] { "kerml operator kerml", "sysml operator kerml" }));
            });
        }

        [Test]
        public void A_keyword_SysML_reserves_keeps_its_SysML_kind_when_its_syntax_comes_from_KerML()
        {
            Assert.That(
                this.fixture.Query.Keyword("new").Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom}"),
                Is.EqualTo(new[] { "kerml contextual kerml", "sysml reserved kerml" }));
        }

        [Test]
        public void Only_the_KerML_productions_SysML_reaches_are_taken()
        {
            // KerML's Invariant writes 'true' too, but SysML text never holds an invariant.
            Assert.That(
                this.fixture.Query.Keyword("true").Value!.Select(use => $"{use.Grammar} {string.Join(",", use.Productions)} {string.Join(",", use.Metaclasses.Select(metaclass => metaclass.Name))}"),
                Is.EqualTo(new[] { "kerml Invariant,OperatorExpression Expression,Invariant", "sysml OperatorExpression Expression" }));
        }

        [Test]
        public void A_keyword_a_grammar_reserves_but_writes_nowhere_is_found_with_no_syntax()
        {
            var result = this.fixture.Query.Keyword("variation");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.Select(use => $"{use.Grammar} {use.Kind} {use.SyntaxFrom}"), Is.EqualTo(new[] { "sysml reserved sysml" }));
                Assert.That(result.Value![0].Productions, Is.Empty);
                Assert.That(result.Value![0].Metaclasses, Is.Empty);
            });
        }

        [Test]
        public void A_KerML_declaration_keyword_is_not_SysML_syntax()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Keyword("feature", grammar: "sysml").Found, Is.False);
                Assert.That(this.fixture.Query.Keyword("feature").Value!.Select(use => use.Grammar), Is.EqualTo(new[] { "kerml" }));
            });
        }

        [Test]
        public void An_unknown_word_or_grammar_is_not_found_and_both_are_matched_exactly()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Keyword("nosuchword").Found, Is.False);
                Assert.That(this.fixture.Query.Keyword("part", grammar: "graphical").Found, Is.False);
                Assert.That(this.fixture.Query.Keyword("part", grammar: "SysML").Found, Is.False);
                Assert.That(this.fixture.Query.Keyword("Part").Found, Is.False);
                Assert.That(this.fixture.Query.Keyword("part").Grounding.Source, Is.EqualTo($"knowledge/{QueryFixture.Tag}/textual-notation/{KeywordDocument.FileName}"));
            });
        }

        [Test]
        public void A_blank_word_is_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.fixture.Query.Keyword(null!), Throws.InstanceOf<ArgumentException>());
                Assert.That(() => this.fixture.Query.Keyword(" "), Throws.InstanceOf<ArgumentException>());
            });
        }

        [Test]
        public void A_missing_or_invalid_keywords_file_is_an_error_naming_the_artifact()
        {
            var file = this.fixture.Layout.Keywords(QueryFixture.Tag);

            File.WriteAllText(file.FullName, "{}");
            Assert.That(() => QueryFixture.NewQuery(this.fixture.Layout, this.fixture.ApiLayout).Keyword("part"), Throws.TypeOf<KnowledgeLookupException>());

            file.Delete();
            Assert.That(
                () => QueryFixture.NewQuery(this.fixture.Layout, this.fixture.ApiLayout).Keyword("part"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("keywords"));
        }
    }
}
