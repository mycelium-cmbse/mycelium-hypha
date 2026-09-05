// ------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// The <c>SessionStart</c> hook entry point: ensures the real <c>hypha</c> CLI is cached, runs
    /// <c>hypha check</c> to compare local releases against what is offerable upstream, and reports
    /// the result. Never fetches or generates anything itself - deciding whether to act on what it
    /// reports is left entirely to the user, through a skill.
    /// </summary>
    /// <remarks>
    /// Committed per platform under <c>hooks/native/&lt;rid&gt;/</c> and reached through a one-line POSIX
    /// shell dispatch shim in <c>.claude-plugin/plugin.json</c> - see <c>tools/hypha-cli/README.md</c>.
    /// Kept deliberately small: this never generates anything itself, only downloads and launches the
    /// tool that does.
    /// </remarks>
    public static class Program
    {
        /// <summary>Runs the hook.</summary>
        public static async Task<int> Main()
        {
            try
            {
                using var client = CliProvisioner.CreateClient();
                await RunAsync(client);
            }
            catch (Exception)
            {
                // A SessionStart hook's failure should never surface as a Claude Code error: silence
                // is exactly what happens when there is nothing actionable to say, which is also the
                // right outcome for a hook that could not even get as far as deciding that.
            }

            return 0;
        }

        /// <summary>
        /// The hook's real work, taking the HTTP client and the cache root resolver as parameters so a
        /// test can supply a stub transport and an isolated cache directory instead of a real one
        /// reaching GitHub and the real, shared <c>%LOCALAPPDATA%\mycelium-hypha</c> (or platform
        /// equivalent) - which a test must never read from or write to, since a machine that has
        /// actually used this hook before may already have a matching version/RID cached there.
        /// </summary>
        internal static async Task RunAsync(
            HttpClient client, Func<Environment.SpecialFolder, string>? getFolderPath = null)
        {
            var pluginRoot = ResolvePluginRoot();
            var version = ReadPinnedCliVersion(pluginRoot);

            if (version is null)
            {
                return;
            }

            var rid = RuntimeInformation.RuntimeIdentifier;
            var cache = new CacheLayout(
                CacheLayout.ResolveRoot(getFolderPath), CacheLayout.ComputeInstallKey(pluginRoot.FullName));

            var provisioner = new CliProvisioner(client);

            var executable = await provisioner.EnsureAsync(cache, version, rid, default);
            if (executable is null)
            {
                return;
            }

            // check only ever compares local releases against what is offerable upstream - cheap
            // enough to wait for here, synchronously, with nothing left to poll for later.
            var output = await CheckRunner.RunAndCaptureOutputAsync(
                executable.FullName,
                ["check", "--json", "--no-logo", "--repository-root", pluginRoot.FullName],
                pluginRoot.FullName,
                default);

            var context = CheckResultSummarizer.Summarize(ParseCheckResult(output), pluginRoot);

            if (context is not null)
            {
                await Console.Out.WriteAsync(JsonSerializer.Serialize(
                    HookOutput.SessionStart(context), HookJsonContext.Default.HookOutput));
            }
        }

        internal static HookCheckResult? ParseCheckResult(string? output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize(output, HookJsonContext.Default.HookCheckResult);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        internal static DirectoryInfo ResolvePluginRoot()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT");
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
            {
                return new DirectoryInfo(fromEnvironment);
            }

            // Falls back to walking up from this executable's own location:
            // hooks/native/<rid>/hypha-hook sits three levels below the plugin root.
            var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            return baseDirectory.Parent?.Parent?.Parent ?? baseDirectory;
        }

        internal static string? ReadPinnedCliVersion(DirectoryInfo pluginRoot)
        {
            var manifestPath = Path.Combine(pluginRoot.FullName, ".claude-plugin", "plugin.json");

            if (!File.Exists(manifestPath))
            {
                return null;
            }

            try
            {
                var manifest = JsonSerializer.Deserialize(
                    File.ReadAllText(manifestPath), HookJsonContext.Default.PluginManifest);

                return string.IsNullOrWhiteSpace(manifest?.HyphaCliVersion) ? null : manifest.HyphaCliVersion;
            }
            catch (JsonException)
            {
                return null;
            }
        }

    }
}
