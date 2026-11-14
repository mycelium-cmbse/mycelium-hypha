// ------------------------------------------------------------------------------------------------
// <copyright file="PackageManifests.cs" company="Starion Group S.A.">
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
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    // Renders every client manifest from manifest.source.json, the one place the package's name, its two
    // versions, its description and the server's command are written. Each client reads a manifest of its
    // own format from the repository, so the manifests are committed; PackageManifestTests fails when one
    // of them is not what this renders, and its explicit Bless test writes them.
    internal static class PackageManifests
    {
        public const string Source = "manifest.source.json";

        public const string ClaudePlugin = ".claude-plugin/plugin.json";

        public const string ClaudeMarketplace = ".claude-plugin/marketplace.json";

        public const string Launcher = "bin/hypha-launch.json";

        public const string AgentPlugin = "plugin.json";

        public const string AgentPluginServers = "mcp.json";

        public const string GeminiExtension = "gemini-extension.json";

        public const string AgentPluginSchema = "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json";

        public const string AgentPluginServersSchema = "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json";

        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        // Every manifest generated whole, by its path from the repository root with / separators.
        public static IReadOnlyDictionary<string, string> Render(JsonObject source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var server = source["server"]!.AsObject();
            var serverName = server["name"]!.GetValue<string>();
            var command = server["command"]!.GetValue<string>();

            return new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                // Claude Code: its own manifest, with the server inline. The plugin root comes from a
                // variable, since Claude Code starts a server from the session's folder, not the plugin's.
                [ClaudePlugin] = Text(new JsonObject
                {
                    ["name"] = Copy(source, "name"),
                    ["displayName"] = Copy(source, "displayName"),
                    ["version"] = Copy(source, "version"),
                    ["description"] = Copy(source, "description"),
                    ["author"] = Copy(source, "author"),
                    ["homepage"] = Copy(source, "homepage"),
                    ["repository"] = Copy(source, "repository"),
                    ["license"] = Copy(source, "license"),
                    ["keywords"] = Copy(source, "keywords"),
                    ["mcpServers"] = new JsonObject
                    {
                        [serverName] = new JsonObject { ["command"] = "${CLAUDE_PLUGIN_ROOT}/" + command },
                    },
                }),

                // The hypha the launcher downloads and starts: the tools' version line, not the package's.
                [Launcher] = Text(new JsonObject { ["hyphaVersion"] = Copy(source, "hyphaVersion") }),

                // Agent Plugins 1.0: a closed schema, so no displayName, and the servers in a file of their own.
                [AgentPlugin] = Text(new JsonObject
                {
                    ["$schema"] = AgentPluginSchema,
                    ["name"] = Copy(source, "name"),
                    ["version"] = Copy(source, "version"),
                    ["description"] = Copy(source, "description"),
                    ["author"] = Copy(source, "author"),
                    ["homepage"] = Copy(source, "homepage"),
                    ["repository"] = Copy(source, "repository"),
                    ["license"] = Copy(source, "license"),
                    ["keywords"] = Copy(source, "keywords"),
                }),

                // A bundled executable is named by a path from the plugin root that begins with "./".
                [AgentPluginServers] = Text(new JsonObject
                {
                    ["$schema"] = AgentPluginServersSchema,
                    ["mcpServers"] = new JsonObject
                    {
                        [serverName] = new JsonObject { ["type"] = "stdio", ["command"] = "./" + command },
                    },
                }),

                // Gemini CLI: the extension's folder and the path separator come from its variables.
                [GeminiExtension] = Text(new JsonObject
                {
                    ["name"] = Copy(source, "name"),
                    ["version"] = Copy(source, "version"),
                    ["description"] = Copy(source, "description"),
                    ["mcpServers"] = new JsonObject
                    {
                        [serverName] = new JsonObject
                        {
                            ["command"] = "${extensionPath}${/}" + string.Join("${/}", command.Split('/')),
                        },
                    },
                }),
            };
        }

        // The marketplace catalog is written by hand, but for the tag its hypha entry installs from, which
        // is the package version's tag (RELEASING.md): the catalog as it is, with that one value set.
        public static string RenderMarketplace(JsonObject source, string marketplace)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(marketplace);

            var catalog = JsonNode.Parse(marketplace)!.AsObject();
            var name = source["name"]!.GetValue<string>();
            var entry = catalog["plugins"]!.AsArray()
                .Select(plugin => plugin!.AsObject())
                .Single(plugin => plugin["name"]!.GetValue<string>() == name);

            entry["source"]!["ref"] = "v" + source["version"]!.GetValue<string>();

            return Text(catalog);
        }

        public static JsonObject ReadSource(DirectoryInfo root) =>
            JsonNode.Parse(File.ReadAllText(Path.Combine(root.FullName, Source)))!.AsObject();

        // Two-space indent, non-ASCII as written, LF endings and one closing newline, whatever the platform.
        private static string Text(JsonNode node) =>
            node.ToJsonString(Format).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";

        private static JsonNode Copy(JsonObject source, string property) =>
            source[property]?.DeepClone() ?? throw new InvalidOperationException($"{Source} has no '{property}'.");
    }
}
