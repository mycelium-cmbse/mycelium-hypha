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
            var configured = this.getEnvironmentVariable("CLAUDE_CONFIG_DIR");
            var home = this.getFolderPath(Environment.SpecialFolder.UserProfile);
            var config = !string.IsNullOrWhiteSpace(configured)
                ? configured.Trim()
                : string.IsNullOrEmpty(home) ? null : Path.Combine(home, ".claude");

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
            var home = this.getFolderPath(Environment.SpecialFolder.UserProfile);
            var configured = this.getEnvironmentVariable("XDG_CONFIG_HOME");

            var settings = this.windows
                ? this.getFolderPath(Environment.SpecialFolder.ApplicationData)
                : this.macOS
                    ? string.IsNullOrEmpty(home) ? null : Path.Combine(home, "Library", "Application Support")
                    : !string.IsNullOrWhiteSpace(configured) ? configured.Trim() : string.IsNullOrEmpty(home) ? null : Path.Combine(home, ".config");

            if (string.IsNullOrEmpty(settings))
            {
                return [];
            }

            return Products
                .Select(product => (product, storage: new DirectoryInfo(Path.Combine(settings, product, "User", "globalStorage", ExtensionId))))
                .Where(candidate => candidate.storage.Exists)
                .Select(candidate => new LegacyInstall(candidate.storage, candidate.product == "Code" ? "VS Code" : candidate.product));
        }
    }
}
