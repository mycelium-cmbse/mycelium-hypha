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
