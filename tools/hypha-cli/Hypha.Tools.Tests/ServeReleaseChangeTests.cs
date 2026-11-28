// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using ModelContextProtocol;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;

    // The release tools through the real hypha serve over standard input and output, against a knowledge
    // folder holding two releases, so the default can change. Nothing here reaches the network.
    [TestFixture]
    public class ServeReleaseChangeTests
    {
        private const string Newer = "2026-05";

        private const string Older = "2026-04";

        private readonly ProgressNotifications progress = new();

        private TemporaryKnowledge knowledge = null!;
        private McpClient client = null!;

        [OneTimeSetUp]
        public async Task StartServer()
        {
            this.knowledge = new TemporaryKnowledge().Install(Newer, Older);
            Golden(this.knowledge.Layout, Newer);
            Golden(this.knowledge.Layout, Older);

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "hypha",
                Command = "dotnet",
                Arguments = [Path.Combine(AppContext.BaseDirectory, "hypha.dll"), "serve", "--repository-root", this.knowledge.Root.FullName],
            });

            // The progress handler is registered for the client's lifetime: the one CallToolAsync registers
            // for a call is removed as soon as the answer is handled, and the client handles each incoming
            // message on the thread pool, so a notification sent before the answer could still be dropped.
            var options = new McpClientOptions
            {
                Handlers = new McpClientHandlers
                {
                    NotificationHandlers = [new(NotificationMethods.ProgressNotification, this.progress.Receive)],
                },
            };

            this.client = await McpClient.CreateAsync(transport, options).WaitAsync(TimeSpan.FromMinutes(1));
        }

        [OneTimeTearDown]
        public async Task StopServer()
        {
            await this.client.DisposeAsync();
            this.knowledge.Dispose();
        }

        [Test]
        public async Task Switching_the_default_changes_what_every_tool_answers_from_and_what_is_recorded()
        {
            try
            {
                var use = await this.client.CallToolAsync("hypha_use_release", new Dictionary<string, object?> { ["tag"] = Older });
                var status = await this.client.CallToolAsync("hypha_release_status");
                var read = await this.client.CallToolAsync(
                    "hypha_metaclass", new Dictionary<string, object?> { ["question"] = "ancestors", ["name"] = "PartUsage" });

                using var switched = JsonDocument.Parse(Text(use));
                using var answer = JsonDocument.Parse(Text(read));

                Assert.Multiple(() =>
                {
                    Assert.That(use.IsError, Is.Not.True, Text(use));
                    Assert.That(switched.RootElement.GetProperty("default").GetString(), Is.EqualTo(Older));
                    Assert.That(switched.RootElement.GetProperty("changed").GetBoolean(), Is.True);
                    Assert.That(Text(status), Does.Contain($"\"default\":\"{Older}\""));
                    Assert.That(answer.RootElement.GetProperty("grounding").GetProperty("release").GetString(), Is.EqualTo(Older));
                    Assert.That(this.knowledge.Layout.DefaultTag, Is.EqualTo(Older), "versions.json records the new default");
                    Assert.That(File.Exists(Path.Combine(this.knowledge.Root.FullName, ReleaseOperationLock.FileName)), Is.False, "the lock is released");
                });
            }
            finally
            {
                await this.client.CallToolAsync("hypha_use_release", new Dictionary<string, object?> { ["tag"] = Newer });
            }
        }

        [Test]
        public async Task A_call_with_a_progress_token_is_sent_progress()
        {
            // A tag that is not shaped like a release is refused before anything goes upstream, so this runs
            // offline; the install says what it starts on before that. The server sends that progress before
            // its answer, but the client may handle the two in either order, so the order is not asserted.
            var token = Guid.NewGuid().ToString("N");

            var result = await this.client.CallToolAsync(new CallToolRequestParams
            {
                Name = "hypha_install_release",
                Arguments = new Dictionary<string, JsonElement>
                {
                    ["tag"] = JsonSerializer.SerializeToElement("not-a-release", McpJsonUtilities.DefaultOptions),
                },
                Meta = new JsonObject { ["progressToken"] = token },
            });

            var first = this.progress.First(new ProgressToken(token));
            var arrived = await Task.WhenAny(first, Task.Delay(TimeSpan.FromSeconds(30))) == first;

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.True, Text(result));
                Assert.That(Text(result), Does.Contain("not-a-release"));
                Assert.That(arrived, Is.True, "the client asked for progress and was sent none");
            });
            Assert.That((await first).Message, Does.Contain("not-a-release"));
        }

        [Test]
        public async Task A_call_without_a_progress_token_is_answered_all_the_same()
        {
            var result = await this.client.CallToolAsync("hypha_install_release", new Dictionary<string, object?> { ["tag"] = "not-a-release" });

            Assert.Multiple(() =>
            {
                Assert.That(result.IsError, Is.True);
                Assert.That(Text(result), Does.Contain("not-a-release"));
            });
        }

        private static void Golden(KnowledgeLayout layout, string tag)
        {
            var golden = new DirectoryInfo(Path.Combine(
                KnowledgeLayout.Discover()!.Root.FullName, "tools", "metamodel-gen", "Hypha.Knowledge.Metamodel.Tests", "Expected", "metamodel"));
            var metamodel = Directory.CreateDirectory(layout.Metamodel(tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "metamodel.json"), layout.MetamodelGraph(tag).FullName);
            File.Copy(Path.Combine(golden.FullName, "index.json"), layout.MetamodelIndex(tag).FullName);
            var elements = Directory.CreateDirectory(Path.Combine(metamodel.FullName, "elements"));
            foreach (var page in new DirectoryInfo(Path.Combine(golden.FullName, "elements")).EnumerateFiles("*.md"))
            {
                page.CopyTo(Path.Combine(elements.FullName, page.Name));
            }
        }

        private static string Text(CallToolResult result) =>
            string.Concat(result.Content.OfType<TextContentBlock>().Select(block => block.Text));

        // The first progress notification each token is sent, waited for by the token rather than by the
        // call, so it is recorded whenever it arrives.
        private sealed class ProgressNotifications
        {
            private readonly ConcurrentDictionary<ProgressToken, TaskCompletionSource<ProgressNotificationValue>> first = new();

            public Task<ProgressNotificationValue> First(ProgressToken token) => this.Of(token).Task;

            public ValueTask Receive(JsonRpcNotification notification, CancellationToken cancellationToken)
            {
                var received = JsonSerializer.Deserialize<ProgressNotificationParams>(notification.Params, McpJsonUtilities.DefaultOptions);
                if (received is not null)
                {
                    this.Of(received.ProgressToken).TrySetResult(received.Progress);
                }

                return ValueTask.CompletedTask;
            }

            private TaskCompletionSource<ProgressNotificationValue> Of(ProgressToken token) =>
                this.first.GetOrAdd(token, _ => new TaskCompletionSource<ProgressNotificationValue>(TaskCreationOptions.RunContinuationsAsynchronously));
        }
    }
}
