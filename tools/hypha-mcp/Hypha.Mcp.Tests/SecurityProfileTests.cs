// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Knowledge.Toolchain;

    using Hypha.Mcp;

    using Microsoft.Extensions.DependencyInjection;

    using ModelContextProtocol.Server;

    [TestFixture]
    public class SecurityProfileTests
    {
        private const string Draft = "unreleased.md";

        private static readonly Regex Code = new("`([^`]+)`", RegexOptions.Compiled);

        private static readonly Regex ToolName = new("^hypha_[a-z_]+$", RegexOptions.Compiled);

        private static readonly Regex Count = new(@"^\s*(?:- )?\*\*(?<what>Tools|Prompts)\*\* \((?<count>\d+)\)", RegexOptions.Compiled | RegexOptions.Multiline);

        private ServiceProvider provider = null!;

        private string profile = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHyphaMcp();
            this.provider = services.BuildServiceProvider();

            this.profile = File.ReadAllText(Path.Combine(ProfileFolder().FullName, Draft));
        }

        [TearDown]
        public async Task TearDown() => await this.provider.DisposeAsync();

        [Test]
        public void The_index_lists_the_draft()
        {
            Assert.That(File.ReadAllText(Path.Combine(ProfileFolder().FullName, "README.md")), Does.Contain($"({Draft})"));
        }

        [Test]
        public void The_profile_lists_every_tool_the_server_has_and_no_other()
        {
            var tools = this.provider.GetServices<McpServerTool>().Select(tool => tool.ProtocolTool.Name);

            Assert.Multiple(() =>
            {
                Assert.That(ToolRows(Section(this.profile, 4)).Keys, Is.EquivalentTo(tools));
                Assert.That(Counted(Section(this.profile, 4), "Tools"), Is.EqualTo(tools.Count()));
            });
        }

        [Test]
        public void The_profile_gives_each_tool_the_annotations_the_server_declares()
        {
            var stated = ToolRows(Section(this.profile, 4));

            Assert.Multiple(() =>
            {
                foreach (var tool in this.provider.GetServices<McpServerTool>().Select(tool => tool.ProtocolTool))
                {
                    // The MCP defaults: a tool that says nothing is taken as writing, destructive and open-world.
                    var declared = new Annotations(
                        tool.Annotations?.ReadOnlyHint ?? false,
                        tool.Annotations?.DestructiveHint ?? true,
                        tool.Annotations?.OpenWorldHint ?? true);

                    if (stated.TryGetValue(tool.Name, out var row))
                    {
                        Assert.That(row, Is.EqualTo(declared), tool.Name);
                    }
                }
            });
        }

        [Test]
        public void The_profile_lists_every_prompt_the_server_has_and_no_other()
        {
            var prompts = this.provider.GetServices<McpServerPrompt>().Select(prompt => prompt.ProtocolPrompt.Name);

            Assert.Multiple(() =>
            {
                Assert.That(Bullet(Section(this.profile, 4), "**Prompts**"), Is.EquivalentTo(prompts));
                Assert.That(Counted(Section(this.profile, 4), "Prompts"), Is.EqualTo(prompts.Count()));
            });
        }

        [Test]
        public void The_profile_lists_every_resource_template_the_server_has_and_no_other()
        {
            var templates = this.provider.GetServices<McpServerResource>()
                .Select(resource => resource.ProtocolResourceTemplate.UriTemplate);

            Assert.That(Bullet(Section(this.profile, 4), "**Resources:**").Where(code => code.Contains("://", StringComparison.Ordinal)), Is.EquivalentTo(templates));
        }

        public static IEnumerable<TestCaseData> Destinations()
        {
            yield return new TestCaseData(Upstream.DefaultApiBaseAddress.Host).SetArgDisplayNames("GitHub API host");
            yield return new TestCaseData(Upstream.DefaultRawContentBaseAddress.Host).SetArgDisplayNames("GitHub raw content host");
            yield return new TestCaseData(ApiUpstream.DefaultPimAddress.Host).SetArgDisplayNames("PIM host");
            yield return new TestCaseData(ApiUpstream.DefaultPimAddress.AbsolutePath).SetArgDisplayNames("PIM path");
            yield return new TestCaseData(Upstream.ReleaseRepository).SetArgDisplayNames("release repository");
            yield return new TestCaseData(Upstream.PilotRepository).SetArgDisplayNames("pilot repository");
            yield return new TestCaseData(ApiUpstream.ServicesRepository).SetArgDisplayNames("API services repository");
            yield return new TestCaseData(UvProvisioner.Repository).SetArgDisplayNames("uv repository");
        }

        [TestCaseSource(nameof(Destinations))]
        public void The_profile_names_every_host_and_repository_the_code_requests(string destination)
        {
            Assert.That(Code.Matches(Section(this.profile, 6)).Select(match => match.Groups[1].Value), Has.Member(destination));
        }

        public static IEnumerable<TestCaseData> EnvironmentVariables()
        {
            yield return new TestCaseData(5, HyphaHome.EnvironmentVariable).SetArgDisplayNames(HyphaHome.EnvironmentVariable);
            yield return new TestCaseData(7, GitHubToken.Variable).SetArgDisplayNames(GitHubToken.Variable);
            yield return new TestCaseData(7, GitHubToken.FallbackVariable).SetArgDisplayNames(GitHubToken.FallbackVariable);
            yield return new TestCaseData(9, UvProvisioner.UvCacheVariable).SetArgDisplayNames(UvProvisioner.UvCacheVariable);
            yield return new TestCaseData(9, UvProvisioner.UvPythonInstallVariable).SetArgDisplayNames(UvProvisioner.UvPythonInstallVariable);
        }

        [TestCaseSource(nameof(EnvironmentVariables))]
        public void The_profile_names_every_environment_variable_the_code_reads_or_sets(int section, string variable)
        {
            Assert.That(Section(this.profile, section), Does.Contain($"`{variable}`"));
        }

        [Test]
        public void The_profile_gives_the_uv_version_the_code_pins()
        {
            var section = Section(this.profile, 9);

            Assert.Multiple(() =>
            {
                Assert.That(section, Does.Contain($"`uv` {UvProvisioner.PinnedVersion}"));
                Assert.That(section, Does.Contain($"toolchain/uv/{UvProvisioner.PinnedVersion}/"));
            });
        }

        [Test]
        public void The_tool_table_is_read_by_its_column_names_and_a_row_may_name_several_tools()
        {
            const string text = """
                ## 4. Capabilities and side effects

                | Tool | Network | Open world | Destructive | Read-only |
                |---|---|---|---|---|
                | `hypha_a`, `hypha_b` | no | no | no | yes |
                | `hypha_c` | yes | yes | yes | no |

                ## 5. Next
                """;

            var rows = ToolRows(Section(text, 4));

            Assert.Multiple(() =>
            {
                Assert.That(rows.Keys, Is.EquivalentTo(new[] { "hypha_a", "hypha_b", "hypha_c" }));
                Assert.That(rows["hypha_b"], Is.EqualTo(new Annotations(true, false, false)));
                Assert.That(rows["hypha_c"], Is.EqualTo(new Annotations(false, true, true)));
            });
        }

        [Test]
        public void A_section_ends_at_the_next_section()
        {
            const string text = """
                ## 6. External network access

                `api.github.com`

                ## 7. Authentication and credentials

                `www.omg.org`
                """;

            Assert.That(Section(text, 6), Does.Contain("api.github.com").And.Not.Contain("www.omg.org"));
        }

        [Test]
        public void A_tool_named_twice_is_reported()
        {
            const string text = """
                | Tool | Read-only | Destructive | Open world |
                |---|---|---|---|
                | `hypha_a` | yes | no | no |
                | `hypha_a` | no | no | no |
                """;

            Assert.That(() => ToolRows(text), Throws.InstanceOf<FormatException>().With.Message.Contains("hypha_a"));
        }

        private static DirectoryInfo ProfileFolder()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder is not null && !File.Exists(Path.Combine(folder.FullName, KnowledgeLayout.CheckoutMarker)))
            {
                folder = folder.Parent;
            }

            return folder is null
                ? throw new DirectoryNotFoundException("the test runs outside the mycelium-hypha checkout")
                : new DirectoryInfo(Path.Combine(folder.FullName, "docs", "security-profile"));
        }

        private static string Section(string text, int number)
        {
            var lines = text.ReplaceLineEndings("\n").Split('\n');
            var start = Array.FindIndex(lines, line => line.StartsWith($"## {number}. ", StringComparison.Ordinal));
            if (start < 0)
            {
                throw new FormatException($"the profile has no section {number}");
            }

            var end = Array.FindIndex(lines, start + 1, line => line.StartsWith("## ", StringComparison.Ordinal));

            return string.Join('\n', lines[start..(end < 0 ? lines.Length : end)]);
        }

        private static Dictionary<string, Annotations> ToolRows(string section)
        {
            var rows = section.Split('\n').Select(line => line.Trim()).Where(line => line.StartsWith('|')).Select(Cells).ToList();
            var header = rows.FindIndex(cells => cells[0] == "Tool");
            if (header < 0)
            {
                throw new FormatException("the section has no table whose first column is Tool");
            }

            var columns = rows[header];
            int Column(string name) => Array.IndexOf(columns, name) is var index and >= 0
                ? index
                : throw new FormatException($"the tool table has no column {name}");

            var (readOnly, destructive, openWorld) = (Column("Read-only"), Column("Destructive"), Column("Open world"));
            var tools = new Dictionary<string, Annotations>(StringComparer.Ordinal);

            foreach (var cells in rows.Skip(header + 1).Where(cells => cells[0].Contains("`hypha_", StringComparison.Ordinal)))
            {
                var annotations = new Annotations(YesNo(cells[readOnly]), YesNo(cells[destructive]), YesNo(cells[openWorld]));
                foreach (var name in Code.Matches(cells[0]).Select(match => match.Groups[1].Value).Where(code => ToolName.IsMatch(code)))
                {
                    if (!tools.TryAdd(name, annotations))
                    {
                        throw new FormatException($"the tool table names {name} twice");
                    }
                }
            }

            return tools;
        }

        private static string[] Cells(string row) =>
            row.Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();

        private static bool YesNo(string cell) => cell switch
        {
            "yes" => true,
            "no" => false,
            _ => throw new FormatException($"an annotation cell holds '{cell}', not yes or no"),
        };

        private static IEnumerable<string> Bullet(string section, string label)
        {
            var line = section.Split('\n').SingleOrDefault(line => line.TrimStart().StartsWith($"- {label}", StringComparison.Ordinal))
                ?? throw new FormatException($"the section has no bullet {label}");

            return Code.Matches(line).Select(match => match.Groups[1].Value);
        }

        private static int Counted(string section, string what) =>
            Count.Matches(section).Where(match => match.Groups["what"].Value == what).Select(match => int.Parse(match.Groups["count"].Value)).Single();

        private sealed record Annotations(bool ReadOnly, bool Destructive, bool OpenWorld);
    }
}
