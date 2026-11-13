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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
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
                Assert.That(this.client.ServerInfo?.Name, Is.EqualTo("hypha"));
                Assert.That(this.client.ServerInstructions, Does.Contain("NORMATIVE").And.Contain("hypha_metaclass"));
            });
        }

        [Test]
        public async Task It_lists_its_read_tools()
        {
            var tools = await this.client.ListToolsAsync();

            Assert.That(tools.Select(tool => tool.Name).ToList(), Has.Count.EqualTo(15).And.Contains("hypha_metaclass").And.Contains("hypha_install_release"));
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
        public async Task It_switches_the_default_release_under_the_lock()
        {
            var result = await this.client.CallToolAsync("hypha_use_release", new Dictionary<string, object?> { ["tag"] = Tag });

            using var answer = JsonDocument.Parse(Text(result));

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.Not.True, Text(result));
                Assert.That(answer.RootElement.GetProperty("default").GetString(), Is.EqualTo(Tag));
                Assert.That(answer.RootElement.GetProperty("changed").GetBoolean(), Is.False);
                Assert.That(File.Exists(Path.Combine(this.knowledge.Root.FullName, ReleaseOperationLock.FileName)), Is.False, "the lock is released");
            });
        }

        [Test]
        public async Task It_refuses_to_remove_the_only_release_without_force()
        {
            var result = await this.client.CallToolAsync("hypha_remove_release", new Dictionary<string, object?> { ["tag"] = Tag });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.True);
                Assert.That(Text(result), Does.Contain("only installed release"));
                Assert.That(this.knowledge.Layout.Knowledge(Tag).Exists, Is.True);
            });
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
        public async Task Serve_writes_nothing_but_the_protocol_to_standard_output_and_exits_when_the_client_leaves()
        {
            // The SDK's client skips a line it cannot parse, so a banner or a log line on standard output
            // would not break a handshake; this reads standard output itself. Debug logging makes the
            // server as chatty as it gets.
            const string Initialize =
                "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2025-06-18\","
                + "\"capabilities\":{},\"clientInfo\":{\"name\":\"test\",\"version\":\"1.0\"}}}";

            using var serve = Start(["serve", "--repository-root", this.knowledge.Root.FullName, "--log-level", "Debug"]);
            var standardError = serve.StandardError.ReadToEndAsync();

            await serve.StandardInput.WriteLineAsync(Initialize);
            await serve.StandardInput.FlushAsync();
            var response = await serve.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromMinutes(1));
            serve.StandardInput.Close();
            var rest = await serve.StandardOutput.ReadToEndAsync().WaitAsync(TimeSpan.FromMinutes(1));
            await serve.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(1));

            var lines = new[] { response }.Concat(rest.Split('\n')).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(serve.ExitCode, Is.Zero, standardError.Result);
                Assert.That(lines, Is.Not.Empty);
                Assert.That(lines, Is.All.Matches<string>(line => line is not null && IsJsonRpc(line)), string.Join(Environment.NewLine, lines));
                Assert.That(standardError.Result, Is.Not.Empty, "the log should have gone to standard error");
            });
        }

        [Test]
        public async Task A_relative_HYPHA_HOME_stops_serve_with_the_reason_on_standard_error()
        {
            // Outside any checkout and without --repository-root, the knowledge folder is HYPHA_HOME's.
            var outside = Directory.CreateTempSubdirectory("hypha-serve-");
            try
            {
                using var serve = Start(["serve"], outside, new Dictionary<string, string> { [HyphaHome.EnvironmentVariable] = "relative-home" });
                var standardOutput = serve.StandardOutput.ReadToEndAsync();
                var standardError = serve.StandardError.ReadToEndAsync();
                await serve.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(1));

                Assert.Multiple(() =>
                {
                    Assert.That(serve.ExitCode, Is.EqualTo(1));
                    Assert.That(standardError.Result, Does.Contain("HYPHA_HOME must be an absolute path").And.Contain("relative-home"));
                    Assert.That(standardOutput.Result, Is.Empty);
                });
            }
            finally
            {
                outside.Delete(recursive: true);
            }
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

            Assert.That(questions, Is.All.Matches<string>(question => question is not null && verbs.Contains(question)));
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

        private static Process Start(string[] arguments, DirectoryInfo? workingDirectory = null, IReadOnlyDictionary<string, string>? environment = null)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory?.FullName ?? Environment.CurrentDirectory,
            };

            start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "hypha.dll"));
            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            foreach (var (name, value) in environment ?? new Dictionary<string, string>())
            {
                start.Environment[name] = value;
            }

            return Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        }

        private static bool IsJsonRpc(string line)
        {
            try
            {
                using var message = JsonDocument.Parse(line);

                return message.RootElement.TryGetProperty("jsonrpc", out var version) && version.GetString() == "2.0";
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string Text(CallToolResult result) =>
            string.Concat(result.Content.OfType<TextContentBlock>().Select(block => block.Text));
    }
}
