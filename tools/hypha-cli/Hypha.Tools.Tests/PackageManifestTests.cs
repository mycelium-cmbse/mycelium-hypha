// ------------------------------------------------------------------------------------------------
// <copyright file="PackageManifestTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Nodes;
    using System.Text.RegularExpressions;

    // The drift check: every client manifest in the repository is what manifest.source.json renders to.
    // After changing the source (a version bump, a new description), run the explicit Bless test:
    //   dotnet test mycelium-hypha.sln --filter "FullyQualifiedName~PackageManifestTests.Bless_the_manifests"
    [TestFixture]
    public class PackageManifestTests
    {
        private static readonly DirectoryInfo Root = FindRoot();

        private JsonObject source = null!;

        [SetUp]
        public void SetUp() => this.source = PackageManifests.ReadSource(Root);

        public static IEnumerable<string> Generated() =>
            PackageManifests.Render(PackageManifests.ReadSource(Root)).Keys;

        [TestCaseSource(nameof(Generated))]
        public void A_manifest_is_what_the_source_renders(string path)
        {
            var expected = PackageManifests.Render(this.source)[path];

            Assert.That(
                Read(path),
                Is.EqualTo(expected),
                $"{path} differs from {PackageManifests.Source}. Change the source and run the Bless_the_manifests test, never the manifest.");
        }

        [Test]
        public void The_marketplace_installs_the_package_version_s_tag()
        {
            var committed = Read(PackageManifests.ClaudeMarketplace);

            Assert.That(
                committed,
                Is.EqualTo(PackageManifests.RenderMarketplace(this.source, committed)),
                $"the hypha entry of {PackageManifests.ClaudeMarketplace} must install from the tag v<version> of {PackageManifests.Source}.");
        }

        [Test]
        [Explicit("Writes the committed manifests from manifest.source.json; run after changing the source.")]
        public void Bless_the_manifests()
        {
            foreach (var (path, content) in PackageManifests.Render(this.source))
            {
                File.WriteAllText(Absolute(path), content);
            }

            File.WriteAllText(
                Absolute(PackageManifests.ClaudeMarketplace),
                PackageManifests.RenderMarketplace(this.source, Read(PackageManifests.ClaudeMarketplace)));
        }

        [Test]
        public void The_two_versions_are_semantic_versions_on_their_own_lines()
        {
            var version = this.source["version"]!.GetValue<string>();
            var hypha = this.source["hyphaVersion"]!.GetValue<string>();

            Assert.Multiple(() =>
            {
                Assert.That(version, Does.Match(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$"), "the package version: the plugin's and every client manifest's");
                Assert.That(hypha, Does.Match(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$"), "the hypha version: the tools release the launcher downloads");
            });
        }

        [Test]
        public void The_agent_plugin_manifest_keeps_to_the_closed_schema()
        {
            var manifest = JsonNode.Parse(PackageManifests.Render(this.source)[PackageManifests.AgentPlugin])!.AsObject();
            string[] allowed = ["$schema", "name", "version", "description", "author", "homepage", "repository", "license", "keywords", "extensions"];

            Assert.Multiple(() =>
            {
                Assert.That(manifest.Select(property => property.Key), Is.SubsetOf(allowed), "any other top-level field does not conform");
                Assert.That(manifest["$schema"]!.GetValue<string>(), Is.EqualTo(PackageManifests.AgentPluginSchema));
                Assert.That(manifest["name"]!.GetValue<string>(), Does.Match("^[a-z0-9.-]{1,64}$"));
                Assert.That(manifest["author"]!.AsObject().Select(property => property.Key), Is.SubsetOf(new[] { "name", "email", "url" }));
                Assert.That(manifest["repository"]!.GetValueKind(), Is.EqualTo(System.Text.Json.JsonValueKind.String));
            });
        }

        [Test]
        public void The_agent_plugin_starts_the_launcher_by_a_path_from_the_plugin_root()
        {
            var servers = JsonNode.Parse(PackageManifests.Render(this.source)[PackageManifests.AgentPluginServers])!.AsObject();
            var server = servers["mcpServers"]!.AsObject().Single().Value!.AsObject();

            Assert.Multiple(() =>
            {
                Assert.That(servers.Select(property => property.Key), Is.EqualTo(new[] { "$schema", "mcpServers" }), "no other top-level field is allowed");
                Assert.That(server["type"]!.GetValue<string>(), Is.EqualTo("stdio"));
                Assert.That(server["command"]!.GetValue<string>(), Is.EqualTo("./bin/hypha-launch"), "a bundled executable begins with ./ and takes no variable");
            });
        }

        [Test]
        public void Every_client_starts_the_same_launcher_under_the_same_server_name()
        {
            var rendered = PackageManifests.Render(this.source);
            var claude = Server(rendered[PackageManifests.ClaudePlugin]);
            var agent = Server(rendered[PackageManifests.AgentPluginServers]);
            var gemini = Server(rendered[PackageManifests.GeminiExtension]);

            Assert.Multiple(() =>
            {
                Assert.That(new[] { claude.Name, agent.Name, gemini.Name }, Is.All.EqualTo("hypha"));
                Assert.That(claude.Command, Is.EqualTo("${CLAUDE_PLUGIN_ROOT}/bin/hypha-launch"));
                Assert.That(gemini.Command, Is.EqualTo("${extensionPath}${/}bin${/}hypha-launch"));
                Assert.That(File.Exists(Absolute("bin/hypha-launch")), Is.True, "the POSIX dispatch script every manifest names");
                Assert.That(File.Exists(Absolute("bin/hypha-launch.exe")), Is.True, "what Windows finds from the same path");
            });

            static (string Name, string Command) Server(string manifest)
            {
                var server = JsonNode.Parse(manifest)!["mcpServers"]!.AsObject().Single();

                return (server.Key, server.Value!["command"]!.GetValue<string>());
            }
        }

        [Test]
        public void Every_platform_the_dispatch_script_starts_is_built_and_released()
        {
            // Three places name the platforms and nothing else ties them together: the script every POSIX
            // client runs, the workflow that builds the launcher it execs, and the release that publishes
            // the hypha that launcher downloads. A platform in one and not the others starts nothing.
            var script = Regex.Matches(Read("bin/hypha-launch"), @"\brid=([a-z0-9-]+)").Select(match => match.Groups[1].Value).ToList();
            var workflow = Read(".github/workflows/launcher-binaries.yml");
            var built = Regex.Matches(workflow, @"^\s+rid: ([a-z0-9-]+)\s*$", RegexOptions.Multiline).Select(match => match.Groups[1].Value).ToList();
            var placed = Regex.Match(workflow, @"for rid in ([a-z0-9 -]+); do").Groups[1].Value.Split(' ');
            var released = Regex.Matches(Read(".github/workflows/release.yml"), @"for RID in ([a-z0-9 -]+); do")
                .Select(match => match.Groups[1].Value.Split(' '))
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(script, Is.EquivalentTo(new[] { "linux-x64", "linux-arm64", "osx-arm64", "osx-x64" }));
                Assert.That(built, Is.EquivalentTo(script.Append("win-x64")), "the launcher workflow builds each platform");
                Assert.That(placed, Is.EquivalentTo(script), "and places each POSIX one under bin/<rid>/");
                Assert.That(released, Has.Count.EqualTo(2), "the publish loop and the archive loop");
                Assert.That(released, Has.All.EquivalentTo(script.Append("win-x64")), "the release publishes a hypha for each");
                Assert.That(
                    script.Where(rid => !File.Exists(Absolute($"bin/{rid}/hypha-launch"))),
                    Is.Empty,
                    "a launcher is committed for each: run the Launcher binaries workflow after adding a platform");
            });
        }

        [Test]
        public void Every_manifest_with_a_version_carries_the_package_version()
        {
            var version = this.source["version"]!.GetValue<string>();
            var rendered = PackageManifests.Render(this.source);

            Assert.Multiple(() =>
            {
                foreach (var path in new[] { PackageManifests.ClaudePlugin, PackageManifests.AgentPlugin, PackageManifests.GeminiExtension })
                {
                    Assert.That(JsonNode.Parse(rendered[path])!["version"]!.GetValue<string>(), Is.EqualTo(version), path);
                }

                Assert.That(
                    JsonNode.Parse(rendered[PackageManifests.Launcher])!["hyphaVersion"]!.GetValue<string>(),
                    Is.EqualTo(this.source["hyphaVersion"]!.GetValue<string>()));
            });
        }

        [Test]
        public void A_rendered_manifest_ends_with_one_line_feed_and_has_no_carriage_return()
        {
            Assert.Multiple(() =>
            {
                foreach (var (path, content) in PackageManifests.Render(this.source))
                {
                    Assert.That(content, Does.Not.Contain("\r"), path);
                    Assert.That(Regex.IsMatch(content, @"[^\n]\n\z"), Is.True, path);
                }
            });
        }

        // The committed file with LF endings: a checkout on Windows may hold it with CRLF.
        private static string Read(string path) =>
            File.ReadAllText(Absolute(path)).Replace("\r\n", "\n", StringComparison.Ordinal);

        private static string Absolute(string path) =>
            Path.Combine(Root.FullName, path.Replace('/', Path.DirectorySeparatorChar));

        private static DirectoryInfo FindRoot()
        {
            var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

            while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "mycelium-hypha.sln")))
            {
                folder = folder.Parent;
            }

            return folder ?? throw new InvalidOperationException("mycelium-hypha.sln was not found above the test folder.");
        }
    }
}
