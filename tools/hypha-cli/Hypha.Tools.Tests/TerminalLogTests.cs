// ------------------------------------------------------------------------------------------------
// <copyright file="TerminalLogTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Spectre.Console;
    using Spectre.Console.Testing;

    [TestFixture]
    public class TerminalLogTests
    {
        private IAnsiConsole previous = null!;

        private TestConsole console = null!;

        [SetUp]
        public void SetUp()
        {
            this.previous = AnsiConsole.Console;
            this.console = new TestConsole().Interactive();
            this.console.Profile.Width = 200;
            AnsiConsole.Console = this.console;
        }

        [TearDown]
        public void TearDown()
        {
            AnsiConsole.Console = this.previous;
            this.console.Dispose();
        }

        [Test]
        public void On_a_terminal_log_lines_go_through_the_console_the_progress_display_draws_on()
        {
            var output = this.Log("list", "Hypha.Tools.Tests", LogLevel.Information, "a log line in C:\\[work]\\sources [/]");

            Assert.Multiple(() =>
            {
                Assert.That(this.console.Output, Does.Contain("[INF] a log line in C:\\[work]\\sources [/]"));
                Assert.That(output, Is.Empty);
            });
        }

        [Test]
        public async Task A_line_logged_while_the_display_runs_is_kept_with_the_bars()
        {
            using var knowledge = new TemporaryKnowledge();
            var parsed = Program.BuildRootCommand().Parse($"list --repository-root \"{knowledge.Root.FullName}\"");
            using var provider = KnowledgeServices.Build(parsed);
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Hypha.Tools.Tests");

            await ProgressDisplay.RunAsync(progress =>
            {
                progress!.Report(new KnowledgeProgress(KnowledgeProgress.Generate, "2026-05", 1, 2, "metamodel: 1 files"));
                logger.LogInformation("keywords started");
                progress.Report(new KnowledgeProgress(KnowledgeProgress.Generate, "2026-05", 2, 2, "keywords: 1 files"));

                return Task.FromResult(0);
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.console.Output, Does.Contain("[INF] keywords started"));
                Assert.That(this.console.Output, Does.Contain("2026-05: keywords: 1 files"));
            });
        }

        [TestCase("System.Net.Http.HttpClient.hypha-upstream.LogicalHandler")]
        [TestCase("Microsoft.Extensions.Http.DefaultHttpClientFactory")]
        [TestCase("Polly")]
        public void On_a_terminal_request_and_retry_chatter_is_held_to_warnings(string category)
        {
            this.Log("list", category, LogLevel.Information, "a request line");
            this.Log("list", category, LogLevel.Warning, "a retry warning");

            Assert.Multiple(() =>
            {
                Assert.That(this.console.Output, Does.Not.Contain("a request line"));
                Assert.That(this.console.Output, Does.Contain("a retry warning"));
            });
        }

        [Test]
        public void On_a_terminal_debug_logging_keeps_the_request_chatter()
        {
            this.Log("list --log-level Debug", "Polly", LogLevel.Information, "a request line");

            Assert.That(this.console.Output, Does.Contain("a request line"));
        }

        private string Log(string commandLine, string category, LogLevel logLevel, string message)
        {
            using var knowledge = new TemporaryKnowledge();
            var parsed = Program.BuildRootCommand().Parse($"{commandLine} --repository-root \"{knowledge.Root.FullName}\"");

            var previousOut = Console.Out;
            using var output = new StringWriter();
            Console.SetOut(output);

            try
            {
                using (var provider = KnowledgeServices.Build(parsed))
                {
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger(category).Log(logLevel, message);
                }

                return output.ToString();
            }
            finally
            {
                Console.SetOut(previousOut);
            }
        }
    }
}
