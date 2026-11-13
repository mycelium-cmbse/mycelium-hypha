// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationSearchTests.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.Query;

    [TestFixture]
    public class SpecificationSearchTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void A_phrase_is_found_whatever_its_case_and_wherever_its_lines_break()
        {
            this.Page("kerml", "07.04.02-feature.md", "A Feature\nis typed BY\n  a Type.");

            var result = this.fixture.Query.SearchSpecification("is typed by");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value!.Select(match => match.Address), Is.EqualTo(new[] { new HyphaAddress(QueryFixture.Tag, "kerml", "7.4.2") }));
                Assert.That(result.Value![0].Title, Is.EqualTo("Feature"));
                Assert.That(result.Value![0].Occurrences, Is.EqualTo(1));
                Assert.That(result.Value![0].Excerpt, Is.EqualTo("A Feature is typed BY a Type."));
                Assert.That(result.Grounding.Tier, Is.EqualTo(Provenance.Derived));
                Assert.That(result.Grounding.Release, Is.EqualTo(QueryFixture.Tag));
            });
        }

        [Test]
        public void The_clauses_that_say_it_most_come_first_then_by_specification_and_clause_number()
        {
            this.Page("kerml", "02-conformance.md", "A redefinition here.");
            this.Page("kerml", "07.04.02-feature.md", "Redefinition, and redefinition again.");
            this.Page("kerml", "08.03.03.01.08-feature.md", "A redefinition there.");
            this.Page("sysml2", "08.03.06.03-partusage.md", "Redefinition of a part.");

            var result = this.fixture.Query.SearchSpecification("redefinition");

            Assert.That(
                result.Value!.Select(match => $"{match.Address.Kind} {match.Address.Name} {match.Occurrences}"),
                Is.EqualTo(new[] { "kerml 7.4.2 2", "kerml 2 1", "kerml 8.3.3.1.8 1", "sysml2 8.3.6.3 1" }));
        }

        [Test]
        public void One_specification_and_a_limit_narrow_the_search()
        {
            this.Page("kerml", "02-conformance.md", "conformance");
            this.Page("kerml", "07.04.02-feature.md", "conformance conformance");
            this.Page("sysml2", "01-scope.md", "conformance");

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.SearchSpecification("conformance", document: "sysml2").Value!.Select(match => match.Address.Name),
                    Is.EqualTo(new[] { "1" }));
                Assert.That(
                    this.fixture.Query.SearchSpecification("conformance", limit: 1).Value!.Select(match => match.Address.Name),
                    Is.EqualTo(new[] { "7.4.2" }));
            });
        }

        [Test]
        public void The_excerpt_is_the_text_around_the_first_occurrence_in_whole_words()
        {
            var before = string.Join(' ', Enumerable.Repeat("before", 60));
            var after = string.Join(' ', Enumerable.Repeat("after", 60));
            this.Page("kerml", "07.04.02-feature.md", $"{before} the needle {after} needle");

            var match = this.fixture.Query.SearchSpecification("needle", document: "kerml").Value!.Single();

            Assert.Multiple(() =>
            {
                Assert.That(match.Occurrences, Is.EqualTo(2));
                Assert.That(match.Excerpt, Does.StartWith("...before ").And.EndWith(" after..."));
                Assert.That(match.Excerpt, Does.Contain("the needle after"));
                Assert.That(match.Excerpt.Length, Is.LessThan(300));
            });
        }

        [Test]
        public void Text_no_clause_contains_finds_nothing()
        {
            var result = this.fixture.Query.SearchSpecification("no clause says this");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Is.Empty);
                Assert.That(result.Available, Is.True);
            });
        }

        [Test]
        public void Without_any_specification_text_the_answer_says_how_to_generate_it()
        {
            Directory.Delete(this.Catalog("kerml").DirectoryName!, recursive: true);
            Directory.Delete(this.Catalog("sysml2").DirectoryName!, recursive: true);

            var result = this.fixture.Query.SearchSpecification("feature");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Unavailable, Does.Contain("hypha generate spec"));
            });
        }

        [Test]
        public void With_one_specification_generated_that_one_is_searched()
        {
            Directory.Delete(this.Catalog("kerml").DirectoryName!, recursive: true);
            this.Page("sysml2", "08.03.06.03-partusage.md", "A part usage.");

            var result = this.fixture.Query.SearchSpecification("part usage");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value!.Select(match => match.Address.Name), Is.EqualTo(new[] { "8.3.6.3" }));
            });
        }

        [TestCase(null, null, 20)]
        [TestCase("  ", null, 20)]
        [TestCase("feature", "uml", 20)]
        [TestCase("feature", null, 0)]
        [TestCase("feature", null, KnowledgeQuery.MaxSearchHits + 1)]
        public void Arguments_that_cannot_be_searched_are_refused(string? text, string? document, int limit)
        {
            Assert.That(() => this.fixture.Query.SearchSpecification(text!, document, limit), Throws.InstanceOf<ArgumentException>());
        }

        private FileInfo Catalog(string document) => this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, document);

        private void Page(string document, string file, string text) =>
            File.WriteAllText(Path.Combine(this.Catalog(document).DirectoryName!, file), $"---\nclause: \"x\"\n---\n\n{text}\n");
    }
}
