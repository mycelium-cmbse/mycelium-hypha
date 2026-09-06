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
        private const string Repository = "astral-sh/uv";

        /// <summary>The pinned uv release. Bumped by PR, the same way <c>hyphaCliVersion</c> is.</summary>
        /// <remarks>Public so tests can address the cache path this provisions without duplicating the value.</remarks>
        public const string PinnedVersion = "0.12.10";

        private const string CacheFolderName = "mycelium-hypha";

        private readonly HttpClient client;
        private readonly ILogger<UvProvisioner> logger;
        private readonly DirectoryInfo cacheRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class.
        /// </summary>
        public UvProvisioner(HttpClient client, ILogger<UvProvisioner> logger)
            : this(client, logger, ResolveCacheRoot())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class with an explicit cache
        /// root, so a test never touches the real <c>%LOCALAPPDATA%</c>.
        /// </summary>
        public UvProvisioner(HttpClient client, ILogger<UvProvisioner> logger, DirectoryInfo cacheRoot)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(cacheRoot);

            this.client = client;
            this.logger = logger;
            this.cacheRoot = cacheRoot;
        }

        /// <summary>
        /// Resolves the cache root via <see cref="Environment.SpecialFolder.LocalApplicationData"/> -
        /// the same folder (and reasoning: deliberately outside any git-managed checkout) the CLI's own
        /// self-fetch hook already caches into, so this is a sibling, not a new location to document.
        /// </summary>
        public static DirectoryInfo ResolveCacheRoot(Func<Environment.SpecialFolder, string>? getFolderPath = null)
        {
            getFolderPath ??= Environment.GetFolderPath;

            return new DirectoryInfo(
                Path.Combine(getFolderPath(Environment.SpecialFolder.LocalApplicationData), CacheFolderName));
        }

        /// <summary><c>uv/&lt;version&gt;/&lt;target&gt;/</c> - where one uv build is cached.</summary>
        public DirectoryInfo BinDirectory(string version, string target) =>
            new(Path.Combine(this.cacheRoot.FullName, "uv", version, target));

        /// <summary>The cached <c>uv</c> executable itself.</summary>
        public FileInfo Executable(string version, string target) =>
            new(Path.Combine(this.BinDirectory(version, target).FullName, UvPlatform.ExecutableName(target)));

        /// <summary>
        /// Written only once the cached download's checksum has been verified - its presence is what
        /// lets a later run skip the network entirely.
        /// </summary>
        public FileInfo OkMarker(string version, string target) =>
            new(Path.Combine(this.BinDirectory(version, target).FullName, ".ok"));

        /// <inheritdoc/>
        public async Task<FileInfo?> EnsureAsync(CancellationToken cancellationToken = default)
        {
            var target = UvPlatform.CurrentTarget;
            if (target is null)
            {
                this.logger.LogWarning(
                    "no uv release targets this platform ({Rid})", RuntimeInformation.RuntimeIdentifier);

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
                await File.WriteAllTextAsync(
                    this.OkMarker(PinnedVersion, target).FullName, expectedSha256, cancellationToken);

                executable.Refresh();

                return executable.Exists ? executable : null;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or IOException or JsonException
                or TaskCanceledException or InvalidOperationException)
            {
                this.logger.LogWarning(exception, "could not provision uv {Version}", PinnedVersion);

                return null;
            }
        }

        private bool IsCached(string version, string target)
        {
            var executable = this.Executable(version, target);
            var marker = this.OkMarker(version, target);

            executable.Refresh();
            marker.Refresh();

            return executable.Exists && marker.Exists;
        }

        /// <summary>uv publishes each asset's checksum as a separate, same-named ``.sha256`` asset.</summary>
        private static (string? DownloadUrl, string? ChecksumUrl) FindAssetUrls(
            JsonElement release, string assetName)
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
        private async Task<string?> ReadChecksumAsync(string checksumUrl, CancellationToken cancellationToken)
        {
            var content = await this.client.GetStringAsync(checksumUrl, cancellationToken);
            var hex = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return string.IsNullOrWhiteSpace(hex) ? null : hex;
        }

        private static bool VerifySha256(string path, string expectedHex)
        {
            using var stream = File.OpenRead(path);
            var actual = Convert.ToHexStringLower(SHA256.HashData(stream));

            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Every uv release target's archive is flat (no wrapping folder) - verified live.</summary>
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
