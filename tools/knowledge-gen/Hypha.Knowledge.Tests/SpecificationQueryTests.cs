// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationQueryTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;

    // Specification text, search, hypha:// addresses and the release inventory.
    [TestFixture]
    public class SpecificationQueryTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void A_clause_comes_with_its_verbatim_text_without_the_front_matter()
        {
            var result = this.fixture.Query.Clause("kerml", "7.4.2");

            Assert.Multiple(() =>
            {
                Assert.That(result.Available, Is.True);
                Assert.That(result.Value, Is.EqualTo(new SpecificationClause(
                    "kerml", "7.4.2", "Feature", "45-47", true, QueryFixture.ClauseText("KerML", "7.4.2"))));
                Assert.That(
                    result.Grounding,
                    Is.EqualTo(new Grounding(
                        Provenance.Normative, QueryFixture.Tag, $"knowledge/{QueryFixture.Tag}/spec/kerml/07.04.02-feature.md")));
            });
        }

        [Test]
        public void The_specification_text_spec_extract_writes_reads_back()
        {
            // spec-extract is Python; this is the one check that its real output fits the C# contract. It needs
            // a release whose specification text was generated locally, so it is ignored in CI.
            var real = Repository.Layout is null
                ? null
                : new DirectoryInfo(Path.Combine(Repository.Layout.Root.FullName, "knowledge"))
                    .EnumerateDirectories()
                    .Select(release => new DirectoryInfo(Path.Combine(release.FullName, "spec")))
                    .Where(spec => File.Exists(Path.Combine(spec.FullName, "kerml", "index.json"))
                                   && File.Exists(Path.Combine(spec.FullName, "sysml2", "index.json")))
                    .OrderByDescending(spec => spec.Parent!.Name, StringComparer.Ordinal)
                    .FirstOrDefault();

            if (real is null)
            {
                Assert.Ignore("no release in this checkout has generated its specification text");
            }

            var spec = new DirectoryInfo(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").DirectoryName!).Parent!;
            spec.Delete(recursive: true);

            foreach (var file in real.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(spec.FullName, Path.GetRelativePath(real.FullName, file.FullName));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                file.CopyTo(target);
            }

            var conformance = this.fixture.Query.Clause("kerml", "2");
            var scope = this.fixture.Query.Clause("sysml2", "1");

            Assert.Multiple(() =>
            {
                Assert.That(conformance.Value!.Title, Is.EqualTo("Conformance"));
                Assert.That(conformance.Value.Text, Does.StartWith("# 2 Conformance"));
                Assert.That(scope.Value!.Title, Is.EqualTo("Scope"));
                Assert.That(this.fixture.Query.Search("Conformance", kind: HyphaAddress.Kerml).Value, Is.Not.Empty);
            });
        }

        [Test]
        public void A_clause_the_specification_does_not_have_is_not_found()
        {
            var result = this.fixture.Query.Clause("sysml2", "99.1");

            Assert.Multiple(() =>
            {
                Assert.That(result.Found, Is.False);
                Assert.That(result.Available, Is.True);
                Assert.That(result.Grounding.Source, Is.EqualTo($"knowledge/{QueryFixture.Tag}/spec/sysml2/index.json"));
            });
        }

        [Test]
        public void A_clause_says_whether_it_is_normative_and_which_pages_it_spans()
        {
            var scope = this.fixture.Query.Clause("sysml2", "1").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(scope.Normative, Is.False);
                Assert.That(scope.Pages, Is.EqualTo("1-2"));
                Assert.That(this.fixture.Query.Clause("kerml", "8.3.3.1.8").Value!.Pages, Is.EqualTo("212"));
            });
        }

        [Test]
        public void A_clause_number_and_search_text_are_trimmed()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Clause("kerml", " 7.4.2 ").Value!.Clause, Is.EqualTo("7.4.2"));
                Assert.That(this.fixture.Api.Clause(" 7.1.4\t").Found, Is.True);
                Assert.That(this.fixture.Query.Search("  7.4.2 ", kind: HyphaAddress.Kerml).Value![0].Match, Is.EqualTo(SearchMatch.Exact));
            });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void A_blank_clause_or_search_text_is_refused(string? text)
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.fixture.Query.Clause("kerml", text!), Throws.InstanceOf<ArgumentException>());
                Assert.That(() => this.fixture.Api.Clause(text!), Throws.InstanceOf<ArgumentException>());
                Assert.That(() => this.fixture.Query.Search(text!), Throws.InstanceOf<ArgumentException>());
            });
        }

        [Test]
        public void An_index_in_the_shape_spec_extract_writes_is_read()
        {
            // The entries of tools/spec-extract/tests/test_markdown.py's render_index_json test, laid out as
            // json.dumps(indent=2) writes them, so a change to either side's shape shows up here.
            var index = this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml");
            File.WriteAllText(
                index.FullName,
                """
                {
                  "schemaVersion": "1.0.0",
                  "document": "KerML",
                  "version": "1.0",
                  "clauses": 2,
                  "entries": {
                    "1": {
                      "title": "Scope",
                      "pages": "1-2",
                      "normative": false,
                      "file": "01-scope.md"
                    },
                    "7.4.2": {
                      "title": "Concrete Syntax",
                      "pages": "45-47",
                      "normative": true,
                      "file": "07.04.02-concrete-syntax.md"
                    }
                  }
                }

                """);
            File.WriteAllText(
                Path.Combine(index.DirectoryName!, "07.04.02-concrete-syntax.md"),
                "---\nclause: \"7.4.2\"\ntitle: \"Concrete Syntax\"\n---\n\n# 7.4.2 Concrete Syntax\n");

            var clause = this.fixture.Query.Clause("kerml", "7.4.2").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(clause.Title, Is.EqualTo("Concrete Syntax"));
                Assert.That(clause.Pages, Is.EqualTo("45-47"));
                Assert.That(clause.Normative, Is.True);
                Assert.That(clause.Text, Is.EqualTo("# 7.4.2 Concrete Syntax\n"));
            });
        }

        [TestCase("{}")]
        [TestCase("not json")]
        [TestCase("""{"schemaVersion": "1.0.0", "document": "KerML", "version": "1.0", "clauses": 1, "entries": {"7.4.2": {"title": "Feature"}}}""")]
        public void A_specification_index_that_is_there_but_invalid_is_an_error_not_unavailable(string json)
        {
            File.WriteAllText(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").FullName, json);
            File.WriteAllText(this.fixture.ApiLayout.SpecificationCatalog(QueryFixture.ApiVersion).FullName, json);
            var address = new HyphaAddress(QueryFixture.Tag, HyphaAddress.Kerml, "7.4.2");

            Assert.Multiple(() =>
            {
                Assert.That(() => this.fixture.Query.Clause("kerml", "7.4.2"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Query.Search("Feature"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Query.Resolve(address), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Api.Clause("7.1.4"), Throws.TypeOf<KnowledgeLookupException>());
            });
        }

        [Test]
        public void A_specification_deleted_or_regenerated_after_a_query_is_read_again()
        {
            var query = this.fixture.Query;
            var index = this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml");
            Assert.That(query.Clause("kerml", "7.4.2").Found, Is.True);

            Directory.Delete(index.DirectoryName!, recursive: true);
            var deleted = query.Clause("kerml", "7.4.2");
            var searched = query.Search("7.4.2", kind: HyphaAddress.Kerml).Value;

            QueryFixture.WriteSpecification(index, "KerML", ("7.4.2", "Features and Types", "50", true, "07.04.02-features.md"));
            var regenerated = query.Clause("kerml", "7.4.2");

            Assert.Multiple(() =>
            {
                Assert.That(deleted.Available, Is.False);
                Assert.That(searched, Is.Empty);
                Assert.That(regenerated.Value!.Title, Is.EqualTo("Features and Types"));
                Assert.That(regenerated.Grounding.Source, Does.EndWith("/07.04.02-features.md"));
            });
        }

        [Test]
        public void A_missing_page_names_the_command_that_regenerates_the_specification_text()
        {
            File.Delete(Path.Combine(
                this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").DirectoryName!, "07.04.02-feature.md"));
            File.Delete(Path.Combine(
                this.fixture.ApiLayout.SpecificationCatalog(QueryFixture.ApiVersion).DirectoryName!, "07.01.04-commit.md"));

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => this.fixture.Query.Clause("kerml", "7.4.2"),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains($"release {QueryFixture.Tag}")
                        .And.Message.Contains($"hypha generate spec --tag {QueryFixture.Tag}"));
                Assert.That(
                    () => this.fixture.Query.Resolve(new HyphaAddress(QueryFixture.Tag, HyphaAddress.Kerml, "7.4.2")),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains($"hypha generate spec --tag {QueryFixture.Tag}"));
                Assert.That(
                    () => this.fixture.Api.Clause("7.1.4"),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains($"API version {QueryFixture.ApiVersion}")
                        .And.Message.Contains($"hypha api generate spec --tag {QueryFixture.ApiVersion}"));
            });
        }

        [Test]
        public void Specification_text_that_was_never_generated_is_unavailable_and_says_how_to_generate_it()
        {
            // The PDFs are optional, so missing text is an answer the skills degrade on, not an error.
            Directory.Delete(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").DirectoryName!, recursive: true);
            Directory.Delete(this.fixture.ApiLayout.SpecificationCatalog(QueryFixture.ApiVersion).DirectoryName!, recursive: true);

            var release = this.fixture.Query.Clause("kerml", "7.4.2");
            var api = this.fixture.Api.Clause("7.1.4");

            Assert.Multiple(() =>
            {
                Assert.That(release.Found, Is.False);
                Assert.That(release.Available, Is.False);
                Assert.That(release.Unavailable, Does.Contain($"hypha fetch --tag {QueryFixture.Tag}")
                    .And.Contain($"hypha generate spec --tag {QueryFixture.Tag}"));
                Assert.That(
                    release.Grounding,
                    Is.EqualTo(new Grounding(
                        Provenance.Normative, QueryFixture.Tag, $"knowledge/{QueryFixture.Tag}/spec/kerml/index.json")));
                Assert.That(api.Available, Is.False);
                Assert.That(api.Unavailable, Does.Contain($"hypha api fetch --tag {QueryFixture.ApiVersion}"));
                Assert.That(this.fixture.Query.Clause("sysml2", "8.3.6.3").Found, Is.True, "the other specification is still there");
            });
        }

        [Test]
        public void An_index_that_points_outside_its_folder_is_refused()
        {
            var index = this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml");
            File.WriteAllText(
                index.FullName,
                File.ReadAllText(index.FullName).Replace("07.04.02-feature.md", "../../metamodel/metamodel.json", StringComparison.Ordinal));

            Assert.That(
                () => this.fixture.Query.Clause("kerml", "7.4.2"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("outside"));
        }

        [Test]
        public void Only_kerml_and_sysml_are_specifications_of_a_release()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.fixture.Query.Clause("uml", "7.4.2"), Throws.ArgumentException);
                Assert.That(() => this.fixture.Query.Clause("KerML", "7.4.2"), Throws.ArgumentException);
                Assert.That(() => this.fixture.Query.Clause(null!, "7.4.2"), Throws.InstanceOf<ArgumentException>());
            });
        }

        [Test]
        public void An_api_clause_it_does_not_have_is_not_found_and_an_api_version_not_installed_is_refused()
        {
            var missing = this.fixture.Api.Clause("99.9");

            Assert.Multiple(() =>
            {
                Assert.That(missing.Found, Is.False);
                Assert.That(missing.Available, Is.True);
                Assert.That(missing.Grounding.Source, Is.EqualTo($"knowledge/api/{QueryFixture.ApiVersion}/spec/index.json"));
                Assert.That(this.fixture.Api.Clause("7.1.4", QueryFixture.ApiVersion).Found, Is.True);
                Assert.That(() => this.fixture.Api.Clause("7.1.4", "2020-01"), Throws.TypeOf<KnowledgeLookupException>());
            });
        }

        [Test]
        public void An_api_clause_names_its_document_api()
        {
            var result = this.fixture.Api.Clause("7.1.4");

            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.Document, Is.EqualTo(ApiQuery.Document));
                Assert.That(result.Value.Title, Is.EqualTo("Commit"));
                Assert.That(result.Value.Text, Is.EqualTo(QueryFixture.ClauseText("Systems Modeling API and Services", "7.1.4")));
                Assert.That(
                    result.Grounding,
                    Is.EqualTo(new Grounding(
                        Provenance.Normative, QueryFixture.ApiVersion, $"knowledge/api/{QueryFixture.ApiVersion}/spec/07.01.04-commit.md")));
            });
        }

        [TestCase("---\na: 1\n---\n\n# Body\n", "# Body\n")]
        [TestCase("---\r\na: 1\r\n---\r\n# Body\r\n", "# Body\n")]
        [TestCase("# No front matter\n", "# No front matter\n")]
        [TestCase("---\nnot closed\n", "---\nnot closed\n")]
        public void Front_matter_is_left_out_of_the_text_only_when_it_is_there(string page, string expected)
        {
            var index = this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml");
            File.WriteAllText(Path.Combine(index.DirectoryName!, "07.04.02-feature.md"), page);

            Assert.That(this.fixture.Query.Clause("kerml", "7.4.2").Value!.Text, Is.EqualTo(expected));
        }

        [Test]
        public void Search_ranks_an_exact_name_before_a_name_that_only_contains_it()
        {
            var hits = this.fixture.Query.Search("PartUsage").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(hits[0].Address, Is.EqualTo(new HyphaAddress(QueryFixture.Tag, HyphaAddress.Metamodel, "PartUsage")));
                Assert.That(hits[0].Match, Is.EqualTo(SearchMatch.Exact));
                Assert.That(hits.Select(hit => hit.Match), Is.Ordered);
                Assert.That(
                    hits.Single(hit => hit.Address.Kind == HyphaAddress.Sysml),
                    Is.EqualTo(new SearchHit(
                        new HyphaAddress(QueryFixture.Tag, HyphaAddress.Sysml, "8.3.6.3"), "PartUsage", SearchMatch.Exact)),
                    "clause 8.3.6.3 is titled PartUsage, and a clause title names the clause");
            });
        }

        [Test]
        public void Search_ignores_case_and_finds_a_library_declaration_by_its_last_part()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Search("partusage", kind: HyphaAddress.Metamodel).Value![0].Match,
                    Is.EqualTo(SearchMatch.IgnoringCase));

                var mass = this.fixture.Query.Search("mass", kind: HyphaAddress.ModelLibrary).Value!;
                Assert.That(mass.Select(hit => hit.Address.Name), Is.EqualTo(new[] { "ISQBase::mass" }));
                Assert.That(mass[0].Match, Is.EqualTo(SearchMatch.TrailingPart));
            });
        }

        [Test]
        public void Search_finds_a_clause_by_number_and_by_title()
        {
            Assert.Multiple(() =>
            {
                var byNumber = this.fixture.Query.Search("7.4.2", kind: HyphaAddress.Kerml).Value!;
                Assert.That(byNumber[0].Address.ToString(), Is.EqualTo($"hypha://{QueryFixture.Tag}/kerml/7.4.2"));
                Assert.That(byNumber[0].Match, Is.EqualTo(SearchMatch.Exact));

                var byTitle = this.fixture.Query.Search("conformance").Value!;
                Assert.That(byTitle.Select(hit => hit.Address.Name), Does.Contain("2"));
            });
        }

        [Test]
        public void Search_keeps_to_the_kind_and_the_limit_it_is_given()
        {
            var limited = this.fixture.Query.Search("Usage", limit: 3).Value!;
            var metamodel = this.fixture.Query.Search("Usage", kind: HyphaAddress.Metamodel, limit: 200).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(limited, Has.Count.EqualTo(3));
                Assert.That(metamodel.Select(hit => hit.Address.Kind), Has.All.EqualTo(HyphaAddress.Metamodel));
                Assert.That(() => this.fixture.Query.Search("Usage", kind: "uml"), Throws.ArgumentException);
                Assert.That(() => this.fixture.Query.Search("Usage", limit: 0), Throws.ArgumentException);
                Assert.That(() => this.fixture.Query.Search("Usage", limit: KnowledgeQuery.MaxSearchHits + 1), Throws.ArgumentException);
                Assert.That(this.fixture.Query.Search("NoSuchThingAnywhere").Value, Is.Empty);
            });
        }

        [Test]
        public void A_clause_titled_with_the_text_is_kept_by_a_small_limit_before_names_that_only_contain_it()
        {
            // Many metaclasses contain "Feature" (FeatureChaining, FeatureTyping, ...); the two clauses titled
            // Feature must not be pushed past the limit by them.
            var tag = QueryFixture.Tag;
            var hits = this.fixture.Query.Search("Feature", limit: 3).Value!;
            var ignoringCase = this.fixture.Query.Search("feature", kind: HyphaAddress.Kerml).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    hits.Select(hit => hit.Address.ToString()),
                    Is.EqualTo(new[] { $"hypha://{tag}/metamodel/Feature", $"hypha://{tag}/kerml/7.4.2", $"hypha://{tag}/kerml/8.3.3.1.8" }));
                Assert.That(hits.Select(hit => hit.Match), Has.All.EqualTo(SearchMatch.Exact));
                Assert.That(ignoringCase.Select(hit => hit.Match), Has.All.EqualTo(SearchMatch.IgnoringCase));
                Assert.That(this.fixture.Query.Search("Feature", limit: KnowledgeQuery.MaxSearchHits).Value, Has.Count.GreaterThan(3));
            });
        }

        [Test]
        public void Hits_that_match_equally_are_ordered_by_kind_then_name()
        {
            // Everything here only contains "Part": metaclass names in ordinal order, then the SysML clause
            // titled PartUsage, since a clause comes after a metaclass.
            var hits = this.fixture.Query.Search("Part", limit: KnowledgeQuery.MaxSearchHits).Value!;
            var metaclasses = hits.Where(hit => hit.Address.Kind == HyphaAddress.Metamodel).Select(hit => hit.Address.Name).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(hits.Select(hit => hit.Match), Has.All.EqualTo(SearchMatch.Contains));
                Assert.That(metaclasses, Has.Count.GreaterThan(1));
                Assert.That(metaclasses, Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal));
                Assert.That(hits[^1].Address, Is.EqualTo(new HyphaAddress(QueryFixture.Tag, HyphaAddress.Sysml, "8.3.6.3")));
                Assert.That(hits.Take(hits.Count - 1).Select(hit => hit.Address.Kind), Has.All.EqualTo(HyphaAddress.Metamodel));
            });
        }

        [Test]
        public void Search_finds_a_metaclass_by_its_qualified_name_or_its_last_parts()
        {
            var full = this.fixture.Query.Search("SysML::Systems::Actions::ActionUsage", kind: HyphaAddress.Metamodel).Value!;
            var trailing = this.fixture.Query.Search("Actions::ActionUsage", kind: HyphaAddress.Metamodel).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(full[0].Address.Name, Is.EqualTo("ActionUsage"));
                Assert.That(full[0].Match, Is.EqualTo(SearchMatch.Exact));
                Assert.That(trailing[0].Address.Name, Is.EqualTo("ActionUsage"));
                Assert.That(trailing[0].Match, Is.EqualTo(SearchMatch.TrailingPart));
            });
        }

        [Test]
        public void Search_leaves_out_a_model_library_that_was_never_generated()
        {
            Directory.Delete(this.fixture.Layout.ModelLibrary(QueryFixture.Tag).FullName, recursive: true);

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Search("mass").Value!.Select(hit => hit.Address.Kind), Has.None.EqualTo(HyphaAddress.ModelLibrary));
                Assert.That(this.fixture.Query.Search("mass", kind: HyphaAddress.ModelLibrary).Value, Is.Empty);
            });
        }

        [Test]
        public void Search_is_grounded_on_the_release_knowledge_folder()
        {
            Assert.That(
                this.fixture.Query.Search("Feature").Grounding,
                Is.EqualTo(new Grounding(Provenance.Derived, QueryFixture.Tag, $"knowledge/{QueryFixture.Tag}")));
        }

        [Test]
        public void Search_leaves_out_specification_text_that_was_never_generated()
        {
            Directory.Delete(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "sysml2").DirectoryName!, recursive: true);

            var hits = this.fixture.Query.Search("PartUsage").Value!;

            Assert.That(hits.Select(hit => hit.Address.Kind), Has.None.EqualTo(HyphaAddress.Sysml));
        }

        [Test]
        public void A_search_hit_of_every_kind_resolves_to_its_page()
        {
            var hits = new[]
            {
                this.fixture.Query.Search("Feature", kind: HyphaAddress.Metamodel).Value![0],
                this.fixture.Query.Search("mass", kind: HyphaAddress.ModelLibrary).Value![0],
                this.fixture.Query.Search("7.4.2", kind: HyphaAddress.Kerml).Value![0],
                this.fixture.Query.Search("PartUsage", kind: HyphaAddress.Sysml).Value![0],
            };

            Assert.Multiple(() =>
            {
                foreach (var hit in hits)
                {
                    var resolved = this.fixture.Query.Resolve(hit.Address);

                    Assert.That(resolved.Found, Is.True, hit.Address.ToString());
                    Assert.That(File.Exists(Path.Combine(this.fixture.Root.FullName, resolved.Value!.Page)), Is.True, resolved.Value.Page);
                    Assert.That(resolved.Value.Title, Is.EqualTo(hit.Title));
                }
            });
        }

        [Test]
        public void An_address_whose_page_is_missing_is_refused()
        {
            // The index lists PartUsage, but the golden pages do not include its page.
            Assert.That(
                () => this.fixture.Query.Resolve(new HyphaAddress(QueryFixture.Tag, HyphaAddress.Metamodel, "PartUsage")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("missing"));
        }

        [Test]
        public void An_address_resolves_through_the_index_of_its_kind()
        {
            var tag = QueryFixture.Tag;

            Assert.Multiple(() =>
            {
                var metaclass = this.fixture.Query.Resolve(HyphaAddress.Parse($"hypha://{tag}/metamodel/Feature"));
                Assert.That(metaclass.Value!.Page, Is.EqualTo($"knowledge/{tag}/metamodel/elements/Feature.md"));
                Assert.That(metaclass.Grounding.Tier, Is.EqualTo(Provenance.Model));

                var declaration = this.fixture.Query.Resolve(HyphaAddress.Parse($"hypha://{tag}/model-library/ISQBase%3A%3Amass"));
                Assert.That(declaration.Value!.Page, Is.EqualTo($"knowledge/{tag}/model-library/packages/ISQBase.md"));
                Assert.That(declaration.Value.Title, Is.EqualTo("attribute"));
                Assert.That(
                    declaration.Grounding,
                    Is.EqualTo(new Grounding(Provenance.Derived, tag, $"knowledge/{tag}/model-library/packages/ISQBase.md")));

                var clause = this.fixture.Query.Resolve(HyphaAddress.Parse($"hypha://{tag}/sysml2/8.3.6.3"));
                Assert.That(clause.Value!.Page, Is.EqualTo($"knowledge/{tag}/spec/sysml2/08.03.06.03-partusage.md"));
                Assert.That(clause.Value.Title, Is.EqualTo("PartUsage"));
                Assert.That(clause.Grounding.Tier, Is.EqualTo(Provenance.Normative));
            });
        }

        [Test]
        public void An_address_of_something_the_release_does_not_have_is_not_found()
        {
            var tag = QueryFixture.Tag;

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Resolve(new HyphaAddress(tag, HyphaAddress.Metamodel, "NoSuchClass")).Found, Is.False);
                Assert.That(this.fixture.Query.Resolve(new HyphaAddress(tag, HyphaAddress.ModelLibrary, "No::such")).Found, Is.False);
                Assert.That(this.fixture.Query.Resolve(new HyphaAddress(tag, HyphaAddress.Kerml, "99")).Found, Is.False);
                Assert.That(
                    this.fixture.Query.Resolve(HyphaAddress.Parse($"hypha://{tag}/metamodel/..%2F..%2Fversions.json")).Found,
                    Is.False,
                    "a name is looked up in the index, never used as a path");
                Assert.That(this.fixture.Query.Resolve(new HyphaAddress(tag, HyphaAddress.Kerml, "../kerml/index.json")).Found, Is.False);
                Assert.That(
                    () => this.fixture.Query.Resolve(new HyphaAddress("2020-01", HyphaAddress.Metamodel, "PartUsage")),
                    Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(
                    () => this.fixture.Query.Resolve(new HyphaAddress(tag, "uml", "Class")),
                    Throws.ArgumentException);
            });
        }

        [Test]
        public void An_address_of_a_clause_never_generated_is_unavailable()
        {
            Directory.Delete(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").DirectoryName!, recursive: true);

            var result = this.fixture.Query.Resolve(new HyphaAddress(QueryFixture.Tag, HyphaAddress.Kerml, "7.4.2"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Found, Is.False);
                Assert.That(result.Available, Is.False);
                Assert.That(result.Unavailable, Does.Contain($"hypha generate spec --tag {QueryFixture.Tag}"));
                Assert.That(
                    result.Grounding,
                    Is.EqualTo(new Grounding(
                        Provenance.Normative, QueryFixture.Tag, $"knowledge/{QueryFixture.Tag}/spec/kerml/index.json")));
            });
        }

        [TestCase("hypha://2026-05/metamodel/PartUsage", "2026-05", "metamodel", "PartUsage")]
        [TestCase("hypha://2026-05/model-library/ISQBase%3A%3Amass", "2026-05", "model-library", "ISQBase::mass")]
        [TestCase("hypha://2026-05/model-library/ISQBase::mass", "2026-05", "model-library", "ISQBase::mass")]
        [TestCase("hypha://2026-05/kerml/7.4.2", "2026-05", "kerml", "7.4.2")]
        [TestCase("hypha://2026-05/sysml2/8.3.6.3", "2026-05", "sysml2", "8.3.6.3")]
        [TestCase("hypha://2026-05/kerml/a/b", "2026-05", "kerml", "a/b")]
        [TestCase("hypha://2026-05/metamodel/A%20B", "2026-05", "metamodel", "A B")]
        [TestCase("hypha://2026-05/metamodel/%A", "2026-05", "metamodel", "%A")]
        public void An_address_parses_into_its_parts(string text, string tag, string kind, string name)
        {
            Assert.That(HyphaAddress.Parse(text), Is.EqualTo(new HyphaAddress(tag, kind, name)));
        }

        [TestCase("http://2026-05/metamodel/PartUsage")]
        [TestCase("hypha://2026-05/metamodel")]
        [TestCase("hypha://2026-05/metamodel/")]
        [TestCase("hypha:///metamodel/PartUsage")]
        [TestCase("hypha://2026-05/uml/Class")]
        [TestCase("hypha://2026-05/Metamodel/PartUsage")]
        [TestCase("HYPHA://2026-05/metamodel/PartUsage")]
        [TestCase("")]
        [TestCase(null)]
        public void Text_that_is_not_an_address_is_refused(string? text)
        {
            Assert.Multiple(() =>
            {
                Assert.That(HyphaAddress.TryParse(text, out var address), Is.False);
                Assert.That(address, Is.Null);
                Assert.That(() => HyphaAddress.Parse(text!), Throws.TypeOf<FormatException>());
            });
        }

        [TestCase(HyphaAddress.ModelLibrary, "ISQBase::mass", "hypha://2026-05/model-library/ISQBase%3A%3Amass")]
        [TestCase(HyphaAddress.Kerml, "a/b", "hypha://2026-05/kerml/a%2Fb")]
        [TestCase(HyphaAddress.Metamodel, "A B", "hypha://2026-05/metamodel/A%20B")]
        [TestCase(HyphaAddress.Metamodel, "%A", "hypha://2026-05/metamodel/%25A")]
        public void An_address_written_and_read_back_is_the_same_address(string kind, string name, string text)
        {
            var address = new HyphaAddress("2026-05", kind, name);

            Assert.Multiple(() =>
            {
                Assert.That(address.ToString(), Is.EqualTo(text));
                Assert.That(HyphaAddress.Parse(address.ToString()), Is.EqualTo(address));
            });
        }

        [Test]
        public void The_inventory_says_which_artifacts_each_release_has_generated()
        {
            var inventory = this.fixture.Query.Releases();
            var release = inventory.Releases.Single();
            var api = inventory.ApiVersions.Single();

            Assert.Multiple(() =>
            {
                Assert.That(release.Tag, Is.EqualTo(QueryFixture.Tag));
                Assert.That(release.IsDefault, Is.True);
                Assert.That(release.Artifacts.Metamodel, Is.True);
                Assert.That(release.Artifacts.ModelLibrary, Is.True);
                Assert.That(release.Artifacts.CrossReferences, Is.True);
                Assert.That(release.Artifacts.Specification, Is.True);
                Assert.That(release.Artifacts.TextualNotation, Is.False, "the fixture generates no textual notation");
                Assert.That(api.Tag, Is.EqualTo(QueryFixture.ApiVersion));
                Assert.That(api.IsDefault, Is.True);
            });

            Directory.Delete(this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml").DirectoryName!, recursive: true);

            Assert.That(this.fixture.Query.Releases().Releases.Single().Artifacts.Specification, Is.False);
        }
    }
}
