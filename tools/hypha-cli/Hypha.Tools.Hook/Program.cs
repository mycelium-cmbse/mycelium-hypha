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
    using System.Runtime.InteropServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// The <c>SessionStart</c> hook entry point: ensures the real <c>hypha</c> CLI is cached, reports
    /// whatever progress a previous <c>hypha sync</c> run left behind, and launches a new one detached.
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
                await RunAsync();
            }
            catch (Exception)
            {
                // A SessionStart hook's failure should never surface as a Claude Code error: silence
                // is exactly what happens when there is nothing actionable to say, which is also the
                // right outcome for a hook that could not even get as far as deciding that.
            }

            return 0;
        }

        private static async Task RunAsync()
        {
            var pluginRoot = ResolvePluginRoot();
            var version = ReadPinnedCliVersion(pluginRoot);

            if (version is null)
            {
                return;
            }

            var rid = RuntimeInformation.RuntimeIdentifier;
            var cache = new CacheLayout(CacheLayout.ResolveRoot(), CacheLayout.ComputeInstallKey(pluginRoot.FullName));

            var status = ReadStatus(cache);
            var context = StatusSummarizer.Summarize(status, cache.SyncLogFile.FullName);

            if (context is not null)
            {
                await Console.Out.WriteAsync(JsonSerializer.Serialize(
                    HookOutput.SessionStart(context), HookJsonContext.Default.HookOutput));
            }

            using var client = CliProvisioner.CreateClient();
            var provisioner = new CliProvisioner(client);

            var executable = await provisioner.EnsureAsync(cache, version, rid, default);
            if (executable is null)
            {
                return;
            }

            cache.StateDirectory.Create();

            DetachedProcessLauncher.Start(
                executable.FullName,
                [
                    "sync",
                    "--repository-root", pluginRoot.FullName,
                    "--status-file", cache.StatusFile.FullName,
                    "--log-file", cache.SyncLogFile.FullName,
                    "--no-logo",
                    "--log-level", "Warning",
                ],
                pluginRoot.FullName);
        }

        private static DirectoryInfo ResolvePluginRoot()
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

        private static string? ReadPinnedCliVersion(DirectoryInfo pluginRoot)
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

        private static HookSyncStatus? ReadStatus(CacheLayout cache)
        {
            var file = cache.StatusFile;
            file.Refresh();

            if (!file.Exists)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize(
                    File.ReadAllText(file.FullName), HookJsonContext.Default.HookSyncStatus);
            }
            catch (IOException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
