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
            var fetched = Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "fetched"));
            var missing = new DirectoryInfo(Path.Combine(this.workspace.FullName, "missing"));

            var layout = new Mock<IApiLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-04", "2025-09"]);
            layout.SetupGet(mock => mock.DefaultTag).Returns("2026-04");
            layout.Setup(mock => mock.Sources(It.IsAny<string>())).Returns(fetched);
            layout.Setup(mock => mock.Knowledge("2026-04")).Returns(fetched);
            layout.Setup(mock => mock.Knowledge("2025-09")).Returns(missing);

            using var console = new RecordedConsole();

            var result = await new ApiListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("api list"), CancellationToken.None);

            var rows = console.Output.Split('\n');

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("sources fetched").And.Contains("knowledge generated"));
                Assert.That(
                    rows.Single(row => row.Contains("2026-04")).Split("yes").Length - 1, Is.EqualTo(3),
                    "the default, with sources fetched and knowledge generated");
                Assert.That(
                    rows.Single(row => row.Contains("2025-09")), Does.Contain("yes").And.Contains("no"),
                    "fetched but not yet generated");
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
