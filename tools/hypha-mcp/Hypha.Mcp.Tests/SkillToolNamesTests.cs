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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Hypha.Mcp;

    using Microsoft.Extensions.DependencyInjection;

    using ModelContextProtocol.Server;

    // The plugin's skills, agents and commands name the server's tools by their bare names, which every
    // client shows with a prefix of its own; a skill that names a tool the server does not have sends the
    // model after nothing. This is the CI lint of Step 6 of hypha-mcp-refactor-plan.md.
    [TestFixture]
    public class SkillToolNamesTests
    {
        private static readonly Regex ToolName = new(@"\bhypha_[a-z_]+\b", RegexOptions.Compiled);

        private ServiceProvider provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHyphaMcp();
            this.provider = services.BuildServiceProvider();
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
                .OrderBy(file => file.FullName, System.StringComparer.Ordinal)
                .Select(file => new TestCaseData(file.FullName).SetArgDisplayNames(Path.GetRelativePath(root.FullName, file.FullName).Replace('\\', '/')));
        }

        [TestCaseSource(nameof(PluginDocuments))]
        public void Every_tool_a_plugin_document_names_is_a_tool_the_server_has(string document)
        {
            var registered = this.provider.GetServices<McpServerTool>().Select(tool => tool.ProtocolTool.Name).ToHashSet(System.StringComparer.Ordinal);

            var named = ToolName.Matches(File.ReadAllText(document)).Select(match => match.Value).Distinct();

            Assert.That(named, Is.All.Matches<string>(name => name is not null && registered.Contains(name)));
        }

        [Test]
        public void The_lint_reads_the_skills()
        {
            Assert.That(PluginDocuments().Select(test => (string)test.Arguments[0]!), Has.Some.EndsWith(Path.Combine("metamodel-lookup", "SKILL.md")));
        }

        private static DirectoryInfo Root()
        {
            var folder = new DirectoryInfo(System.AppContext.BaseDirectory);
            while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "mycelium-hypha.sln")))
            {
                folder = folder.Parent;
            }

            return folder ?? throw new DirectoryNotFoundException("the test runs outside the mycelium-hypha checkout");
        }
    }
}
