// ------------------------------------------------------------------------------------------------
// <copyright file="CacheLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Provisioning
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Where the downloaded CLI binary and an install's operational state live, under a root outside
    /// the plugin's own git-managed folder: a plugin update or reinstall could reset that tree mid-run,
    /// and an in-tree file would show as untracked noise in a maintainer's own checkout.
    /// </summary>
    /// <remarks>
    /// The launcher passes the <c>HYPHA_HOME</c> folder as the root (see <c>HomeFolder</c>), the one
    /// every client shares.
    /// </remarks>
    public sealed class CacheLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheLayout"/> class.
        /// </summary>
        /// <param name="root">The cache root: the <c>HYPHA_HOME</c> folder for the launcher.</param>
        /// <param name="installKey">See <see cref="ComputeInstallKey"/>.</param>
        public CacheLayout(DirectoryInfo root, string installKey)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentException.ThrowIfNullOrWhiteSpace(installKey);

            this.Root = root;
            this.InstallKey = installKey;
        }

        /// <summary>Gets the cache root, such as the <c>HYPHA_HOME</c> folder.</summary>
        public DirectoryInfo Root { get; }

        /// <summary>Gets the short key identifying this plugin install among possibly several.</summary>
        public string InstallKey { get; }

        /// <summary><c>bin/&lt;version&gt;/&lt;rid&gt;/</c>: where one CLI build is cached.</summary>
        /// <param name="version">The CLI version.</param>
        /// <param name="rid">The runtime identifier the build targets, e.g. <c>win-x64</c>.</param>
        /// <returns>The folder, which need not exist.</returns>
        public DirectoryInfo BinDirectory(string version, string rid) =>
            new(Path.Combine(this.Root.FullName, "bin", version, rid));

        /// <summary>The cached <c>hypha</c> executable itself.</summary>
        /// <remarks>
        /// The extension is derived from <paramref name="rid"/>, not the host OS: in practice they
        /// always agree (a published launcher only ever asks for its own platform's RID), but
        /// deriving it from the RID keeps this a pure function of its arguments rather than of the
        /// environment it happens to run in.
        /// </remarks>
        /// <param name="version">The CLI version.</param>
        /// <param name="rid">The runtime identifier the build targets, e.g. <c>win-x64</c>.</param>
        /// <returns>The executable, <c>hypha.exe</c> for a Windows RID and <c>hypha</c> otherwise; it need not exist.</returns>
        public FileInfo CliExecutable(string version, string rid) =>
            new(Path.Combine(
                this.BinDirectory(version, rid).FullName,
                rid.StartsWith("win", StringComparison.Ordinal) ? "hypha.exe" : "hypha"));

        /// <summary>
        /// Written only once the cached CLI's checksum has been verified; its presence is what lets a
        /// later run skip the network entirely.
        /// </summary>
        /// <param name="version">The CLI version.</param>
        /// <param name="rid">The runtime identifier the build targets, e.g. <c>win-x64</c>.</param>
        /// <returns>The <c>.ok</c> marker file, which need not exist.</returns>
        public FileInfo OkMarker(string version, string rid) =>
            new(Path.Combine(this.BinDirectory(version, rid).FullName, ".ok"));

        /// <summary><c>state/&lt;install-key&gt;/</c>: this install's operational state.</summary>
        public DirectoryInfo StateDirectory => new(Path.Combine(this.Root.FullName, "state", this.InstallKey));

        /// <summary>Where a failed CLI download/verification is recorded for manual diagnosis.</summary>
        public FileInfo DownloadErrorLog => new(Path.Combine(this.StateDirectory.FullName, "download-error.log"));

        /// <summary>
        /// A short, stable, filesystem-safe key for a plugin root path, so a maintainer's dev checkout
        /// and a marketplace install never collide.
        /// </summary>
        /// <param name="pluginRootFullPath">The plugin root's path, compared case-insensitively.</param>
        /// <returns>The first 16 lowercase hexadecimal digits of the path's SHA256.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="pluginRootFullPath"/> is <c>null</c>, empty or white space.
        /// </exception>
        public static string ComputeInstallKey(string pluginRootFullPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginRootFullPath);

            var normalized = Path.GetFullPath(pluginRootFullPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Case-insensitive: the same checkout can be reached via differently-cased paths on
            // Windows/macOS, and should still resolve to the same cache entry.
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized.ToUpperInvariant()));

            return Convert.ToHexStringLower(hash)[..16];
        }
    }
}
