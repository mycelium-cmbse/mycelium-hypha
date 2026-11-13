// ------------------------------------------------------------------------------------------------
// <copyright file="QueryJsonContextTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization.Metadata;

    using Hypha.Knowledge.Query;

    // hypha query --json, and later the MCP tools, write every answer through QueryJsonContext, so it
    // must hold the result type of every query, and write it the way the generated knowledge is written.
    [TestFixture]
    public class QueryJsonContextTests
    {
        private QueryFixture fixture = null!;

        [SetUp]
        public void SetUp() => this.fixture = new QueryFixture();

        [TearDown]
        public void TearDown() => this.fixture.Dispose();

        [Test]
        public void The_context_holds_the_result_of_every_query()
        {
            var results = typeof(IKnowledgeQuery).GetMethods()
                .Concat(typeof(IApiQuery).GetMethods())
                .Select(method => method.ReturnType)
                .Distinct()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.GreaterThan(20));

                foreach (var type in results)
                {
                    Assert.That(QueryJsonContext.Default.GetTypeInfo(type), Is.Not.Null, type.ToString());
                    Assert.That(QueryJsonContext.Readable.GetTypeInfo(type), Is.Not.Null, type.ToString());
                }
            });
        }

        [Test]
        public void A_result_is_written_in_camel_case_with_its_grounding()
        {
            var node = Serialize(this.fixture.Query.Subclasses("Usage"));

            Assert.Multiple(() =>
            {
                Assert.That(node.AsObject().Select(property => property.Key), Is.EquivalentTo(new[] { "value", "grounding", "unavailable", "found", "available" }));
                Assert.That(node["value"]!.AsArray(), Is.Not.Empty);
                Assert.That((string?)node["grounding"]!["tier"], Is.EqualTo("DERIVED"));
                Assert.That((string?)node["grounding"]!["release"], Is.EqualTo(QueryFixture.Tag));
                Assert.That((string?)node["grounding"]!["source"], Is.EqualTo($"knowledge/{QueryFixture.Tag}/metamodel/metamodel.json"));
                Assert.That((bool?)node["found"], Is.True);
            });
        }

        [Test]
        public void An_answer_that_is_not_found_keeps_its_grounding_and_a_null_value()
        {
            var node = Serialize(this.fixture.Query.Element("NoSuchElement"));

            Assert.Multiple(() =>
            {
                Assert.That(node["value"], Is.Null);
                Assert.That((bool?)node["found"], Is.False);
                Assert.That(node["grounding"], Is.Not.Null);
            });
        }

        [Test]
        public void A_search_match_is_written_by_name()
        {
            var node = Serialize(this.fixture.Query.Search("PartUsage"));

            Assert.That((string?)node["value"]![0]!["match"], Is.EqualTo("exact"));
        }

        [Test]
        public void An_address_is_written_with_the_uri_a_client_opens()
        {
            var node = Serialize(this.fixture.Query.Search("PartUsage"));

            Assert.That((string?)node["value"]![0]!["address"]!["uri"], Is.EqualTo($"hypha://{QueryFixture.Tag}/metamodel/PartUsage"));
        }

        [Test]
        public void A_specification_match_is_written_with_its_address_occurrences_and_excerpt()
        {
            var node = Serialize(this.fixture.Query.SearchSpecification("text of clause"));
            var first = node["value"]![0]!;

            Assert.Multiple(() =>
            {
                Assert.That((string?)first["address"]!["uri"], Does.StartWith($"hypha://{QueryFixture.Tag}/kerml/"));
                Assert.That((int?)first["occurrences"], Is.EqualTo(1));
                Assert.That((string?)first["excerpt"], Does.Contain("text of clause"));
            });
        }

        [Test]
        public void The_readable_context_writes_operators_as_they_are()
        {
            var result = new QueryResult<IReadOnlyList<string>>([":>>", "=>"], new Grounding("DERIVED", QueryFixture.Tag, "knowledge/x.json"));
            var type = (JsonTypeInfo<QueryResult<IReadOnlyList<string>>>)QueryJsonContext.Readable.GetTypeInfo(result.GetType())!;
            var strict = (JsonTypeInfo<QueryResult<IReadOnlyList<string>>>)QueryJsonContext.Default.GetTypeInfo(result.GetType())!;

            Assert.Multiple(() =>
            {
                Assert.That(JsonSerializer.Serialize(result, type), Does.Contain("[\":>>\",\"=>\"]"));
                Assert.That(JsonSerializer.Serialize(result, strict), Does.Not.Contain(":>>"));
            });
        }

        private static JsonNode Serialize<T>(QueryResult<T> result)
            where T : class =>
            JsonSerializer.SerializeToNode(result, (JsonTypeInfo<QueryResult<T>>)QueryJsonContext.Default.GetTypeInfo(typeof(QueryResult<T>))!)!;
    }
}
