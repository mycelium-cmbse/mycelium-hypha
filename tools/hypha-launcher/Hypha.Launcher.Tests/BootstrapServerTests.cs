// ------------------------------------------------------------------------------------------------
// <copyright file="BootstrapServerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;

    // Drives the bootstrap server as a client would, line by line, and hands it over to the hypha this
    // solution built.
    [TestFixture]
    public class BootstrapServerTests
    {
        private const string Initialize =
            """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}""";

        private DirectoryInfo workspace = null!;
        private Lines toServer = null!;
        private Lines fromServer = null!;
        private TaskCompletionSource<LaunchTarget> download = null!;
        private Task<int> server = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-bootstrap-{Guid.NewGuid():N}"));
            this.toServer = new Lines();
            this.fromServer = new Lines();
            this.download = new TaskCompletionSource<LaunchTarget>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.server = BootstrapServer.RunAsync(this.toServer, this.fromServer.AsWriter(), this.download.Task, this.Start, "1.6.0");
        }

        [TearDown]
        public async Task TearDown()
        {
            this.toServer.Close();
            await this.server.WaitAsync(TimeSpan.FromMinutes(1));
            this.server.Dispose();
            this.toServer.Dispose();
            this.fromServer.Dispose();
            this.workspace.Delete(recursive: true);
        }

        [Test]
        public async Task The_client_is_answered_at_once_and_gets_the_real_tools_once_hypha_is_there()
        {
            this.toServer.Send(Initialize);
            var initialize = await this.fromServer.Next();
            this.toServer.Send("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
            this.toServer.Send("""{"jsonrpc":"2.0","id":2,"method":"tools/list"}""");
            var during = await this.fromServer.Next();

            this.download.SetResult(LaunchTarget.Found(TestHypha.Executable(), "1.6.0"));
            var changed = new[] { await this.fromServer.Next(), await this.fromServer.Next(), await this.fromServer.Next() };
            this.toServer.Send("""{"jsonrpc":"2.0","id":3,"method":"tools/list"}""");
            var after = await this.fromServer.Next();

            Assert.Multiple(() =>
            {
                Assert.That((string?)initialize["result"]!["serverInfo"]!["version"], Is.EqualTo("1.6.0"));
                Assert.That((bool?)initialize["result"]!["capabilities"]!["tools"]!["listChanged"], Is.True);
                Assert.That((string?)initialize["result"]!["instructions"], Does.Contain("being downloaded"));
                Assert.That(during["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Is.EqualTo(new[] { "hypha_status" }));
                Assert.That(
                    changed.Select(message => (string?)message["method"]),
                    Is.EqualTo(new[] { "notifications/tools/list_changed", "notifications/resources/list_changed", "notifications/prompts/list_changed" }));
                Assert.That((int?)after["id"], Is.EqualTo(3), "the answer to the replayed initialize never reaches the client");
                Assert.That(after["result"]!["tools"]!.AsArray().Select(tool => (string?)tool!["name"]), Does.Contain("hypha_metaclass"));
            });
        }

        [Test]
        public async Task A_download_that_fails_leaves_hypha_status_saying_why()
        {
            this.toServer.Send(Initialize);
            await this.fromServer.Next();

            this.download.SetResult(LaunchTarget.Missing("hypha 1.6.0 for win-x64 could not be downloaded: no route."));
            var changed = await this.fromServer.Next();
            this.toServer.Send("""{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"hypha_status"}}""");
            var status = await this.fromServer.Next();

            Assert.Multiple(() =>
            {
                Assert.That((string?)changed["method"], Is.EqualTo("notifications/tools/list_changed"));
                Assert.That((string?)status["result"]!["content"]![0]!["text"], Is.EqualTo("hypha could not start: hypha 1.6.0 for win-x64 could not be downloaded: no route."));
            });
        }

        [Test]
        public async Task A_download_that_finishes_before_the_client_initializes_hands_its_initialize_straight_over()
        {
            this.download.SetResult(LaunchTarget.Found(TestHypha.Executable(), "1.6.0"));
            await Task.Delay(200);

            this.toServer.Send(Initialize);
            var initialize = await this.fromServer.Next();

            Assert.Multiple(() =>
            {
                Assert.That((int?)initialize["id"], Is.EqualTo(1));
                Assert.That((string?)initialize["result"]!["instructions"], Does.Contain("NORMATIVE"), "the real server's");
            });
        }

        private Process Start(FileInfo executable)
        {
            var start = new ProcessStartInfo(executable.FullName)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = new UTF8Encoding(false),
                StandardOutputEncoding = new UTF8Encoding(false),
            };
            start.ArgumentList.Add("serve");
            start.ArgumentList.Add("--repository-root");
            start.ArgumentList.Add(this.workspace.FullName);

            var serve = Process.Start(start)!;
            serve.ErrorDataReceived += (_, _) => { };
            serve.BeginErrorReadLine();

            return serve;
        }

        // One side of the line-by-line conversation: what is written becomes lines to read.
        private sealed class Lines : TextReader
        {
            private readonly Channel<string> lines = Channel.CreateUnbounded<string>();
            private readonly StringBuilder partial = new();

            public void Send(string line) => this.lines.Writer.TryWrite(line);

            public override void Close() => this.lines.Writer.TryComplete();

            public async Task<JsonNode> Next() =>
                JsonNode.Parse(await this.lines.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromMinutes(1)))!;

            public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
            {
                try
                {
                    return await this.lines.Reader.ReadAsync(cancellationToken);
                }
                catch (ChannelClosedException)
                {
                    return null;
                }
            }

            public TextWriter AsWriter() => new Writer(this);

            public void Accept(string text)
            {
                foreach (var character in text)
                {
                    if (character == '\n')
                    {
                        this.lines.Writer.TryWrite(this.partial.ToString());
                        this.partial.Clear();
                    }
                    else
                    {
                        this.partial.Append(character);
                    }
                }
            }

            private sealed class Writer(Lines lines) : TextWriter
            {
                public override Encoding Encoding => Encoding.UTF8;

                public override void Write(char value) => lines.Accept(value.ToString());

                public override Task WriteAsync(string? value)
                {
                    lines.Accept(value ?? string.Empty);

                    return Task.CompletedTask;
                }

                public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
        }
    }
}
