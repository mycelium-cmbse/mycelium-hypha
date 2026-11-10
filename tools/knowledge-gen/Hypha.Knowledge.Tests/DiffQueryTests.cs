// ------------------------------------------------------------------------------------------------
// <copyright file="DiffQueryTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json;
    using System.Text.Json.Nodes;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.Query;

    using Moq;

    // The release diff of #206. The golden metamodel is the older release; each test installs a copy of
    // it with a few known edits as the newer one and expects exactly the changes those edits make.
    [TestFixture]
    public class DiffQueryTests
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
        public void A_release_compared_with_itself_has_no_changes()
        {
            this.Install(_ => { });

            Assert.That(this.Diff().Metamodel, Is.Empty);
        }

        [Test]
        public void Both_releases_are_named_and_the_diff_is_grounded_on_the_newer_one()
        {
            this.Install(document => document["sourceXmiSha256"] = "newer");

            var result = this.fixture.Query.Diff(QueryFixture.Tag, Newer);
            var older = JsonNode.Parse(File.ReadAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName))!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Value!.From,
                    Is.EqualTo(new DiffSide(QueryFixture.Tag, $"knowledge/{QueryFixture.Tag}/metamodel/metamodel.json", (string)older["sourceXmiSha256"]!)));
                Assert.That(result.Value.To, Is.EqualTo(new DiffSide(Newer, $"knowledge/{Newer}/metamodel/metamodel.json", "newer")));
                Assert.That(result.Grounding, Is.EqualTo(new Grounding(Provenance.Derived, Newer, $"knowledge/{Newer}/metamodel/metamodel.json")));
            });
        }

        [Test]
        public void A_metaclass_with_the_same_id_under_another_name_is_renamed()
        {
            this.Install(document => Class(document, "PartUsage")["name"] = "Part");

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[] { new MetamodelChange("Part", "class", null, null, "renamed", "name", "PartUsage", "Part") }));
        }

        [Test]
        public void The_changes_to_one_element_are_found_by_its_name_in_either_release()
        {
            this.Install(document =>
            {
                var partUsage = Class(document, "PartUsage");
                partUsage["name"] = "Part";
                partUsage["isAbstract"] = true;
                Class(document, "Element")["isAbstract"] = false;
            });

            var expected = new[]
            {
                new MetamodelChange("Part", "class", null, null, "changed", "isAbstract", "false", "true"),
                new MetamodelChange("Part", "class", null, null, "renamed", "name", "PartUsage", "Part"),
            };

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "PartUsage").Value!.Metamodel, Is.EqualTo(expected));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "Part").Value!.Metamodel, Is.EqualTo(expected));
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "NoSuchElement").Found, Is.False);
                Assert.That(this.fixture.Query.Diff(QueryFixture.Tag, Newer, "Usage").Value!.Metamodel, Is.Empty);
            });
        }

        [Test]
        public void Metaclasses_enumerations_and_primitive_types_are_added_and_removed()
        {
            this.Install(document =>
            {
                var classes = document["classes"]!.AsArray();
                var copy = Class(document, "PartUsage").DeepClone();
                copy["name"] = "WheelUsage";
                copy["xmiId"] = "new-class";
                classes.Add(copy);
                classes.Remove(Class(document, "JoinNode"));

                var primitives = document["primitiveTypes"]!.AsArray();
                primitives.Remove(primitives.Single(node => (string?)node!["name"] == "Real"));

                var enumerations = document["enumerations"]!.AsArray();
                var kind = Enumeration(document, "VisibilityKind").DeepClone();
                kind["name"] = "AccessKind";
                kind["xmiId"] = "new-enumeration";
                enumerations.Add(kind);
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("AccessKind", "enumeration", null, null, "added", null, null, null),
                    new MetamodelChange("JoinNode", "class", null, null, "removed", null, null, null),
                    new MetamodelChange("Real", "primitiveType", null, null, "removed", null, null, null),
                    new MetamodelChange("WheelUsage", "class", null, null, "added", null, null, null),
                }));
        }

        [Test]
        public void A_metaclass_moved_flipped_respecialized_or_redocumented_is_changed()
        {
            this.Install(document =>
            {
                var feature = Class(document, "Feature");
                feature["qualifiedName"] = "KerML::Core::Moved::Feature";
                feature["isAbstract"] = true;
                feature["extends"] = new JsonArray("Type", "Element");
                feature["documentation"] = "Rewritten.";
                feature["visibility"] = "private";
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("Feature", "class", null, null, "changed", "extends", "Type", "Element, Type"),
                    new MetamodelChange("Feature", "class", null, null, "changed", "isAbstract", "false", "true"),
                    new MetamodelChange("Feature", "class", null, null, "changed", "package", "KerML::Core::Features", "KerML::Core::Moved"),
                    new MetamodelChange("Feature", "class", null, null, "changed", "visibility", "public", "private"),
                    new MetamodelChange("Feature", "class", null, null, "documentation", null, null, null),
                }));
        }

        [Test]
        public void An_attribute_is_added_removed_renamed_and_changed_field_by_field()
        {
            string removed = null!, renamed = null!, changed = null!;

            this.Install(document =>
            {
                var attributes = Class(document, "MultiplicityRange")["ownedAttributes"]!.AsArray();
                removed = (string)attributes[0]!["name"]!;
                renamed = (string)attributes[1]!["name"]!;
                changed = (string)attributes[2]!["name"]!;

                var added = attributes[2]!.DeepClone();
                added["name"] = "step";
                added["xmiId"] = "new-attribute";

                attributes[1]!["name"] = "renamedBound";
                var target = attributes[2]!;
                target["lower"] = 7;
                target["upper"] = 8;
                target["type"] = "Boolean";
                target["isDerived"] = !(bool)target["isDerived"]!;
                target["isDerivedUnion"] = !(bool)target["isDerivedUnion"]!;
                target["isComposite"] = !(bool)target["isComposite"]!;
                target["isOrdered"] = !(bool)target["isOrdered"]!;
                target["isUnique"] = !(bool)target["isUnique"]!;
                target["isReadOnly"] = !(bool)target["isReadOnly"]!;
                target["defaultValue"] = "true";
                target["visibility"] = "private";
                target["redefines"] = new JsonArray(Reference("Element", "name"));
                target["subsets"] = new JsonArray(Reference("Element", "owner"), Reference("Element", "documentation"));
                target["opposite"] = Reference("Element", "owner");
                target["documentation"] = "Rewritten.";

                attributes.RemoveAt(0);
                attributes.Add(added);
            });

            var older = Class(JsonNode.Parse(File.ReadAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName))!, "MultiplicityRange")
                ["ownedAttributes"]![2]!;

            string? Value(string field) => older[field] is JsonValue value
                ? value.GetValueKind() switch
                {
                    System.Text.Json.JsonValueKind.True => "true",
                    System.Text.Json.JsonValueKind.False => "false",
                    _ => value.ToString(),
                }
                : null;

            string? References(string field) => older[field]!.AsArray().Count == 0
                ? null
                : string.Join(", ", older[field]!.AsArray().Select(node => $"{(string?)node!["class"] ?? (string?)node["association"]}::{node["feature"]}").Order(StringComparer.Ordinal));

            var opposite = older["opposite"] is JsonObject end ? $"{(string?)end["class"] ?? (string?)end["association"]}::{end["feature"]}" : null;

            MetamodelChange Changed(string field, string? from, string? to) =>
                new("MultiplicityRange", "class", changed, "attribute", "changed", field, from, to);

            var expected = new[]
                {
                    new MetamodelChange("MultiplicityRange", "class", removed, "attribute", "removed", null, null, null),
                    new MetamodelChange("MultiplicityRange", "class", "renamedBound", "attribute", "renamed", "name", renamed, "renamedBound"),
                    new MetamodelChange("MultiplicityRange", "class", "step", "attribute", "added", null, null, null),
                    Changed("defaultValue", Value("defaultValue"), "true"),
                    Changed("isComposite", Value("isComposite"), Value("isComposite") == "true" ? "false" : "true"),
                    Changed("isDerived", Value("isDerived"), Value("isDerived") == "true" ? "false" : "true"),
                    Changed("isDerivedUnion", Value("isDerivedUnion"), Value("isDerivedUnion") == "true" ? "false" : "true"),
                    Changed("isOrdered", Value("isOrdered"), Value("isOrdered") == "true" ? "false" : "true"),
                    Changed("isReadOnly", Value("isReadOnly"), Value("isReadOnly") == "true" ? "false" : "true"),
                    Changed("isUnique", Value("isUnique"), Value("isUnique") == "true" ? "false" : "true"),
                    Changed("lower", Value("lower"), "7"),
                    Changed("opposite", opposite, "Element::owner"),
                    Changed("redefines", References("redefines"), "Element::name"),
                    Changed("subsets", References("subsets"), "Element::documentation, Element::owner"),
                    Changed("type", Value("type"), "Boolean"),
                    Changed("upper", Value("upper"), "8"),
                    Changed("visibility", Value("visibility"), "private"),
                    new MetamodelChange("MultiplicityRange", "class", changed, "attribute", "documentation", null, null, null),
                }
                .Where(change => change.Kind != "changed" || change.From != change.To)
                .OrderBy(change => change.Member, StringComparer.Ordinal)
                .ThenBy(change => change.Kind, StringComparer.Ordinal)
                .ThenBy(change => change.Field ?? string.Empty, StringComparer.Ordinal)
                .ToList();

            Assert.That(this.Diff().Metamodel, Is.EqualTo(expected));
        }

        [Test]
        public void An_operation_is_changed_and_its_parameters_are_compared_by_id()
        {
            this.Install(document =>
            {
                var operation = Class(document, "MultiplicityRange")["ownedOperations"]!.AsArray()
                    .Single(node => (string?)node!["name"] == "hasBounds")!;
                operation["body"] = "true";
                operation["isQuery"] = !(bool)operation["isQuery"]!;

                // The two parameters swap places, and the second is renamed.
                var parameters = operation["parameters"]!.AsArray();
                var lower = parameters[0]!.DeepClone();
                var upper = parameters[1]!.DeepClone();
                upper["name"] = "top";
                parameters.Clear();
                parameters.Add(upper);
                parameters.Add(lower);
            });

            var changes = this.Diff().Metamodel;

            Assert.Multiple(() =>
            {
                Assert.That(
                    changes.Where(change => change.MemberKind == "operation").Select(change => $"{change.Member} {change.Kind} {change.Field}"),
                    Is.EqualTo(new[] { "hasBounds changed body", "hasBounds changed isQuery" }));
                Assert.That(
                    changes.Where(change => change.MemberKind == "parameter"),
                    Is.EqualTo(new[]
                    {
                        new MetamodelChange("MultiplicityRange", "class", "hasBounds(lower)", "parameter", "changed", "position", "0", "1"),
                        new MetamodelChange("MultiplicityRange", "class", "hasBounds(top)", "parameter", "changed", "position", "1", "0"),
                        new MetamodelChange("MultiplicityRange", "class", "hasBounds(top)", "parameter", "renamed", "name", "upper", "top"),
                    }));
            });
        }

        [Test]
        public void A_parameter_is_changed_field_by_field()
        {
            this.Install(document =>
            {
                var lower = HasBounds(document)["parameters"]![0]!;
                lower["type"] = "Boolean";
                lower["direction"] = "out";
                lower["lower"] = 0;
                lower["upper"] = -1;
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("MultiplicityRange", "class", "hasBounds(lower)", "parameter", "changed", "direction", "in", "out"),
                    new MetamodelChange("MultiplicityRange", "class", "hasBounds(lower)", "parameter", "changed", "lower", "1", "0"),
                    new MetamodelChange("MultiplicityRange", "class", "hasBounds(lower)", "parameter", "changed", "type", "Integer", "Boolean"),
                    new MetamodelChange("MultiplicityRange", "class", "hasBounds(lower)", "parameter", "changed", "upper", "1", "-1"),
                }));
        }

        [Test]
        public void Operations_and_parameters_are_added_removed_and_renamed_under_the_newer_operation_name()
        {
            this.Install(document =>
            {
                var operations = Class(document, "MultiplicityRange")["ownedOperations"]!.AsArray();
                var hasBounds = HasBounds(document);

                // A new operation lists no parameter changes of its own, and nor does a removed one.
                var fresh = hasBounds.DeepClone();
                fresh["name"] = "fresh";
                fresh["xmiId"] = "new-operation";
                fresh["parameters"]![0]!["xmiId"] = "new-parameter-1";
                fresh["parameters"]![1]!["xmiId"] = "new-parameter-2";
                operations.Remove(hasBounds);
                operations.Add(fresh);

                var valueOf = operations.Single(node => (string?)node!["name"] == "valueOf")!;
                valueOf["name"] = "evaluate";
                var parameters = valueOf["parameters"]!.AsArray();
                var extra = parameters[0]!.DeepClone();
                extra["name"] = "extra";
                extra["xmiId"] = "new-parameter";
                parameters.Clear();
                parameters.Add(extra);
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("MultiplicityRange", "class", "evaluate", "operation", "renamed", "name", "valueOf", "evaluate"),
                    new MetamodelChange("MultiplicityRange", "class", "evaluate(bound)", "parameter", "removed", null, null, null),
                    new MetamodelChange("MultiplicityRange", "class", "evaluate(extra)", "parameter", "added", null, null, null),
                    new MetamodelChange("MultiplicityRange", "class", "fresh", "operation", "added", null, null, null),
                    new MetamodelChange("MultiplicityRange", "class", "hasBounds", "operation", "removed", null, null, null),
                }));
        }

        [Test]
        public void An_operation_return_redefinition_and_documentation_are_compared()
        {
            this.Install(document =>
            {
                var valueOf = Class(document, "MultiplicityRange")["ownedOperations"]!.AsArray()
                    .Single(node => (string?)node!["name"] == "valueOf")!;
                valueOf["returnType"] = "Integer";
                valueOf["returnLower"] = 1;
                valueOf["returnUpper"] = -1;
                valueOf["redefines"] = new JsonArray(new JsonObject { ["class"] = "Element", ["operation"] = "effectiveName" });
                valueOf["documentation"] = "Rewritten.";
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("MultiplicityRange", "class", "valueOf", "operation", "changed", "redefines", null, "Element::effectiveName"),
                    new MetamodelChange("MultiplicityRange", "class", "valueOf", "operation", "changed", "returnLower", "0", "1"),
                    new MetamodelChange("MultiplicityRange", "class", "valueOf", "operation", "changed", "returnType", "UnlimitedNatural", "Integer"),
                    new MetamodelChange("MultiplicityRange", "class", "valueOf", "operation", "changed", "returnUpper", "1", "-1"),
                    new MetamodelChange("MultiplicityRange", "class", "valueOf", "operation", "documentation", null, null, null),
                }));
        }

        [Test]
        public void Constraints_are_added_removed_renamed_and_redocumented()
        {
            this.Install(document =>
            {
                var constraints = Class(document, "PartUsage")["constraints"]!.AsArray();
                constraints[1]!["name"] = "checkPartUsageGeneralization";
                constraints[2]!["documentation"] = "Rewritten.";

                var added = constraints[3]!.DeepClone();
                added["name"] = "checkPartUsageNew";
                added["xmiId"] = "new-constraint";
                constraints.Add(added);
                constraints.RemoveAt(0);
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("PartUsage", "class", "checkPartUsageActorSpecialization", "constraint", "removed", null, null, null),
                    new MetamodelChange(
                        "PartUsage", "class", "checkPartUsageGeneralization", "constraint", "renamed", "name", "checkPartUsageSpecialization", "checkPartUsageGeneralization"),
                    new MetamodelChange("PartUsage", "class", "checkPartUsageNew", "constraint", "added", null, null, null),
                    new MetamodelChange("PartUsage", "class", "checkPartUsageStakeholderSpecialization", "constraint", "documentation", null, null, null),
                }));
        }

        [Test]
        public void A_member_is_matched_on_its_id_before_its_name()
        {
            // lowerBound is renamed to minimum, and a new member with another id takes the old name.
            this.Install(document =>
            {
                var attributes = Class(document, "MultiplicityRange")["ownedAttributes"]!.AsArray();
                var lowerBound = attributes.Single(node => (string?)node!["name"] == "lowerBound")!;
                var replacement = lowerBound.DeepClone();
                replacement["xmiId"] = "new-attribute";
                lowerBound["name"] = "minimum";
                attributes.Add(replacement);
            });

            Assert.That(
                this.Diff().Metamodel,
                Is.EqualTo(new[]
                {
                    new MetamodelChange("MultiplicityRange", "class", "lowerBound", "attribute", "added", null, null, null),
                    new MetamodelChange("MultiplicityRange", "class", "minimum", "attribute", "renamed", "name", "lowerBound", "minimum"),
                }));
        }

        [Test]
        public void Members_without_an_id_are_matched_on_their_names_and_never_on_the_empty_id()
        {
            static void WithoutIds(JsonNode document)
            {
                foreach (var attribute in Class(document, "MultiplicityRange")["ownedAttributes"]!.AsArray().Take(2))
                {
                    attribute!["xmiId"] = string.Empty;
                }
            }

            var older = this.Older();
            WithoutIds(older);
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, older.ToJsonString());

            this.Install(document =>
            {
                WithoutIds(document);
                var attributes = Class(document, "MultiplicityRange")["ownedAttributes"]!.AsArray();
                var first = attributes[0]!;
                attributes.RemoveAt(0);
                attributes.Insert(1, first);
            });

            Assert.That(this.Diff().Metamodel, Is.Empty);
        }

        [Test]
        public void The_changes_to_an_enumeration_or_a_primitive_type_are_found_by_its_name()
        {
            this.Install(document =>
            {
                var primitives = document["primitiveTypes"]!.AsArray();
                primitives.Remove(primitives.Single(node => (string?)node!["name"] == "Real"));
                Enumeration(document, "VisibilityKind")["literals"]![0]!["documentation"] = "Rewritten.";
                Class(document, "Element")["isAbstract"] = false;
            });

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Diff(QueryFixture.Tag, Newer, "Real").Value!.Metamodel,
                    Is.EqualTo(new[] { new MetamodelChange("Real", "primitiveType", null, null, "removed", null, null, null) }));
                Assert.That(
                    this.fixture.Query.Diff(QueryFixture.Tag, Newer, "VisibilityKind").Value!.Metamodel,
                    Is.EqualTo(new[] { new MetamodelChange("VisibilityKind", "enumeration", "private", "literal", "documentation", null, null, null) }));
            });
        }

        [Test]
        public void The_reverse_diff_reports_a_removal_as_an_addition()
        {
            this.Install(document => document["classes"]!.AsArray().Remove(Class(document, "JoinNode")));

            var reverse = this.fixture.Query.Diff(Newer, QueryFixture.Tag).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(reverse.From.Release, Is.EqualTo(Newer));
                Assert.That(reverse.To.Release, Is.EqualTo(QueryFixture.Tag));
                Assert.That(
                    reverse.Metamodel,
                    Is.EqualTo(new[] { new MetamodelChange("JoinNode", "class", null, null, "added", null, null, null) }));
            });
        }

        [Test]
        public void A_constraint_and_a_literal_are_compared()
        {
            this.Install(document =>
            {
                var constraint = Class(document, "PartUsage")["constraints"]![0]!;
                constraint["ocl"] = "true";
                constraint["constrainedElements"] = new JsonArray(
                    new JsonObject { ["kind"] = "property", ["owner"] = "PartUsage", ["name"] = "partDefinition" });

                var literals = Enumeration(document, "VisibilityKind")["literals"]!.AsArray();
                literals.RemoveAt(1);
                literals[0]!["documentation"] = "Rewritten.";
                literals.Add(new JsonObject { ["name"] = "package", ["xmiId"] = "new-literal", ["documentation"] = string.Empty });
            });

            var constraint = (string)Class(this.Older(), "PartUsage")["constraints"]![0]!["name"]!;
            var changes = this.Diff().Metamodel;

            Assert.Multiple(() =>
            {
                Assert.That(
                    changes.Where(change => change.Element == "PartUsage").Select(change => $"{change.Member} {change.Kind} {change.Field} {change.To}"),
                    Is.EqualTo(new[] { $"{constraint} changed constrainedElements property PartUsage::partDefinition", $"{constraint} changed ocl true" }));
                Assert.That(
                    changes.Where(change => change.Element == "VisibilityKind"),
                    Is.EqualTo(new[]
                    {
                        new MetamodelChange("VisibilityKind", "enumeration", "package", "literal", "added", null, null, null),
                        new MetamodelChange("VisibilityKind", "enumeration", "private", "literal", "documentation", null, null, null),
                        new MetamodelChange("VisibilityKind", "enumeration", "protected", "literal", "removed", null, null, null),
                    }));
            });
        }

        [Test]
        public void An_element_whose_id_changed_is_matched_on_its_name()
        {
            this.Install(document =>
            {
                Class(document, "PartUsage")["xmiId"] = "regenerated";
                Class(document, "PartUsage")["ownedAttributes"]![0]!["xmiId"] = "regenerated-attribute";
            });

            Assert.That(this.Diff().Metamodel, Is.Empty);
        }

        [Test]
        public void Inherited_members_are_not_compared()
        {
            this.Install(document => Class(document, "PartUsage")["inheritedAttributes"]![0]!["lower"] = 5);

            Assert.That(this.Diff().Metamodel, Is.Empty);
        }

        [Test]
        public void Releases_of_different_schema_versions_are_refused()
        {
            this.Install(document => document["schemaVersion"] = "9.9.9");

            Assert.That(
                () => this.Diff(),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("9.9.9").And.Message.Contains($"hypha generate --tag {Newer}"));
        }

        [Test]
        public void A_release_that_is_not_installed_is_refused_and_blank_arguments_are_refused()
        {
            var query = this.fixture.Query;

            Assert.Multiple(() =>
            {
                Assert.That(() => query.Diff(QueryFixture.Tag, "2020-01"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => query.Diff("2020-01", Newer), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => query.Diff(null!, Newer), Throws.ArgumentNullException);
                Assert.That(() => query.Diff(QueryFixture.Tag, null!), Throws.ArgumentNullException);
                Assert.That(() => query.Diff(" ", Newer), Throws.ArgumentException);
                Assert.That(() => query.Diff(QueryFixture.Tag, ""), Throws.ArgumentException);
                Assert.That(() => query.Diff(QueryFixture.Tag, Newer, " "), Throws.ArgumentException);
            });
        }

        [Test]
        public void The_query_compares_through_the_differ_it_is_given()
        {
            this.Install(_ => { });
            var change = new MetamodelChange("Stub", "class", null, null, MetamodelChange.Added, null, null, null);
            var differ = new Mock<IMetamodelDiffer>();
            differ.Setup(mock => mock.Compare(It.IsAny<MetamodelDocument>(), It.IsAny<MetamodelDocument>())).Returns(new[] { change });
            var query = new KnowledgeQuery(
                this.fixture.Layout, this.fixture.ApiLayout, this.fixture.Documents, new SpecificationReader(this.fixture.Documents), differ.Object, new CrossReferenceDiffer());

            Assert.That(query.Diff(QueryFixture.Tag, Newer).Value!.Metamodel, Is.EqualTo(new[] { change }));
            differ.Verify(mock => mock.Compare(It.IsAny<MetamodelDocument>(), It.IsAny<MetamodelDocument>()), Times.Once);
        }

        [Test]
        public void The_differ_needs_both_documents()
        {
            var document = JsonSerializer.Deserialize(
                File.ReadAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName),
                MetamodelJsonContext.Default.MetamodelDocument)!;
            var differ = new MetamodelDiffer();

            Assert.Multiple(() =>
            {
                Assert.That(() => differ.Compare(null!, document), Throws.ArgumentNullException);
                Assert.That(() => differ.Compare(document, null!), Throws.ArgumentNullException);
            });
        }

        [Test]
        public void A_metamodel_without_member_ids_is_refused_and_says_to_regenerate()
        {
            // A release generated before schema 2.1.0 has no XMI id on its members.
            this.Install(document => Class(document, "PartUsage")["ownedAttributes"]![0]!.AsObject().Remove("xmiId"));

            Assert.That(
                () => this.Diff(),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains($"hypha generate --tag {Newer}"));
        }

        private static JsonNode Class(JsonNode document, string name) =>
            document["classes"]!.AsArray().Single(node => (string?)node!["name"] == name)!;

        private static JsonNode HasBounds(JsonNode document) =>
            Class(document, "MultiplicityRange")["ownedOperations"]!.AsArray().Single(node => (string?)node!["name"] == "hasBounds")!;

        private static JsonNode Enumeration(JsonNode document, string name) =>
            document["enumerations"]!.AsArray().Single(node => (string?)node!["name"] == name)!;

        private static JsonObject Reference(string owner, string feature) =>
            new() { ["class"] = owner, ["association"] = null, ["feature"] = feature };

        private ReleaseDiff Diff() => this.fixture.Query.Diff(QueryFixture.Tag, Newer).Value!;

        private JsonNode Older() => JsonNode.Parse(File.ReadAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName))!;

        private void Install(Action<JsonNode> edit)
        {
            var document = this.Older();
            edit(document);

            Directory.CreateDirectory(this.fixture.Layout.Metamodel(Newer).FullName);
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(Newer).FullName, document.ToJsonString());
        }
    }
}
