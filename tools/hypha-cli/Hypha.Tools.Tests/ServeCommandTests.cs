// ------------------------------------------------------------------------------------------------
// <copyright file="ServeCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Mcp.Tools;
    using Hypha.Tools.Commands;

    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;

    // Starts the real hypha serve over standard input and output, as an AI client does, against a
    // knowledge folder holding one release whose metamodel is the committed golden metamodel.json.
    [TestFixture]
    public class ServeCommandTests
    {
        private const string Tag = "2026-05";

        private TemporaryKnowledge knowledge = null!;
        private ConcurrentQueue<string> standardError = null!;
        private McpClient client = null!;

        [OneTimeSetUp]
        public async Task StartServer()
        {
            this.knowledge = new TemporaryKnowledge().Install(Tag);

            var golden = new DirectoryInfo(Path.Combine(
                KnowledgeLayout.Discover()!.Root.FullName, "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel"));
            var metamodel = Directory.CreateDirectory(this.knowledge.Layout.Metamodel(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), this.knowledge.Layout.MetamodelGraph(Tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "index.json"), this.knowledge.Layout.MetamodelIndex(Tag).FullName);
            var elements = Directory.CreateDirectory(Path.Combine(metamodel.FullName, "elements"));
            foreach (var page in new DirectoryInfo(Path.Combine(golden.FullName, "elements")).EnumerateFiles("*.md"))
            {
                page.CopyTo(Path.Combine(elements.FullName, page.Name));
            }

            this.standardError = new ConcurrentQueue<string>();

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "hypha",
                Command = "dotnet",
                Arguments = [Path.Combine(AppContext.BaseDirectory, "hypha.dll"), "serve", "--repository-root", this.knowledge.Root.FullName],
                StandardErrorLines = line => this.standardError.Enqueue(line),
            });

            this.client = await McpClient.CreateAsync(transport).WaitAsync(TimeSpan.FromMinutes(1));
        }

        [OneTimeTearDown]
        public async Task StopServer()
        {
            await this.client.DisposeAsync();
            this.knowledge.Dispose();
        }

        [Test]
        public void The_server_introduces_itself_with_its_instructions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.client.ServerInfo.Name, Is.EqualTo("hypha"));
                Assert.That(this.client.ServerInstructions, Does.Contain("NORMATIVE").And.Contain("hypha_metaclass"));
            });
        }

        [Test]
        public async Task It_lists_its_read_tools()
        {
            var tools = await this.client.ListToolsAsync();

            Assert.That(tools.Select(tool => tool.Name).ToList(), Has.Count.EqualTo(12).And.Contains("hypha_metaclass").And.Contains("hypha_lookup"));
        }

        [Test]
        public async Task It_answers_from_the_knowledge_folder_it_was_given()
        {
            var result = await this.client.CallToolAsync(
                "hypha_metaclass", new Dictionary<string, object?> { ["question"] = "ancestors", ["name"] = "PartUsage" });

            using var answer = JsonDocument.Parse(Text(result));

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.Not.True);
                Assert.That(answer.RootElement.GetProperty("value").EnumerateArray().Select(name => name.GetString()), Does.Contain("Usage"));
                Assert.That(answer.RootElement.GetProperty("grounding").GetProperty("release").GetString(), Is.EqualTo(Tag));
            });
        }

        [Test]
        public async Task It_reports_the_installed_release()
        {
            var result = await this.client.CallToolAsync("hypha_release_status");

            Assert.That(Text(result), Does.Contain($"\"default\":\"{Tag}\""));
        }

        [Test]
        public async Task A_question_it_cannot_answer_is_a_tool_error_the_model_can_read()
        {
            var result = await this.client.CallToolAsync(
                "hypha_metaclass", new Dictionary<string, object?> { ["question"] = "ancestors", ["name"] = "PartUsage", ["tag"] = "2099-01" });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.True);
                Assert.That(Text(result), Does.Contain("2099-01"));
            });
        }

        [Test]
        public void Nothing_but_the_protocol_reached_standard_output()
        {
            // A banner or a log line on standard output would have broken the handshake in StartServer;
            // the log goes to standard error instead.
            Assert.That(this.client.ServerInfo, Is.Not.Null);
        }

        [Test]
        public void Every_question_a_tool_offers_is_a_hypha_query_verb()
        {
            // The MCP tools and the CLI answer the same questions under the same names, so a model and a
            // person can move between them; a renamed verb or question fails here.
            var query = new QueryCommand();
            var verbs = query.Verbs.Select(verb => verb.Name).ToHashSet(StringComparer.Ordinal);
            var questions = new[] { typeof(MetaclassQuestion), typeof(FeatureQuestion), typeof(TypedByQuestion), typeof(SyntaxQuestion), typeof(SpecificationQuestion), typeof(ApiQuestion) }
                .SelectMany(type => type.GetFields().Where(field => field.IsLiteral))
                .Select(field => field.GetCustomAttributes(typeof(JsonStringEnumMemberNameAttribute), false)
                    .Cast<JsonStringEnumMemberNameAttribute>().Single().Name);

            Assert.That(questions, Is.All.Matches<string>(verbs.Contains));
        }

        [Test]
        public void Serve_owns_standard_output_and_other_verbs_do_not()
        {
            var root = Program.BuildRootCommand();

            Assert.Multiple(() =>
            {
                Assert.That(ServeCommand.Owns(root.Parse(["serve"])), Is.True);
                Assert.That(ServeCommand.Owns(root.Parse(["list"])), Is.False);
            });
        }

        private static string Text(CallToolResult result) =>
            string.Concat(result.Content.OfType<TextContentBlock>().Select(block => block.Text));
    }
}
