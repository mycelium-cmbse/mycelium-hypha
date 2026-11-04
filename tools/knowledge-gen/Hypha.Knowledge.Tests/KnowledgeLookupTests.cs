// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeLookupTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Suite of tests for <see cref="KnowledgeLookup"/>, against a small release written to a
    /// temporary folder in the shape the generators write.
    /// </summary>
    [TestFixture]
    public class KnowledgeLookupTests
    {
        private const string Tag = "2026-09";

        private DirectoryInfo root = null!;
        private KnowledgeLayout layout = null!;
        private KnowledgeLookup lookup = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), "hypha-lookup-" + Guid.NewGuid().ToString("N")));
            this.root.Create();
            this.layout = new KnowledgeLayout(this.root);
            this.lookup = new KnowledgeLookup(this.layout);

            Install(this.layout, Tag);

            this.Write(
                "metamodel/index.json",
                """
                {
                  "schemaVersion": "1.0.0",
                  "entries": {
                    "PartUsage": { "kind": "class", "qualifiedName": "SysML::Systems::Parts::PartUsage", "file": "elements/PartUsage.md" },
                    "PartDefinition": { "kind": "class", "qualifiedName": "SysML::Systems::Parts::PartDefinition", "file": "elements/PartDefinition.md" },
                    "ActionUsage": { "kind": "class", "qualifiedName": "SysML::Systems::Actions::ActionUsage", "file": "elements/ActionUsage.md" }
                  }
                }
                """);
            this.Write("metamodel/elements/PartUsage.md", Lines("PartUsage line", 400));
            this.Write("metamodel/elements/PartDefinition.md", Lines("PartDefinition line", 10));
            this.Write("metamodel/elements/ActionUsage.md", Lines("ActionUsage line", 10));

            this.Write(
                "model-library/index.json",
                """
                {
                  "schemaVersion": "1.0.0",
                  "tag": "2026-09",
                  "counts": { "files": 3, "declarations": 6 },
                  "declarations": {
                    "ScalarValues": { "kind": "standard library package", "file": "packages/ScalarValues.md", "source": "ScalarValues.kerml" },
                    "ScalarValues::Real": { "kind": "datatype", "file": "packages/ScalarValues.md", "source": "ScalarValues.kerml" },
                    "ISQSpaceTime::LengthValue::length": { "kind": "attribute", "file": "packages/ISQSpaceTime.md", "source": "ISQSpaceTime.sysml" },
                    "ISQSpaceTime::length": { "kind": "attribute", "file": "packages/ISQSpaceTime.md", "source": "ISQSpaceTime.sysml" },
                    "ISQBase::mass": { "kind": "attribute", "file": "packages/ISQBase.md", "source": "ISQBase.sysml" },
                    "ISQMechanics::mass": { "kind": "attribute", "file": "packages/ISQBase.md", "source": "ISQMechanics.sysml" }
                  }
                }
                """);
            this.Write(
                "model-library/packages/ScalarValues.md",
                """
                ---
                name: ScalarValues
                ---

                # ScalarValues

                ```kerml
                standard library package ScalarValues {
                    abstract datatype ScalarValue;
                    datatype Boolean specializes ScalarValue;
                    datatype Real specializes NumericalValue;
                }
                ```
                """);
            this.Write(
                "model-library/packages/ISQSpaceTime.md",
                """
                ---
                name: ISQSpaceTime
                ---

                ```sysml
                standard library package ISQSpaceTime {
                    // length is described below
                    attribute def LengthValue {
                        attribute length;
                    }
                    attribute length: LengthValue;
                }
                ```
                """);
            this.Write("model-library/packages/ISQBase.md", "```sysml\nattribute mass;\n```\n");
        }

        [TearDown]
        public void TearDown()
        {
            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [Test]
        public void An_exact_metamodel_name_returns_the_first_page_of_its_element_file()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.Release, Is.EqualTo(Tag));
                Assert.That(result.Name, Is.EqualTo("PartUsage"));
                Assert.That(result.Path, Is.EqualTo(this.PathOf("metamodel/elements/PartUsage.md")));
                Assert.That(result.StartLine, Is.EqualTo(1));
                Assert.That(result.EndLine, Is.EqualTo(LookupRequest.DefaultMaxLines));
                Assert.That(result.TotalLines, Is.EqualTo(400));
                Assert.That(result.NextLine, Is.EqualTo(LookupRequest.DefaultMaxLines + 1));
                Assert.That(result.Content!.Split('\n'), Has.Length.EqualTo(LookupRequest.DefaultMaxLines));
                Assert.That(result.Content, Does.StartWith("PartUsage line 1\n"));
            });
        }

        [Test]
        public void The_last_page_has_no_next_line()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", StartLine: 321));

            Assert.Multiple(() =>
            {
                Assert.That(result.StartLine, Is.EqualTo(321));
                Assert.That(result.EndLine, Is.EqualTo(400));
                Assert.That(result.NextLine, Is.Null);
                Assert.That(result.Content, Does.EndWith("PartUsage line 400"));
            });
        }

        [Test]
        public void A_start_line_past_the_end_is_refused()
        {
            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", StartLine: 401)),
                Throws.ArgumentException.With.Message.Contains("400 lines"));
        }

        [TestCase(0)]
        [TestCase(LookupRequest.LargestPage + 1)]
        public void A_page_size_out_of_range_is_refused(int maxLines)
        {
            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", MaxLines: maxLines)),
                Throws.ArgumentException);
        }

        [Test]
        public void A_name_in_another_case_resolves_to_its_one_entry_rather_than_listing_every_substring_match()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "partusage"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.Name, Is.EqualTo("PartUsage"));
            });
        }

        [TestCase("SysML::Systems::Parts::PartUsage")]
        [TestCase("Parts::PartUsage")]
        [TestCase("parts::partusage")]
        public void A_qualified_or_partly_qualified_metamodel_name_resolves(string name)
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, name));

            Assert.That(result.Name, Is.EqualTo("PartUsage"));
        }

        [Test]
        public void A_short_model_library_name_resolves_to_its_one_qualified_declaration()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "Real"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.Name, Is.EqualTo("ScalarValues::Real"));
            });
        }

        [Test]
        public void A_model_library_lookup_starts_at_the_declaration_not_the_top_of_the_package_page()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ScalarValues::Real"));

            Assert.Multiple(() =>
            {
                Assert.That(result.DeclarationLine, Is.EqualTo(11));
                Assert.That(result.StartLine, Is.EqualTo(11));
                Assert.That(result.Content, Does.StartWith("    datatype Real specializes NumericalValue;"));
            });
        }

        [Test]
        public void A_member_is_told_apart_from_a_same_named_member_of_a_nested_scope()
        {
            var outer = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ISQSpaceTime::length"));
            var nested = this.lookup.Lookup(
                new LookupRequest(KnowledgeKind.ModelLibrary, "ISQSpaceTime::LengthValue::length"));

            Assert.Multiple(() =>
            {
                Assert.That(nested.DeclarationLine, Is.EqualTo(9));
                Assert.That(nested.Content, Does.StartWith("        attribute length;"));

                // Neither the comment mentioning it nor the nested member declared before it is
                // mistaken for the package's own attribute.
                Assert.That(outer.DeclarationLine, Is.EqualTo(11));
                Assert.That(outer.Content, Does.StartWith("    attribute length: LengthValue;"));
            });
        }

        [Test]
        public void An_explicit_start_line_wins_over_the_declaration()
        {
            var result = this.lookup.Lookup(
                new LookupRequest(KnowledgeKind.ModelLibrary, "ScalarValues::Real", StartLine: 1));

            Assert.Multiple(() =>
            {
                Assert.That(result.StartLine, Is.EqualTo(1));
                Assert.That(result.DeclarationLine, Is.EqualTo(11));
            });
        }

        [Test]
        public void A_metamodel_lookup_has_no_declaration_line()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "ActionUsage"));

            Assert.That(result.DeclarationLine, Is.Null);
        }

        [Test]
        public void The_same_short_name_in_two_packages_is_ambiguous()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "mass"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Ambiguous));
                Assert.That(result.Matches, Is.EqualTo(new[] { "ISQBase::mass", "ISQMechanics::mass" }));
                Assert.That(result.TotalMatches, Is.EqualTo(2));
                Assert.That(result.Content, Is.Null);
                Assert.That(result.Path, Is.Null);
            });
        }

        [Test]
        public void A_substring_match_is_only_ever_a_candidate_even_when_it_is_the_only_one()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "ActionUsag"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Candidates));
                Assert.That(result.Matches, Is.EqualTo(new[] { "ActionUsage" }));
                Assert.That(result.Content, Is.Null);
            });
        }

        [Test]
        public void Candidates_are_sorted_and_capped()
        {
            var entries = string.Join(
                ",\n",
                Enumerable.Range(0, KnowledgeLookup.MaxMatches + 5)
                    .Select(index => $"\"Widget{index:D2}\": {{ \"kind\": \"class\", \"file\": \"elements/W.md\" }}"));
            this.Write("metamodel/index.json", $"{{ \"entries\": {{ {entries} }} }}");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "widget"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Candidates));
                Assert.That(result.Matches, Has.Count.EqualTo(KnowledgeLookup.MaxMatches));
                Assert.That(result.Matches, Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal));
                Assert.That(result.TotalMatches, Is.EqualTo(KnowledgeLookup.MaxMatches + 5));
            });
        }

        [Test]
        public void A_name_nothing_contains_is_not_found()
        {
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Spaceship"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.NotFound));
                Assert.That(result.Matches, Is.Empty);
            });
        }

        [Test]
        public void A_named_release_is_used_instead_of_the_default()
        {
            Install(this.layout, Tag, "2026-05");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", "2026-05")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha generate --tag 2026-05"));
        }

        [Test]
        public void A_release_that_is_not_installed_is_refused_and_the_installed_ones_are_named()
        {
            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", "2025-01")),
                Throws.TypeOf<KnowledgeLookupException>()
                    .With.Message.Contains("2025-01").And.Message.Contains(Tag));
        }

        [Test]
        public void Nothing_installed_says_how_to_install_a_release()
        {
            this.layout.VersionManifest.Delete();

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha fetch"));
        }

        [Test]
        public void An_index_that_is_not_valid_json_asks_for_regeneration()
        {
            this.Write("metamodel/index.json", "{ not json");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha generate --tag 2026-09"));
        }

        [TestCase("../outside.md")]
        [TestCase("elements/../../outside.md")]
        [TestCase("elements\\PartUsage.md")]
        public void An_index_path_that_leaves_the_artifact_folder_is_refused(string file)
        {
            this.Write("outside.md", "outside");
            this.Write(
                "metamodel/index.json",
                $"{{ \"entries\": {{ \"Escape\": {{ \"kind\": \"class\", \"file\": \"{file.Replace("\\", "\\\\", StringComparison.Ordinal)}\" }} }} }}");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Escape")),
                Throws.TypeOf<KnowledgeLookupException>());
        }

        [Test]
        public void An_absolute_index_path_is_refused()
        {
            var absolute = this.PathOf("metamodel/elements/PartUsage.md").Replace("\\", "/", StringComparison.Ordinal);
            this.Write(
                "metamodel/index.json",
                $"{{ \"entries\": {{ \"Escape\": {{ \"kind\": \"class\", \"file\": \"{absolute}\" }} }} }}");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Escape")),
                Throws.TypeOf<KnowledgeLookupException>());
        }

        [Test]
        public void A_page_that_links_outside_the_artifact_folder_is_refused()
        {
            this.Write("outside.md", "outside");
            var link = this.PathOf("metamodel/elements/Link.md");

            try
            {
                File.CreateSymbolicLink(link, this.PathOf("outside.md"));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Windows only lets an account create symbolic links with Developer Mode on or as an
                // administrator. Without that, there is no link to refuse.
                Assert.Ignore($"symbolic links cannot be created here: {exception.Message}");
            }

            this.Write(
                "metamodel/index.json",
                "{ \"entries\": { \"Link\": { \"kind\": \"class\", \"file\": \"elements/Link.md\" } } }");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Link")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("links outside"));
        }

        [Test]
        public void A_page_named_by_the_index_but_missing_asks_for_regeneration()
        {
            File.Delete(this.PathOf("metamodel/elements/ActionUsage.md"));

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "ActionUsage")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha generate"));
        }

        [Test]
        public void An_empty_name_is_refused()
        {
            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "  ")),
                Throws.ArgumentException);
        }

        [Test]
        public void An_exact_name_wins_over_the_same_name_in_another_case()
        {
            this.WriteLibrary(("P::Real", "datatype"), ("P::real", "datatype"));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::Real"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.Name, Is.EqualTo("P::Real"));
            });
        }

        [Test]
        public void An_exact_short_name_wins_over_one_in_another_case()
        {
            this.WriteLibrary(("P::Real", "datatype"), ("P::real", "datatype"));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "Real"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.Name, Is.EqualTo("P::Real"));
            });
        }

        [Test]
        public void An_exact_key_wins_over_a_qualified_name_ending_in_it()
        {
            this.WriteLibrary(("ScalarValues", "package"), ("Other::ScalarValues", "package"));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ScalarValues"));

            Assert.That(result.Name, Is.EqualTo("ScalarValues"));
        }

        [TestCase("{ not json")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{ \"declarations\": [] }")]
        [TestCase("{ \"declarations\": { \"X\": null } }")]
        [TestCase("{ \"declarations\": { \"X\": { \"file\": \"packages/X.md\" } } }")]
        [TestCase("{ \"declarations\": { \"X\": { \"kind\": \"part\" } } }")]
        [TestCase("{ \"declarations\": { \"X\": { \"kind\": \"part\", \"file\": 3 } } }")]
        public void A_model_library_index_that_is_not_valid_asks_for_regeneration(string index)
        {
            this.Write("model-library/index.json", index);

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "X")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha generate --tag 2026-09"));
        }

        [TestCase("{}")]
        [TestCase("{ \"entries\": [] }")]
        [TestCase("{ \"entries\": { \"X\": \"elements/X.md\" } }")]
        [TestCase("{ \"entries\": { \"X\": { \"kind\": \"class\" } } }")]
        [TestCase("{ \"entries\": { \"X\": { \"kind\": \"class\", \"file\": false } } }")]
        [TestCase("{ \"entries\": { \"X\": { \"kind\": 1, \"file\": \"elements/X.md\" } } }")]
        public void A_metamodel_index_that_is_not_valid_asks_for_regeneration(string index)
        {
            this.Write("metamodel/index.json", index);

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "X")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("hypha generate --tag 2026-09"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void A_start_line_below_one_is_refused(int startLine)
        {
            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", StartLine: startLine)),
                Throws.ArgumentException);
        }

        [Test]
        public void The_largest_page_is_accepted()
        {
            var result = this.lookup.Lookup(
                new LookupRequest(KnowledgeKind.Metamodel, "PartUsage", MaxLines: LookupRequest.LargestPage));

            Assert.That(result.EndLine, Is.EqualTo(400));
        }

        [Test]
        public void A_declaration_whose_scopes_cannot_be_followed_is_found_by_name()
        {
            // ISQMechanics::mass is indexed on a page that has no ISQMechanics scope at all.
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "ISQMechanics::mass"));

            Assert.Multiple(() =>
            {
                Assert.That(result.DeclarationLine, Is.EqualTo(2));
                Assert.That(result.Content, Does.StartWith("attribute mass;"));
            });
        }

        [Test]
        public void A_declaration_missing_from_its_page_starts_at_the_top()
        {
            this.WriteLibrary(("P::Ghost", "part"));
            this.Write("model-library/packages/P.md", "```sysml\npackage P {\n    part Other;\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::Ghost"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Found));
                Assert.That(result.DeclarationLine, Is.Null);
                Assert.That(result.StartLine, Is.EqualTo(1));
            });
        }

        [Test]
        public void A_member_on_the_same_line_as_its_scope_is_found()
        {
            this.WriteLibrary(("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                "```sysml\npackage P {\n    attribute x;\n    part def A { attribute x; }\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(4));
        }

        [Test]
        public void A_scope_whose_brace_is_on_the_next_line_is_followed()
        {
            this.WriteLibrary(("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                "```sysml\npackage P\n{\n    attribute x;\n    part def A\n    {\n        attribute x;\n    }\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(7));
        }

        [Test]
        public void Braces_in_comments_and_quoted_names_do_not_change_the_scope()
        {
            this.WriteLibrary(("P::A::x", "attribute"), ("P::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                string.Join(
                    '\n',
                    "```sysml",
                    "package P {",
                    "    part def A {",
                    "        doc /* opens { here",
                    "           and } there { */",
                    "        attribute '{y}';",
                    "        attribute x;",
                    "    }",
                    "    // a stray } in a comment",
                    "    attribute x;",
                    "}",
                    "```",
                    string.Empty));

            var nested = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));
            var outer = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::x"));

            Assert.Multiple(() =>
            {
                Assert.That(nested.DeclarationLine, Is.EqualTo(7));
                Assert.That(outer.DeclarationLine, Is.EqualTo(10));
            });
        }

        [Test]
        public void A_type_reference_or_a_comment_is_not_taken_for_an_enclosing_scope()
        {
            this.WriteLibrary(("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                string.Join(
                    '\n',
                    "```sysml",
                    "package P {",
                    "    // A { is described below",
                    "    part p : A { attribute x; }",
                    "    part q :> A {",
                    "        attribute x;",
                    "    }",
                    "    part def A {",
                    "        attribute x;",
                    "    }",
                    "}",
                    "```",
                    string.Empty));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(9));
        }

        [Test]
        public void A_quoted_declaration_name_is_found()
        {
            this.WriteLibrary(("P::'Real Value'", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                "```sysml\npackage P {\n    attribute other;\n    attribute 'Real Value';\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::'Real Value'"));

            Assert.That(result.DeclarationLine, Is.EqualTo(4));
        }

        [Test]
        public void A_keyword_of_several_words_matches_across_extra_whitespace()
        {
            this.WriteLibrary(("P::X", "attribute def"));
            this.Write(
                "model-library/packages/P.md",
                "```sysml\npackage P {\n    attribute X;\n    attribute  def X;\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::X"));

            Assert.That(result.DeclarationLine, Is.EqualTo(4));
        }

        [Test]
        public void Entries_of_a_generated_metamodel_index_resolve_to_their_pages()
        {
            var expected = new DirectoryInfo(Path.Combine(
                (Repository.Layout ?? throw new InvalidOperationException("not run inside the repository")).Root.FullName,
                "tools", "metamodel-gen", "Hypha.MetamodelGen.Tests", "Expected", "metamodel"));
            Copy(expected, this.layout.Metamodel(Tag));

            Assert.Multiple(() =>
            {
                foreach (var name in new[] { "ActionUsage", "Annotations::Annotation", "sysml::systems::actions::actionusage" })
                {
                    var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, name, MaxLines: 5));

                    Assert.That(result.Status, Is.EqualTo(LookupStatus.Found), name);
                    Assert.That(File.Exists(result.Path), Is.True, name);
                    Assert.That(result.Content, Does.StartWith("---"), name);
                }
            });
        }

        [Test]
        public void A_page_reached_through_a_linked_folder_outside_the_artifact_folder_is_refused()
        {
            var outside = Directory.CreateDirectory(Path.Combine(this.root.FullName, "elsewhere"));
            File.WriteAllText(Path.Combine(outside.FullName, "Linked.md"), "outside");
            var linked = this.PathOf("metamodel/linked");

            try
            {
                Directory.CreateSymbolicLink(linked, outside.FullName);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Assert.Ignore($"symbolic links cannot be created here: {exception.Message}");
            }

            this.Write(
                "metamodel/index.json",
                "{ \"entries\": { \"Linked\": { \"kind\": \"class\", \"file\": \"linked/Linked.md\" } } }");

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Linked")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("links outside"));
        }

        [Test]
        public void A_link_that_stays_inside_the_artifact_folder_is_followed()
        {
            var link = this.PathOf("metamodel/elements/Alias.md");

            try
            {
                File.CreateSymbolicLink(link, this.PathOf("metamodel/elements/ActionUsage.md"));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Assert.Ignore($"symbolic links cannot be created here: {exception.Message}");
            }

            this.Write(
                "metamodel/index.json",
                "{ \"entries\": { \"Alias\": { \"kind\": \"class\", \"file\": \"elements/Alias.md\" } } }");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Alias"));

            Assert.That(result.Content, Does.StartWith("ActionUsage line 1"));
        }

        [Test]
        public void A_page_that_cannot_be_read_says_to_try_again()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Ignore("only Windows refuses to read a file another handle holds exclusively");
            }

            using var held = new FileStream(
                this.PathOf("metamodel/elements/ActionUsage.md"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            Assert.That(
                () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "ActionUsage")),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("try again"));
        }

        [Test]
        public void A_declaration_written_with_all_is_found()
        {
            this.WriteLibrary(("P", "package"), ("P::L", "assoc"), ("P::L::source", "feature"), ("P::M::source", "feature"));
            this.Write(
                "model-library/packages/P.md",
                string.Join(
                    '\n',
                    "```kerml",
                    "package P {",
                    "    assoc all L specializes BinaryLink {",
                    "        feature all source : Occurrence;",
                    "    }",
                    "    assoc M {",
                    "        feature source : Occurrence;",
                    "    }",
                    "}",
                    "```",
                    string.Empty));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::L::source"));

            Assert.That(result.DeclarationLine, Is.EqualTo(4));
        }

        [Test]
        public void A_scope_named_by_specializes_before_its_own_declaration_is_not_taken_for_it()
        {
            this.WriteLibrary(("P", "package"), ("P::B", "part def"), ("P::A", "part def"), ("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                string.Join(
                    '\n',
                    "```sysml",
                    "package P {",
                    "    part def B specializes A {",
                    "        attribute x redefines x;",
                    "    }",
                    "    part def A {",
                    "        attribute x;",
                    "    }",
                    "}",
                    "```",
                    string.Empty));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(7));
        }

        [Test]
        public void A_scope_the_index_does_not_have_is_still_not_taken_from_a_specializes()
        {
            // Only the member is indexed, so the scope is matched by name alone.
            this.WriteLibrary(("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                "```sysml\npackage P {\n    part def B specializes A {\n        attribute x;\n    }\n    part def A {\n        attribute x;\n    }\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(7));
        }

        [Test]
        public void A_scope_name_mentioned_without_a_body_is_not_taken_for_the_scope()
        {
            this.WriteLibrary(("P::A::x", "attribute"));
            this.Write(
                "model-library/packages/P.md",
                string.Join(
                    '\n',
                    "```sysml",
                    "package P {",
                    "    private import A::*;",
                    "    attribute x;",
                    "    part def A {",
                    "        attribute x;",
                    "    }",
                    "}",
                    "```",
                    string.Empty));

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::A::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(6));
        }

        [Test]
        public void A_declaration_whose_keyword_differs_from_the_index_is_found_by_its_bare_name()
        {
            this.WriteLibrary(("P::x", "attribute"));
            this.Write("model-library/packages/P.md", "```sysml\npackage P {\n    part other;\n    ref x;\n}\n```\n");

            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.ModelLibrary, "P::x"));

            Assert.That(result.DeclarationLine, Is.EqualTo(4));
        }

        [Test]
        public void A_name_that_ends_other_names_without_a_scope_boundary_is_only_a_candidate()
        {
            // "Usage" ends "PartUsage" and "ActionUsage", but not after "::": neither is an answer.
            var result = this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "Usage"));

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(LookupStatus.Candidates));
                Assert.That(result.Matches, Is.EqualTo(new[] { "ActionUsage", "PartUsage" }));
            });
        }

        [Test]
        public void A_page_that_cannot_be_read_says_to_try_again_on_unix()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Ignore("covered on Windows by locking the file");
                return;
            }

            var page = this.PathOf("metamodel/elements/ActionUsage.md");
            File.SetUnixFileMode(page, UnixFileMode.None);

            try
            {
                if (File.Exists(page) && CanRead(page))
                {
                    Assert.Ignore("running as root, which reads any file");
                }

                Assert.That(
                    () => this.lookup.Lookup(new LookupRequest(KnowledgeKind.Metamodel, "ActionUsage")),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("try again"));
            }
            finally
            {
                File.SetUnixFileMode(page, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }

        /// <summary>Records <paramref name="tags"/> as installed, the first as the default.</summary>
        internal static void Install(IKnowledgeLayout layout, params string[] tags)
        {
            VersionManifestFile.Write(
                VersionManifest.Build(
                    tags[0],
                    tags.Select(tag => new InstalledVersion(
                        tag,
                        new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                        new UpstreamReference(Upstream.PilotRepository, "def")))),
                layout.VersionManifest.FullName);
        }

        private static void Copy(DirectoryInfo source, DirectoryInfo destination)
        {
            foreach (var file in source.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(destination.FullName, Path.GetRelativePath(source.FullName, file.FullName));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                file.CopyTo(target, overwrite: true);
            }
        }

        /// <summary>Replaces the model-library index with these declarations, all on <c>packages/P.md</c>.</summary>
        private void WriteLibrary(params (string Name, string Kind)[] declarations)
        {
            var entries = string.Join(
                ",\n",
                declarations.Select(declaration =>
                    $"\"{declaration.Name}\": {{ \"kind\": \"{declaration.Kind}\", \"file\": \"packages/P.md\", \"source\": \"P.sysml\" }}"));
            this.Write("model-library/index.json", $"{{ \"declarations\": {{ {entries} }} }}");

            if (!File.Exists(this.PathOf("model-library/packages/P.md")))
            {
                this.Write("model-library/packages/P.md", "```sysml\npackage P;\n```\n");
            }
        }

        private static bool CanRead(string path)
        {
            try
            {
                File.ReadAllBytes(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static string Lines(string prefix, int count) =>
            string.Join('\n', Enumerable.Range(1, count).Select(index => $"{prefix} {index}")) + "\n";

        private string PathOf(string relative) =>
            Path.Combine(this.root.FullName, "knowledge", Tag, relative.Replace('/', Path.DirectorySeparatorChar));

        private void Write(string relative, string content)
        {
            var path = this.PathOf(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content.Replace("\r\n", "\n", StringComparison.Ordinal));
        }
    }
}
