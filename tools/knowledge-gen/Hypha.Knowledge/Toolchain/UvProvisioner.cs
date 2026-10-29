// ------------------------------------------------------------------------------------------------
// <copyright file="UvProvisioner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Toolchain
{
    using System;
    using System.Collections.Generic;
    using System.Formats.Tar;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Fetches, checksum-verifies and caches the <c>uv</c> binary from its GitHub releases the first
    /// time it is needed.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>Hypha.Tools.Hook</c>'s <c>CliProvisioner</c> architecturally (cache-check, download,
    /// checksum-verify, extract, mark-ok, never throw) but is independent code: the hook project is
    /// deliberately kept free of this project's reflection-heavy dependencies (Handlebars.Net, uml4net)
    /// to stay NativeAOT-friendly, so neither project references the other.
    /// <para>
    /// Never throws past <see cref="EnsureAsync"/>: any failure here (offline, rate-limited, corrupt
    /// download, an unsupported platform) means the caller degrades to a
    /// <c>GenerationResult.Skipped</c> rather than aborting a whole <c>hypha generate</c> run over one
    /// artifact.
    /// </para>
    /// </remarks>
    public sealed class UvProvisioner : IUvProvisioner
    {
        /// <summary>The GitHub repository uv is released from, as <c>owner/name</c>.</summary>
        private const string Repository = "astral-sh/uv";

        /// <summary>The pinned uv release. Bumped by PR, the same way <c>hyphaCliVersion</c> is.</summary>
        /// <remarks>Public so tests can address the cache path this provisions without duplicating the value.</remarks>
        public const string PinnedVersion = "0.12.10";

        /// <summary>The folder under the per-user local application data that the cache lives in.</summary>
        private const string CacheFolderName = "mycelium-hypha";

        /// <summary>The client the release and its checksum are downloaded with; its base address is the GitHub API.</summary>
        private readonly HttpClient client;

        /// <summary>The log, which receives every reason uv could not be provisioned.</summary>
        private readonly ILogger<UvProvisioner> logger;

        /// <summary>Where the <c>uv</c> executable is cached.</summary>
        private readonly DirectoryInfo cacheRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class.
        /// </summary>
        /// <param name="client">The client the release and its checksum are downloaded with.</param>
        /// <param name="logger">The log, which receives every reason uv could not be provisioned.</param>
        public UvProvisioner(HttpClient client, ILogger<UvProvisioner> logger)
            : this(client, logger, ResolveCacheRoot())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class with an explicit cache
        /// root, so a test never touches the real <c>%LOCALAPPDATA%</c>.
        /// </summary>
        /// <param name="client">The client the release and its checksum are downloaded with.</param>
        /// <param name="logger">The log, which receives every reason uv could not be provisioned.</param>
        /// <param name="cacheRoot">Where the <c>uv</c> executable is cached.</param>
        /// <param name="ownsToolchain">
        /// <c>true</c> to also keep uv's package cache (<c>uv-cache/</c>) and the Python it installs
        /// (<c>python/</c>) under <paramref name="cacheRoot"/>, through <see cref="Environment"/>;
        /// <c>false</c> to leave them in uv's default, per-user locations.
        /// </param>
        public UvProvisioner(HttpClient client, ILogger<UvProvisioner> logger, DirectoryInfo cacheRoot, bool ownsToolchain = false)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(cacheRoot);

            this.client = client;
            this.logger = logger;
            this.cacheRoot = cacheRoot;
            this.Environment = ownsToolchain
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [UvCacheVariable] = Path.Combine(cacheRoot.FullName, "uv-cache"),
                    [UvPythonInstallVariable] = Path.Combine(cacheRoot.FullName, "python"),
                }
                : new Dictionary<string, string>(StringComparer.Ordinal);
        }

        /// <summary>The variable uv reads its package cache location from.</summary>
        public const string UvCacheVariable = "UV_CACHE_DIR";

        /// <summary>The variable uv reads the location of the Pythons it installs from.</summary>
        public const string UvPythonInstallVariable = "UV_PYTHON_INSTALL_DIR";

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Environment { get; }

        /// <summary>
        /// Resolves the cache root via <see cref="System.Environment.SpecialFolder.LocalApplicationData"/> -
        /// the same folder (and reasoning: deliberately outside any git-managed checkout) the CLI's own
        /// self-fetch hook already caches into, so this is a sibling, not a new location to document.
        /// </summary>
        /// <param name="getFolderPath">
        /// Resolves a special folder to its path; <c>null</c> uses
        /// <see cref="System.Environment.GetFolderPath(System.Environment.SpecialFolder)"/>. A test passes its own so it never
        /// touches the real folder.
        /// </param>
        /// <returns>The <c>mycelium-hypha</c> folder under the per-user local application data.</returns>
        public static DirectoryInfo ResolveCacheRoot(Func<System.Environment.SpecialFolder, string>? getFolderPath = null)
        {
            getFolderPath ??= System.Environment.GetFolderPath;

            return new DirectoryInfo(Path.Combine(getFolderPath(System.Environment.SpecialFolder.LocalApplicationData), CacheFolderName));
        }

        /// <summary><c>uv/&lt;version&gt;/&lt;target&gt;/</c> - where one uv build is cached.</summary>
        /// <param name="version">The uv release, such as <see cref="PinnedVersion"/>.</param>
        /// <param name="target">The uv release target (a Rust target triple), as <see cref="UvPlatform.CurrentTarget"/> names it.</param>
        /// <returns>The build's folder under the cache root; it may not exist yet.</returns>
        public DirectoryInfo BinDirectory(string version, string target) =>
            new(Path.Combine(this.cacheRoot.FullName, "uv", version, target));

        /// <summary>The cached <c>uv</c> executable itself.</summary>
        /// <param name="version">The uv release, such as <see cref="PinnedVersion"/>.</param>
        /// <param name="target">The uv release target (a Rust target triple), as <see cref="UvPlatform.CurrentTarget"/> names it.</param>
        /// <returns>The executable's path in <see cref="BinDirectory"/>; it may not exist yet.</returns>
        public FileInfo Executable(string version, string target)
        {
            var directory = this.BinDirectory(version, target).FullName;
            var executableName = UvPlatform.ExecutableName(target);
            var flat = Path.Combine(directory, executableName);
            var wrapped = Path.Combine(directory, $"uv-{target}", executableName);

            return new FileInfo(File.Exists(flat) || !File.Exists(wrapped) ? flat : wrapped);
        }

        /// <summary>
        /// Written only once the cached download's checksum has been verified - its presence is what
        /// lets a later run skip the network entirely.
        /// </summary>
        /// <param name="version">The uv release, such as <see cref="PinnedVersion"/>.</param>
        /// <param name="target">The uv release target (a Rust target triple), as <see cref="UvPlatform.CurrentTarget"/> names it.</param>
        /// <returns>The marker's path in <see cref="BinDirectory"/>; it may not exist yet.</returns>
        public FileInfo OkMarker(string version, string target) => new(Path.Combine(this.BinDirectory(version, target).FullName, ".ok"));

        /// <inheritdoc/>
        public async Task<FileInfo?> EnsureAsync(CancellationToken cancellationToken = default)
        {
            var target = UvPlatform.CurrentTarget;
            if (target is null)
            {
                this.logger.LogWarning("no uv release targets this platform ({Rid})", RuntimeInformation.RuntimeIdentifier);

                return null;
            }

            var executable = this.Executable(PinnedVersion, target);

            if (this.IsCached(PinnedVersion, target))
            {
                return executable;
            }

            try
            {
                var assetName = UvPlatform.AssetName(target);

                var releasePayload = await this.client.GetStringAsync(
                    $"repos/{Repository}/releases/tags/{PinnedVersion}", cancellationToken);
                using var release = JsonDocument.Parse(releasePayload);

                var (downloadUrl, checksumUrl) = FindAssetUrls(release.RootElement, assetName);
                if (downloadUrl is null || checksumUrl is null)
                {
                    this.logger.LogWarning("uv {Version} has no {Asset} release asset", PinnedVersion, assetName);

                    return null;
                }

                var expectedSha256 = await this.ReadChecksumAsync(checksumUrl, cancellationToken);
                if (expectedSha256 is null)
                {
                    this.logger.LogWarning("could not read the checksum for {Asset}", assetName);

                    return null;
                }

                var binDirectory = this.BinDirectory(PinnedVersion, target);
                binDirectory.Create();

                var archivePath = Path.Combine(binDirectory.FullName, assetName + ".download");

                await using (var archiveStream = File.Create(archivePath))
                await using (var downloadStream = await this.client.GetStreamAsync(downloadUrl, cancellationToken))
                {
                    await downloadStream.CopyToAsync(archiveStream, cancellationToken);
                }

                if (!VerifySha256(archivePath, expectedSha256))
                {
                    File.Delete(archivePath);
                    this.logger.LogWarning("downloaded {Asset} failed checksum verification", assetName);

                    return null;
                }

                Extract(archivePath, binDirectory.FullName, target);
                File.Delete(archivePath);
                executable = this.Executable(PinnedVersion, target);
                if (!executable.Exists)
                {
                    this.logger.LogWarning("uv {Version} archive did not contain the expected executable", PinnedVersion);
                    return null;
                }

                if (!OperatingSystem.IsWindows())
                {
                    executable.Refresh();
                    if (executable.Exists)
                    {
                        File.SetUnixFileMode(
                            executable.FullName,
                            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                            | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                            | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                }

                // Written last, only once extraction succeeded: its presence is what IsCached trusts to
                // skip the network on every later run.
                await File.WriteAllTextAsync(this.OkMarker(PinnedVersion, target).FullName, expectedSha256, cancellationToken);

                executable.Refresh();

                return executable.Exists ? executable : null;
            }
            catch (Exception exception) when (exception is HttpRequestException or IOException or JsonException
                                                  or TaskCanceledException or InvalidOperationException)
            {
                this.logger.LogWarning(exception, "could not provision uv {Version}", PinnedVersion);

                return null;
            }
        }

        /// <summary>Whether a uv build is cached and was verified when it was downloaded.</summary>
        /// <param name="version">The uv release.</param>
        /// <param name="target">The uv release target.</param>
        /// <returns><c>true</c> when both the executable and its <see cref="OkMarker"/> exist.</returns>
        private bool IsCached(string version, string target)
        {
            var executable = this.Executable(version, target);
            var marker = this.OkMarker(version, target);

            executable.Refresh();
            marker.Refresh();

            return executable.Exists && marker.Exists;
        }

        /// <summary>uv publishes each asset's checksum as a separate, same-named ``.sha256`` asset.</summary>
        /// <param name="release">The GitHub API's release document.</param>
        /// <param name="assetName">The archive's asset name, as <see cref="UvPlatform.AssetName"/> gives it.</param>
        /// <returns>
        /// The archive's and its checksum's download URLs; either is <c>null</c> when the release has no such asset.
        /// </returns>
        private static (string? DownloadUrl, string? ChecksumUrl) FindAssetUrls(JsonElement release, string assetName)
        {
            string? downloadUrl = null;
            string? checksumUrl = null;

            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();

                if (string.Equals(name, assetName, StringComparison.Ordinal))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                }
                else if (string.Equals(name, assetName + ".sha256", StringComparison.Ordinal))
                {
                    checksumUrl = asset.GetProperty("browser_download_url").GetString();
                }
            }

            return (downloadUrl, checksumUrl);
        }

        /// <summary>Reads a `sha256sum`-format checksum file: "&lt;hex&gt; *&lt;filename&gt;".</summary>
        /// <param name="checksumUrl">The checksum asset's download URL.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The expected SHA-256 as hexadecimal text, or <c>null</c> when the file is empty.</returns>
        private async Task<string?> ReadChecksumAsync(string checksumUrl, CancellationToken cancellationToken)
        {
            var content = await this.client.GetStringAsync(checksumUrl, cancellationToken);
            var hex = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return string.IsNullOrWhiteSpace(hex) ? null : hex;
        }

        /// <summary>Checks a downloaded file against its published SHA-256.</summary>
        /// <param name="path">The downloaded file.</param>
        /// <param name="expectedHex">The published SHA-256, as hexadecimal text in either case.</param>
        /// <returns><c>true</c> when the file's hash matches.</returns>
        private static bool VerifySha256(string path, string expectedHex)
        {
            using var stream = File.OpenRead(path);
            var actual = Convert.ToHexStringLower(SHA256.HashData(stream));

            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

    /// <summary>Extracts uv's platform archive, which may contain a platform-named wrapper folder.</summary>
        /// <param name="archivePath">The downloaded, verified archive.</param>
        /// <param name="destination">The folder to extract into; existing files are overwritten.</param>
        /// <param name="target">The uv release target, which decides whether the archive is a zip or a gzipped tar.</param>
        private static void Extract(string archivePath, string destination, string target)
        {
            if (UvPlatform.IsZip(target))
            {
                ZipFile.ExtractToDirectory(archivePath, destination, overwriteFiles: true);

                return;
            }

            using var fileStream = File.OpenRead(archivePath);
            using var gzip = new GZipStream(fileStream, CompressionMode.Decompress);
            TarFile.ExtractToDirectory(gzip, destination, overwriteFiles: true);
        }
    }
}
