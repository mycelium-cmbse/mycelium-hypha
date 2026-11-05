// ------------------------------------------------------------------------------------------------
// <copyright file="CheckCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Moq;

    /// <summary>
    /// Suite of tests for <see cref="CheckCommand"/>: read-only comparison, no fetching or generating.
    /// </summary>
    [TestFixture]
    public class CheckCommandTests
    {
        /// <summary>A root command holding only <see cref="CheckCommand"/>, to parse the command lines against.</summary>
        private RootCommand root = null!;

        /// <summary>Offers six releases upstream, newest first – more than <see cref="CheckResult.MaxAvailableOnline"/>.</summary>
        private Mock<IReleaseDiscovery> discovery = null!;

        /// <summary>Reports <c>2026-05</c> and <c>2026-04</c> as installed, <c>2026-05</c> as the default.</summary>
        private Mock<IKnowledgeLayout> layout = null!;

        /// <summary>The handler under test, over <see cref="discovery"/> and <see cref="layout"/>.</summary>
        private CheckCommand.Handler handler = null!;

        /// <summary>Builds the command, the mocks and the handler afresh for each test.</summary>
        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new CheckCommand());

            this.discovery = new Mock<IReleaseDiscovery>();
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06", "2026-05", "2026-04", "2026-03", "2025-12", "2025-09"]);

            this.layout = new Mock<IKnowledgeLayout>();
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-05", "2026-04"]);
            this.layout.SetupGet(mock => mock.DefaultTag).Returns("2026-05");

            this.handler = new CheckCommand.Handler(this.discovery.Object, this.layout.Object);
        }

        [Test]
        public async Task Json_output_reports_installed_default_and_capped_available_online()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("check --json --no-logo");

            Assert.That(result, Is.EqualTo(0));

            var parsed = JsonSerializer.Deserialize<CheckResult>(console.Output.Trim())!;

            Assert.Multiple(() =>
            {
                Assert.That(parsed.InstalledTags, Is.EqualTo(new[] { "2026-05", "2026-04" }));
                Assert.That(parsed.DefaultTag, Is.EqualTo("2026-05"));
                Assert.That(parsed.AvailableOnline, Has.Count.EqualTo(CheckResult.MaxAvailableOnline));
                Assert.That(parsed.AvailableOnline[0], Is.EqualTo("2026-06"));
            });
        }

        [Test]
        public async Task Json_output_keeps_the_property_names_the_hook_reads()
        {
            using var console = new RecordedConsole();

            await this.Invoke("check --json --no-logo");

            // Read as a raw document rather than through CheckResult, so a renamed property fails
            // here instead of passing on both sides of the serializer.
            using var document = JsonDocument.Parse(console.Output);
            var json = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(json.GetProperty("installedTags")[0].GetString(), Is.EqualTo("2026-05"));
                Assert.That(json.GetProperty("defaultTag").GetString(), Is.EqualTo("2026-05"));
                Assert.That(json.GetProperty("availableOnline")[0].GetString(), Is.EqualTo("2026-06"));
                Assert.That(json.EnumerateObject().Count(), Is.EqualTo(3));
            });
        }

        [Test]
        public async Task Json_output_is_exactly_one_line_with_no_banner_noise()
        {
            using var console = new RecordedConsole();

            await this.Invoke("check --json --no-logo");

            Assert.That(console.Output.Trim().Split('\n'), Has.Length.EqualTo(1));
        }

        [Test]
        public async Task Human_output_names_a_newer_release_when_one_exists()
        {
            using var console = new RecordedConsole();

            await this.Invoke("check");

            Assert.That(console.Output, Does.Contain("2026-06").And.Contains("hypha fetch"));
        }

        [Test]
        public async Task Human_output_says_nothing_extra_when_already_on_the_newest()
        {
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-06"]);
            this.layout.SetupGet(mock => mock.DefaultTag).Returns("2026-06");

            using var console = new RecordedConsole();

            await this.Invoke("check");

            Assert.That(console.Output, Does.Not.Contain("hypha fetch"));
        }

        [Test]
        public async Task Nothing_installed_is_still_a_successful_comparison()
        {
            this.layout.SetupGet(mock => mock.InstalledTags).Returns([]);
            this.layout.SetupGet(mock => mock.DefaultTag).Returns((string?)null);

            var result = await this.Invoke("check --json --no-logo");

            Assert.That(result, Is.EqualTo(0));
        }

        /// <summary>Parses <paramref name="commandLine"/> and runs the handler on it.</summary>
        /// <param name="commandLine">The command line, starting with <c>check</c>.</param>
        /// <returns>The handler's exit code.</returns>
        private async Task<int> Invoke(string commandLine)
        {
            var parsed = this.root.Parse(commandLine);

            return await this.handler.InvokeAsync(parsed, CancellationToken.None);
        }
    }
}
