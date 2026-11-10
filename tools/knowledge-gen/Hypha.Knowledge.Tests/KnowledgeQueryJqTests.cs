// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeQueryJqTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    // Each test runs a jq query as the skills, the metamodel-navigator agent and the metamodel-gen README
    // document it, against the same file, and checks the query API gives the same answer. The query API
    // replaces those jq calls, so an answer that differs is a regression, not a style choice.
    [TestFixture]
    public class KnowledgeQueryJqTests
    {
        private static readonly JsonSerializerOptions Metamodel = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private static readonly JsonSerializerOptions Api = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void Releases_match_the_manifests()
        {
            var inventory = this.fixture.Query.Releases();

            Assert.Multiple(() =>
            {
                Assert.That(
                    new[] { inventory.Default! }.Concat(inventory.Releases.Select(release => release.Tag)),
                    Is.EqualTo(Jq.Lines(".default, (.versions[].tag)", this.fixture.Layout.VersionManifest)));
                Assert.That(
                    new[] { inventory.ApiDefault! }.Concat(inventory.ApiVersions.Select(version => version.Tag)),
                    Is.EqualTo(Jq.Lines(".default, (.versions[].tag)", this.fixture.ApiLayout.VersionManifest)));
            });
        }

        [Test]
        public void Concrete_subclasses_match()
        {
            var result = this.fixture.Query.Subclasses("Usage", concreteOnly: true);

            Assert.That(
                result.Value,
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.allAncestors | index(\"Usage\")) | select(.isAbstract|not) | .name",
                    this.Graph()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void Classes_with_a_feature_typed_by_a_type_match()
        {
            // The documented query prints a class once per matching feature; the query API names it once.
            var result = this.fixture.Query.ClassesWithFeatureTyped("Expression");

            Assert.That(
                result.Value,
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.ownedAttributes[]? | .type==\"Expression\") | .name",
                    this.Graph()).Distinct().Sorted()).And.Not.Empty);
        }

        [Test]
        public void Classes_with_a_composite_feature_typed_by_a_type_match()
        {
            var result = this.fixture.Query.ClassesWithFeatureTyped("Expression", compositeOnly: true);

            Assert.That(
                result.Value,
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.ownedAttributes[]? | .isComposite and .type==\"Expression\") | .name",
                    this.Graph()).Distinct().Sorted()).And.Not.Empty);
        }

        [Test]
        public void The_origin_of_an_inherited_feature_matches()
        {
            var result = this.fixture.Query.Features("PartUsage", "name");

            Assert.That(
                result.Value!.Select(feature => feature.InheritedFrom),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | .inheritedAttributes[] | select(.name==\"name\") | .inheritedFrom",
                    this.Graph())).And.Not.Empty);
        }

        [Test]
        public void All_features_of_a_metaclass_match_in_order()
        {
            var result = this.fixture.Query.Features("PartUsage");

            Assert.That(
                result.Value!.Select(feature => feature.Name),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | (.ownedAttributes + .inheritedAttributes)[].name",
                    this.Graph())).And.Not.Empty);
        }

        [Test]
        public void Every_field_of_every_feature_matches()
        {
            var features = this.fixture.Query.Features("PartUsage").Value!;
            var expected = features.Select(feature => JsonSerializer.SerializeToNode(feature, Metamodel)!).ToList();

            var actual = Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | (.ownedAttributes[] + {inheritedFrom: null, redefinedBy: null}), .inheritedAttributes[]"
                    + " | {name, type, lower, upper, isDerived, isDerivedUnion, isComposite, isOrdered, isUnique, isReadOnly,"
                    + " defaultValue, redefines, subsets, opposite, inheritedFrom, redefinedBy} | tojson",
                    this.Graph())
                .Select(line => JsonNode.Parse(line)!)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(actual, Has.Count.EqualTo(expected.Count).And.Not.Empty);

                foreach (var (feature, line) in expected.Zip(actual))
                {
                    Assert.That(JsonNode.DeepEquals(feature, line), Is.True, $"query API: {feature}\njq: {line}");
                }
            });
        }

        [Test]
        public void Whether_a_feature_is_reachable_matches()
        {
            var result = this.fixture.Query.Features("PartUsage", "owningDefinition");

            Assert.That(
                result.Value!.Select(feature => feature.Name),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | (.ownedAttributes[], .inheritedAttributes[]) | select(.name==\"owningDefinition\") | .name",
                    this.Graph())).And.Not.Empty);
        }

        [Test]
        public void Redefinitions_match()
        {
            var result = this.fixture.Query.Redefinitions("definition");

            Assert.That(
                result.Value!.Select(redefinition => redefinition.Class),
                Is.EqualTo(Jq.Lines(References("redefines", "definition", "$c.name"), this.Graph()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void Redefining_features_match()
        {
            var result = this.fixture.Query.Redefinitions("definition");

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Value!.Select(redefinition => $"{redefinition.Class}.{redefinition.Feature}"),
                    Is.EqualTo(Jq.Lines(
                        References("redefines", "definition", "\"\\($c.name).\\($a.name)\""), this.Graph()).Sorted()).And.Not.Empty);
                Assert.That(result.Value!.Select(redefinition => redefinition.Redefines.Feature), Has.All.EqualTo("definition"));
            });
        }

        [TestCase("type", "Feature")]
        [TestCase("type", "FeatureTyping")]
        public void Redefinitions_of_a_feature_one_owner_declares_match(string feature, string owner)
        {
            // Both Feature and FeatureTyping declare a 'type'; each is redefined by different features.
            var result = this.fixture.Query.Redefinitions(feature, owner: owner);

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Value!.Select(redefinition => $"{redefinition.Class}.{redefinition.Feature}"),
                    Is.EqualTo(Jq.Lines(
                        References("redefines", feature, "\"\\($c.name).\\($a.name)\"", owner), this.Graph()).Sorted()).And.Not.Empty);
                Assert.That(result.Value!.Select(redefinition => redefinition.Redefines.Class), Has.All.EqualTo(owner));
                Assert.That(result.Value, Has.Count.LessThan(this.fixture.Query.Redefinitions(feature).Value!.Count));
            });
        }

        [Test]
        public void Ancestors_match()
        {
            var result = this.fixture.Query.Ancestors("PartUsage");

            Assert.That(
                result.Value,
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | .allAncestors[]", this.Graph()).Sorted()).And.Not.Empty);
        }

        [TestCase("PartUsage", "Feature")]
        [TestCase("PartUsage", "Element")]
        [TestCase("PartUsage", "PartDefinition")]
        [TestCase("Feature", "PartUsage")]
        public void Whether_one_metaclass_specializes_another_matches(string specific, string general)
        {
            var answer = this.fixture.Query.Specializes(specific, general).Value!;
            var expected = Jq.Lines(
                $".classes[] | select(.name==\"{specific}\") | .allAncestors | index(\"{general}\") != null", this.Graph());

            Assert.That(new[] { answer.Specializes ? "true" : "false" }, Is.EqualTo(expected));
        }

        [Test]
        public void A_specialization_chain_follows_direct_generalizations()
        {
            var path = this.fixture.Query.Specializes("PartUsage", "Element").Value!.Path;
            var extends = Jq.Lines(
                    ".classes[] | \"\\(.name) \\(.extends | join(\",\"))\"", this.Graph())
                .Select(line => line.Split(' '))
                .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1].Split(',') : []);

            Assert.Multiple(() =>
            {
                Assert.That(path[0], Is.EqualTo("PartUsage"));
                Assert.That(path[^1], Is.EqualTo("Element"));

                foreach (var (from, to) in path.Zip(path.Skip(1)))
                {
                    Assert.That(extends[from], Does.Contain(to), $"{from} does not directly specialize {to}");
                }
            });
        }

        [Test]
        public void Own_constraints_match_field_by_field()
        {
            var constraints = this.fixture.Query.Constraints("PartUsage", includeInherited: false).Value!;
            var expected = constraints.Select(constraint => JsonSerializer.SerializeToNode(constraint.Member, Metamodel)!).ToList();

            var actual = Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | .constraints[] | tojson", this.Graph())
                .Select(line => JsonNode.Parse(line)!)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(actual, Has.Count.EqualTo(expected.Count).And.Not.Empty);
                Assert.That(expected.Zip(actual).All(pair => JsonNode.DeepEquals(pair.First, pair.Second)), Is.True);
                Assert.That(constraints.Select(constraint => constraint.DeclaredIn), Has.All.EqualTo("PartUsage"));
            });
        }

        [Test]
        public void Inherited_constraints_match()
        {
            var constraints = this.fixture.Query.Constraints("PartUsage").Value!;

            Assert.That(
                constraints.Select(constraint => $"{constraint.DeclaredIn} {constraint.Member.Name}"),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | (.constraints[] | \"PartUsage \\(.name)\"),"
                    + " (.inheritedConstraints | sort_by(.class, .constraint)[] | \"\\(.class) \\(.constraint)\")",
                    this.Graph())).And.Not.Empty);
        }

        [Test]
        public void An_inherited_constraint_reads_its_body_from_the_metaclass_that_declares_it()
        {
            var inherited = this.fixture.Query.Constraints("PartUsage").Value!.First(constraint => constraint.DeclaredIn != "PartUsage");
            var expected = Jq.Lines(
                $".classes[] | select(.name==\"{inherited.DeclaredIn}\") | .constraints[] | select(.name==\"{inherited.Member.Name}\") | tojson",
                this.Graph()).Single();

            Assert.That(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(inherited.Member, Metamodel), JsonNode.Parse(expected)), Is.True);
        }

        [Test]
        public void Inherited_operations_match()
        {
            var operations = this.fixture.Query.Operations("ConstraintUsage").Value!;

            Assert.That(
                operations.Select(operation => $"{operation.DeclaredIn} {operation.Member.Name}"),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"ConstraintUsage\") | (.ownedOperations[] | \"ConstraintUsage \\(.name)\"),"
                    + " (.inheritedOperations | map(select(.redefinedBy == null)) | sort_by(.class, .operation)[] | \"\\(.class) \\(.operation)\")",
                    this.Graph())).And.Not.Empty);
        }

        [Test]
        public void An_inherited_operation_a_metaclass_redefines_gives_way_to_the_redefinition()
        {
            // Feature declares namingFeature, Usage redefines it and ConstraintUsage redefines it again:
            // only ConstraintUsage's body applies to a ConstraintUsage.
            var naming = this.fixture.Query.Operations("ConstraintUsage").Value!
                .Where(operation => operation.Member.Name == "namingFeature")
                .ToList();
            var redefinedBy = Jq.Lines(
                ".classes[] | select(.name==\"ConstraintUsage\") | .inheritedOperations[] | select(.operation==\"namingFeature\")"
                + " | \"\\(.class) \\(.redefinedBy.class)\"",
                this.Graph());

            Assert.Multiple(() =>
            {
                Assert.That(naming.Select(operation => operation.DeclaredIn), Is.EqualTo(new[] { "ConstraintUsage" }));
                Assert.That(redefinedBy, Is.EquivalentTo(new[] { "Feature Usage", "Usage ConstraintUsage" }));
            });
        }

        [Test]
        public void Own_operations_match_field_by_field()
        {
            var operations = this.fixture.Query.Operations("ActionUsage", includeInherited: false).Value!;
            var expected = operations.Select(operation => JsonSerializer.SerializeToNode(operation.Member, Metamodel)!).ToList();

            var actual = Jq.Lines(
                    ".classes[] | select(.name==\"ActionUsage\") | .ownedOperations[] | tojson", this.Graph())
                .Select(line => JsonNode.Parse(line)!)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(actual, Has.Count.EqualTo(expected.Count).And.Not.Empty);
                Assert.That(expected.Zip(actual).All(pair => JsonNode.DeepEquals(pair.First, pair.Second)), Is.True);
            });
        }

        [Test]
        public void Enumeration_literals_match()
        {
            var result = this.fixture.Query.Literals("VisibilityKind");

            Assert.That(
                result.Value!.Select(literal => literal.Name),
                Is.EqualTo(Jq.Lines(
                    ".enumerations[] | select(.name==\"VisibilityKind\") | .literals[].name", this.Graph())).And.Not.Empty);
        }

        [Test]
        public void Subsetting_features_match()
        {
            var result = this.fixture.Query.Subsettings("usage");

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Value!.Select(subsetting => $"{subsetting.Class}.{subsetting.Feature}"),
                    Is.EqualTo(Jq.Lines(
                        References("subsets", "usage", "\"\\($c.name).\\($a.name)\""), this.Graph()).Sorted()).And.Not.Empty);
                Assert.That(result.Value!.Select(subsetting => subsetting.Subsets.Feature), Has.All.EqualTo("usage"));
            });
        }

        [TestCase("usage", "Definition")]
        [TestCase("annotation", "A_annotatedElement_annotation")]
        public void Subsettings_of_a_feature_one_owner_declares_match(string feature, string owner)
        {
            // The second owner is an association: the end it owns is subsetted too.
            var result = this.fixture.Query.Subsettings(feature, owner: owner);

            Assert.That(
                result.Value!.Select(subsetting => $"{subsetting.Class}.{subsetting.Feature}"),
                Is.EqualTo(Jq.Lines(
                    References("subsets", feature, "\"\\($c.name).\\($a.name)\"", owner), this.Graph()).Sorted()).And.Not.Empty);
        }

        [TestCase("PartUsage")]
        [TestCase("Feature")]
        public void Containers_match(string element)
        {
            var result = this.fixture.Query.Containers(element);

            Assert.That(
                result.Value!.Select(feature => $"{feature.Class}::{feature.Feature}"),
                Is.EqualTo(Jq.Lines(Accepted(element) + " | .classes[] | . as $k | .ownedAttributes[]"
                    + " | select(.isComposite and (.type as $t | $ok | index($t))) | \"\\($k.name)::\\(.name)\"", this.Graph()).Sorted())
                    .And.Not.Empty);
        }

        [Test]
        public void Containers_within_one_metaclass_match_its_effective_features()
        {
            var result = this.fixture.Query.Containers("PartUsage", "ActorMembership");

            // The whole row, so a field taken from the wrong place in either branch shows.
            Assert.That(
                result.Value!.Select(feature =>
                    $"{feature.Class}::{feature.Feature} {feature.Type} {feature.Lower}..{feature.Upper} {feature.IsComposite} {feature.IsDerived}"),
                Is.EqualTo(Jq.Lines(Accepted("PartUsage") + " | .classes[] | select(.name==\"ActorMembership\") | "
                    + Effective("ActorMembership") + " | select(.isComposite and (.type as $t | $ok | index($t)))"
                    + " | \"\\(.c)::\\(.name) \\(.type) \\(.lower)..\\(.upper) \\(if .isComposite then \"True\" else \"False\" end)"
                    + " \\(if .isDerived then \"True\" else \"False\" end)\"",
                    this.Graph()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void Features_accepting_a_metaclass_match()
        {
            var result = this.fixture.Query.FeaturesAccepting("PartUsage");

            Assert.That(
                result.Value!.Select(feature => $"{feature.Class}::{feature.Feature}"),
                Is.EqualTo(Jq.Lines(Accepted("PartUsage") + " | .classes[] | . as $k | .ownedAttributes[]"
                    + " | select(.type as $t | $ok | index($t)) | \"\\($k.name)::\\(.name)\"", this.Graph()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void Features_accepting_a_primitive_type_match()
        {
            var result = this.fixture.Query.FeaturesAccepting("Boolean");

            Assert.That(
                result.Value!.Select(feature => $"{feature.Class}::{feature.Feature}"),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | . as $k | .ownedAttributes[] | select(.type==\"Boolean\") | \"\\($k.name)::\\(.name)\"",
                    this.Graph()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void Neighbours_match()
        {
            var result = this.fixture.Query.Neighbours("PartUsage").Value!;

            Assert.Multiple(() =>
            {
                Assert.That(
                    result.Outgoing.Select(feature => feature.Feature),
                    Is.EqualTo(Jq.Lines(
                        "(.classes | map(.name)) as $names | .classes[] | select(.name==\"PartUsage\") | .ownedAttributes[]"
                        + " | select(.type as $t | $names | index($t)) | .name",
                        this.Graph()).Sorted()).And.Not.Empty);
                Assert.That(
                    result.Incoming.Select(feature => $"{feature.Class}::{feature.Feature}"),
                    Is.EqualTo(Jq.Lines(
                        ".classes[] | select(.name!=\"PartUsage\") | . as $k | .ownedAttributes[] | select(.type==\"PartUsage\")"
                        + " | \"\\($k.name)::\\(.name)\"",
                        this.Graph()).Sorted()).And.Not.Empty);
            });
        }

        [Test]
        public void A_package_matches()
        {
            var package = this.fixture.Query.Package("KerML::Core").Value!.Single();

            Assert.Multiple(() =>
            {
                Assert.That(
                    package.Packages,
                    Is.EqualTo(Jq.Lines(".packages[] | select(.parent==\"Core\") | .qualifiedName", this.Graph()).Sorted()).And.Not.Empty);
                Assert.That(
                    package.AllClasses,
                    Is.EqualTo(Jq.Lines(
                        ".packages[] | select(.qualifiedName==\"KerML::Core\" or (.qualifiedName | startswith(\"KerML::Core::\"))) | .classes[]",
                        this.Graph()).Sorted()).And.Not.Empty);
            });
        }

        [Test]
        public void Effective_features_match()
        {
            var result = this.fixture.Query.EffectiveFeatures("Dependency");

            Assert.That(
                result.Value!.Select(feature => feature.Name),
                Is.EqualTo(Jq.Lines(
                    ".classes[] | select(.name==\"Dependency\") | (.ownedAttributes + (.inheritedAttributes | map(select(.redefinedBy == null))))[].name",
                    this.Graph())).And.Not.Empty);
        }

        [TestCase("Type", "ownedFeature")]
        [TestCase("PartUsage", "owningType")]
        public void An_opposite_end_matches(string className, string feature)
        {
            var result = this.fixture.Query.Opposite(className, feature).Value!;
            var expected = Jq.Lines(
                    $".classes[] | select(.name==\"{className}\") | (.ownedAttributes[], .inheritedAttributes[]) | select(.name==\"{feature}\")"
                    + " | .opposite | tojson",
                    this.Graph())
                .Single();

            Assert.That(
                JsonNode.DeepEquals(JsonSerializer.SerializeToNode(result.Other, Metamodel), JsonNode.Parse(expected)),
                Is.True,
                $"query API: {JsonSerializer.Serialize(result.Other, Metamodel)}\njq: {expected}");
        }

        [Test]
        public void Contributors_to_a_derived_union_match()
        {
            var result = this.fixture.Query.Contributors("Namespace", "ownedMember");

            // jq follows the subsets and redefines references the same way, recursively; a contributor
            // reached along two paths is the same reference, so the jq side is made unique.
            var expected = Jq.Lines(
                    "[.classes[] | . as $k | .ownedAttributes[] | . as $a"
                    + " | ((.subsets[] | {k: \"subsets\", r: .}), (.redefines[] | {k: \"redefines\", r: .}))"
                    + " | {c: $k.name, f: $a.name, k: .k, o: (.r.class // .r.association), s: .r.feature}] as $e"
                    + " | def contributors($o; $s): $e[] | select(.o==$o and .s==$s) | ., contributors(.c; .f);"
                    + " [contributors(\"Namespace\"; \"ownedMember\")] | unique[] | \"\\(.c)::\\(.f) \\(.k) \\(.o)::\\(.s)\"",
                    this.Graph())
                .Sorted();

            Assert.That(
                result.Value!.Select(contribution =>
                    $"{contribution.Class}::{contribution.Feature} {contribution.Through} "
                    + $"{contribution.Contributes.Class ?? contribution.Contributes.Association}::{contribution.Contributes.Feature}")
                    .Sorted(),
                Is.EqualTo(expected).And.Not.Empty);
        }

        [Test]
        public void The_elements_a_production_builds_match()
        {
            var result = this.fixture.Query.BuiltBy("PartUsage");

            Assert.That(
                result.Value!.Select(use => $"{use.Element} {use.Edge.Grammar}"),
                Is.EqualTo(Jq.Lines(
                    ".entries | to_entries[] | . as $e | $e.value.grammar[] | select(.production==\"PartUsage\") | \"\\($e.key) \\(.grammar)\"",
                    this.CrossReferences()).Sorted()).And.Not.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void The_elements_a_clause_treats_match(bool includeSubclauses)
        {
            var clause = includeSubclauses ? "8.3" : "7.4.2";
            var match = includeSubclauses
                ? "(.clause==\"8.3\" or (.clause | startswith(\"8.3.\")))"
                : ".clause==\"7.4.2\"";

            var result = this.fixture.Query.ElementsInClause("kerml", clause, includeSubclauses);

            Assert.That(
                result.Value!.Select(use => $"{use.Element} {use.Edge.Clause}"),
                Is.EqualTo(Jq.Lines(
                    $".entries | to_entries[] | . as $e | $e.value.clauses[] | select(.document==\"kerml\" and {match}) | \"\\($e.key) \\(.clause)\"",
                    this.CrossReferences()).Sorted()).And.Not.Empty);
        }

        [Test]
        public void A_specification_clause_matches_its_index_entry()
        {
            var clause = this.fixture.Query.Clause("kerml", "7.4.2").Value!;
            var expected = new JsonObject
            {
                ["title"] = clause.Title,
                ["pages"] = clause.Pages,
                ["normative"] = clause.Normative,
            };

            Assert.That(
                JsonNode.DeepEquals(
                    expected,
                    Jq.Json(
                        ".entries[\"7.4.2\"] | {title, pages, normative}",
                        this.fixture.Layout.SpecificationCatalog(QueryFixture.Tag, "kerml"))),
                Is.True);
        }

        [Test]
        public void An_index_entry_matches()
        {
            var result = this.fixture.Query.Element("PartUsage");

            AssertSameJson(result.Value, Jq.Json(".entries[\"PartUsage\"]", this.fixture.Layout.MetamodelIndex(QueryFixture.Tag)), Metamodel);
        }

        [Test]
        public void Clauses_and_examples_of_an_element_match()
        {
            var entry = this.fixture.Query.CrossReference("PartUsage").Value!;
            var expected = new JsonObject
            {
                ["clauses"] = new JsonArray([.. entry.Clauses.Select(edge => (JsonNode)$"{edge.Document} {edge.Clause}")]),
                ["examples"] = new JsonArray([.. entry.Examples.Select(edge => (JsonNode)edge.File)]),
            };

            Assert.That(
                JsonNode.DeepEquals(
                    expected,
                    Jq.Json(
                        ".entries[\"PartUsage\"] | {clauses: [.clauses[] | \"\\(.document) \\(.clause)\"], examples: [.examples[].file]}",
                        this.CrossReferences())),
                Is.True);
        }

        [Test]
        public void The_productions_that_build_an_element_match()
        {
            var entry = this.fixture.Query.CrossReference("PartUsage").Value!;

            Assert.That(
                entry.Grammar.Select(edge => $"{edge.Grammar} {edge.Production}"),
                Is.EqualTo(Jq.Lines(
                    ".entries[\"PartUsage\"].grammar[] | \"\\(.grammar) \\(.production)\"", this.CrossReferences())).And.Not.Empty);
        }

        [Test]
        public void The_features_an_element_fills_in_match()
        {
            var entry = this.fixture.Query.CrossReference("Feature").Value!;

            Assert.That(
                entry.Features.Select(edge =>
                    $"{edge.Grammar}: {edge.Feature} {edge.Operator} via {string.Join(", ", edge.Productions)}"),
                Is.EqualTo(Jq.Lines(
                    ".entries[\"Feature\"].features[] | \"\\(.grammar): \\(.feature) \\(.operator) via \\(.productions|join(\", \"))\"",
                    this.CrossReferences())).And.Not.Empty);
        }

        [Test]
        public void The_clauses_of_an_element_match()
        {
            var entry = this.fixture.Query.CrossReference("Feature").Value!;

            Assert.That(
                entry.Clauses.Select(edge => $"{edge.Document} {edge.Clause}"),
                Is.EqualTo(Jq.Lines(
                    ".entries[\"Feature\"].clauses[] | \"\\(.document) \\(.clause)\"", this.CrossReferences())).And.Not.Empty);
        }

        [Test]
        public void Which_elements_set_a_feature_matches()
        {
            var result = this.fixture.Query.FeatureAssignments(feature: "declaredName");

            Assert.That(
                result.Value!.Select(assignment =>
                    $"{assignment.Element}: {string.Join(", ", assignment.Edge.Productions)}").ToList(),
                Is.EqualTo(Jq.Lines(
                    ".entries | to_entries[] | . as $e | $e.value.features[] | select(.feature==\"declaredName\") | \"\\($e.key): \\(.productions|join(\", \"))\"",
                    this.CrossReferences())).And.Count.EqualTo(2));
        }

        [Test]
        public void Which_elements_assign_with_an_operator_matches()
        {
            var result = this.fixture.Query.FeatureAssignments(assignmentOperator: "?=");

            Assert.That(
                result.Value!.Select(assignment => $"{assignment.Element}: {assignment.Edge.Feature}"),
                Is.EqualTo(Jq.Lines(
                    ".entries | to_entries[] | . as $e | $e.value.features[] | select(.operator==\"?=\") | \"\\($e.key): \\(.feature)\"",
                    this.CrossReferences())).And.Not.Empty);
        }

        [Test]
        public void A_library_declaration_matches()
        {
            var result = this.fixture.Query.Declaration("ISQBase::mass");

            AssertSameJson(
                result.Value,
                Jq.Json(".declarations[\"ISQBase::mass\"]", this.fixture.Layout.ModelLibraryIndex(QueryFixture.Tag)),
                Metamodel);
        }

        [Test]
        public void The_owned_attributes_of_a_pim_class_match()
        {
            var node = this.fixture.Api.PimClass("PartUsage").Value!;
            var expected = node.OwnedAttributes
                .Select(attribute => new { attribute.Name, attribute.Lower, attribute.Upper, attribute.IsDerived, attribute.Subsets })
                .Select(attribute => JsonSerializer.SerializeToNode(attribute, Metamodel)!)
                .ToList();

            var actual = Jq.Lines(
                    ".classes[] | select(.name==\"PartUsage\") | .ownedAttributes[] | {name, lower, upper, isDerived, subsets} | tojson",
                    this.fixture.ApiLayout.PimGraph(QueryFixture.ApiVersion))
                .Select(line => JsonNode.Parse(line)!)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(actual, Has.Count.EqualTo(expected.Count).And.Not.Empty);
                Assert.That(expected.Zip(actual).All(pair => JsonNode.DeepEquals(pair.First, pair.Second)), Is.True);
            });
        }

        [Test]
        public void An_api_type_matches()
        {
            var result = this.fixture.Api.Type("Commit");

            AssertSameJson(
                result.Value,
                Jq.Json(".types[\"Commit\"]", this.fixture.ApiLayout.CrossReferences(QueryFixture.ApiVersion)),
                Api);
        }

        [Test]
        public void An_api_operation_matches()
        {
            var result = this.fixture.Api.Operation("ProjectService.createProject");

            AssertSameJson(
                result.Value,
                Jq.Json(".operations[\"ProjectService.createProject\"]", this.fixture.ApiLayout.CrossReferences(QueryFixture.ApiVersion)),
                Api);
        }

        private static void AssertSameJson<T>(T value, JsonNode? expected, JsonSerializerOptions options)
        {
            Assert.That(value, Is.Not.Null);

            var actual = JsonSerializer.SerializeToNode(value, options);

            Assert.That(JsonNode.DeepEquals(actual, expected), Is.True, $"query API: {actual}\njq: {expected}");
        }

        // The names a value of the type conforms to: the type and its ancestors, as $ok.
        private static string Accepted(string type) =>
            $"(.classes[] | select(.name==\"{type}\") | .allAncestors + [\"{type}\"]) as $ok";

        // A metaclass's effective features, each tagged with its declaring class as .c.
        private static string Effective(string className) =>
            $"((.ownedAttributes[] | . + {{c: \"{className}\"}}), (.inheritedAttributes[] | select(.redefinedBy == null) | . + {{c: .inheritedFrom}}))";

        // One line per reference to the feature (an attribute may name two features of that name), the
        // owner matched on either a class or an association.
        private static string References(string list, string feature, string output, string? owner = null) =>
            $".classes[] | . as $c | .ownedAttributes[] | . as $a | .{list}[] | select(.feature==\"{feature}\")"
            + (owner is null ? string.Empty : $" | select(.class==\"{owner}\" or .association==\"{owner}\")")
            + $" | {output}";

        private System.IO.FileInfo Graph() => this.fixture.Layout.MetamodelGraph(QueryFixture.Tag);

        private System.IO.FileInfo CrossReferences() => this.fixture.Layout.CrossReferences(QueryFixture.Tag);
    }
}
