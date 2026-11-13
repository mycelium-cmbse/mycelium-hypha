// ------------------------------------------------------------------------------------------------
// <copyright file="ServeInstallLiveTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;

    // Installs a real release through hypha serve into an empty folder, reading the protocol off standard
    // output line by line: the one place the progress notifications are seen as the client receives them,
    // in the order they arrive. (The SDK's own client hands notifications to their handlers side by side,
    // so it cannot show the order.) Without the specification PDFs, so it needs neither uv nor Python.
    [TestFixture]
    [Explicit("Hits the network; run by hand when changing the release tools.")]
    [Category("Live")]
    public class ServeInstallLiveTests
    {
        private const string Tag = "2026-05";

        [Test]
        public async Task Installing_a_release_reports_progress_in_order_and_the_query_tools_answer_from_it_at_once()
        {
            var root = Directory.CreateTempSubdirectory("hypha-serve-install-");
            try
            {
                var start = new ProcessStartInfo("dotnet")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                foreach (var argument in new[] { Path.Combine(AppContext.BaseDirectory, "hypha.dll"), "serve", "--repository-root", root.FullName })
                {
                    start.ArgumentList.Add(argument);
                }

                using var serve = Process.Start(start)!;
                var standardError = serve.StandardError.ReadToEndAsync();

                await Send(serve, Request(1, "initialize", new JsonObject
                {
                    ["protocolVersion"] = "2025-06-18",
                    ["capabilities"] = new JsonObject(),
                    ["clientInfo"] = new JsonObject { ["name"] = "test", ["version"] = "1.0" },
                }));
                await Until(serve, 1);
                await Send(serve, new JsonObject { ["jsonrpc"] = "2.0", ["method"] = "notifications/initialized" });

                await Send(serve, Request(2, "tools/call", new JsonObject
                {
                    ["name"] = "hypha_install_release",
                    ["arguments"] = new JsonObject { ["tag"] = Tag, ["includeSpecifications"] = false },
                    ["_meta"] = new JsonObject { ["progressToken"] = "install" },
                }));
                var install = await Until(serve, 2);

                await Send(serve, Request(3, "tools/call", new JsonObject
                {
                    ["name"] = "hypha_metaclass",
                    ["arguments"] = new JsonObject { ["question"] = "ancestors", ["name"] = "PartUsage" },
                }));
                var ancestors = await Until(serve, 3);

                serve.StandardInput.Close();
                await serve.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(1));

                var notes = install.Before
                    .Where(message => message["method"]?.GetValue<string>() == "notifications/progress")
                    .Select(message => message["params"]!)
                    .ToList();
                var progress = notes.Select(note => note["progress"]!.GetValue<double>()).ToList();
                var answer = install.Response["result"]!;

                Assert.Multiple(() =>
                {
                    Assert.That(answer["isError"]?.GetValue<bool>(), Is.Not.True, answer.ToJsonString() + Environment.NewLine + standardError.Result);
                    Assert.That(Text(answer), Does.Contain($"\"tag\":\"{Tag}\"").And.Contain("\"artifact\":\"metamodel\""));
                    Assert.That(notes.Select(note => note["progressToken"]!.GetValue<string>()), Is.All.EqualTo("install"));
                    Assert.That(progress, Has.Count.GreaterThan(10));
                    Assert.That(progress, Is.Ordered.Ascending);
                    Assert.That(progress.Distinct().Count(), Is.EqualTo(progress.Count), "progress must grow with every notification");
                    Assert.That(notes.Select(note => note["message"]!.GetValue<string>()), Has.Some.StartWith("fetch textual").And.Some.StartWith("generate 2026-05"));
                    Assert.That(Text(ancestors.Response["result"]!), Does.Contain("Usage"));
                });
            }
            finally
            {
                root.Delete(recursive: true);
            }
        }

        private static JsonObject Request(int id, string method, JsonObject parameters) =>
            new() { ["jsonrpc"] = "2.0", ["id"] = id, ["method"] = method, ["params"] = parameters };

        private static async Task Send(Process serve, JsonObject message)
        {
            await serve.StandardInput.WriteLineAsync(message.ToJsonString());
            await serve.StandardInput.FlushAsync();
        }

        // Reads standard output until the response to the request with this id, keeping what came first.
        private static async Task<(List<JsonNode> Before, JsonNode Response)> Until(Process serve, int id)
        {
            var before = new List<JsonNode>();

            while (await serve.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromMinutes(10)) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var message = JsonNode.Parse(line)!;
                if (message["id"]?.GetValueKind() == JsonValueKind.Number && message["id"]!.GetValue<int>() == id)
                {
                    return (before, message);
                }

                before.Add(message);
            }

            throw new InvalidOperationException($"the server closed standard output before answering request {id}");
        }

        private static string Text(JsonNode result) =>
            string.Concat(result["content"]!.AsArray().Select(block => block!["text"]?.GetValue<string>()));
    }
}
