// ------------------------------------------------------------------------------------------------
// <copyright file="CliProvisioner.cs" company="Starion Group S.A.">
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
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Ensures the real, full <c>hypha</c> CLI is cached on disk, downloading, checksum-verifying and
    /// extracting it from the pinned <c>tools-v*</c> GitHub release the first time it is needed.
    /// </summary>
    /// <remarks>
    /// Never throws past a public method: any failure here (offline, rate-limited, corrupt download)
    /// means this session's sync is simply skipped, silently, and retried automatically next session:
    /// there is no throttling upstream of this to make a failure here rare enough to be worth alarming
    /// the user over every time.
    /// </remarks>
    public sealed class CliProvisioner
    {
        /// <summary>The GitHub repository, <c>owner/name</c>, whose releases carry the CLI builds.</summary>
        private const string Repository = "mycelium-cmbse/mycelium-hypha";

        /// <summary>The client the GitHub API and the asset downloads go through.</summary>
        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliProvisioner"/> class.
        /// </summary>
        /// <param name="client">
        /// The client the GitHub API and the asset downloads go through, its base address the API root
        /// (see <see cref="CreateClient"/>).
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is <c>null</c>.</exception>
        public CliProvisioner(HttpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.client = client;
        }

        /// <summary>
        /// Gets why the last <see cref="EnsureAsync"/> returned <c>null</c>, for a person; <c>null</c> when it
        /// returned the executable.
        /// </summary>
        public string? LastFailure { get; private set; }

        /// <summary>Builds an <see cref="HttpClient"/> configured the way GitHub's API requires.</summary>
        /// <remarks>
        /// Authenticates with <c>GITHUB_TOKEN</c>, or else <c>GH_TOKEN</c>, when either is set, and
        /// gives up on a request after 30 seconds.
        /// </remarks>
        /// <returns>The client; the caller owns and disposes it.</returns>
        public static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com/"),
                Timeout = TimeSpan.FromSeconds(30),
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("mycelium-hypha");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                token = Environment.GetEnvironmentVariable("GH_TOKEN");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        /// <summary>Whether the CLI for this version/RID is already cached and verified.</summary>
        /// <param name="cache">The cache to look in.</param>
        /// <param name="version">The CLI version.</param>
        /// <param name="rid">The runtime identifier the build targets.</param>
        /// <returns><c>true</c> when both the executable and its <c>.ok</c> marker exist.</returns>
        public static bool IsCached(CacheLayout cache, string version, string rid)
        {
            var executable = cache.CliExecutable(version, rid);
            var marker = cache.OkMarker(version, rid);

            executable.Refresh();
            marker.Refresh();

            return executable.Exists && marker.Exists;
        }

        /// <summary>
        /// Ensures the CLI is cached, downloading it if necessary.
        /// </summary>
        /// <remarks>
        /// A failed download or verification is appended to <see cref="CacheLayout.DownloadErrorLog"/>.
        /// </remarks>
        /// <param name="cache">The cache the CLI is provisioned into.</param>
        /// <param name="version">The CLI version, whose <c>tools-v&lt;version&gt;</c> release is downloaded.</param>
        /// <param name="rid">The runtime identifier the build targets.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The cached executable, or <c>null</c> when it could not be provisioned.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cache"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="version"/> or <paramref name="rid"/> is <c>null</c>, empty or white space.
        /// </exception>
        public async Task<FileInfo?> EnsureAsync(
            CacheLayout cache, string version, string rid, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrWhiteSpace(version);
            ArgumentException.ThrowIfNullOrWhiteSpace(rid);

            var executable = cache.CliExecutable(version, rid);
            this.LastFailure = null;

            if (IsCached(cache, version, rid))
            {
                return executable;
            }

            try
            {
                var assetName = $"hypha-{version}-{rid}.zip";

                var response = await this.client.GetAsync(
                    $"repos/{Repository}/releases/tags/tools-v{version}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var release = await response.Content.ReadFromJsonAsync(
                    ProvisioningJsonContext.Default.GitHubRelease, cancellationToken);

                var asset = release?.Assets?.FirstOrDefault(
                    candidate => string.Equals(candidate.Name, assetName, StringComparison.Ordinal));

                if (asset?.BrowserDownloadUrl is not { Length: > 0 } downloadUrl)
                {
                    this.LastFailure = $"the release tools-v{version} has no {assetName}";

                    return null;
                }

                var expectedSha256 = ChecksumTable.Find(release!.Body, assetName);
                if (expectedSha256 is null)
                {
                    this.LastFailure = $"the release tools-v{version} publishes no checksum for {assetName}";

                    return null;
                }

                var binDirectory = cache.BinDirectory(version, rid);
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
                    this.LastFailure = $"{assetName} did not match the checksum the release tools-v{version} publishes";

                    return null;
                }

                await ZipFile.ExtractToDirectoryAsync(
                    archivePath, binDirectory.FullName, overwriteFiles: true, cancellationToken);
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

                // Written last, only once extraction succeeded: its presence is what IsCached trusts
                // to skip the network on every later run.
                await File.WriteAllTextAsync(cache.OkMarker(version, rid).FullName, expectedSha256, cancellationToken);

                executable.Refresh();
                this.LastFailure = executable.Exists ? null : $"{assetName} holds no {executable.Name}";

                return executable.Exists ? executable : null;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or IOException or JsonException
                or TaskCanceledException or InvalidOperationException)
            {
                TryLog(cache, exception);
                this.LastFailure = $"downloading hypha {version} for {rid} from GitHub failed: {exception.Message}";

                return null;
            }
        }

        /// <summary>Whether the file at <paramref name="path"/> has the SHA256 <paramref name="expectedHex"/>.</summary>
        /// <param name="path">The file to hash.</param>
        /// <param name="expectedHex">The expected digest, as hexadecimal in either case.</param>
        /// <returns><c>true</c> when the digests match.</returns>
        private static bool VerifySha256(string path, string expectedHex)
        {
            using var stream = File.OpenRead(path);
            var actual = Convert.ToHexStringLower(SHA256.HashData(stream));

            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Appends <paramref name="exception"/> to <see cref="CacheLayout.DownloadErrorLog"/>, ignoring
        /// any I/O failure in doing so.
        /// </summary>
        /// <param name="cache">The cache whose state folder holds the log.</param>
        /// <param name="exception">The failure to record.</param>
        private static void TryLog(CacheLayout cache, Exception exception)
        {
            try
            {
                cache.StateDirectory.Create();
                File.AppendAllText(
                    cache.DownloadErrorLog.FullName,
                    $"[{DateTimeOffset.UtcNow:O}] {exception.GetType().Name}: {exception.Message}{Environment.NewLine}");
            }
            catch (IOException)
            {
                // Nothing more useful to do: this is diagnostic-only, and the session must still exit.
            }
        }
    }
}
