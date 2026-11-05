// ------------------------------------------------------------------------------------------------
// <copyright file="DiscoverAndListCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.CommandLine;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Moq;

    [TestFixture]
    public class DiscoverAndListCommandTests
    {
        private RootCommand root = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new DiscoverCommand());
            this.root.Add(new ListCommand());
        }

        [Test]
        public async Task Discover_lists_the_available_releases()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover(["2026-05", "2026-04"], "discover");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("2026-05").And.Contains("2026-04"));
            });
        }

        [Test]
        public async Task Discover_truncates_to_the_limit_and_says_it_did()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover(["2026-05", "2026-04", "2025-09"], "discover --limit 1");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("2026-05"));
                Assert.That(
                    console.Output, Does.Not.Contain("2025-09"), "the limit should have cut this one");
                Assert.That(
                    console.Output, Does.Contain("--limit 0"),
                    "a truncated list has to say how to see the rest");
            });
        }

        [Test]
        public async Task Discover_shows_everything_at_limit_zero()
        {
            using var console = new RecordedConsole();

            await this.Discover(["2026-05", "2026-04", "2025-09"], "discover --limit 0");

            Assert.That(console.Output, Does.Contain("2025-09"));
        }

        [Test]
        public async Task Discover_reports_finding_nothing_rather_than_printing_an_empty_table()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover([], "discover");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("No release"));
            });
        }

        [Test]
        public async Task List_shows_the_installed_releases_and_marks_the_default()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-05", "2026-04");
            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(knowledge.Layout).InvokeAsync(this.root.Parse("list"), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("2026-05").And.Contains("2026-04"));
                Assert.That(console.Output, Does.Contain("yes"));
                Assert.That(console.Output, Does.Contain("incomplete"), "nothing has been generated yet");
            });
        }

        [Test]
        public async Task List_on_a_checkout_with_no_releases_points_at_fetch()
        {
            using var knowledge = new TemporaryKnowledge();
            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(knowledge.Layout).InvokeAsync(this.root.Parse("list"), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("hypha fetch"));
            });
        }

        [Test]
        public async Task List_json_reports_every_release_and_what_has_been_generated()
        {
            using var knowledge = new TemporaryKnowledge()
                .Install("2026-09", "2026-05")
                .Write("2026-09", "metamodel/index.json", "{}")
                .Write("2026-09", "textual-notation/index.md", "#")
                .Write("2026-09", "model-library/index.json", "{}")
                .Write("2026-09", "cross-references.json", "{}");
            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(knowledge.Layout)
                .InvokeAsync(this.root.Parse("list --json"), CancellationToken.None);

            var line = console.Output.TrimEnd('\r', '\n');
            using var document = JsonDocument.Parse(line);
            var json = document.RootElement;
            var newest = json.GetProperty("releases")[0];
            var artifacts = newest.GetProperty("artifacts");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(line, Does.Not.Contain("\n"), "the JSON document has to be a single line");

                // Pinned: these names are what every host reads. Renaming one is a breaking change.
                Assert.That(json.GetProperty("defaultTag").GetString(), Is.EqualTo("2026-09"));
                Assert.That(json.GetProperty("releases").GetArrayLength(), Is.EqualTo(2));
                Assert.That(newest.GetProperty("tag").GetString(), Is.EqualTo("2026-09"));
                Assert.That(newest.GetProperty("isDefault").GetBoolean(), Is.True);
                Assert.That(newest.GetProperty("sourcesFetched").GetBoolean(), Is.False);
                Assert.That(artifacts.GetProperty("metamodel").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("textualNotation").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("modelLibrary").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("crossReferences").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("specification").GetBoolean(), Is.False);
                Assert.That(artifacts.GetProperty("core").GetBoolean(), Is.True);
                Assert.That(
                    json.GetProperty("releases")[1].GetProperty("artifacts").GetProperty("core").GetBoolean(),
                    Is.False);
            });
        }

        [Test]
        public async Task List_json_with_nothing_installed_is_an_empty_list_not_a_failure()
        {
            using var knowledge = new TemporaryKnowledge();
            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(knowledge.Layout)
                .InvokeAsync(this.root.Parse("list --json"), CancellationToken.None);

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(document.RootElement.GetProperty("defaultTag").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(document.RootElement.GetProperty("releases").GetArrayLength(), Is.EqualTo(0));
            });
        }

        private Task<int> Discover(string[] available, string commandLine)
        {
            var discovery = new Mock<IReleaseDiscovery>();
            discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(available);

            return new DiscoverCommand.Handler(discovery.Object).InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }
    }
}
