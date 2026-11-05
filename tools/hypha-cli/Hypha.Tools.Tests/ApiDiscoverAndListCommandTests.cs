// ------------------------------------------------------------------------------------------------
// <copyright file="ApiDiscoverAndListCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Tools.Commands.Api;

    using Moq;

    /// <summary>
    /// Suite of tests for the two read-only API verbs: <see cref="ApiDiscoverCommand"/> asks the
    /// upstreams what exists, <see cref="ApiListCommand"/> reads what this checkout carries.
    /// </summary>
    [TestFixture]
    public class ApiDiscoverAndListCommandTests
    {
        private RootCommand root = null!;
        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp()
        {
            var api = new ApiCommand();
            api.Add(new ApiDiscoverCommand());
            api.Add(new ApiListCommand());

            this.root = new RootCommand();
            this.root.Add(api);

            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-list-{Guid.NewGuid():N}"));
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Discover_lists_the_available_api_versions()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover(["2026-04", "2025-09"], "api discover");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("API version"));
                Assert.That(console.Output, Does.Contain("2026-04").And.Contains("2025-09"));
                Assert.That(console.Output, Does.Not.Contain("--limit 0"), "nothing was cut, so nothing to say");
            });
        }

        [Test]
        public async Task Discover_truncates_to_the_limit_and_says_it_did()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover(["2026-04", "2025-09", "2025-07"], "api discover --limit 1");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("2026-04"));
                Assert.That(
                    console.Output, Does.Not.Contain("2025-07"), "the limit should have cut this one");
                Assert.That(console.Output, Does.Contain("2 older API versions not shown"));
                Assert.That(
                    console.Output, Does.Contain("--limit 0"),
                    "a truncated list has to say how to see the rest");
            });
        }

        [Test]
        public async Task Discover_shows_everything_at_limit_zero()
        {
            using var console = new RecordedConsole();

            await this.Discover(["2026-04", "2025-09", "2025-07"], "api discover --limit 0");

            Assert.That(console.Output, Does.Contain("2025-07"));
        }

        [Test]
        public async Task Discover_lists_ten_by_default()
        {
            var available = Enumerable.Range(1, 12).Select(month => $"2025-{month:00}").Reverse().ToArray();

            using var console = new RecordedConsole();

            await this.Discover(available, "api discover");

            Assert.Multiple(() =>
            {
                Assert.That(console.Output, Does.Contain("2025-12").And.Contains("2025-03"));
                Assert.That(console.Output, Does.Not.Contain("2025-02"));
                Assert.That(console.Output, Does.Contain("2 older API versions not shown"));
            });
        }

        [Test]
        public async Task Discover_reports_finding_nothing_rather_than_printing_an_empty_table()
        {
            using var console = new RecordedConsole();

            var result = await this.Discover([], "api discover");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("No API version"));
            });
        }

        [Test]
        public async Task List_shows_the_installed_api_versions_and_marks_the_default()
        {
            var layout = this.Layout("2026-04", "2026-04", "2025-09");
            this.FetchSources("2026-04");
            this.FetchSources("2025-09");
            this.GenerateCore("2026-04");

            using var console = new RecordedConsole();

            var result = await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list"), CancellationToken.None);

            var rows = console.Output.Split('\n');

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                // Five columns wrap the longer headers in a narrow console, so only the unwrapped ones are checked.
                Assert.That(console.Output, Does.Contain("sources fetched").And.Contains("specification"));
                Assert.That(
                    rows.Single(row => row.Contains("2026-04")).Split("yes").Length - 1, Is.EqualTo(3),
                    "the default, with sources fetched and knowledge generated");
                Assert.That(
                    rows.Single(row => row.Contains("2025-09")), Does.Contain("yes").And.Contains("incomplete"),
                    "fetched but not yet generated");
            });
        }

        [Test]
        public async Task List_json_reports_every_api_version_and_what_has_been_generated()
        {
            var layout = this.Layout("2026-04", "2026-04", "2025-09");
            this.FetchSources("2026-04");
            this.GenerateCore("2026-04");
            this.Write("2025-09", "pim", "metamodel.json");

            using var console = new RecordedConsole();

            var result = await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list --json"), CancellationToken.None);

            var line = console.Output.TrimEnd('\r', '\n');
            using var document = JsonDocument.Parse(line);
            var json = document.RootElement;
            var newest = json.GetProperty("versions")[0];
            var artifacts = newest.GetProperty("artifacts");
            var older = json.GetProperty("versions")[1];

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(line, Does.Not.Contain("\n"), "the JSON document has to be a single line");

                // Pinned: these names are what every host reads. Renaming one is a breaking change.
                Assert.That(json.GetProperty("defaultTag").GetString(), Is.EqualTo("2026-04"));
                Assert.That(json.GetProperty("versions").GetArrayLength(), Is.EqualTo(2));
                Assert.That(newest.GetProperty("tag").GetString(), Is.EqualTo("2026-04"));
                Assert.That(newest.GetProperty("isDefault").GetBoolean(), Is.True);
                Assert.That(newest.GetProperty("sourcesFetched").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("pim").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("openApi").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("crossReferences").GetBoolean(), Is.True);
                Assert.That(artifacts.GetProperty("specification").GetBoolean(), Is.False);
                Assert.That(artifacts.GetProperty("core").GetBoolean(), Is.True);

                Assert.That(older.GetProperty("isDefault").GetBoolean(), Is.False);
                Assert.That(older.GetProperty("sourcesFetched").GetBoolean(), Is.False);
                Assert.That(older.GetProperty("artifacts").GetProperty("pim").GetBoolean(), Is.True);
                Assert.That(
                    older.GetProperty("artifacts").GetProperty("core").GetBoolean(), Is.False,
                    "a PIM alone is not a usable API version");
            });
        }

        [Test]
        public async Task List_json_does_not_count_an_empty_file_as_generated()
        {
            // An interrupted write leaves an empty file behind; reporting it as generated would send a
            // host to read knowledge that is not there.
            var layout = this.Layout("2026-04", "2026-04");
            this.GenerateCore("2026-04");
            File.WriteAllText(layout.Object.CrossReferences("2026-04").FullName, string.Empty);

            using var console = new RecordedConsole();

            await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list --json"), CancellationToken.None);

            using var document = JsonDocument.Parse(console.Output);
            var artifacts = document.RootElement.GetProperty("versions")[0].GetProperty("artifacts");

            Assert.Multiple(() =>
            {
                Assert.That(artifacts.GetProperty("crossReferences").GetBoolean(), Is.False);
                Assert.That(artifacts.GetProperty("core").GetBoolean(), Is.False);
            });
        }

        [Test]
        public async Task List_json_reports_the_specification_text_once_it_is_extracted()
        {
            var layout = this.Layout("2026-04", "2026-04");
            this.Write("2026-04", "spec", "index.json");

            using var console = new RecordedConsole();

            await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list --json"), CancellationToken.None);

            using var document = JsonDocument.Parse(console.Output);
            var artifacts = document.RootElement.GetProperty("versions")[0].GetProperty("artifacts");

            Assert.That(artifacts.GetProperty("specification").GetBoolean(), Is.True);
        }

        [Test]
        public async Task List_json_with_nothing_installed_is_an_empty_list_not_a_failure()
        {
            var layout = this.Layout(null);

            using var console = new RecordedConsole();

            var result = await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list --json"), CancellationToken.None);

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(document.RootElement.GetProperty("defaultTag").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(document.RootElement.GetProperty("versions").GetArrayLength(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task List_on_a_checkout_with_no_api_versions_points_at_api_fetch()
        {
            var layout = new Mock<IApiLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns([]);

            using var console = new RecordedConsole();

            var result = await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list"), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("No API version is installed"));
                Assert.That(console.Output, Does.Contain("hypha api fetch"));
            });
        }

        [Test]
        public void A_cancelled_list_does_not_report_success()
        {
            var layout = new Mock<IApiLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-04"]);

            using var source = new CancellationTokenSource();
            source.Cancel();

            Assert.That(
                () => new ApiListCommand.Handler(layout.Object)
                    .InvokeAsync(this.root.Parse("api list"), source.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        private Mock<IApiLayout> Layout(string? defaultTag, params string[] tags)
        {
            var knowledge = Path.Combine(this.workspace.FullName, "knowledge", "api");

            var layout = new Mock<IApiLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns(tags);
            layout.SetupGet(mock => mock.DefaultTag).Returns(defaultTag);
            layout
                .Setup(mock => mock.Sources(It.IsAny<string>()))
                .Returns((string tag) => new DirectoryInfo(Path.Combine(this.workspace.FullName, "sources", "api", tag)));
            layout
                .Setup(mock => mock.PimGraph(It.IsAny<string>()))
                .Returns((string tag) => new FileInfo(Path.Combine(knowledge, tag, "pim", "metamodel.json")));
            layout
                .Setup(mock => mock.OpenApiIndex(It.IsAny<string>()))
                .Returns((string tag) => new FileInfo(Path.Combine(knowledge, tag, "openapi", "index.json")));
            layout
                .Setup(mock => mock.CrossReferences(It.IsAny<string>()))
                .Returns((string tag) => new FileInfo(Path.Combine(knowledge, tag, "cross-references.json")));
            layout
                .Setup(mock => mock.SpecificationCatalog(It.IsAny<string>()))
                .Returns((string tag) => new FileInfo(Path.Combine(knowledge, tag, "spec", "index.json")));

            return layout;
        }

        private void FetchSources(string tag) =>
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "sources", "api", tag));

        private void GenerateCore(string tag)
        {
            this.Write(tag, "pim", "metamodel.json");
            this.Write(tag, "openapi", "index.json");
            this.Write(tag, "cross-references.json");
        }

        private void Write(string tag, params string[] relative)
        {
            var file = new FileInfo(
                Path.Combine([this.workspace.FullName, "knowledge", "api", tag, .. relative]));
            file.Directory!.Create();
            File.WriteAllText(file.FullName, "{}");
        }

        private Task<int> Discover(string[] available, string commandLine)
        {
            var discovery = new Mock<IApiDiscovery>();
            discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(available);

            return new ApiDiscoverCommand.Handler(discovery.Object)
                .InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }
    }
}
