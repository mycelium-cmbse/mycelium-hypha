// ------------------------------------------------------------------------------------------------
// <copyright file="RegistrationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Lookup;
    using Hypha.Mcp;
    using Hypha.Mcp.Tools;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using ModelContextProtocol.Server;

    [TestFixture]
    public class RegistrationTests
    {
        private static readonly string[] ReadTools =
        [
            "hypha_release_status",
            "hypha_metaclass",
            "hypha_feature",
            "hypha_typed_by",
            "hypha_package",
            "hypha_syntax",
            "hypha_spec",
            "hypha_search",
            "hypha_resolve",
            "hypha_lookup",
            "hypha_diff",
            "hypha_api",
        ];

        private ServiceProvider provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHyphaMcp();
            this.provider = services.BuildServiceProvider();
        }

        [TearDown]
        public async Task TearDown() => await this.provider.DisposeAsync();

        private IReadOnlyList<McpServerTool> Tools => this.provider.GetServices<McpServerTool>().ToList();

        [Test]
        public void Every_read_tool_is_registered_and_nothing_else()
        {
            Assert.That(this.Tools.Select(tool => tool.ProtocolTool.Name), Is.EquivalentTo(ReadTools));
        }

        [Test]
        public void Every_tool_is_described_and_marked_read_only_idempotent_and_closed_world()
        {
            Assert.Multiple(() =>
            {
                foreach (var tool in this.Tools.Select(tool => tool.ProtocolTool))
                {
                    Assert.That(tool.Description, Is.Not.Null.And.Length.GreaterThan(40), tool.Name);
                    Assert.That(tool.Annotations?.ReadOnlyHint, Is.True, tool.Name);
                    Assert.That(tool.Annotations?.IdempotentHint, Is.True, tool.Name);
                    Assert.That(tool.Annotations?.DestructiveHint, Is.False, tool.Name);
                    Assert.That(tool.Annotations?.OpenWorldHint, Is.False, tool.Name);
                }
            });
        }

        [Test]
        public void The_instructions_name_every_tool_and_no_other()
        {
            var instructions = this.provider.GetRequiredService<IOptions<McpServerOptions>>().Value.ServerInstructions;

            var named = Regex.Matches(instructions!, @"\bhypha_[a-z_]+\b").Select(match => match.Value).Distinct();

            Assert.That(named, Is.EquivalentTo(ReadTools));
        }

        [Test]
        public void The_instructions_explain_every_grounding_tier()
        {
            Assert.That(
                ServerInstructions.Text,
                Does.Contain("NORMATIVE").And.Contain("MODEL").And.Contain("DERIVED").And.Contain("unavailable"));
        }

        [Test]
        public void The_server_names_itself_hypha()
        {
            var options = this.provider.GetRequiredService<IOptions<McpServerOptions>>().Value;

            Assert.Multiple(() =>
            {
                Assert.That(options.ServerInfo?.Name, Is.EqualTo("hypha"));
                Assert.That(options.ServerInfo?.Version, Is.Not.Empty);
            });
        }

        [TestCase("hypha_metaclass", "question", typeof(MetaclassQuestion))]
        [TestCase("hypha_feature", "question", typeof(FeatureQuestion))]
        [TestCase("hypha_typed_by", "question", typeof(TypedByQuestion))]
        [TestCase("hypha_syntax", "question", typeof(SyntaxQuestion))]
        [TestCase("hypha_spec", "question", typeof(SpecificationQuestion))]
        [TestCase("hypha_api", "question", typeof(ApiQuestion))]
        [TestCase("hypha_lookup", "kind", typeof(KnowledgeKind))]
        public void An_enumerated_argument_offers_exactly_the_names_of_its_values(string tool, string argument, Type values)
        {
            var schema = this.Tools.Single(registered => registered.ProtocolTool.Name == tool).ProtocolTool.InputSchema;
            var offered = schema.GetProperty("properties").GetProperty(argument).GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString());

            var names = values.GetFields().Where(field => field.IsLiteral)
                .Select(field => field.GetCustomAttributes(typeof(JsonStringEnumMemberNameAttribute), false)
                    .Cast<JsonStringEnumMemberNameAttribute>().Single().Name);

            Assert.That(offered, Is.EquivalentTo(names));
        }
    }
}
