// ------------------------------------------------------------------------------------------------
// <copyright file="StructuralQueryTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json.Nodes;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.Query;

    // The structural queries of #209: what can own an element, what can hold a value, what is around a
    // metaclass, packages, effective features, opposite ends, the constraints of one feature and the
    // contributors to a derived union. The jq comparisons are in KnowledgeQueryJqTests.
    [TestFixture]
    public class StructuralQueryTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void A_name_the_release_does_not_have_is_not_found()
        {
            var query = this.fixture.Query;

            Assert.Multiple(() =>
            {
                Assert.That(query.Containers("NoSuchClass").Found, Is.False);
                Assert.That(query.Containers("PartUsage", "NoSuchClass").Found, Is.False);
                Assert.That(query.FeaturesAccepting("NoSuchType").Found, Is.False);
                Assert.That(query.Neighbours("NoSuchClass").Found, Is.False);
                Assert.That(query.Package("NoSuchPackage").Found, Is.False);
                Assert.That(query.EffectiveFeatures("NoSuchClass").Found, Is.False);
                Assert.That(query.Opposite("Type", "noSuchFeature").Found, Is.False);
                Assert.That(query.FeatureConstraints("Type", "noSuchFeature").Found, Is.False);
                Assert.That(query.Contributors("NoSuchClass", "ownedMember").Found, Is.False);
            });
        }

        [Test]
        public void A_blank_name_is_refused()
        {
            var query = this.fixture.Query;

            Assert.Multiple(() =>
            {
                Assert.That(() => query.Containers(" "), Throws.ArgumentException);
                Assert.That(() => query.FeaturesAccepting(""), Throws.ArgumentException);
                Assert.That(() => query.Neighbours(" "), Throws.ArgumentException);
                Assert.That(() => query.Package(" "), Throws.ArgumentException);
                Assert.That(() => query.EffectiveFeatures(" "), Throws.ArgumentException);
                Assert.That(() => query.Opposite("Type", " "), Throws.ArgumentException);
                Assert.That(() => query.FeatureConstraints(" ", "ownedFeature"), Throws.ArgumentException);
                Assert.That(() => query.Contributors("Namespace", " "), Throws.ArgumentException);
            });
        }

        [Test]
        public void Containers_name_each_composite_feature_where_it_is_declared()
        {
            var containers = this.fixture.Query.Containers("PartUsage").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(containers.All(feature => feature.IsComposite), Is.True);
                Assert.That(
                    containers.Select(feature => $"{feature.Class}::{feature.Feature}"),
                    Is.SupersetOf(new[] { "FeatureMembership::ownedMemberFeature", "Relationship::ownedRelatedElement" }));
                Assert.That(
                    containers.Select(feature => $"{feature.Class}::{feature.Feature}"),
                    Is.Unique,
                    "a feature declared once is listed once, not for every metaclass that inherits it");
            });
        }

        [Test]
        public void Within_one_metaclass_a_redefined_inherited_feature_is_left_out()
        {
            // ActorMembership inherits OwningMembership::ownedMemberElement and FeatureMembership's
            // ownedMemberFeature, but each is redefined there, the first by the second and the second by
            // ActorMembership's own ownedActorParameter: only that one, and Relationship's, can own it.
            var everywhere = this.fixture.Query.Containers("PartUsage").Value!.Select(feature => $"{feature.Class}::{feature.Feature}");
            var within = this.fixture.Query.Containers("PartUsage", "ActorMembership").Value!.Select(feature => $"{feature.Class}::{feature.Feature}");

            Assert.Multiple(() =>
            {
                Assert.That(everywhere, Is.SupersetOf(new[] { "OwningMembership::ownedMemberElement", "FeatureMembership::ownedMemberFeature" }));
                Assert.That(within, Is.EqualTo(new[] { "ActorMembership::ownedActorParameter", "Relationship::ownedRelatedElement" }));
            });
        }

        [Test]
        public void A_container_row_carries_the_type_multiplicity_and_kind_of_its_feature()
        {
            Assert.That(
                this.fixture.Query.Containers("PartUsage", "ActorMembership").Value,
                Is.EqualTo(new[]
                {
                    new TypedFeature("ActorMembership", "ownedActorParameter", "PartUsage", 1, 1, true, true),
                    new TypedFeature("Relationship", "ownedRelatedElement", "Element", 0, -1, true, false),
                }));
        }

        [Test]
        public void A_feature_typed_by_an_ancestor_accepts_the_value_and_one_typed_by_a_descendant_does_not()
        {
            var accepting = this.fixture.Query.FeaturesAccepting("Feature").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(accepting.Select(feature => feature.Type), Is.SupersetOf(new[] { "Element", "Feature" }));
                Assert.That(accepting.Select(feature => feature.Type), Has.No.Member("PartUsage"));
                Assert.That(this.fixture.Query.FeaturesAccepting("Boolean").Value!.All(feature => feature.Type == "Boolean"), Is.True);
            });
        }

        [Test]
        public void An_enumeration_is_accepted_only_by_the_features_it_types()
        {
            Assert.That(
                this.fixture.Query.FeaturesAccepting("VisibilityKind").Value!.Select(feature => $"{feature.Class}::{feature.Feature}"),
                Is.EqualTo(new[] { "Expose::visibility", "Import::visibility", "Membership::visibility" }));
        }

        [Test]
        public void Neighbours_point_both_ways_and_leave_out_values_that_are_not_metaclasses()
        {
            var element = this.fixture.Query.Neighbours("Element").Value!;

            Assert.Multiple(() =>
            {
                // Element::owner is typed by Element: it is an outgoing feature, not an incoming one.
                Assert.That(element.Outgoing.Select(feature => feature.Feature), Has.Member("owner"));
                Assert.That(element.Incoming.Select(feature => feature.Class), Has.No.Member("Element"));
                Assert.That(element.Outgoing.Select(feature => feature.Type), Has.No.Member("Boolean").And.No.Member("String"));
                Assert.That(element.Incoming, Is.Not.Empty);
            });
        }

        [Test]
        public void A_package_name_two_packages_share_gives_both_and_a_qualified_name_gives_one()
        {
            var shared = this.fixture.Query.Package("Metadata").Value!;
            var qualified = this.fixture.Query.Package("KerML::Kernel::Metadata").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    shared.Select(package => package.QualifiedName),
                    Is.EqualTo(new[] { "KerML::Kernel::Metadata", "SysML::Systems::Metadata" }));
                Assert.That(qualified.Select(package => package.QualifiedName), Is.EqualTo(new[] { "KerML::Kernel::Metadata" }));
                Assert.That(qualified.Single().AllClasses, Is.EqualTo(qualified.Single().Classes), "the package has no nested packages");
            });
        }

        [Test]
        public void A_package_collects_the_metaclasses_of_its_nested_packages()
        {
            var kerml = this.fixture.Query.Package("KerML").Value!.Single();
            var core = this.fixture.Query.Package("KerML::Core").Value!.Single();

            Assert.Multiple(() =>
            {
                Assert.That(kerml.Classes, Is.Empty, "KerML holds only packages");
                Assert.That(kerml.Packages, Has.Member("KerML::Core").And.No.Member("KerML::Core::Features"));
                Assert.That(kerml.AllClasses, Is.SupersetOf(core.AllClasses));
                Assert.That(core.AllClasses, Is.SupersetOf(new[] { "Feature", "Type" }));
            });
        }

        [Test]
        public void Effective_features_leave_out_the_inherited_ones_the_metaclass_redefines()
        {
            var all = this.fixture.Query.Features("Dependency").Value!.Select(feature => feature.Name).ToList();
            var effective = this.fixture.Query.EffectiveFeatures("Dependency").Value!.Select(feature => feature.Name).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(all, Is.SupersetOf(new[] { "source", "client" }));
                Assert.That(effective, Has.Member("client").And.No.Member("source"), "client redefines source");
                Assert.That(effective, Is.SubsetOf(all));
            });
        }

        [Test]
        public void An_opposite_end_names_the_metaclass_that_declares_the_feature()
        {
            var owned = this.fixture.Query.Opposite("Type", "ownedFeature").Value!;
            var inherited = this.fixture.Query.Opposite("PartUsage", "owningType").Value!;
            var none = this.fixture.Query.Opposite("Element", "isImpliedIncluded").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    owned,
                    Is.EqualTo(new OppositeEnd("Type", "ownedFeature", "Type", new FeatureReference("Feature", null, "owningType"), null)));
                Assert.That(inherited.DeclaredIn, Is.EqualTo("Feature"));
                Assert.That(inherited.Other, Is.EqualTo(new FeatureReference("Type", null, "ownedFeature")));
                Assert.That(inherited.RedefinedBy, Is.Null);
                Assert.That(none.Other, Is.Null, "an attribute with no association has no other end");
            });
        }

        [Test]
        public void An_opposite_end_an_association_owns_names_the_association()
        {
            Assert.That(
                this.fixture.Query.Opposite("AcceptActionUsage", "payloadArgument").Value!.Other,
                Is.EqualTo(new FeatureReference(null, "A_payloadArgument_acceptingActionUsage", "acceptingActionUsage")));
        }

        [Test]
        public void An_opposite_end_of_a_redefined_inherited_feature_names_what_redefines_it()
        {
            var definition = this.fixture.Query.Opposite("PartUsage", "definition").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(definition.DeclaredIn, Is.EqualTo("Usage"));
                Assert.That(definition.RedefinedBy, Is.EqualTo(new FeatureReference("OccurrenceUsage", null, "occurrenceDefinition")));
            });
        }

        [Test]
        public void No_constraint_of_the_releases_so_far_constrains_a_feature()
        {
            // The KerML and SysML constraints name their metaclass, never one of its features.
            Assert.That(this.fixture.Query.FeatureConstraints("Type", "ownedFeature").Value, Is.Empty);
        }

        [Test]
        public void A_constraint_that_names_a_feature_is_found_from_a_metaclass_that_inherits_it()
        {
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var type = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Type")!;
            type["constraints"]![0]!["constrainedElements"]!.AsArray()
                .Add(new JsonObject { ["kind"] = "property", ["owner"] = "Type", ["name"] = "ownedFeature" });
            File.WriteAllText(graph.FullName, document.ToJsonString());

            var expected = (string)type["constraints"]![0]!["name"]!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.FeatureConstraints("Type", "ownedFeature").Value!.Select(member => $"{member.DeclaredIn}::{member.Member.Name}"),
                    Is.EqualTo(new[] { $"Type::{expected}" }));
                Assert.That(
                    this.fixture.Query.FeatureConstraints("Feature", "ownedFeature").Value,
                    Has.Count.EqualTo(1),
                    "Feature inherits Type::ownedFeature, so the same constraint applies");
            });
        }

        [Test]
        public void A_constraint_that_names_a_feature_of_another_owner_or_kind_is_not_found()
        {
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var type = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Type")!;
            var elements = type["constraints"]![0]!["constrainedElements"]!.AsArray();
            elements.Add(new JsonObject { ["kind"] = "property", ["owner"] = "Feature", ["name"] = "ownedFeature" });
            elements.Add(new JsonObject { ["kind"] = "operation", ["owner"] = "Type", ["name"] = "ownedFeature" });
            File.WriteAllText(graph.FullName, document.ToJsonString());

            var result = this.fixture.Query.FeatureConstraints("Type", "ownedFeature");

            Assert.Multiple(() =>
            {
                Assert.That(result.Found, Is.True);
                Assert.That(result.Value, Is.Empty, "Feature::ownedFeature and an operation of that name are not Type::ownedFeature");
            });
        }

        [Test]
        public void Contributors_include_the_features_that_subset_a_contributor()
        {
            var contributors = this.fixture.Query.Contributors("Namespace", "ownedMember").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    contributors.Where(contribution => contribution.Contributes == new FeatureReference("Namespace", null, "ownedMember")),
                    Is.Not.Empty);
                Assert.That(
                    contributors,
                    Has.Member(new Contribution("MultiplicityRange", "lowerBound", "subsets", new FeatureReference("MultiplicityRange", null, "bound"))),
                    "lowerBound subsets bound, which contributes to ownedMember");
                Assert.That(contributors, Is.Unique);
            });
        }

        [Test]
        public void Contributors_include_the_features_that_redefine_a_contributor()
        {
            // enumeratedValue subsets nothing, but redefines variant, which subsets ownedMember.
            var enumeratedValue = new Contribution(
                "EnumerationDefinition", "enumeratedValue", "redefines", new FeatureReference("Definition", null, "variant"));

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Contributors("Definition", "variant").Value, Has.Member(enumeratedValue));
                Assert.That(this.fixture.Query.Contributors("Namespace", "ownedMember").Value, Has.Member(enumeratedValue));
            });
        }

        [Test]
        public void A_feature_nothing_contributes_to_has_no_contributors()
        {
            var result = this.fixture.Query.Contributors("Element", "isImpliedIncluded");

            Assert.Multiple(() =>
            {
                Assert.That(result.Found, Is.True);
                Assert.That(result.Value, Is.Empty);
            });
        }

        [Test]
        public void Contributors_that_lead_back_to_the_feature_are_followed_once()
        {
            // Namespace::ownedMember is made to subset one of its own contributors, a cycle.
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var ownedMember = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Namespace")!["ownedAttributes"]!
                .AsArray().Single(node => (string?)node!["name"] == "ownedMember")!;
            ownedMember["subsets"]!.AsArray()
                .Add(new JsonObject { ["class"] = "MultiplicityRange", ["association"] = null, ["feature"] = "lowerBound" });
            File.WriteAllText(graph.FullName, document.ToJsonString());

            var contributors = this.fixture.Query.Contributors("Namespace", "ownedMember").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    contributors.Count(contribution => contribution is { Class: "Namespace", Feature: "ownedMember" }),
                    Is.EqualTo(1));
                Assert.That(contributors, Is.Unique);
            });
        }

        [Test]
        public void Contributors_of_an_inherited_feature_are_those_of_the_feature_it_is()
        {
            Assert.That(
                this.fixture.Query.Contributors("Package", "ownedMember").Value,
                Is.EqualTo(this.fixture.Query.Contributors("Namespace", "ownedMember").Value));
        }

        [Test]
        public void The_queries_are_grounded_on_the_metamodel_graph()
        {
            var query = this.fixture.Query;
            var graph = $"knowledge/{QueryFixture.Tag}/metamodel/metamodel.json";

            Assert.Multiple(() =>
            {
                Assert.That(query.Containers("PartUsage").Grounding, Is.EqualTo(new Grounding(Provenance.Derived, QueryFixture.Tag, graph)));
                Assert.That(query.FeaturesAccepting("PartUsage").Grounding.Tier, Is.EqualTo(Provenance.Derived));
                Assert.That(query.Neighbours("PartUsage").Grounding, Is.EqualTo(new Grounding(Provenance.Model, QueryFixture.Tag, graph)));
                Assert.That(query.Package("KerML").Grounding.Tier, Is.EqualTo(Provenance.Model));
                Assert.That(query.EffectiveFeatures("PartUsage").Grounding.Tier, Is.EqualTo(Provenance.Derived));
                Assert.That(query.Opposite("Type", "ownedFeature").Grounding.Tier, Is.EqualTo(Provenance.Derived));
                Assert.That(query.FeatureConstraints("Type", "ownedFeature").Grounding.Tier, Is.EqualTo(Provenance.Model));
                Assert.That(
                    query.FeatureConstraints("Feature", "ownedFeature").Grounding.Tier,
                    Is.EqualTo(Provenance.Derived),
                    "Feature inherits ownedFeature, found through the inheritance closure");
                Assert.That(query.Contributors("Namespace", "ownedMember").Grounding.Tier, Is.EqualTo(Provenance.Derived));
            });
        }
    }
}
