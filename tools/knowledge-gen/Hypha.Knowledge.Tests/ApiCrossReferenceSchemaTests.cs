// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceSchemaTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;

    /// <summary>
    /// Keeps the hand-written <c>api-cross-references.schema.json</c> honest without pulling in a
    /// JSON-Schema validator.
    /// </summary>
    /// <remarks>
    /// Checks the shape the builder actually produces against the <c>required</c> lists and the
    /// declared <c>properties</c> of the schema, so adding a member to one and not the other fails
    /// here - the same technique <c>CrossReferenceSchemaTests</c> uses. Every object in the schema is
    /// closed, so a member the schema does not know would make it reject real documents.
    /// </remarks>
    [TestFixture]
    public class ApiCrossReferenceSchemaTests
    {
        private JsonDocument schema = null!;
        private JsonDocument produced = null!;

        [SetUp]
        public void SetUp()
        {
            if (Repository.Layout is not { } layout)
            {
                Assert.Ignore("The tests are not running inside a checkout.");
                return;
            }

            var schemaFile = Path.Combine(layout.Root.FullName, "knowledge", "api-cross-references.schema.json");
            this.schema = JsonDocument.Parse(File.ReadAllText(schemaFile, Encoding.UTF8));

            var document = new ApiCrossReferenceBuilder().Build(
                "1.0",
                ApiCrossReferenceBuilderTests.Pim,
                ApiCrossReferenceBuilderTests.OpenApi,
                [
                    ApiCrossReferenceBuilderTests.ProjectClause,
                    ApiCrossReferenceBuilderTests.DataModelClause,
                    ApiCrossReferenceBuilderTests.ServiceClause,
                    ApiCrossReferenceBuilderTests.ModelMappingClause,
                    ApiCrossReferenceBuilderTests.EndpointMappingClause,
                ]);

            this.produced = JsonDocument.Parse(ApiJson.Render(document));
        }

        [TearDown]
        public void TearDown()
        {
            this.schema?.Dispose();
            this.produced?.Dispose();
        }

        [Test]
        public void The_document_has_exactly_the_top_level_members_the_schema_requires()
        {
            Assert.That(
                Keys(this.produced.RootElement),
                Is.EquivalentTo(Required(this.schema.RootElement)));
        }

        [Test]
        public void The_sources_and_counts_blocks_match_the_schema()
        {
            var properties = this.schema.RootElement.GetProperty("properties");

            Assert.Multiple(() =>
            {
                Assert.That(
                    Keys(this.produced.RootElement.GetProperty("sources")),
                    Is.EquivalentTo(Required(properties.GetProperty("sources"))));
                Assert.That(
                    Keys(this.produced.RootElement.GetProperty("counts")),
                    Is.EquivalentTo(Required(properties.GetProperty("counts"))));
            });
        }

        [Test]
        public void Every_type_and_everything_it_holds_matches_the_schema()
        {
            var types = this.produced.RootElement.GetProperty("types").EnumerateObject().ToList();

            Assert.That(types, Is.Not.Empty);

            Assert.Multiple(() =>
            {
                foreach (var type in types.Select(entry => entry.Value))
                {
                    this.AssertMatches(type, "type");

                    foreach (var clause in type.GetProperty("clauses").EnumerateArray())
                    {
                        this.AssertMatches(clause, "clauseEdge");
                    }

                    if (type.TryGetProperty("schema", out var edge))
                    {
                        this.AssertMatches(edge, "schemaEdge");
                    }

                    if (type.TryGetProperty("members", out var members))
                    {
                        foreach (var member in members.EnumerateArray())
                        {
                            this.AssertMatches(member, "member");
                        }
                    }
                }
            });
        }

        [Test]
        public void The_fixture_exercises_the_optional_members_too()
        {
            // Members that are left out when null are only checked if some entry carries them.
            var types = this.produced.RootElement.GetProperty("types").EnumerateObject()
                .Select(entry => entry.Value).ToList();
            var operations = this.produced.RootElement.GetProperty("operations").EnumerateObject()
                .Select(entry => entry.Value).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(types.Any(type => type.TryGetProperty("schema", out _)), Is.True);
                Assert.That(types.Any(type => type.TryGetProperty("psmOnly", out _)), Is.True);
                Assert.That(
                    types.Where(type => type.TryGetProperty("members", out _))
                        .SelectMany(type => type.GetProperty("members").EnumerateArray())
                        .Any(member => member.TryGetProperty("inheritedFrom", out _)),
                    Is.True);
                Assert.That(operations.Any(operation => operation.TryGetProperty("mappingClause", out _)), Is.True);
                Assert.That(
                    operations.SelectMany(operation => operation.GetProperty("endpoints").EnumerateArray())
                        .Any(endpoint => endpoint.TryGetProperty("operationId", out _)),
                    Is.True);
            });
        }

        [Test]
        public void Every_operation_and_endpoint_matches_the_schema()
        {
            var operations = this.produced.RootElement.GetProperty("operations").EnumerateObject().ToList();

            Assert.That(operations, Is.Not.Empty);

            Assert.Multiple(() =>
            {
                foreach (var operation in operations.Select(entry => entry.Value))
                {
                    this.AssertMatches(operation, "operation");

                    foreach (var clause in operation.GetProperty("clauses").EnumerateArray())
                    {
                        this.AssertMatches(clause, "clauseEdge");
                    }

                    foreach (var endpoint in operation.GetProperty("endpoints").EnumerateArray())
                    {
                        this.AssertMatches(endpoint, "endpointEdge");
                    }
                }
            });
        }

        [Test]
        public void Every_model_mapping_row_matches_the_schema()
        {
            var rows = this.produced.RootElement.GetProperty("modelMapping").EnumerateArray().ToList();

            Assert.That(rows, Is.Not.Empty);

            Assert.Multiple(() =>
            {
                foreach (var row in rows)
                {
                    this.AssertMatches(row, "modelMappingRow");
                }
            });
        }

        [Test]
        public void The_methods_are_exactly_the_ones_the_schema_enumerates()
        {
            var definitions = this.schema.RootElement.GetProperty("$defs");

            var enumerated = Enumerated(definitions.GetProperty("clauseEdge").GetProperty("properties").GetProperty("method"))
                .Concat(Enumerated(
                    definitions.GetProperty("schemaEdge").GetProperty("properties").GetProperty("responseMethod")))
                .Concat(Enumerated(
                    definitions.GetProperty("schemaEdge").GetProperty("properties").GetProperty("requestMethod")));

            Assert.That(
                enumerated,
                Is.EquivalentTo(new[]
                {
                    ApiCrossReferenceDocument.TitleMethod,
                    ApiCrossReferenceDocument.DefinitionMethod,
                    ApiCrossReferenceDocument.BoldMentionMethod,
                    ApiCrossReferenceDocument.SignatureMethod,
                    ApiCrossReferenceDocument.MappingTableMethod,
                    ApiCrossReferenceDocument.SameNameMethod,
                    ApiCrossReferenceDocument.TypeConstantMethod,
                }));
        }

        [Test]
        public void The_kinds_the_builder_writes_are_ones_the_schema_enumerates()
        {
            var kinds = Enumerated(
                this.schema.RootElement.GetProperty("$defs").GetProperty("type").GetProperty("properties").GetProperty("kind"));

            Assert.That(
                this.produced.RootElement.GetProperty("types").EnumerateObject()
                    .Select(type => type.Value.GetProperty("kind").GetString()).Distinct(),
                Is.EquivalentTo(kinds));
        }

        /// <summary>
        /// A produced object carries every member the definition requires and none it does not declare.
        /// A subset rather than an equal set: members whose value is null are left out of the document.
        /// </summary>
        private void AssertMatches(JsonElement produced, string definition)
        {
            var declared = this.schema.RootElement.GetProperty("$defs").GetProperty(definition);
            var keys = Keys(produced);

            Assert.That(keys, Is.SupersetOf(Required(declared)), $"{definition}: a required member is missing");
            Assert.That(
                keys,
                Is.SubsetOf(Keys(declared.GetProperty("properties"))),
                $"{definition}: a member the schema does not declare");
        }

        private static List<string> Keys(JsonElement element) =>
            element.EnumerateObject().Select(property => property.Name).ToList();

        private static List<string> Required(JsonElement element) =>
            element.TryGetProperty("required", out var required)
                ? required.EnumerateArray().Select(name => name.GetString()!).ToList()
                : [];

        private static List<string> Enumerated(JsonElement element) =>
            element.GetProperty("enum").EnumerateArray().Select(value => value.GetString()!).ToList();
    }
}
