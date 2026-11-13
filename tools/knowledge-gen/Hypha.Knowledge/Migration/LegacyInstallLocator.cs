// ------------------------------------------------------------------------------------------------
// <copyright file="LegacyInstallLocator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Finds the Claude plugin's and the VS Code extension's own knowledge folders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Claude plugin keeps its knowledge in its own folder, one per installed plugin version, under
    /// <c>plugins/cache/&lt;marketplace&gt;/hypha/&lt;version&gt;/</c> of the Claude configuration folder
    /// (<c>CLAUDE_CONFIG_DIR</c>, or <c>.claude</c> in the user's home).
    /// </para>
    /// <para>
    /// The VS Code extension keeps it in its global storage,
    /// <c>&lt;product&gt;/User/globalStorage/mycelium-cmbse.mycelium-hypha/</c> of the per-user
    /// configuration folder: <c>%APPDATA%</c> on Windows, <c>~/Library/Application Support</c> on macOS,
    /// and <c>$XDG_CONFIG_HOME</c> or <c>~/.config</c> on Linux, for VS Code, VS Code Insiders and VSCodium.
    /// </para>
    /// </remarks>
    public sealed class LegacyInstallLocator : ILegacyInstallLocator
    {
        /// <summary>The plugin's name, as its folder in the plugin cache is named.</summary>
        public const string PluginName = "hypha";

        /// <summary>The extension's identifier, as its global storage folder is named.</summary>
        public const string ExtensionId = "mycelium-cmbse.mycelium-hypha";

        /// <summary>The VS Code builds whose global storage is looked in, by their configuration folder's name.</summary>
        private static readonly string[] Products = ["Code", "Code - Insiders", "VSCodium"];

        /// <summary>Reads an environment variable.</summary>
        private readonly Func<string, string?> getEnvironmentVariable;

        /// <summary>Resolves a special folder to its path.</summary>
        private readonly Func<Environment.SpecialFolder, string> getFolderPath;

        /// <summary>Whether this is Windows, which keeps VS Code's configuration in the roaming application data.</summary>
        private readonly bool windows;

        /// <summary>Whether this is macOS, which keeps it in <c>~/Library/Application Support</c>.</summary>
        private readonly bool macOS;

        /// <summary>Initializes a new instance of the <see cref="LegacyInstallLocator"/> class, for this machine.</summary>
        public LegacyInstallLocator()
            : this(Environment.GetEnvironmentVariable, Environment.GetFolderPath, OperatingSystem.IsWindows(), OperatingSystem.IsMacOS())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LegacyInstallLocator"/> class.</summary>
        /// <param name="getEnvironmentVariable">Reads an environment variable; a test passes its own.</param>
        /// <param name="getFolderPath">Resolves a special folder to its path; a test passes its own.</param>
        /// <param name="windows">Whether the folders are Windows'.</param>
        /// <param name="macOS">Whether the folders are macOS'.</param>
        /// <exception cref="ArgumentNullException"><paramref name="getEnvironmentVariable"/> or <paramref name="getFolderPath"/> is <c>null</c>.</exception>
        public LegacyInstallLocator(
            Func<string, string?> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath, bool windows, bool macOS)
        {
            ArgumentNullException.ThrowIfNull(getEnvironmentVariable);
            ArgumentNullException.ThrowIfNull(getFolderPath);

            this.getEnvironmentVariable = getEnvironmentVariable;
            this.getFolderPath = getFolderPath;
            this.windows = windows;
            this.macOS = macOS;
        }

        /// <summary>Finds every such folder on this machine that has installed something.</summary>
        /// <returns>The folders holding a <c>knowledge/versions.json</c>.</returns>
        public IReadOnlyList<LegacyInstall> Locate() =>
            this.Plugins().Concat(this.Extensions())
                .Where(install => File.Exists(Path.Combine(install.Root.FullName, "knowledge", VersionManifest.FileName)))
                .ToList();

        /// <summary>Every version of the Claude plugin in the plugin cache.</summary>
        /// <returns>Each version's folder.</returns>
        private IEnumerable<LegacyInstall> Plugins()
        {
            var config = this.ClaudeConfigFolder();
            var cache = config is null ? null : new DirectoryInfo(Path.Combine(config, "plugins", "cache"));

            if (cache is null || !cache.Exists)
            {
                return [];
            }

            return cache.EnumerateDirectories()
                .Select(marketplace => new DirectoryInfo(Path.Combine(marketplace.FullName, PluginName)))
                .Where(plugin => plugin.Exists)
                .SelectMany(plugin => plugin.EnumerateDirectories())
                .Select(version => new LegacyInstall(version, $"Claude plugin {version.Name}"));
        }

        /// <summary>The VS Code extension's global storage of each VS Code build.</summary>
        /// <returns>Each build's storage folder.</returns>
        private IEnumerable<LegacyInstall> Extensions()
        {
            var settings = this.SettingsFolder();

            if (string.IsNullOrEmpty(settings))
            {
                return [];
            }

            return Products
                .Select(product => (product, storage: new DirectoryInfo(Path.Combine(settings, product, "User", "globalStorage", ExtensionId))))
                .Where(candidate => candidate.storage.Exists)
                .Select(candidate => new LegacyInstall(candidate.storage, candidate.product == "Code" ? "VS Code" : candidate.product));
        }

        /// <summary>The Claude configuration folder: <c>CLAUDE_CONFIG_DIR</c>, or <c>.claude</c> in the user's home.</summary>
        /// <returns>The folder, or <c>null</c> when neither is known.</returns>
        private string? ClaudeConfigFolder()
        {
            var configured = this.getEnvironmentVariable("CLAUDE_CONFIG_DIR");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.Trim();
            }

            return this.InHome(".claude");
        }

        /// <summary>
        /// The per-user folder VS Code keeps its configuration in: the roaming application data on Windows,
        /// <c>~/Library/Application Support</c> on macOS, and <c>$XDG_CONFIG_HOME</c> or <c>~/.config</c> on Linux.
        /// </summary>
        /// <returns>The folder, or <c>null</c> when it is not known.</returns>
        private string? SettingsFolder()
        {
            if (this.windows)
            {
                var data = this.getFolderPath(Environment.SpecialFolder.ApplicationData);

                return string.IsNullOrEmpty(data) ? null : data;
            }

            if (this.macOS)
            {
                return this.InHome("Library", "Application Support");
            }

            var configured = this.getEnvironmentVariable("XDG_CONFIG_HOME");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.Trim();
            }

            return this.InHome(".config");
        }

        /// <summary>A folder in the user's home.</summary>
        /// <param name="parts">The folder's path below the home.</param>
        /// <returns>The folder, or <c>null</c> when the home is not known.</returns>
        private string? InHome(params string[] parts)
        {
            var home = this.getFolderPath(Environment.SpecialFolder.UserProfile);

            return string.IsNullOrEmpty(home) ? null : Path.Combine([home, .. parts]);
        }
    }
}
