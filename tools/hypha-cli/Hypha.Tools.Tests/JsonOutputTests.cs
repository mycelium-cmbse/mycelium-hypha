// ------------------------------------------------------------------------------------------------
// <copyright file="JsonOutputTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Toolchain;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The <c>--json</c> contract, exercised through <see cref="Program.BuildRootCommand"/> - the
    /// command line a host really runs - rather than a handler on its own.
    /// </summary>
    [TestFixture]
    public class JsonOutputTests
    {
        [Test]
        public async Task Json_output_has_no_banner_even_without_no_logo()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"list --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            var lines = console.Output.TrimEnd('\r', '\n').Split('\n');

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(lines, Has.Length.EqualTo(1), "the JSON document has to be the only line");
                Assert.That(() => JsonDocument.Parse(lines[0]).Dispose(), Throws.Nothing);
            });
        }

        [Test]
        public async Task Without_json_the_banner_is_shown()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();

            await Program.BuildRootCommand()
                .Parse($"list --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            Assert.That(console.Output, Does.StartWith("hypha"));
        }

        [Test]
        public async Task A_failure_outside_the_verb_is_still_a_json_error()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();
            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();

            var result = await Program.BuildRootCommand()
                .Parse($"list --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync(cancellationToken: cancelled.Token);

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(130));
                Assert.That(document.RootElement.GetProperty("error").GetString(), Is.EqualTo("Cancelled."));
            });
        }

        [Test]
        public async Task Without_json_a_failure_outside_the_verb_is_a_sentence()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();
            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();

            var result = await Program.BuildRootCommand()
                .Parse($"list --no-logo --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync(cancellationToken: cancelled.Token);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(130));
                Assert.That(console.Output.Trim(), Is.EqualTo("Cancelled."));
            });
        }

        [Test]
        public void The_toolchain_cache_root_on_the_command_line_reaches_uv()
        {
            using var knowledge = new TemporaryKnowledge();
            var toolchain = Path.Combine(knowledge.Root.FullName, "toolchain");
            var parsed = Program.BuildRootCommand().Parse(
                $"generate --repository-root \"{knowledge.Root.FullName}\" --toolchain-cache-root \"{toolchain}\"");

            using var provider = KnowledgeServices.Build(parsed);
            var environment = provider.GetRequiredService<IUvProvisioner>().Environment;

            Assert.That(
                environment[UvProvisioner.UvCacheVariable],
                Is.EqualTo(Path.Combine(toolchain, "uv-cache")));
        }

        [TestCase("check --json", true)]
        [TestCase("list --json", true)]
        [TestCase("lookup --kind metamodel --name PartUsage --json", true)]
        [TestCase("list", false)]
        [TestCase("generate", false)]
        public void Json_output_is_recognised_on_every_verb_that_offers_it(string commandLine, bool expected)
        {
            Assert.That(JsonOutput.IsRequested(Program.BuildRootCommand().Parse(commandLine)), Is.EqualTo(expected));
        }
    }
}
