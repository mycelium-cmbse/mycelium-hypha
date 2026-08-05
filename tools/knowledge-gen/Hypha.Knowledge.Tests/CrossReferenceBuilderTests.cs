// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceBuilderTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Tests for the cross-reference join, on synthetic data - no knowledge base and no PDFs.
    /// </summary>
    [TestFixture]
    public class CrossReferenceBuilderTests
    {
        internal static readonly ElementRecord[] Elements =
        [
            new("PartUsage", "class", "elements/PartUsage.md"),
            new("Feature", "class", "elements/Feature.md"),
            new("VisibilityKind", "enumeration", "elements/VisibilityKind.md"),
        ];

        internal static readonly Dictionary<string, SpecificationDocument> Documents = new(StringComparer.Ordinal)
        {
            ["kerml"] = new("KerML", "1.0"),
            ["sysml2"] = new("SysML", "2.0"),
        };

        private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> ClauseTitles =
            new(StringComparer.Ordinal)
            {
                ["kerml"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["7.4.2"] = "Feature", ["8.3.3.1.8"] = "Feature", ["2"] = "Conformance",
                },
                ["sysml2"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["8.3.6.3"] = "PartUsage", ["1"] = "Scope",
                },
            };

        private ICrossReferenceBuilder builder = null!;

        [SetUp]
        public void SetUp() => this.builder = new CrossReferenceBuilder();

        internal static CrossReferenceInputs Inputs(
            IReadOnlyList<ElementRecord>? elements = null,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? clauseTitles = null,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>? productions = null,
            IReadOnlyDictionary<string, IReadOnlyList<string>>? examples = null) =>
            new()
            {
                Elements = elements ?? Elements,
                ModelVersionUri = "https://www.omg.org/spec/SysML/20250201",
                Documents = Documents,
                ClauseTitles = clauseTitles ?? ClauseTitles,
                Productions = productions ?? new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                Examples = examples ?? new Dictionary<string, IReadOnlyList<string>>(),
            };

        internal static Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> PerGrammar(
            string grammar, string element, params string[] values) =>
            new(StringComparer.Ordinal)
            {
                [grammar] = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    [element] = values,
                },
            };

        [Test]
        public void Clause_edges_match_titles_exactly_and_keep_every_match()
        {
            var entries = this.builder.Build(Inputs()).Entries;

            Assert.Multiple(() =>
            {
                Assert.That(
                    entries["Feature"].Clauses.Select(edge => edge.Clause),
                    Is.EqualTo(new[] { "7.4.2", "8.3.3.1.8" }));
                Assert.That(
                    entries["PartUsage"].Clauses.Select(edge => edge.Document), Is.EqualTo(new[] { "sysml2" }));
                Assert.That(entries["VisibilityKind"].Clauses, Is.Empty);
            });
        }

        [Test]
        public void Clause_edges_are_tagged_with_provenance_and_method()
        {
            var edge = this.builder.Build(Inputs()).Entries["PartUsage"].Clauses[0];

            Assert.Multiple(() =>
            {
                Assert.That(edge.Provenance, Is.EqualTo(Provenance.Derived));
                Assert.That(edge.Method, Is.EqualTo(CrossReferenceBuilder.ExactTitleMatch));
            });
        }

        [Test]
        public void Clause_edges_sort_documents_before_clause_numbers()
        {
            var titles = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
            {
                ["sysml2"] = new Dictionary<string, string>(StringComparer.Ordinal) { ["1.1"] = "Feature" },
                ["kerml"] = new Dictionary<string, string>(StringComparer.Ordinal) { ["9.9"] = "Feature" },
            };

            var edges = this.builder.Build(Inputs(clauseTitles: titles)).Entries["Feature"].Clauses;

            Assert.That(
                edges.Select(edge => (edge.Document, edge.Clause)),
                Is.EqualTo(new[] { ("kerml", "9.9"), ("sysml2", "1.1") }));
        }

        [Test]
        public void Clause_numbers_sort_numerically_not_lexically()
        {
            var inputs = Inputs(clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>()) with
            {
                GrammarClauses = PerGrammar("kerml", "Feature", "8.3.6.10", "8.3.6.3", "8.3.6.2"),
                GrammarDocuments = new Dictionary<string, string>(StringComparer.Ordinal) { ["kerml"] = "kerml" },
                AllowGrammarOnly = true,
            };

            Assert.That(
                this.builder.Build(inputs).Entries["Feature"].Clauses.Select(edge => edge.Clause),
                Is.EqualTo(new[] { "8.3.6.2", "8.3.6.3", "8.3.6.10" }));
        }

        [Test]
        public void A_grammar_stated_clause_upgrades_a_title_match()
        {
            // The same clause found both ways must not stay labelled as inferred, nor be listed twice.
            var inputs = Inputs() with
            {
                GrammarClauses = PerGrammar("kerml", "Feature", "7.4.2"),
                GrammarDocuments = new Dictionary<string, string>(StringComparer.Ordinal) { ["kerml"] = "kerml" },
            };

            var edges = this.builder.Build(inputs).Entries["Feature"].Clauses;
            var upgraded = edges.Single(edge => edge.Clause == "7.4.2");

            Assert.Multiple(() =>
            {
                Assert.That(upgraded.Provenance, Is.EqualTo(Provenance.Model));
                Assert.That(upgraded.Method, Is.EqualTo(CrossReferenceBuilder.GrammarClause));
            });
        }

        [Test]
        public void A_grammar_stated_clause_is_added_when_the_title_never_matched()
        {
            var inputs = Inputs() with
            {
                GrammarClauses = PerGrammar("kerml", "VisibilityKind", "8.3.1"),
                GrammarDocuments = new Dictionary<string, string>(StringComparer.Ordinal) { ["kerml"] = "kerml" },
            };

            Assert.That(
                this.builder.Build(inputs).Entries["VisibilityKind"].Clauses.Select(edge => edge.Clause),
                Is.EqualTo(new[] { "8.3.1" }));
        }

        [Test]
        public void Grammar_edges_distinguish_a_declared_production_from_a_name_match()
        {
            var inputs = Inputs(
                productions: PerGrammar("sysml", "PartUsage", "PartUsage", "PartUsageDeclaration"));

            var methods = this.builder.Build(inputs).Entries["PartUsage"].Grammar
                .ToDictionary(edge => edge.Production, edge => edge.Method, StringComparer.Ordinal);

            Assert.Multiple(() =>
            {
                Assert.That(methods["PartUsage"], Is.EqualTo("production-name"));
                Assert.That(methods["PartUsageDeclaration"], Is.EqualTo("declared-production"));
            });
        }

        [Test]
        public void Grammar_edges_are_model_provenance()
        {
            // The grammar states what a production builds; this is read, not inferred.
            var inputs = Inputs(productions: PerGrammar("kerml", "Feature", "Feature"));

            Assert.That(
                this.builder.Build(inputs).Entries["Feature"].Grammar[0].Provenance,
                Is.EqualTo(Provenance.Model));
        }

        [Test]
        public void The_same_feature_may_appear_under_both_grammars()
        {
            // SysML re-declares many KerML productions. That is one fact stated twice, not a duplicate.
            var inputs = Inputs() with
            {
                GrammarFeatures = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>>>(
                    StringComparer.Ordinal)
                {
                    ["kerml"] = Assignments("Feature", "declaredName"),
                    ["sysml"] = Assignments("Feature", "declaredName"),
                },
            };

            var edges = this.builder.Build(inputs).Entries["Feature"].Features;

            Assert.That(edges.Select(edge => edge.Grammar), Is.EqualTo(new[] { "kerml", "sysml" }));
        }

        [Test]
        public void Example_edges_invert_the_curated_front_matter()
        {
            var examples = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            {
                ["textual-notation/examples/part-definitions.md"] = ["PartUsage", "Feature"],
                ["textual-notation/examples/generalization.md"] = ["Feature"],
            };

            var entries = this.builder.Build(Inputs(examples: examples)).Entries;

            Assert.Multiple(() =>
            {
                Assert.That(entries["Feature"].Examples.Select(edge => edge.File), Is.EqualTo(new[]
                {
                    "textual-notation/examples/generalization.md",
                    "textual-notation/examples/part-definitions.md",
                }));
                Assert.That(entries["VisibilityKind"].Examples, Is.Empty);
            });
        }

        [Test]
        public void Counts_report_coverage_and_entries_carry_their_element()
        {
            var document = this.builder.Build(Inputs(
                productions: PerGrammar("sysml", "PartUsage", "PartUsage"),
                examples: new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    ["textual-notation/examples/part-definitions.md"] = ["PartUsage"],
                }));

            Assert.Multiple(() =>
            {
                Assert.That(document.Counts, Is.EqualTo(new CrossReferenceCounts(
                    Elements: 3,
                    WithClauses: 2,
                    WithoutClauses: 1,
                    WithGrammar: 1,
                    WithFeatures: 0,
                    WithExamples: 1,
                    ClauseEdges: 3,
                    StatedClauseEdges: 0,
                    GrammarEdges: 1,
                    FeatureEdges: 0,
                    ExampleEdges: 1)));
                Assert.That(
                    document.Entries["PartUsage"].Element, Is.EqualTo("metamodel/elements/PartUsage.md"));
                Assert.That(document.Entries["VisibilityKind"].Kind, Is.EqualTo("enumeration"));
            });
        }

        [Test]
        public void Entries_are_ordered_by_name_whatever_order_the_elements_arrive_in()
        {
            var forwards = this.builder.Build(Inputs());
            var backwards = this.builder.Build(Inputs(elements: Elements.Reverse().ToList()));

            Assert.Multiple(() =>
            {
                Assert.That(
                    forwards.Entries.Keys, Is.EqualTo(new[] { "Feature", "PartUsage", "VisibilityKind" }));
                Assert.That(
                    CrossReferenceFile.Render(backwards), Is.EqualTo(CrossReferenceFile.Render(forwards)));
            });
        }

        [Test]
        public void The_output_carries_no_clause_titles()
        {
            // The licensing guarantee: an edge records a clause number, never the OMG wording it points
            // at. This is what lets the file be committed while knowledge/<tag>/spec cannot be.
            var titles = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
            {
                ["sysml2"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["8.3.6.3"] = "PartUsage", ["4"] = "Terms and Definitions",
                },
            };

            var text = CrossReferenceFile.Render(this.builder.Build(Inputs(clauseTitles: titles)));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Not.Contain("Terms and Definitions"));
                Assert.That(text, Does.Not.Contain("\"title\""));
            });
        }

        [Test]
        public void The_rendered_document_ends_with_a_single_newline_and_uses_lf()
        {
            var text = CrossReferenceFile.Render(this.builder.Build(Inputs()));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.EndWith("}\n"));
                Assert.That(text, Does.Not.Contain("\r"));
            });
        }

        [Test]
        public void Carried_title_matches_stand_in_for_a_missing_specification_catalog()
        {
            // The whole point of the carry-forward: no PDFs, same document.
            var withCatalog = this.builder.Build(Inputs());

            var withoutCatalog = this.builder.Build(Inputs(
                clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>()) with
            {
                CarriedClauseEdges = CrossReferenceFile.TitleMatchedEdges(withCatalog),
            });

            Assert.That(
                CrossReferenceFile.Render(withoutCatalog), Is.EqualTo(CrossReferenceFile.Render(withCatalog)));
        }

        [Test]
        public void A_carried_edge_for_an_element_that_no_longer_exists_is_dropped()
        {
            var carried = new Dictionary<string, IReadOnlyList<ClauseEdge>>(StringComparer.Ordinal)
            {
                ["Removed"] = [new ClauseEdge("kerml", "9.9", Provenance.Derived, CrossReferenceBuilder.ExactTitleMatch)],
            };

            var document = this.builder.Build(
                Inputs(clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>()) with
                {
                    CarriedClauseEdges = carried,
                });

            Assert.That(document.Entries.Keys, Does.Not.Contain("Removed"));
        }

        [Test]
        public void A_cold_start_refuses_to_build_rather_than_dropping_a_third_of_the_clause_edges()
        {
            // A new release tag on a machine without the PDFs: no catalog to match titles against, and
            // no previous document to carry them from. Writing anyway would produce a file that looks
            // complete and is not.
            var inputs = Inputs(clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>());

            var exception = Assert.Throws<InvalidOperationException>(() => this.builder.Build(inputs));

            Assert.Multiple(() =>
            {
                Assert.That(exception!.Message, Does.Contain("clause catalog"));
                Assert.That(exception.Message, Does.Contain("carry the matches forward"));
                Assert.That(
                    exception.Message, Does.Contain(nameof(CrossReferenceInputs.AllowGrammarOnly)),
                    "the message must name the way out, not just the problem");
            });
        }

        [Test]
        public void A_grammar_only_document_is_allowed_when_it_is_asked_for()
        {
            var inputs = Inputs(clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>()) with
            {
                AllowGrammarOnly = true,
            };

            Assert.That(this.builder.Build(inputs).Counts.ClauseEdges, Is.Zero);
        }

        [Test]
        public void A_catalog_alone_is_enough_to_build()
        {
            // The two supported paths must stay open: catalog present, and catalog absent but a
            // previous document to carry from.
            Assert.Multiple(() =>
            {
                Assert.That(() => this.builder.Build(Inputs()), Throws.Nothing);
                Assert.That(
                    () => this.builder.Build(
                        Inputs(clauseTitles: new Dictionary<string, IReadOnlyDictionary<string, string>>()) with
                        {
                            CarriedClauseEdges = CrossReferenceFile.TitleMatchedEdges(
                                this.builder.Build(Inputs())),
                        }),
                    Throws.Nothing);
            });
        }

        private static Dictionary<string, IReadOnlyList<FeatureLink>> Assignments(
            string element, string feature) =>
            new(StringComparer.Ordinal)
            {
                [element] = [new FeatureLink(feature, "=", ["SomeProduction"])],
            };
    }
}
