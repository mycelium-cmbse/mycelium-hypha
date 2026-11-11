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
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Toolchain;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    [TestFixture]
    public class JsonOutputTests
    {
        [TestCase("list --json")]
        [TestCase("api list --json")]
        public async Task Json_output_has_no_banner_even_without_no_logo(string commandLine)
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"{commandLine} --repository-root \"{knowledge.Root.FullName}\"")
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

            Assert.That(environment[UvProvisioner.UvCacheVariable], Is.EqualTo(Path.Combine(toolchain, "uv-cache")));
        }

        [Test]
        public void With_json_log_lines_go_to_standard_error_not_standard_output()
        {
            var (output, error) = Log("list --json");

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Not.Contain("a log line"));
                Assert.That(error, Does.Contain("a log line"));
            });
        }

        [TestCase("System.Net.Http.HttpClient.hypha-upstream.LogicalHandler")]
        [TestCase("Microsoft.Extensions.Http.DefaultHttpClientFactory")]
        [TestCase("Polly")]
        public void With_json_request_and_retry_chatter_is_held_to_warnings(string category)
        {
            // A caller that never reads standard error (the SessionStart hook) would otherwise hang
            // once enough of these filled the pipe.
            var (_, quiet) = Log("list --json", category, LogLevel.Information);
            var (_, warning) = Log("list --json", category, LogLevel.Warning);

            Assert.Multiple(() =>
            {
                Assert.That(quiet, Does.Not.Contain("a log line"));
                Assert.That(warning, Does.Contain("a log line"));
            });
        }

        [Test]
        public void Without_json_request_chatter_is_still_logged()
        {
            var (output, _) = Log("list", "System.Net.Http.HttpClient.hypha-upstream.LogicalHandler", LogLevel.Information);

            Assert.That(output, Does.Contain("a log line"));
        }

        [Test]
        public void A_requested_log_level_above_warning_is_kept_for_the_quiet_categories()
        {
            var (_, error) = Log("list --json --log-level Error", "Polly", LogLevel.Warning);

            Assert.That(error, Does.Not.Contain("a log line"));
        }

        [TestCase("list --json")]
        [TestCase("lookup --kind metamodel --name X --json")]
        public async Task A_corrupt_version_manifest_is_a_json_error(string commandLine)
        {
            using var knowledge = new TemporaryKnowledge();
            Directory.CreateDirectory(knowledge.Layout.VersionManifest.DirectoryName!);
            await File.WriteAllTextAsync(knowledge.Layout.VersionManifest.FullName, "{ not json");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"{commandLine} --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            var lines = console.Output.TrimEnd('\r', '\n').Split('\n');
            using var document = JsonDocument.Parse(lines[0]);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(lines, Has.Length.EqualTo(1));
                Assert.That(
                    document.RootElement.GetProperty("error").GetString(),
                    Does.StartWith("A knowledge file could not be read"));
            });
        }

        [Test]
        public async Task A_corrupt_api_version_manifest_is_a_json_error()
        {
            using var knowledge = new TemporaryKnowledge();
            var manifest = new FileInfo(Path.Combine(knowledge.Root.FullName, "knowledge", "api", "versions.json"));
            manifest.Directory!.Create();
            await File.WriteAllTextAsync(manifest.FullName, "{ not json");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"api list --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            var lines = console.Output.TrimEnd('\r', '\n').Split('\n');
            using var document = JsonDocument.Parse(lines[0]);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(lines, Has.Length.EqualTo(1));
                Assert.That(
                    document.RootElement.GetProperty("error").GetString(),
                    Does.StartWith("A knowledge file could not be read"));
            });
        }

        [Test]
        public async Task Without_json_a_corrupt_version_manifest_is_a_sentence()
        {
            using var knowledge = new TemporaryKnowledge();
            Directory.CreateDirectory(knowledge.Layout.VersionManifest.DirectoryName!);
            await File.WriteAllTextAsync(knowledge.Layout.VersionManifest.FullName, "{ not json");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"list --no-logo --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("A knowledge file could not be read"));
            });
        }

        [Test]
        public void Without_json_log_lines_go_to_standard_output()
        {
            var (output, error) = Log("list");

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain("a log line"));
                Assert.That(error, Does.Not.Contain("a log line"));
            });
        }

        [Test]
        public async Task Lookup_through_the_command_line_reads_the_given_repository_root()
        {
            using var knowledge = new TemporaryKnowledge()
                .Install("2026-09")
                .Write("2026-09", "metamodel/index.json",
                    "{\"schemaVersion\":\"1.0.0\",\"metamodel\":\"SysML\",\"modelVersionUri\":\"https://www.omg.org/spec/SysML/20250201\","
                    + "\"entries\":{\"PartUsage\":{\"kind\":\"class\",\"package\":\"Parts\",\"qualifiedName\":\"SysML::Parts::PartUsage\","
                    + "\"isAbstract\":false,\"summary\":\"\",\"extends\":[],\"file\":\"elements/PartUsage.md\"}}}")
                .Write("2026-09", "metamodel/elements/PartUsage.md", "a\nb\n");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"lookup --kind metamodel --name PartUsage --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            var lines = console.Output.TrimEnd('\r', '\n').Split('\n');
            using var document = JsonDocument.Parse(lines[0]);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(lines, Has.Length.EqualTo(1));
                Assert.That(document.RootElement.GetProperty("status").GetString(), Is.EqualTo("found"));
                Assert.That(document.RootElement.GetProperty("content").GetString(), Is.EqualTo("a\nb"));
            });
        }

        [Test]
        public async Task Lookup_through_the_command_line_of_an_ungenerated_release_is_a_json_error()
        {
            using var knowledge = new TemporaryKnowledge().Install("2026-09");
            using var console = new RecordedConsole();

            var result = await Program.BuildRootCommand()
                .Parse($"lookup --kind metamodel --name PartUsage --json --repository-root \"{knowledge.Root.FullName}\"")
                .InvokeAsync();

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    document.RootElement.GetProperty("error").GetString(),
                    Does.Contain("hypha generate --tag 2026-09"));
            });
        }

        [TestCase("lookup", "--json", "--name", "X")]
        [TestCase("lookup", "--json", "--kind", "metamodel", "--name", "X", "--max-lines", "abc")]
        [TestCase("lookup", "--json", "--kind", "spec", "--name", "X")]
        public async Task A_command_line_that_does_not_parse_is_a_json_error(params string[] args)
        {
            using var console = new RecordedConsole();

            var result = await Program.Main(args);

            using var document = JsonDocument.Parse(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(document.RootElement.GetProperty("error").GetString(), Is.Not.Empty);
            });
        }

        [TestCase("check --json", true)]
        [TestCase("list --json", true)]
        [TestCase("api list --json", true)]
        [TestCase("lookup --kind metamodel --name PartUsage --json", true)]
        [TestCase("list", false)]
        [TestCase("api list", false)]
        [TestCase("generate", false)]
        public void Json_output_is_recognised_on_every_verb_that_offers_it(string commandLine, bool expected)
        {
            Assert.That(JsonOutput.IsRequested(Program.BuildRootCommand().Parse(commandLine)), Is.EqualTo(expected));
        }

        private static (string Output, string Error) Log(
            string commandLine, string category = "Hypha.Tools.Tests", LogLevel logLevel = LogLevel.Information)
        {
            using var knowledge = new TemporaryKnowledge();
            var parsed = Program.BuildRootCommand().Parse($"{commandLine} --repository-root \"{knowledge.Root.FullName}\"");

            var previousOut = Console.Out;
            var previousError = Console.Error;
            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            try
            {
                using (var provider = KnowledgeServices.Build(parsed))
                {
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger(category).Log(logLevel, "a log line");
                }

                return (output.ToString(), error.ToString());
            }
            finally
            {
                Console.SetOut(previousOut);
                Console.SetError(previousError);
            }
        }
    }
}
