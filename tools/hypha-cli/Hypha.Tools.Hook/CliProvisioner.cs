// ------------------------------------------------------------------------------------------------
// <copyright file="CliProvisioner.cs" company="Starion Group S.A.">
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
    /// Ensures the real, full <c>hypha</c> CLI is cached on disk - downloading, checksum-verifying and
    /// extracting it from the pinned <c>tools-v*</c> GitHub release the first time it is needed.
    /// </summary>
    /// <remarks>
    /// Never throws past a public method: any failure here (offline, rate-limited, corrupt download)
    /// means this session's sync is simply skipped, silently, and retried automatically next session -
    /// there is no throttling upstream of this to make a failure here rare enough to be worth alarming
    /// the user over every time.
    /// </remarks>
    public sealed class CliProvisioner
    {
        private const string Repository = "mycelium-cmbse/mycelium-hypha";

        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliProvisioner"/> class.
        /// </summary>
        public CliProvisioner(HttpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.client = client;
        }

        /// <summary>Builds an <see cref="HttpClient"/> configured the way GitHub's API requires.</summary>
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
        /// <returns>The cached executable, or <c>null</c> when it could not be provisioned.</returns>
        public async Task<FileInfo?> EnsureAsync(
            CacheLayout cache, string version, string rid, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrWhiteSpace(version);
            ArgumentException.ThrowIfNullOrWhiteSpace(rid);

            var executable = cache.CliExecutable(version, rid);

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
                    HookJsonContext.Default.GitHubRelease, cancellationToken);

                var asset = release?.Assets?.FirstOrDefault(
                    candidate => string.Equals(candidate.Name, assetName, StringComparison.Ordinal));

                if (asset?.BrowserDownloadUrl is not { Length: > 0 } downloadUrl)
                {
                    return null;
                }

                var expectedSha256 = ChecksumTable.Find(release!.Body, assetName);
                if (expectedSha256 is null)
                {
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

                    return null;
                }

                ZipFile.ExtractToDirectory(archivePath, binDirectory.FullName, overwriteFiles: true);
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
                File.WriteAllText(cache.OkMarker(version, rid).FullName, expectedSha256);

                executable.Refresh();

                return executable.Exists ? executable : null;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or IOException or JsonException
                or TaskCanceledException or InvalidOperationException)
            {
                TryLog(cache, exception);

                return null;
            }
        }

        private static bool VerifySha256(string path, string expectedHex)
        {
            using var stream = File.OpenRead(path);
            var actual = Convert.ToHexStringLower(SHA256.HashData(stream));

            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

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
                // Nothing more useful to do - this is diagnostic-only, and the session must still exit.
            }
        }
    }
}
