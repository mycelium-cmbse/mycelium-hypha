// ------------------------------------------------------------------------------------------------
// <copyright file="Launch.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Launcher.Provisioning;

    /// <summary>
    /// Finds the <c>hypha</c> a client package starts: <c>HYPHA_BIN</c> when it is set, otherwise the
    /// version <c>bin/hypha-launch.json</c> names, provisioned into <c>HYPHA_HOME/bin/</c>.
    /// </summary>
    /// <remarks>
    /// It resolves the package from its own location and <c>HYPHA_HOME</c> itself, and never reads a
    /// client's path variables (<c>CLAUDE_PLUGIN_ROOT</c> and the like), so one launcher serves every
    /// package format.
    /// </remarks>
    internal sealed class Launch
    {
        /// <summary>The environment variable naming a <c>hypha</c> to start instead of provisioning one.</summary>
        public const string BinVariable = "HYPHA_BIN";

        /// <summary>The first <c>hypha</c> that has <c>serve</c>; an older one prints its help on standard output instead.</summary>
        public const string MinimumVersion = "1.6.0";

        /// <summary>How many folders above the launcher <c>bin/hypha-launch.json</c> may be: <c>bin/</c> on Windows, <c>bin/&lt;rid&gt;/</c> elsewhere.</summary>
        private const int ManifestSearchDepth = 3;

        /// <summary>Reads an environment variable.</summary>
        private readonly Func<string, string?> getEnvironmentVariable;

        /// <summary>Resolves a special folder to its path.</summary>
        private readonly Func<Environment.SpecialFolder, string> getFolderPath;

        /// <summary>Downloads and verifies a <c>hypha</c> release.</summary>
        private readonly CliProvisioner provisioner;

        /// <summary>Initializes a new instance of the <see cref="Launch"/> class.</summary>
        /// <param name="getEnvironmentVariable">Reads an environment variable.</param>
        /// <param name="getFolderPath">Resolves a special folder to its path.</param>
        /// <param name="provisioner">Downloads and verifies a <c>hypha</c> release.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public Launch(Func<string, string?> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath, CliProvisioner provisioner)
        {
            ArgumentNullException.ThrowIfNull(getEnvironmentVariable);
            ArgumentNullException.ThrowIfNull(getFolderPath);
            ArgumentNullException.ThrowIfNull(provisioner);

            this.getEnvironmentVariable = getEnvironmentVariable;
            this.getFolderPath = getFolderPath;
            this.provisioner = provisioner;
        }

        /// <summary>Finds <c>bin/hypha-launch.json</c> above the launcher.</summary>
        /// <param name="baseDirectory">The launcher's own folder.</param>
        /// <returns>The manifest, or <c>null</c> when none is within reach.</returns>
        public static FileInfo? FindManifest(DirectoryInfo baseDirectory)
        {
            ArgumentNullException.ThrowIfNull(baseDirectory);

            var folder = baseDirectory;
            for (var depth = 0; depth < ManifestSearchDepth && folder is not null; depth++, folder = folder.Parent)
            {
                var manifest = new FileInfo(Path.Combine(folder.FullName, LaunchManifest.FileName));
                if (manifest.Exists)
                {
                    return manifest;
                }
            }

            return null;
        }

        /// <summary>Reads the version a manifest names.</summary>
        /// <param name="manifest">The manifest.</param>
        /// <returns>The version, or <c>null</c> when the manifest names none or cannot be read.</returns>
        public static string? ReadVersion(FileInfo manifest)
        {
            ArgumentNullException.ThrowIfNull(manifest);

            try
            {
                var read = JsonSerializer.Deserialize(File.ReadAllText(manifest.FullName), LauncherJsonContext.Default.LaunchManifest);

                return string.IsNullOrWhiteSpace(read?.HyphaVersion) ? null : read.HyphaVersion.Trim();
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>Finds, or provisions, the <c>hypha</c> to start.</summary>
        /// <param name="baseDirectory">The launcher's own folder, where the package's <c>bin/</c> is.</param>
        /// <param name="rid">The runtime identifier of this platform, such as <c>win-x64</c>.</param>
        /// <param name="cancellationToken">Cancels a download.</param>
        /// <returns>The executable, or why there is none and how to fix it.</returns>
        public async Task<LaunchTarget> FindAsync(DirectoryInfo baseDirectory, string rid, CancellationToken cancellationToken = default)
        {
            var located = this.Locate(baseDirectory, rid);

            return located.NeedsDownload ? await this.DownloadAsync(located, rid, cancellationToken) : located;
        }

        /// <summary>
        /// Finds the <c>hypha</c> to start without the network: <c>HYPHA_BIN</c>, or the version the package
        /// names when it is cached already; otherwise the download it still needs, or why there is none.
        /// </summary>
        /// <param name="baseDirectory">The launcher's own folder, where the package's <c>bin/</c> is.</param>
        /// <param name="rid">The runtime identifier of this platform, such as <c>win-x64</c>.</param>
        /// <returns>The executable, the pending download, or why there is none and how to fix it.</returns>
        public LaunchTarget Locate(DirectoryInfo baseDirectory, string rid)
        {
            ArgumentNullException.ThrowIfNull(baseDirectory);
            ArgumentException.ThrowIfNullOrWhiteSpace(rid);

            if (this.getEnvironmentVariable(BinVariable)?.Trim() is { Length: > 0 } configured)
            {
                // A relative path would be resolved against whatever folder the client starts the server
                // in, which differs between clients and between their sessions.
                if (!Path.IsPathFullyQualified(configured))
                {
                    return LaunchTarget.Missing(
                        $"{BinVariable} must be an absolute path to an installed hypha {MinimumVersion} or later, but is '{configured}'. "
                        + "Give its full path, or unset it to let the launcher download one.");
                }

                var executable = new FileInfo(configured);

                return executable.Exists
                    ? LaunchTarget.Found(executable, null)
                    : LaunchTarget.Missing($"{BinVariable} names '{configured}', which does not exist. Point it at an installed hypha {MinimumVersion} or later, or unset it to let the launcher download one.");
            }

            var manifest = FindManifest(baseDirectory);
            var version = manifest is null ? null : ReadVersion(manifest);
            if (version is null)
            {
                return LaunchTarget.Missing(
                    $"this package carries no {LaunchManifest.FileName} naming the hypha version to start, so it is damaged. "
                    + $"Reinstall it, or set {BinVariable} to an installed hypha {MinimumVersion} or later.");
            }

            DirectoryInfo home;
            try
            {
                home = HomeFolder.Resolve(this.getEnvironmentVariable, this.getFolderPath);
            }
            catch (InvalidOperationException exception)
            {
                return LaunchTarget.Missing($"{exception.Message}.", version);
            }

            var cache = new CacheLayout(home, CacheLayout.ComputeInstallKey(manifest!.DirectoryName!));

            return CliProvisioner.IsCached(cache, version, rid)
                ? LaunchTarget.Found(cache.CliExecutable(version, rid), version)
                : LaunchTarget.Pending(version, cache);
        }

        /// <summary>Downloads the version a <see cref="Locate"/> result still needs.</summary>
        /// <param name="pending">The pending download.</param>
        /// <param name="rid">The runtime identifier of this platform.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The executable, or why there is none and how to fix it; never an exception.</returns>
        /// <exception cref="ArgumentException"><paramref name="pending"/> needs no download.</exception>
        public async Task<LaunchTarget> DownloadAsync(LaunchTarget pending, string rid, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pending);
            if (!pending.NeedsDownload)
            {
                throw new ArgumentException("the target needs no download", nameof(pending));
            }

            var version = pending.Version!;
            FileInfo? found;
            string? reason;
            try
            {
                found = await this.provisioner.EnsureAsync(pending.Cache!, version, rid, cancellationToken);
                reason = this.provisioner.LastFailure;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Whatever went wrong, the client is told through hypha_status rather than left without a server.
                found = null;
                reason = exception.Message;
            }

            return found is not null
                ? LaunchTarget.Found(found, version)
                : LaunchTarget.Missing(
                    $"hypha {version} for {rid} could not be downloaded: {reason ?? "the download failed"}. "
                    + "Check the network connection, and a proxy through HTTPS_PROXY if one is needed; an unauthenticated GitHub "
                    + $"API allows 60 requests an hour, so set GITHUB_TOKEN if that is the limit met. Or install hypha {MinimumVersion} "
                    + $"or later yourself and set {BinVariable} to it.",
                    version);
        }
    }
}
