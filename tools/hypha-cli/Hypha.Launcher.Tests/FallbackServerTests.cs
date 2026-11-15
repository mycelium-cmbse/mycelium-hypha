// ------------------------------------------------------------------------------------------------
// <copyright file="FallbackServerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System.IO;
    using System.Linq;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;

    [TestFixture]
    public class FallbackServerTests
    {
        private const string Failure = "hypha 1.6.0 could not be downloaded: no route. Check the network.";

        [Test]
        public void Initialize_answers_as_hypha_with_the_reason_in_the_instructions_and_the_client_protocol_version()
        {
            var answer = Answer("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-03-26","capabilities":{}}}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((int?)answer["id"], Is.EqualTo(1));
                Assert.That((string?)answer["result"]!["protocolVersion"], Is.EqualTo("2025-03-26"));
                Assert.That((string?)answer["result"]!["serverInfo"]!["name"], Is.EqualTo("hypha"));
                Assert.That((string?)answer["result"]!["instructions"], Does.Contain("hypha_status").And.Contain(Failure));
                Assert.That(answer["result"]!["capabilities"]!["tools"], Is.Not.Null);
            });
        }

        [Test]
        public void The_one_tool_is_hypha_status_and_it_says_why()
        {
            var list = Answer("""{"jsonrpc":"2.0","id":"a","method":"tools/list"}""")!;
            var call = Answer("""{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"hypha_status","arguments":{}}}""")!;

            Assert.Multiple(() =>
            {
                Assert.That(list["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Is.EqualTo(new[] { "hypha_status" }));
                Assert.That((string?)list["id"], Is.EqualTo("a"));
                Assert.That((string?)call["result"]!["content"]![0]!["text"], Is.EqualTo($"hypha could not start: {Failure}"));
                Assert.That((bool?)call["result"]!["isError"], Is.False);
            });
        }

        [Test]
        public void Another_tool_or_method_is_an_error_that_names_hypha_status()
        {
            var tool = Answer("""{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"hypha_metaclass"}}""")!;
            var method = Answer("""{"jsonrpc":"2.0","id":4,"method":"resources/list"}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((int?)tool["error"]!["code"], Is.EqualTo(-32602));
                Assert.That((int?)method["error"]!["code"], Is.EqualTo(-32601));
                Assert.That((string?)method["error"]!["message"], Does.Contain("hypha_status"));
            });
        }

        [Test]
        public void A_notification_gets_no_answer_and_a_line_that_is_not_json_a_parse_error()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Answer("""{"jsonrpc":"2.0","method":"notifications/initialized"}"""), Is.Null);
                Assert.That((int?)Answer("{not json")!["error"]!["code"], Is.EqualTo(-32700));
                Assert.That((string?)Answer("""{"jsonrpc":"2.0","id":5,"method":"ping"}""")!["result"]?.ToJsonString(), Is.EqualTo("{}"));
            });
        }

        [Test]
        public async Task It_answers_line_by_line_until_the_client_closes_standard_input()
        {
            using var input = new StringReader(
                """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}
                {"jsonrpc":"2.0","method":"notifications/initialized"}
                {"jsonrpc":"2.0","id":2,"method":"tools/list"}
                """);
            using var output = new StringWriter();

            await FallbackServer.RunAsync(input, output, Failure, "1.6.0");

            var lines = output.ToString().Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
            Assert.Multiple(() =>
            {
                Assert.That(lines, Has.Length.EqualTo(2));
                Assert.That((string?)JsonNode.Parse(lines[0])!["result"]!["serverInfo"]!["version"], Is.EqualTo("1.6.0"));
                Assert.That((string?)JsonNode.Parse(lines[0])!["result"]!["protocolVersion"], Is.EqualTo("2025-06-18"));
                Assert.That((int?)JsonNode.Parse(lines[1])!["id"], Is.EqualTo(2));
            });
        }

        [Test]
        public void A_ping_is_answered_with_an_empty_result()
        {
            var answer = Answer("""{"jsonrpc":"2.0","id":"p","method":"ping"}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((string?)answer["id"], Is.EqualTo("p"));
                Assert.That(answer["result"]!.AsObject(), Is.Empty);
                Assert.That(answer["error"], Is.Null);
            });
        }

        [TestCase("""[{"jsonrpc":"2.0","id":1,"method":"ping"}]""")]
        [TestCase("42")]
        [TestCase("\"initialize\"")]
        [TestCase("""{"jsonrpc":"2.0","id":null,"method":"ping"}""")]
        public void A_message_that_is_not_a_request_object_with_an_id_gets_no_answer(string line)
        {
            Assert.That(Answer(line), Is.Null);
        }

        [Test]
        public void A_tool_call_that_names_no_tool_is_an_invalid_params_error()
        {
            var answer = Answer("""{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"arguments":{}}}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((int?)answer["error"]!["code"], Is.EqualTo(-32602));
                Assert.That((string?)answer["error"]!["message"], Does.Contain("hypha_status"));
            });
        }

        [Test]
        public void A_method_that_is_not_a_string_is_method_not_found()
        {
            var answer = Answer("""{"jsonrpc":"2.0","id":6,"method":7}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((int?)answer["id"], Is.EqualTo(6));
                Assert.That((int?)answer["error"]!["code"], Is.EqualTo(-32601));
            });
        }

        [Test]
        public void An_initialize_whose_protocol_version_is_not_a_string_gets_the_default()
        {
            var answer = Answer("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":2025}}""")!;

            Assert.That((string?)answer["result"]!["protocolVersion"], Is.EqualTo("2025-06-18"));
        }

        [Test]
        public void While_hypha_is_set_up_the_lists_are_empty_and_announced_as_changing()
        {
            const string Progress = "hypha 1.6.0 is being downloaded.";

            var initialize = FallbackServer.Answer("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}""", Progress, "1.6.0", settingUp: true)!;
            var resources = FallbackServer.Answer("""{"jsonrpc":"2.0","id":2,"method":"resources/list"}""", Progress, "1.6.0", settingUp: true)!;
            var templates = FallbackServer.Answer("""{"jsonrpc":"2.0","id":3,"method":"resources/templates/list"}""", Progress, "1.6.0", settingUp: true)!;
            var prompts = FallbackServer.Answer("""{"jsonrpc":"2.0","id":4,"method":"prompts/list"}""", Progress, "1.6.0", settingUp: true)!;
            var status = FallbackServer.Answer(
                """{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"hypha_status","arguments":{}}}""", Progress, "1.6.0", settingUp: true)!;
            var other = FallbackServer.Answer("""{"jsonrpc":"2.0","id":6,"method":"completion/complete"}""", Progress, "1.6.0", settingUp: true)!;

            Assert.Multiple(() =>
            {
                foreach (var kind in new[] { "tools", "resources", "prompts" })
                {
                    Assert.That((bool?)initialize["result"]!["capabilities"]![kind]!["listChanged"], Is.True, kind);
                }

                Assert.That((string?)initialize["result"]!["instructions"], Does.Contain("starting for the first time").And.Contain(Progress));
                Assert.That(resources["result"]!["resources"]!.AsArray(), Is.Empty);
                Assert.That(templates["result"]!["resourceTemplates"]!.AsArray(), Is.Empty);
                Assert.That(prompts["result"]!["prompts"]!.AsArray(), Is.Empty);
                Assert.That((string?)status["result"]!["content"]![0]!["text"], Is.EqualTo(Progress), "how far it is, not that it could not start");
                Assert.That((string?)other["error"]!["message"], Does.Contain("not ready yet"));
            });
        }

        [Test]
        public void Once_hypha_could_not_start_the_lists_of_resources_and_prompts_are_not_offered()
        {
            var resources = Answer("""{"jsonrpc":"2.0","id":2,"method":"resources/list"}""")!;
            var prompts = Answer("""{"jsonrpc":"2.0","id":3,"method":"prompts/list"}""")!;

            Assert.Multiple(() =>
            {
                Assert.That((int?)resources["error"]!["code"], Is.EqualTo(-32601));
                Assert.That((int?)prompts["error"]!["code"], Is.EqualTo(-32601));
            });
        }

        private static JsonObject? Answer(string line) => FallbackServer.Answer(line, Failure, "1.6.0");
    }
}
