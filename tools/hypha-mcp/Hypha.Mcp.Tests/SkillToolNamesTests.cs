// ------------------------------------------------------------------------------------------------
// <copyright file="SkillToolNamesTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Hypha.Mcp;

    using Microsoft.Extensions.DependencyInjection;

    using ModelContextProtocol.Server;

    // The plugin's skills, agents and commands name the server's tools by their bare names, which every
    // client shows with a prefix of its own, and the questions and arguments those tools take; a skill
    // that names a tool, a question or an argument the server does not have sends the model after
    // nothing. This is the CI lint of Step 6 of hypha-mcp-refactor-plan.md.
    //
    // A question or argument is read from where the skills write it:
    // - a table after a line naming "`hypha_x` with the `question`": its second column holds question
    //   values and its third the arguments (the lower-case identifiers in it);
    // - a line naming one tool: every `argument: value` on it, and the `question` in "`hypha_x`, `question`".
    [TestFixture]
    public class SkillToolNamesTests
    {
        private static readonly Regex ToolName = new(@"\bhypha_[a-z_]+\b", RegexOptions.Compiled);

        private static readonly Regex TableLeadIn = new(@"`(hypha_[a-z_]+)` with\s+the `question`", RegexOptions.Compiled);

        private static readonly Regex Code = new("`([^`]+)`", RegexOptions.Compiled);

        private static readonly Regex Argument = new("^[a-z][A-Za-z]*$", RegexOptions.Compiled);

        private static readonly Regex Assignment = new("^([a-z][A-Za-z]*): (.+)$", RegexOptions.Compiled);

        private static readonly Regex QuestionAfterTool = new("`(hypha_[a-z_]+)`, `([a-z]+(?:-[a-z]+)*)`", RegexOptions.Compiled);

        private ServiceProvider provider = null!;

        private Dictionary<string, JsonElement> arguments = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHyphaMcp();
            this.provider = services.BuildServiceProvider();

            this.arguments = this.provider.GetServices<McpServerTool>()
                .ToDictionary(tool => tool.ProtocolTool.Name, tool => tool.ProtocolTool.InputSchema.GetProperty("properties"), StringComparer.Ordinal);
        }

        [TearDown]
        public async Task TearDown() => await this.provider.DisposeAsync();

        public static IEnumerable<TestCaseData> PluginDocuments()
        {
            var root = Root();

            return new[] { "skills", "agents", "commands" }
                .Select(folder => new DirectoryInfo(Path.Combine(root.FullName, folder)))
                .Where(folder => folder.Exists)
                .SelectMany(folder => folder.EnumerateFiles("*.md", SearchOption.AllDirectories))
                .OrderBy(file => file.FullName, StringComparer.Ordinal)
                .Select(file => new TestCaseData(file.FullName).SetArgDisplayNames(Path.GetRelativePath(root.FullName, file.FullName).Replace('\\', '/')));
        }

        [TestCaseSource(nameof(PluginDocuments))]
        public void Every_tool_a_plugin_document_names_is_a_tool_the_server_has(string document)
        {
            Assert.That(this.UnknownTools(File.ReadAllText(document)), Is.Empty);
        }

        [TestCaseSource(nameof(PluginDocuments))]
        public void Every_question_and_argument_a_plugin_document_names_is_one_its_tool_accepts(string document)
        {
            Assert.That(this.Checks(File.ReadAllText(document)).Where(check => !check.Accepted), Is.Empty);
        }

        [Test]
        public void The_lint_reads_the_skills()
        {
            Assert.That(PluginDocuments().Select(test => (string)test.Arguments[0]!), Has.Some.EndsWith(Path.Combine("metamodel-lookup", "SKILL.md")));
        }

        [Test]
        public void The_lint_finds_the_tools_metamodel_lookup_names()
        {
            var named = ToolName.Matches(File.ReadAllText(MetamodelLookup())).Select(match => match.Value);

            Assert.That(named, Is.SupersetOf(new[] { "hypha_metaclass", "hypha_feature", "hypha_typed_by", "hypha_syntax", "hypha_lookup" }));
        }

        [Test]
        public void The_lint_checks_the_questions_of_every_table_in_metamodel_lookup()
        {
            var checks = this.Checks(File.ReadAllText(MetamodelLookup())).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(checks.Where(check => check.What == "question").Select(check => check.Tool).Distinct(), Is.EquivalentTo(new[] { "hypha_metaclass", "hypha_feature", "hypha_typed_by", "hypha_syntax" }));
                Assert.That(checks, Has.Some.Matches<Check>(check => check is { Tool: "hypha_metaclass", What: "argument", Name: "concreteOnly" }));
                Assert.That(checks, Has.Some.Matches<Check>(check => check is { Tool: "hypha_lookup", What: "kind", Name: "metamodel" }));
            });
        }

        [Test]
        public void The_lint_reports_a_tool_the_server_does_not_have()
        {
            Assert.That(this.UnknownTools("Call `hypha_metaclass`, then `hypha_nonexistent`."), Is.EqualTo(new[] { "hypha_nonexistent" }));
        }

        [Test]
        public void The_lint_reports_a_question_or_argument_the_tool_does_not_accept()
        {
            const string text = """
                One metaclass, `hypha_metaclass` with the `question`:

                | Question | `question` | Arguments |
                | --- | --- | --- |
                | Known | `element` | `name` |
                | Unknown | `no-such-question` | `noSuchArgument` |

                Then `hypha_lookup` with `kind: "no-such-kind"` and `size: 3`,
                or `hypha_feature`, `no-such-feature-question`.
                """;

            var rejected = this.Checks(text).Where(check => !check.Accepted).Select(check => $"{check.Tool} {check.What} {check.Name}");

            Assert.That(rejected, Is.EquivalentTo(new[]
            {
                "hypha_metaclass question no-such-question",
                "hypha_metaclass argument noSuchArgument",
                "hypha_lookup kind no-such-kind",
                "hypha_lookup argument size",
                "hypha_feature question no-such-feature-question",
            }));
        }

        private IEnumerable<string> UnknownTools(string text) =>
            ToolName.Matches(text).Select(match => match.Value).Distinct().Where(name => !this.arguments.ContainsKey(name));

        private IEnumerable<Check> Checks(string text)
        {
            string? table = null;
            var pastHeader = false;

            foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
            {
                var trimmed = line.Trim();
                var leadIn = TableLeadIn.Match(line);

                if (leadIn.Success)
                {
                    table = leadIn.Groups[1].Value;
                    pastHeader = false;
                }
                else if (table is not null && trimmed.StartsWith('|'))
                {
                    var cells = trimmed.Trim('|').Split('|');

                    if (!pastHeader)
                    {
                        pastHeader = cells.All(cell => cell.Trim().StartsWith("---", StringComparison.Ordinal));
                        continue;
                    }

                    foreach (var question in Codes(cells.ElementAtOrDefault(1)))
                    {
                        yield return this.Question(table, question);
                    }

                    foreach (var argument in Codes(cells.ElementAtOrDefault(2)).Where(code => Argument.IsMatch(code) || Assignment.IsMatch(code)))
                    {
                        yield return this.ArgumentOf(table, Assignment.Match(argument) is { Success: true } assigned ? assigned.Groups[1].Value : argument);
                    }

                    continue;
                }
                else if (trimmed.Length > 0)
                {
                    table = null;
                }

                foreach (var check in this.LineChecks(line))
                {
                    yield return check;
                }
            }
        }

        private IEnumerable<Check> LineChecks(string line)
        {
            var tools = ToolName.Matches(line).Select(match => match.Value).Distinct().ToList();
            if (tools.Count != 1 || !this.arguments.ContainsKey(tools[0]))
            {
                yield break;
            }

            var tool = tools[0];

            foreach (var assignment in Codes(line).Select(code => Assignment.Match(code)).Where(match => match.Success))
            {
                var name = assignment.Groups[1].Value;
                yield return this.ArgumentOf(tool, name);

                var value = assignment.Groups[2].Value;
                if (value.Length > 1 && value[0] == '"' && value[^1] == '"' && this.Enumeration(tool, name) is { } values)
                {
                    var literal = value[1..^1];
                    yield return new Check(tool, name, literal, values.Contains(literal));
                }
            }

            foreach (Match question in QuestionAfterTool.Matches(line))
            {
                yield return this.Question(question.Groups[1].Value, question.Groups[2].Value);
            }
        }

        private Check Question(string tool, string question) =>
            new(tool, "question", question, this.Enumeration(tool, "question")?.Contains(question) == true);

        private Check ArgumentOf(string tool, string argument) =>
            new(tool, "argument", argument, this.arguments.TryGetValue(tool, out var properties) && properties.TryGetProperty(argument, out _));

        private HashSet<string>? Enumeration(string tool, string argument) =>
            this.arguments.TryGetValue(tool, out var properties)
            && properties.TryGetProperty(argument, out var property)
            && property.TryGetProperty("enum", out var values)
                ? values.EnumerateArray().Select(value => value.GetString() ?? string.Empty).ToHashSet(StringComparer.Ordinal)
                : null;

        private static IEnumerable<string> Codes(string? text) =>
            text is null ? [] : Code.Matches(text).Select(match => match.Groups[1].Value);

        private static string MetamodelLookup() => Path.Combine(Root().FullName, "skills", "metamodel-lookup", "SKILL.md");

        private static DirectoryInfo Root()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "mycelium-hypha.sln")))
            {
                folder = folder.Parent;
            }

            return folder ?? throw new DirectoryNotFoundException("the test runs outside the mycelium-hypha checkout");
        }

        private sealed record Check(string Tool, string What, string Name, bool Accepted);
    }
}
