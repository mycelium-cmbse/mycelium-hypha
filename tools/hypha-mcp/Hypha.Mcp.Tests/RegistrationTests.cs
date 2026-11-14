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
            "hypha_search_spec",
            "hypha_resolve",
            "hypha_lookup",
            "hypha_diff",
            "hypha_api",
        ];

        private static readonly string[] ChangeTools =
        [
            "hypha_install_release",
            "hypha_use_release",
            "hypha_remove_release",
        ];

        private static readonly string[] UpstreamTools = ["hypha_check_releases"];

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

        private IReadOnlyList<McpServerPrompt> Prompts => this.provider.GetServices<McpServerPrompt>().ToList();

        [Test]
        public void Every_read_tool_and_release_change_tool_is_registered_and_nothing_else()
        {
            Assert.That(this.Tools.Select(tool => tool.ProtocolTool.Name), Is.EquivalentTo(ReadTools.Concat(ChangeTools).Concat(UpstreamTools)));
        }

        [Test]
        public void Every_read_tool_is_described_and_marked_read_only_idempotent_and_closed_world()
        {
            Assert.Multiple(() =>
            {
                foreach (var tool in this.Tools.Select(tool => tool.ProtocolTool).Where(tool => ReadTools.Contains(tool.Name)))
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
        public void The_upstream_check_reads_only_but_reaches_GitHub()
        {
            var tool = this.Tools.Single(registered => registered.ProtocolTool.Name == "hypha_check_releases").ProtocolTool;

            Assert.Multiple(() =>
            {
                Assert.That(tool.Description, Is.Not.Null.And.Contains("GitHub").And.Contains("hypha_install_release").And.Contains("axis 'api'"));
                Assert.That(tool.Annotations?.ReadOnlyHint, Is.True);
                Assert.That(tool.Annotations?.IdempotentHint, Is.True);
                Assert.That(tool.Annotations?.DestructiveHint, Is.False);
                Assert.That(tool.Annotations?.OpenWorldHint, Is.True);
            });
        }

        [TestCase("hypha_install_release", false, true, true)]
        [TestCase("hypha_use_release", false, true, false)]
        [TestCase("hypha_remove_release", true, false, false)]
        public void A_release_change_tool_says_it_writes_and_how(string name, bool destructive, bool idempotent, bool openWorld)
        {
            var tool = this.Tools.Single(registered => registered.ProtocolTool.Name == name).ProtocolTool;

            Assert.Multiple(() =>
            {
                Assert.That(tool.Description, Is.Not.Null.And.Contains("Only when the user asks"));
                Assert.That(tool.Annotations?.ReadOnlyHint, Is.False);
                Assert.That(tool.Annotations?.DestructiveHint, Is.EqualTo(destructive));
                Assert.That(tool.Annotations?.IdempotentHint, Is.EqualTo(idempotent));
                Assert.That(tool.Annotations?.OpenWorldHint, Is.EqualTo(openWorld));
            });
        }

        [Test]
        public void The_install_schema_holds_its_arguments_and_not_the_progress_or_the_cancellation()
        {
            var install = this.Tools.Single(tool => tool.ProtocolTool.Name == "hypha_install_release").ProtocolTool.InputSchema;
            var properties = install.GetProperty("properties").EnumerateObject().Select(property => property.Name);

            Assert.That(properties, Is.EquivalentTo(new[] { "tag", "includeSpecifications", "makeDefault", "axis" }));
        }

        [Test]
        public void The_instructions_name_every_tool_and_no_other()
        {
            var instructions = this.provider.GetRequiredService<IOptions<McpServerOptions>>().Value.ServerInstructions;

            var named = Regex.Matches(instructions!, @"\bhypha_[a-z_]+\b").Select(match => match.Value).Distinct();

            Assert.That(named, Is.EquivalentTo(ReadTools.Concat(ChangeTools).Concat(UpstreamTools)));
        }

        [Test]
        public void The_instructions_explain_every_grounding_tier()
        {
            Assert.That(
                ServerInstructions.Text,
                Does.Contain("NORMATIVE").And.Contain("MODEL").And.Contain("DERIVED").And.Contain("unavailable"));
        }

        [Test]
        public void Every_hypha_address_is_one_resource_template_of_markdown_pages()
        {
            var templates = this.provider.GetServices<McpServerResource>().Select(resource => resource.ProtocolResourceTemplate).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(templates.Select(template => template.UriTemplate), Is.EqualTo(new[] { "hypha://{tag}/{kind}/{name}" }));
                Assert.That(templates[0].MimeType, Is.EqualTo("text/markdown"));
                Assert.That(templates[0].Description, Is.Not.Null.And.Contains("model-library"));
            });
        }

        [Test]
        public void The_guided_flows_are_registered_as_prompts_with_their_arguments_required()
        {
            var prompts = this.Prompts.Select(prompt => prompt.ProtocolPrompt).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(
                    prompts.Select(prompt => prompt.Name),
                    Is.EquivalentTo(new[] { "overview", "explain_metaclass", "cite_specification", "validate_notation", "api_lookup" }));
                Assert.That(prompts.Select(prompt => prompt.Description), Is.All.Not.Null.And.All.Length.GreaterThan(30));
                Assert.That(prompts.SelectMany(prompt => prompt.Arguments ?? []).Select(argument => argument.Required), Is.All.True);
                Assert.That(prompts.Single(prompt => prompt.Name == "overview").Arguments ?? [], Is.Empty);
            });
        }

        [Test]
        public void The_prompts_name_only_tools_the_server_has()
        {
            var texts = new[]
            {
                Hypha.Mcp.Prompts.KnowledgePrompts.Overview(),
                Hypha.Mcp.Prompts.KnowledgePrompts.ExplainMetaclass("PartUsage"),
                Hypha.Mcp.Prompts.KnowledgePrompts.CiteSpecification("redefinition"),
                Hypha.Mcp.Prompts.KnowledgePrompts.ValidateNotation("part def Vehicle;"),
                Hypha.Mcp.Prompts.KnowledgePrompts.ApiLookup("Which operation creates a project?"),
            };
            var registered = this.Tools.Select(tool => tool.ProtocolTool.Name).ToHashSet();

            var named = texts.SelectMany(text => Regex.Matches(text, "hypha_[a-z_]+").Select(match => match.Value)).Distinct();

            Assert.That(named, Is.All.Matches<string>(name => name is not null && registered.Contains(name)));
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
        [TestCase("hypha_install_release", "axis", typeof(VersionAxis))]
        [TestCase("hypha_use_release", "axis", typeof(VersionAxis))]
        [TestCase("hypha_remove_release", "axis", typeof(VersionAxis))]
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
