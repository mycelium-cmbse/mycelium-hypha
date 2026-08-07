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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Moq;

    /// <summary>
    /// Suite of tests for the two read-only verbs: <see cref="DiscoverCommand"/> asks the upstreams
    /// what exists, <see cref="ListCommand"/> reads what this checkout carries.
    /// </summary>
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
            var layout = new Mock<IKnowledgeLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-05", "2026-04"]);
            layout.SetupGet(mock => mock.DefaultTag).Returns("2026-05");
            layout.SetupGet(mock => mock.Root).Returns(new DirectoryInfo(Path.GetTempPath()));
            layout.Setup(mock => mock.Xmi(It.IsAny<string>()))
                .Returns(new DirectoryInfo(Path.GetTempPath()));
            layout.Setup(mock => mock.TextualSources(It.IsAny<string>()))
                .Returns(new DirectoryInfo(Path.GetTempPath()));

            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("list"), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("2026-05").And.Contains("2026-04"));
                Assert.That(console.Output, Does.Contain("yes"));
            });
        }

        [Test]
        public async Task List_on_a_checkout_with_no_releases_points_at_fetch()
        {
            var layout = new Mock<IKnowledgeLayout>();
            layout.SetupGet(mock => mock.InstalledTags).Returns([]);
            layout.SetupGet(mock => mock.Root).Returns(new DirectoryInfo(Path.GetTempPath()));

            using var console = new RecordedConsole();

            var result = await new ListCommand.Handler(layout.Object)
                .InvokeAsync(this.root.Parse("list"), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("hypha fetch"));
            });
        }

        private Task<int> Discover(string[] available, string commandLine)
        {
            var discovery = new Mock<IReleaseDiscovery>();
            discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(available);

            return new DiscoverCommand.Handler(discovery.Object)
                .InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }
    }
}
