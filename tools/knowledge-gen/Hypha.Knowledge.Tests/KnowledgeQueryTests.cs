// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeQueryTests.cs" company="Starion Group S.A.">
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
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.Query;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    [TestFixture]
    public class KnowledgeQueryTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void The_metamodel_reads_back_through_the_default_contract()
        {
            // The camel-case naming policy belongs to the contract, not only to the generator's options.
            var golden = QueryFixture.GoldenMetamodel();

            var graph = JsonSerializer.Deserialize(
                File.ReadAllText(Path.Combine(golden.FullName, "metamodel.json")),
                MetamodelJsonContext.Default.MetamodelDocument)!;
            var index = JsonSerializer.Deserialize(
                File.ReadAllText(Path.Combine(golden.FullName, "index.json")),
                MetamodelJsonContext.Default.MetamodelLookup)!;

            Assert.Multiple(() =>
            {
                Assert.That(graph.Metamodel, Is.Not.Empty);
                Assert.That(graph.Classes, Has.Count.EqualTo(graph.Counts.Classes).And.Not.Empty);
                Assert.That(graph.Classes.All(node => node.OwnedAttributes is not null && node.AllAncestors is not null), Is.True);
                Assert.That(index.Entries, Does.ContainKey("PartUsage"));
                Assert.That(index.Entries["PartUsage"].QualifiedName, Is.EqualTo("SysML::Systems::Parts::PartUsage"));
            });
        }

        [Test]
        public void A_metaclass_the_release_does_not_have_is_not_found()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Subclasses("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Features("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Element("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Ancestors("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Specializes("NoSuchClass", "Element").Found, Is.False);
                Assert.That(this.fixture.Query.Specializes("PartUsage", "NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Constraints("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Operations("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Literals("NoSuchKind").Found, Is.False);
                Assert.That(this.fixture.Query.Literals("PartUsage").Found, Is.False, "a metaclass is not an enumeration");
                Assert.That(this.fixture.Query.CrossReference("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Query.Declaration("NoSuch::name").Found, Is.False);
                Assert.That(this.fixture.Api.PimClass("NoSuchClass").Found, Is.False);
                Assert.That(this.fixture.Api.Type("NoSuchType").Found, Is.False);
                Assert.That(this.fixture.Api.Operation("NoSuchService.op").Found, Is.False);
            });
        }

        [Test]
        public void A_query_that_finds_its_element_but_nothing_else_returns_an_empty_list()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Features("PartUsage", "noSuchFeature").Value, Is.Empty);
                Assert.That(this.fixture.Query.ClassesWithFeatureTyped("NoSuchType").Value, Is.Empty);
                Assert.That(this.fixture.Query.Redefinitions("noSuchFeature").Value, Is.Empty);
                Assert.That(this.fixture.Query.FeatureAssignments(feature: "noSuchFeature").Value, Is.Empty);
                Assert.That(this.fixture.Query.Subsettings("noSuchFeature").Value, Is.Empty);
                Assert.That(this.fixture.Query.BuiltBy("NoSuchProduction").Value, Is.Empty);
                Assert.That(this.fixture.Query.BuiltBy("PartUsage", grammar: "kerml").Value, Is.Empty);
                Assert.That(this.fixture.Query.ElementsInClause("kerml", "99").Value, Is.Empty);
                Assert.That(this.fixture.Query.Specializes("PartUsage", "PartDefinition").Value!.Path, Is.Empty);
                Assert.That(this.fixture.Query.Ancestors("Element").Value, Is.Empty);
            });
        }

        [Test]
        public void Subclasses_include_abstract_ones_unless_asked_not_to()
        {
            var all = this.fixture.Query.Subclasses("Usage").Value!;
            var concrete = this.fixture.Query.Subclasses("Usage", concreteOnly: true).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(concrete, Is.SubsetOf(all));
                Assert.That(all, Has.Count.GreaterThan(concrete.Count));
                Assert.That(all, Does.Not.Contain("Usage"), "a metaclass is not its own subclass");
                Assert.That(all, Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal));
            });
        }

        [Test]
        public void The_second_argument_of_redefinitions_and_subsettings_is_still_the_release()
        {
            // The owner filter came after tag, so a call written before it still names the release.
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Redefinitions("definition", QueryFixture.Tag).Value,
                    Is.EqualTo(this.fixture.Query.Redefinitions("definition").Value).And.Not.Empty);
                Assert.That(
                    this.fixture.Query.Subsettings("usage", QueryFixture.Tag).Value,
                    Is.EqualTo(this.fixture.Query.Subsettings("usage").Value).And.Not.Empty);
                Assert.That(
                    () => this.fixture.Query.Redefinitions("definition", "2020-01"),
                    Throws.TypeOf<KnowledgeLookupException>());
            });
        }

        [Test]
        public void A_metaclass_specializes_itself_with_a_chain_of_one()
        {
            var answer = this.fixture.Query.Specializes("PartUsage", "PartUsage").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(answer.Specializes, Is.True);
                Assert.That(answer.Path, Is.EqualTo(new[] { "PartUsage" }));
            });
        }

        [Test]
        public void Whether_a_metaclass_specializes_another_is_the_generated_closure()
        {
            Assert.Multiple(() =>
            {
                var yes = this.fixture.Query.Specializes("PartUsage", "Element").Value!;
                Assert.That(yes.Specializes, Is.True);
                Assert.That(yes.Path[0], Is.EqualTo("PartUsage"));
                Assert.That(yes.Path[^1], Is.EqualTo("Element"));

                var no = this.fixture.Query.Specializes("Element", "PartUsage").Value!;
                Assert.That(no.Specializes, Is.False);
                Assert.That(no.Path, Is.Empty);
            });
        }

        [Test]
        public void An_inherited_feature_says_which_feature_redefines_it()
        {
            var features = this.fixture.Query.Features("Dependency").Value!.ToDictionary(feature => feature.Name);

            Assert.Multiple(() =>
            {
                Assert.That(features["source"].InheritedFrom, Is.EqualTo("Relationship"));
                Assert.That(features["source"].RedefinedBy, Is.EqualTo(new FeatureReference("Dependency", null, "client")));
                Assert.That(features["client"].Redefines, Is.EqualTo(new[] { new FeatureReference("Relationship", null, "source") }));
                Assert.That(features["client"].RedefinedBy, Is.Null, "an owned feature is not redefined in its own context");
            });
        }

        [Test]
        public void A_feature_says_what_its_opposite_end_is()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.fixture.Query.Features("Type", "ownedFeature").Value!.Single().Opposite,
                    Is.EqualTo(new FeatureReference("Feature", null, "owningType")));
                Assert.That(this.fixture.Query.Features("Element", "isImpliedIncluded").Value!.Single().Opposite, Is.Null);
            });
        }

        [Test]
        public void A_metamodel_that_names_an_inherited_member_its_metaclass_does_not_own_is_refused()
        {
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var partUsage = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "PartUsage")!;
            partUsage["inheritedConstraints"]![0]!["constraint"] = "noSuchConstraint";
            File.WriteAllText(graph.FullName, document.ToJsonString());

            Assert.That(
                () => this.fixture.Query.Constraints("PartUsage"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("inconsistent"));
        }

        [Test]
        public void A_metamodel_that_names_an_inherited_operation_of_a_metaclass_it_lacks_is_refused()
        {
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var usage = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "ConstraintUsage")!;
            usage["inheritedOperations"]!.AsArray().First(node => node!["redefinedBy"] is null)!["class"] = "NoSuchClass";
            File.WriteAllText(graph.FullName, document.ToJsonString());

            Assert.That(
                () => this.fixture.Query.Operations("ConstraintUsage"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("inconsistent"));
        }

        [Test]
        public void An_owner_that_declares_no_such_feature_gives_no_redefinitions_or_subsettings()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Redefinitions("type", owner: "PartUsage").Value, Is.Empty);
                Assert.That(this.fixture.Query.Subsettings("usage", owner: "NoSuchClass").Value, Is.Empty);
            });
        }

        [Test]
        public void A_feature_that_redefines_two_features_of_one_name_gives_one_redefinition_for_each()
        {
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            var dependency = document["classes"]!.AsArray().Single(node => (string?)node!["name"] == "Dependency")!;
            dependency["ownedAttributes"]!.AsArray().Single(node => (string?)node!["name"] == "client")!["redefines"]!.AsArray()
                .Add(new JsonObject { ["class"] = "Element", ["association"] = null, ["feature"] = "source" });
            File.WriteAllText(graph.FullName, document.ToJsonString());

            var client = this.fixture.Query.Redefinitions("source").Value!
                .Where(redefinition => redefinition is { Class: "Dependency", Feature: "client" })
                .Select(redefinition => redefinition.Redefines.Class)
                .ToList();

            Assert.That(client, Is.EquivalentTo(new[] { "Relationship", "Element" }));
        }

        [Test]
        public void A_metamodel_in_the_previous_format_is_refused_and_says_to_regenerate()
        {
            // A release generated before schema 2.0.0 has no inherited operations or constraints.
            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);
            var document = JsonNode.Parse(File.ReadAllText(graph.FullName))!;
            foreach (var node in document["classes"]!.AsArray())
            {
                node!.AsObject().Remove("inheritedOperations");
                node.AsObject().Remove("inheritedConstraints");
            }

            File.WriteAllText(graph.FullName, document.ToJsonString());

            Assert.That(
                () => this.fixture.Query.Subclasses("Usage"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains($"hypha generate --tag {QueryFixture.Tag}"));
        }

        [Test]
        public void A_metaclass_declares_its_own_constraints_before_those_it_inherits()
        {
            var all = this.fixture.Query.Constraints("PartUsage").Value!;
            var own = this.fixture.Query.Constraints("PartUsage", includeInherited: false).Value!;

            Assert.Multiple(() =>
            {
                Assert.That(all.Take(own.Count), Is.EqualTo(own));
                Assert.That(all.Count, Is.GreaterThan(own.Count), "PartUsage inherits constraints from Usage and Feature");
                Assert.That(all.Skip(own.Count).Select(constraint => constraint.DeclaredIn), Has.None.EqualTo("PartUsage"));
            });
        }

        [Test]
        public void A_clause_is_not_a_prefix_match()
        {
            // 7.4 includes 7.4.2 as a subclause, but 7.4 itself is not 7.4.2, and 7 is not 7.4.
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.ElementsInClause("kerml", "7.4").Value, Is.Empty);
                Assert.That(this.fixture.Query.ElementsInClause("kerml", "7.4", includeSubclauses: true).Value, Is.Not.Empty);
                Assert.That(this.fixture.Query.ElementsInClause("kerml", "7.4.", includeSubclauses: true).Value, Is.Empty);
                Assert.That(this.fixture.Query.ElementsInClause("sysml2", "7.4.2").Value, Is.Empty, "the document must match too");
            });
        }

        [Test]
        public void Owned_features_come_first_and_name_no_origin()
        {
            var features = this.fixture.Query.Features("PartUsage").Value!;
            var firstInherited = features.ToList().FindIndex(feature => feature.InheritedFrom is not null);

            Assert.Multiple(() =>
            {
                Assert.That(firstInherited, Is.GreaterThan(0));
                Assert.That(features.Skip(firstInherited).All(feature => feature.InheritedFrom is not null), Is.True);
            });
        }

        [Test]
        public void Every_result_says_where_it_comes_from()
        {
            var graph = $"knowledge/{QueryFixture.Tag}/metamodel/metamodel.json";
            var index = $"knowledge/{QueryFixture.Tag}/metamodel/index.json";
            var crossReferences = $"knowledge/{QueryFixture.Tag}/cross-references.json";
            var library = $"knowledge/{QueryFixture.Tag}/model-library/index.json";
            var pim = $"knowledge/api/{QueryFixture.ApiVersion}/pim/metamodel.json";
            var apiCrossReferences = $"knowledge/api/{QueryFixture.ApiVersion}/cross-references.json";

            var query = this.fixture.Query;
            var api = this.fixture.Api;

            Assert.Multiple(() =>
            {
                Assert.That(query.Subclasses("Usage").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(query.ClassesWithFeatureTyped("Expression").Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.Features("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(query.Redefinitions("definition").Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.Element("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Model, index)));
                Assert.That(query.CrossReference("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, crossReferences)));
                Assert.That(query.FeatureAssignments().Grounding, Is.EqualTo(Release(Provenance.Derived, crossReferences)));
                Assert.That(query.Declaration("ISQBase::mass").Grounding, Is.EqualTo(Release(Provenance.Derived, library)));
                Assert.That(query.Ancestors("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(query.Specializes("PartUsage", "Element").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(query.Constraints("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(
                    query.Constraints("PartUsage", includeInherited: false).Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.Operations("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, graph)));
                Assert.That(
                    query.Operations("PartUsage", includeInherited: false).Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.Literals("VisibilityKind").Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.Subsettings("usage").Grounding, Is.EqualTo(Release(Provenance.Model, graph)));
                Assert.That(query.BuiltBy("PartUsage").Grounding, Is.EqualTo(Release(Provenance.Derived, crossReferences)));
                Assert.That(query.ElementsInClause("kerml", "7.4.2").Grounding, Is.EqualTo(Release(Provenance.Derived, crossReferences)));
                Assert.That(api.PimClass("PartUsage").Grounding, Is.EqualTo(Api(Provenance.Model, pim)));
                Assert.That(api.Type("Commit").Grounding, Is.EqualTo(Api(Provenance.Derived, apiCrossReferences)));
                Assert.That(
                    api.Operation("ProjectService.createProject").Grounding,
                    Is.EqualTo(Api(Provenance.Derived, apiCrossReferences)));
            });

            static Grounding Release(string tier, string source) => new(tier, QueryFixture.Tag, source);

            static Grounding Api(string tier, string source) => new(tier, QueryFixture.ApiVersion, source);
        }

        [Test]
        public void The_source_is_relative_to_the_output_root_when_the_output_is_redirected()
        {
            // --output: the manifests stay with the sources, the generated knowledge goes elsewhere.
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-query-in-{Guid.NewGuid():N}"));
            var output = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-query-out-{Guid.NewGuid():N}"));

            try
            {
                var layout = new KnowledgeLayout(root, output);
                KnowledgeLookupTests.Install(layout, QueryFixture.Tag);
                Directory.CreateDirectory(layout.Metamodel(QueryFixture.Tag).FullName);
                File.Copy(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, layout.MetamodelGraph(QueryFixture.Tag).FullName);

                var result = QueryFixture.NewQuery(layout, new ApiLayout(layout)).Subclasses("Usage");

                Assert.Multiple(() =>
                {
                    Assert.That(result.Found, Is.True);
                    Assert.That(result.Grounding.Source, Is.EqualTo($"knowledge/{QueryFixture.Tag}/metamodel/metamodel.json"));
                });
            }
            finally
            {
                root.Delete(recursive: true);
                output.Delete(recursive: true);
            }
        }

        [Test]
        public void A_named_api_version_answers_and_an_uninstalled_one_is_refused()
        {
            var exception = Assert.Throws<KnowledgeLookupException>(() => this.fixture.Api.Type("Commit", version: "2020-01"));

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Api.Type("Commit", version: QueryFixture.ApiVersion).Found, Is.True);
                Assert.That(exception!.Message, Does.Contain("API version 2020-01").And.Contain(QueryFixture.ApiVersion));
            });
        }

        [Test]
        public void An_installed_release_with_no_default_says_how_to_choose_one()
        {
            // The real manifests always name an installed default; another layout need not.
            var layout = new Mock<IKnowledgeLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns([QueryFixture.Tag]);
            layout.SetupGet(mock => mock.DefaultTag).Returns((string?)null);
            var apiLayout = new Mock<IApiLayout>();
            apiLayout.SetupGet(mock => mock.InstalledTags).Returns([QueryFixture.ApiVersion]);
            apiLayout.SetupGet(mock => mock.DefaultTag).Returns((string?)null);

            var release = Assert.Throws<KnowledgeLookupException>(
                () => QueryFixture.NewQuery(layout.Object, apiLayout.Object).Subclasses("Usage"));
            var api = Assert.Throws<KnowledgeLookupException>(
                () => QueryFixture.NewApi(apiLayout.Object, layout.Object).Type("Commit"));

            Assert.Multiple(() =>
            {
                Assert.That(release!.Message, Is.EqualTo("No default release is selected. Run 'hypha use --tag <release>'."));
                Assert.That(api!.Message, Is.EqualTo("No default API version is selected. Run 'hypha api use --tag <version>'."));
            });
        }

        [Test]
        public void A_named_release_answers_and_an_uninstalled_one_is_refused()
        {
            var exception = Assert.Throws<KnowledgeLookupException>(() => this.fixture.Query.Subclasses("Usage", tag: "2020-01"));

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Subclasses("Usage", tag: QueryFixture.Tag).Found, Is.True);
                Assert.That(exception!.Message, Does.Contain("2020-01").And.Contain(QueryFixture.Tag));
            });
        }

        [Test]
        public void Nothing_installed_says_how_to_install()
        {
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-query-empty-{Guid.NewGuid():N}"));

            try
            {
                var layout = new KnowledgeLayout(root);
                var apiLayout = new ApiLayout(layout);

                var release = Assert.Throws<KnowledgeLookupException>(
                    () => QueryFixture.NewQuery(layout, apiLayout).Subclasses("Usage"));
                var api = Assert.Throws<KnowledgeLookupException>(() => QueryFixture.NewApi(apiLayout, layout).Type("Commit"));
                var inventory = QueryFixture.NewQuery(layout, apiLayout).Releases();

                Assert.Multiple(() =>
                {
                    Assert.That(release!.Message, Does.Contain("hypha fetch"));
                    Assert.That(api!.Message, Does.Contain("hypha api fetch"));
                    Assert.That(inventory.Default, Is.Null);
                    Assert.That(inventory.Releases, Is.Empty);
                    Assert.That(inventory.ApiVersions, Is.Empty);
                });
            }
            finally
            {
                root.Delete(recursive: true);
            }
        }

        [Test]
        public void A_document_not_generated_says_how_to_generate_it()
        {
            File.Delete(this.fixture.Layout.CrossReferences(QueryFixture.Tag).FullName);

            var exception = Assert.Throws<KnowledgeLookupException>(() => this.fixture.Query.CrossReference("PartUsage"));

            Assert.That(exception!.Message, Does.Contain($"hypha generate --tag {QueryFixture.Tag}"));
        }

        [Test]
        public void An_invalid_document_is_refused_rather_than_answered_from()
        {
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, "{ not json");

            var exception = Assert.Throws<KnowledgeLookupException>(() => this.fixture.Query.Subclasses("Usage"));

            Assert.That(exception!.Message, Does.Contain("not a valid generated document"));
        }

        [Test]
        public void A_document_of_the_wrong_shape_is_refused_rather_than_answered_from()
        {
            // An empty object, or a member that holds null, reads as JSON but is not a generated document.
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, "{}");
            File.WriteAllText(this.fixture.Layout.CrossReferences(QueryFixture.Tag).FullName, "{}");
            File.WriteAllText(this.fixture.Layout.MetamodelIndex(QueryFixture.Tag).FullName, "{ \"entries\": null }");
            File.WriteAllText(this.fixture.Layout.ModelLibraryIndex(QueryFixture.Tag).FullName, "{}");
            File.WriteAllText(this.fixture.ApiLayout.CrossReferences(QueryFixture.ApiVersion).FullName, "{}");
            File.WriteAllText(this.fixture.ApiLayout.PimGraph(QueryFixture.ApiVersion).FullName, "{}");

            Assert.Multiple(() =>
            {
                Assert.That(() => this.fixture.Query.Subclasses("Usage"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Query.CrossReference("PartUsage"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Query.Element("PartUsage"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Query.Declaration("ISQBase::mass"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Api.Type("Commit"), Throws.TypeOf<KnowledgeLookupException>());
                Assert.That(() => this.fixture.Api.PimClass("PartUsage"), Throws.TypeOf<KnowledgeLookupException>());
            });
        }

        [TestCase("null")]
        [TestCase("   ")]
        [TestCase("")]
        public void An_empty_document_is_refused_rather_than_answered_from(string content)
        {
            File.WriteAllText(this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, content);
            File.WriteAllText(this.fixture.ApiLayout.CrossReferences(QueryFixture.ApiVersion).FullName, content);

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => this.fixture.Query.Subclasses("Usage"),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("is empty")
                        .And.Message.Contains($"hypha generate --tag {QueryFixture.Tag}"));
                Assert.That(
                    () => this.fixture.Api.Type("Commit"),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("is empty")
                        .And.Message.Contains($"hypha api generate --tag {QueryFixture.ApiVersion}"));
            });
        }

        [Test]
        public void A_document_that_cannot_be_read_says_to_try_again()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Ignore("only Windows refuses to read a file another handle holds exclusively");
            }

            using var held = new FileStream(
                this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            Assert.That(
                () => this.fixture.Query.Subclasses("Usage"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("try again"));
        }

        [Test]
        public void A_document_that_cannot_be_read_says_to_try_again_on_unix()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Ignore("covered on Windows by locking the file");
                return;
            }

            var graph = this.fixture.Layout.MetamodelGraph(QueryFixture.Tag).FullName;
            File.SetUnixFileMode(graph, UnixFileMode.None);

            try
            {
                if (CanRead(graph))
                {
                    Assert.Ignore("running as root, which reads any file");
                }

                Assert.That(
                    () => this.fixture.Query.Subclasses("Usage"),
                    Throws.TypeOf<KnowledgeLookupException>().With.Message.Contains("try again"));
            }
            finally
            {
                File.SetUnixFileMode(graph, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }

        [Test]
        public void A_document_rewritten_at_the_same_length_is_read_again()
        {
            // Only the write time changes: a rename of the same length, such as mass to mast.
            var file = this.fixture.Layout.ModelLibraryIndex(QueryFixture.Tag);

            Assert.That(this.fixture.Query.Declaration("ISQBase::mass").Found, Is.True);

            var before = new FileInfo(file.FullName).Length;
            File.WriteAllText(
                file.FullName,
                File.ReadAllText(file.FullName).Replace("ISQBase::mass", "ISQBase::mast", StringComparison.Ordinal));
            File.SetLastWriteTimeUtc(file.FullName, DateTime.UtcNow.AddMinutes(1));

            Assert.Multiple(() =>
            {
                Assert.That(new FileInfo(file.FullName).Length, Is.EqualTo(before));
                Assert.That(this.fixture.Query.Declaration("ISQBase::mass").Found, Is.False);
                Assert.That(this.fixture.Query.Declaration("ISQBase::mast").Found, Is.True);
            });
        }

        [Test]
        public void Queries_from_several_threads_at_once_give_the_same_answer()
        {
            var expected = this.fixture.Query.Subclasses("Usage").Value;
            var fresh = QueryFixture.NewQuery(this.fixture.Layout, this.fixture.ApiLayout);

            var answers = new IReadOnlyList<string>?[32];
            Parallel.For(0, answers.Length, i => answers[i] = fresh.Subclasses("Usage").Value);

            Assert.That(answers, Has.All.EqualTo(expected));
        }

        [Test]
        public void The_feature_and_operator_filters_combine()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.FeatureAssignments("declaredName", "?=").Value, Is.Empty);
                Assert.That(
                    this.fixture.Query.FeatureAssignments("isComposite", "?=").Value!.Select(assignment => assignment.Element),
                    Is.EqualTo(new[] { "Feature" }));
                Assert.That(
                    this.fixture.Query.FeatureAssignments("declaredName").Value!.Select(assignment => assignment.Element),
                    Is.EqualTo(new[] { "Feature", "PartUsage" }));
            });
        }

        [Test]
        public void A_regenerated_document_is_read_again()
        {
            // Another process may regenerate the release while a server holds the old document.
            var file = this.fixture.Layout.ModelLibraryIndex(QueryFixture.Tag);

            Assert.That(this.fixture.Query.Declaration("ISQBase::mass").Found, Is.True);

            File.WriteAllText(
                file.FullName,
                File.ReadAllText(file.FullName).Replace("ISQBase::mass", "ISQBase::weight", StringComparison.Ordinal));
            File.SetLastWriteTimeUtc(file.FullName, DateTime.UtcNow.AddMinutes(1));

            Assert.Multiple(() =>
            {
                Assert.That(this.fixture.Query.Declaration("ISQBase::mass").Found, Is.False);
                Assert.That(this.fixture.Query.Declaration("ISQBase::weight").Found, Is.True);
            });
        }

        [Test]
        public void An_unchanged_document_is_not_read_again()
        {
            var first = this.fixture.Query.Element("PartUsage").Value;
            var second = this.fixture.Query.Element("PartUsage").Value;

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Both_queries_are_registered()
        {
            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.RepositoryRoot = this.fixture.Root)
                .BuildServiceProvider();

            var first = provider.GetRequiredService<IKnowledgeQuery>();
            var second = provider.GetRequiredService<IKnowledgeQuery>();
            var documents = provider.GetRequiredService<IDocumentCache>();

            Assert.Multiple(() =>
            {
                Assert.That(first.Releases().Default, Is.EqualTo(QueryFixture.Tag));
                Assert.That(provider.GetRequiredService<IApiQuery>().Type("Commit").Found, Is.True);
                Assert.That(first, Is.SameAs(second));
                Assert.That(provider.GetRequiredService<IDocumentCache>(), Is.SameAs(documents));
                Assert.That(provider.GetRequiredService<ISpecificationReader>(), Is.InstanceOf<SpecificationReader>());
                Assert.That(provider.GetRequiredService<IMetamodelDiffer>(), Is.InstanceOf<MetamodelDiffer>());
            });
        }

        [Test]
        public void A_query_reads_through_the_cache_it_is_given()
        {
            var documents = new Mock<IDocumentCache>();
            documents
                .Setup(mock => mock.Get(
                    It.IsAny<FileInfo>(),
                    It.IsAny<System.Text.Json.Serialization.Metadata.JsonTypeInfo<MetamodelDocument>>(),
                    It.IsAny<Func<MetamodelDocument, bool>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new KnowledgeLookupException("refused by the cache"));
            var query = new KnowledgeQuery(
                this.fixture.Layout, this.fixture.ApiLayout, documents.Object, new SpecificationReader(documents.Object), new MetamodelDiffer());

            Assert.That(
                () => query.Subclasses("Usage"),
                Throws.TypeOf<KnowledgeLookupException>().With.Message.EqualTo("refused by the cache"));
        }

        [Test]
        public void A_query_needs_every_collaborator()
        {
            var layout = this.fixture.Layout;
            var apiLayout = this.fixture.ApiLayout;
            var documents = this.fixture.Documents;
            var specifications = new SpecificationReader(documents);
            var differ = new MetamodelDiffer();

            Assert.Multiple(() =>
            {
                Assert.That(() => new KnowledgeQuery(layout, apiLayout, null!, specifications, differ), Throws.ArgumentNullException);
                Assert.That(() => new KnowledgeQuery(layout, apiLayout, documents, null!, differ), Throws.ArgumentNullException);
                Assert.That(() => new KnowledgeQuery(layout, apiLayout, documents, specifications, null!), Throws.ArgumentNullException);
                Assert.That(() => new ApiQuery(apiLayout, layout, null!, specifications), Throws.ArgumentNullException);
                Assert.That(() => new ApiQuery(apiLayout, layout, documents, null!), Throws.ArgumentNullException);
                Assert.That(() => new SpecificationReader(null!), Throws.ArgumentNullException);
            });
        }

        private static bool CanRead(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
