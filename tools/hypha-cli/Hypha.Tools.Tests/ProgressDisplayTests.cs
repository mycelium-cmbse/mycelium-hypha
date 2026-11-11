// ------------------------------------------------------------------------------------------------
// <copyright file="ProgressDisplayTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;
    using Hypha.Tools.Commands;

    using Spectre.Console;
    using Spectre.Console.Testing;

    [TestFixture]
    public class ProgressDisplayTests
    {
        private IAnsiConsole previous = null!;

        [SetUp]
        public void SetUp() => this.previous = AnsiConsole.Console;

        [TearDown]
        public void TearDown() => AnsiConsole.Console = this.previous;

        [Test]
        public async Task A_terminal_shows_one_bar_per_step_with_its_latest_message()
        {
            var console = new TestConsole().Interactive();
            console.Profile.Width = 120;
            AnsiConsole.Console = console;

            var result = await ProgressDisplay.RunAsync(progress =>
            {
                Assert.That(progress, Is.Not.Null);
                progress!.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 2, "1 of 2 files"));
                progress.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 2, 2, "2 of 2 files"));
                progress.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 1, 309, "1 of 309 files"));

                return Task.FromResult(42);
            });

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(42));
                Assert.That(console.Output, Does.Contain("metamodel: 2 of 2 files"));
                Assert.That(console.Output, Does.Contain("textual: 1 of 309 files"));
            });
        }

        [Test]
        public async Task Redirected_output_shows_nothing_and_the_work_gets_no_reporter()
        {
            var console = new TestConsole();
            AnsiConsole.Console = console;
            IProgress<KnowledgeProgress>? given = new Progress<KnowledgeProgress>();

            var result = await ProgressDisplay.RunAsync(progress =>
            {
                given = progress;

                return Task.FromResult("done");
            });

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo("done"));
                Assert.That(given, Is.Null);
                Assert.That(console.Output, Is.Empty);
            });
        }

        [Test]
        public void The_work_is_required_and_its_failure_reaches_the_caller()
        {
            AnsiConsole.Console = new TestConsole().Interactive();

            Assert.Multiple(() =>
            {
                Assert.That(() => ProgressDisplay.RunAsync<int>(null!), Throws.ArgumentNullException);
                Assert.That(
                    () => ProgressDisplay.RunAsync<int>(_ => throw new InvalidOperationException("boom")),
                    Throws.InvalidOperationException.With.Message.EqualTo("boom"));
            });
        }
    }
}
