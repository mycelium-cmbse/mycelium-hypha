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

        private static JsonObject? Answer(string line) => FallbackServer.Answer(line, Failure, "1.6.0");
    }
}
