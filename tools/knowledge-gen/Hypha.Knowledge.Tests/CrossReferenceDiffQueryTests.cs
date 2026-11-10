// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceDiffQueryTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json.Nodes;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;

    // The cross-reference half of the release diff of #206. The fixture's cross-references are the older
    // release; each test installs an edited copy as the newer one, next to a copy of the metamodel.
    [TestFixture]
    public class CrossReferenceDiffQueryTests
    {
        private const string Newer = "2026-08";

        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp()
        {
            this.fixture = new QueryFixture();
            KnowledgeLookupTests.Install(this.fixture.Layout, QueryFixture.Tag, Newer);
        }

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void Unchanged_cross_references_have_no_changes_and_missing_ones_are_null()
        {
            this.Install(crossReferences: _ => { });
            var unchanged = this.Diff();

            File.Delete(this.fixture.Layout.CrossReferences(Newer).FullName);

            Assert.Multiple(() =>
            {
                Assert.That(unchanged.CrossReferences, Is.Not.Null.And.Empty);
                Assert.That(this.Diff().CrossReferences, Is.Null);
                Assert.That(this.Diff().Metamodel, Is.Empty);
            });
        }

        [Test]
        public void Every_facet_is_compared_edge_by_edge()
        {
            this.Install(
                older: document =>
                {
                    var examples = Entry(document, "PartUsage")["examples"]!.AsArray();
                    examples.Add(Example("examples/Vehicle.md"));
                    examples.Add(Example("examples/Wheel.md"));
                },
                crossReferences: document =>
            {
                var partUsage = Entry(document, "PartUsage");
                var clauses = partUsage["clauses"]!.AsArray();
                clauses.Clear();
                clauses.Add(Clause("sysml2", "9.1", Provenance.Derived, ClauseEdge.ExactTitleMatch));

                partUsage["grammar"]!.AsArray().Single(edge => (string?)edge!["production"] == "PartUsageDeclaration")!["method"] = "production-name";
                partUsage["features"]![0]!["productions"] = new JsonArray("Other", "Identification");

                var examples = partUsage["examples"]!.AsArray();
                examples.Remove(examples.Single(edge => (string?)edge!["file"] == "examples/Wheel.md"));
                examples.Add(Example("examples/Engine.md"));

                Entry(document, "Feature")["clauses"]!.AsArray().Single(edge => (string?)edge!["clause"] == "7.4.2")!["provenance"] = Provenance.Model;
            });

            Assert.That(
                this.Diff().CrossReferences,
                Is.EqualTo(new[]
                {
                    new CrossReferenceChange("Feature", "class", "clause", "kerml 7.4.2", "changed", "provenance", Provenance.Derived, Provenance.Model),
                    new CrossReferenceChange("PartUsage", "class", "clause", "sysml2 8.3.6.3", "removed", null, null, null),
                    new CrossReferenceChange("PartUsage", "class", "clause", "sysml2 9.1", "added", null, null, null),
                    new CrossReferenceChange("PartUsage", "class", "example", "examples/Engine.md", "added", null, null, null),
                    new CrossReferenceChange("PartUsage", "class", "example", "examples/Wheel.md", "removed", null, null, null),
                    new CrossReferenceChange(
                        "PartUsage", "class", "feature", "kerml declaredName =", "changed", "productions", "Identification", "Identification, Other"),
                    new CrossReferenceChange(
                        "PartUsage", "class", "grammar", "sysml PartUsageDeclaration", "changed", "method", "declared-production", "production-name"),
                }));
        }

        [Test]
        public void A_reordered_list_of_productions_is_not_a_change()
        {
            this.Install(crossReferences: document =>
            {
                var declaredName = Entry(document, "Feature")["features"]!.AsArray().Single(edge => (string?)edge!["feature"] == "declaredName")!;
                var productions = declaredName["productions"]!.AsArray().Select(node => (string)node!).Reverse().ToArray();
                declaredName["productions"] = new JsonArray(productions.Select(production => (JsonNode)production).ToArray());
            });

            Assert.That(this.Diff().CrossReferences, Is.Empty);
        }

        [Test]
        public void A_renamed_element_is_compared_under_its_newer_name_and_found_by_either()
        {
            this.Install(
                metamodel: document => document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "PartUsage")!["name"] = "Part",
                crossReferences: document =>
                {
                    var entries = document["entries"]!.AsObject();
                    var entry = entries["PartUsage"]!;
                    entries.Remove("PartUsage");
                    entry["clauses"]!.AsArray().RemoveAt(0);
                    entries["Part"] = entry;
                });

            var expected = new[] { new CrossReferenceChange("Part", "class", "clause", "sysml2 8.3.6.3", "removed", null, null, null) };

            Assert.Multiple(() =>
            {
                Assert.That(this.Diff().CrossReferences, Is.EqualTo(expected));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "PartUsage").Value!.CrossReferences, Is.EqualTo(expected));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "Part").Value!.CrossReferences, Is.EqualTo(expected));
            });
        }

        [Test]
        public void The_edges_of_an_element_added_or_removed_are_not_listed()
        {
            this.Install(crossReferences: document =>
            {
                var entries = document["entries"]!.AsObject();
                entries.Remove("VisibilityKind");
                entries["WheelUsage"] = entries["PartUsage"]!.DeepClone();
            });

            Assert.That(this.Diff().CrossReferences, Is.Empty);
        }

        [Test]
        public void The_changes_to_one_element_are_filtered_like_its_metamodel_changes()
        {
            this.Install(crossReferences: document =>
            {
                Entry(document, "PartUsage")["examples"]!.AsArray().Clear();
                Entry(document, "Feature")["features"]!.AsArray().RemoveAt(0);
            });

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Diff(QueryFixture.Tag, Newer, "Feature").Value!.CrossReferences,
                    Is.EqualTo(new[] { new CrossReferenceChange("Feature", "class", "feature", "kerml declaredName =", "removed", null, null, null) }));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "VisibilityKind").Value!.CrossReferences, Is.Empty);
            });
        }

        [Test]
        public void Cross_references_of_different_schema_versions_are_null_and_the_metamodel_is_still_compared()
        {
            this.Install(
                metamodel: document => document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Element")!["isAbstract"] = false,
                crossReferences: document => document["schemaVersion"] = "9.9.9");

            var diff = this.Diff();

            Assert.Multiple(() =>
            {
                Assert.That(diff.CrossReferences, Is.Null);
                Assert.That(diff.Metamodel.Select(change => $"{change.Element} {change.Field}"), Is.EqualTo(new[] { "Element isAbstract" }));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "NoSuchElement").Found, Is.False);
            });
        }

        [Test]
        public void An_element_in_neither_release_is_not_found_before_the_cross_references_are_read()
        {
            this.Install(crossReferences: _ => { });
            File.WriteAllText(this.fixture.Layout.CrossReferences(Newer).FullName, "{}");

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "NoSuchElement").Found, Is.False);
                Assert.That(() => this.Diff(), Throws.TypeOf<KnowledgeLookupException>());
            });
        }

        [Test]
        public void A_renamed_member_does_not_hide_the_cross_reference_changes_of_its_owner()
        {
            string attribute = null!;

            this.Install(
                metamodel: document =>
                {
                    var owned = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Feature")!["ownedAttributes"]![0]!;
                    attribute = (string)owned["name"]!;
                    owned["name"] = "renamedMember";
                },
                crossReferences: document =>
                    Entry(document, "Feature")["clauses"]!.AsArray().Single(edge => (string?)edge!["clause"] == "7.4.2")!["provenance"] = Provenance.Model);

            var diff = this.Diff();

            Assert.Multiple(() =>
            {
                Assert.That(
                    diff.Metamodel,
                    Is.EqualTo(new[] { new MetamodelChange("Feature", "class", "renamedMember", "attribute", "renamed", "name", attribute, "renamedMember") }));
                Assert.That(
                    diff.CrossReferences,
                    Is.EqualTo(new[] { new CrossReferenceChange("Feature", "class", "clause", "kerml 7.4.2", "changed", "provenance", Provenance.Derived, Provenance.Model) }));
            });
        }

        [Test]
        public void An_element_another_was_renamed_to_is_not_compared_with_the_old_one_of_that_name()
        {
            // A is renamed to B while the old B is removed: the old A is compared with the new B.
            var document = new CrossReferenceBuilder().Build(CrossReferenceBuilderTests.Inputs());
            var a = document.Entries["PartUsage"];
            var b = document.Entries["Feature"];
            var older = document with { Entries = new Dictionary<string, CrossReferenceEntry>(StringComparer.Ordinal) { ["A"] = a, ["B"] = b } };
            var newer = document with { Entries = new Dictionary<string, CrossReferenceEntry>(StringComparer.Ordinal) { ["B"] = a } };
            var renames = new Dictionary<string, string>(StringComparer.Ordinal) { ["A"] = "B" };

            Assert.That(new CrossReferenceDiffer().Compare(older, newer, renames), Is.Empty);
        }

        [Test]
        public void The_differ_needs_both_documents_and_the_renames()
        {
            var document = new CrossReferenceBuilder().Build(CrossReferenceBuilderTests.Inputs());
            var renames = new Dictionary<string, string>(StringComparer.Ordinal);
            var differ = new CrossReferenceDiffer();

            Assert.Multiple(() =>
            {
                Assert.That(() => differ.Compare(null!, document, renames), Throws.ArgumentNullException);
                Assert.That(() => differ.Compare(document, null!, renames), Throws.ArgumentNullException);
                Assert.That(() => differ.Compare(document, document, null!), Throws.ArgumentNullException);
            });
        }

        private static JsonNode Entry(JsonNode document, string name) => document["entries"]![name]!;

        private static JsonObject Clause(string document, string clause, string provenance, string method) =>
            new() { ["document"] = document, ["clause"] = clause, ["provenance"] = provenance, ["method"] = method };

        private static JsonObject Example(string file) =>
            new() { ["file"] = file, ["provenance"] = Provenance.Derived, ["method"] = "curated-front-matter" };

        private ReleaseDiff Diff() => this.fixture.Query.Diff(QueryFixture.Tag, Newer).Value!;

        private void Install(Action<JsonNode>? metamodel = null, Action<JsonNode>? crossReferences = null, Action<JsonNode>? older = null)
        {
            if (older is not null)
            {
                var file = this.fixture.Layout.CrossReferences(QueryFixture.Tag).FullName;
                var original = JsonNode.Parse(File.ReadAllText(file))!;
                older(original);
                File.WriteAllText(file, original.ToJsonString());
            }

            var graph = JsonNode.Parse(File.ReadAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName))!;
            metamodel?.Invoke(graph);
            Directory.CreateDirectory(this.fixture.Layout.Metamodel(Newer).FullName);
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(Newer).FullName, graph.ToJsonString());

            if (crossReferences is not null)
            {
                var document = JsonNode.Parse(File.ReadAllText(this.fixture.Layout.CrossReferences(QueryFixture.Tag).FullName))!;
                crossReferences(document);
                File.WriteAllText(this.fixture.Layout.CrossReferences(Newer).FullName, document.ToJsonString());
            }
        }
    }
}
