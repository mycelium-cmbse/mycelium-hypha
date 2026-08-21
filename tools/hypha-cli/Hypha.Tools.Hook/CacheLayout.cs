// ------------------------------------------------------------------------------------------------
// <copyright file="CacheLayout.cs" company="Starion Group S.A.">
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
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Where the downloaded CLI binary and this install's operational state live - deliberately
    /// outside the plugin's own git-managed folder, since a plugin update/reinstall could reset that
    /// tree mid-run and an in-tree file would show as untracked noise in a maintainer's own checkout.
    /// </summary>
    /// <remarks>
    /// Resolved from <see cref="System.Environment.SpecialFolder.LocalApplicationData"/> - the
    /// standard cross-platform .NET API (<c>%LOCALAPPDATA%</c> on Windows, <c>~/.local/share</c> on
    /// Linux, <c>~/Library/Application Support</c> on macOS), so this hand-rolls no per-OS env var
    /// lookup of its own.
    /// </remarks>
    public sealed class CacheLayout
    {
        private const string CacheFolderName = "mycelium-hypha";

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheLayout"/> class.
        /// </summary>
        /// <param name="root">The cache root - see <see cref="ResolveRoot"/>.</param>
        /// <param name="installKey">See <see cref="ComputeInstallKey"/>.</param>
        public CacheLayout(DirectoryInfo root, string installKey)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentException.ThrowIfNullOrWhiteSpace(installKey);

            this.Root = root;
            this.InstallKey = installKey;
        }

        /// <summary>Gets the cache root, e.g. <c>%LOCALAPPDATA%\mycelium-hypha</c>.</summary>
        public DirectoryInfo Root { get; }

        /// <summary>Gets the short key identifying this plugin install among possibly several.</summary>
        public string InstallKey { get; }

        /// <summary><c>bin/&lt;version&gt;/&lt;rid&gt;/</c> - where one CLI build is cached.</summary>
        public DirectoryInfo BinDirectory(string version, string rid) =>
            new(Path.Combine(this.Root.FullName, "bin", version, rid));

        /// <summary>The cached <c>hypha</c> executable itself.</summary>
        /// <remarks>
        /// The extension is derived from <paramref name="rid"/>, not the host OS: in practice they
        /// always agree (a published hook binary only ever asks for its own platform's RID), but
        /// deriving it from the RID keeps this a pure function of its arguments rather than of the
        /// environment it happens to run in.
        /// </remarks>
        public FileInfo CliExecutable(string version, string rid) =>
            new(Path.Combine(
                this.BinDirectory(version, rid).FullName,
                rid.StartsWith("win", StringComparison.Ordinal) ? "hypha.exe" : "hypha"));

        /// <summary>
        /// Written only once the cached CLI's checksum has been verified - its presence is what lets a
        /// later run skip the network entirely.
        /// </summary>
        public FileInfo OkMarker(string version, string rid) =>
            new(Path.Combine(this.BinDirectory(version, rid).FullName, ".ok"));

        /// <summary><c>state/&lt;install-key&gt;/</c> - this install's operational state.</summary>
        public DirectoryInfo StateDirectory => new(Path.Combine(this.Root.FullName, "state", this.InstallKey));

        /// <summary>Where a failed CLI download/verification is recorded for manual diagnosis.</summary>
        public FileInfo DownloadErrorLog => new(Path.Combine(this.StateDirectory.FullName, "download-error.log"));

        /// <summary>
        /// Resolves the cache root via <see cref="System.Environment.SpecialFolder.LocalApplicationData"/>.
        /// </summary>
        /// <param name="getFolderPath">
        /// Injectable for tests; defaults to <see cref="System.Environment.GetFolderPath(System.Environment.SpecialFolder)"/>.
        /// </param>
        public static DirectoryInfo ResolveRoot(Func<Environment.SpecialFolder, string>? getFolderPath = null)
        {
            getFolderPath ??= Environment.GetFolderPath;

            return new DirectoryInfo(
                Path.Combine(getFolderPath(Environment.SpecialFolder.LocalApplicationData), CacheFolderName));
        }

        /// <summary>
        /// A short, stable, filesystem-safe key for a plugin root path, so a maintainer's dev checkout
        /// and a marketplace install never collide.
        /// </summary>
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
