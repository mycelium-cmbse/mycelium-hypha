// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseFetcher.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Downloads one release's inputs into <c>sources/&lt;tag&gt;/</c>.
    /// </summary>
    /// <remarks>
    /// The Python original opened a new connection per file and was reset by the host after roughly
    /// 190 of the 317 files in a release. Two things address that here: <see cref="HttpClient"/>
    /// reuses connections, and the resilience handler registered in
    /// <see cref="ServiceCollectionExtensions"/> retries with <b>jittered</b> backoff. Jitter matters
    /// now that downloads run concurrently - without it a throttled batch would retry in lockstep.
    /// <para>
    /// Concurrency is deliberately modest. The goal is to stop opening a connection per file, not to
    /// saturate the host that was throttling us in the first place.
    /// </para>
    /// </remarks>
    public sealed class ReleaseFetcher : IReleaseFetcher
    {
        /// <summary>Relative to the API base address, so the host stays configurable.</summary>
        private const string TreePath = "repos/{0}/git/trees/{1}?recursive=1";

        private const string RawUrl = "https://raw.githubusercontent.com/{0}/{1}/{2}";

        private readonly HttpClient client;
        private readonly Uri apiBaseAddress;
        private readonly int maxConcurrency;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseFetcher"/> class.
        /// </summary>
        /// <param name="client">A client configured with credentials and a resilience handler.</param>
        /// <param name="apiBaseAddress">The API host; defaults to the public GitHub API.</param>
        /// <param name="maxConcurrency">Concurrent downloads; kept low on purpose.</param>
        public ReleaseFetcher(HttpClient client, Uri? apiBaseAddress = null, int maxConcurrency = 6)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);

            this.client = client;
            this.apiBaseAddress = apiBaseAddress ?? client.BaseAddress ?? Upstream.DefaultApiBaseAddress;
            this.maxConcurrency = maxConcurrency;
        }

        /// <summary>Every blob path in <paramref name="repository"/> at <paramref name="tag"/>.</summary>
        /// <exception cref="InvalidOperationException">
        /// The API truncated the listing. Failing loudly matters: a truncated tree would silently
        /// drop inputs and produce a knowledge base that looks complete.
        /// </exception>
        public async Task<IReadOnlyList<string>> ListTreeAsync(
            string repository, string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repository);
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var path = string.Format(CultureInfo.InvariantCulture, TreePath, repository, tag);
            var payload = await this.client.GetStringAsync(new Uri(this.apiBaseAddress, path), cancellationToken);

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.TryGetProperty("truncated", out var truncated) && truncated.GetBoolean())
            {
                throw new InvalidOperationException($"tree listing for {repository}@{tag} was truncated");
            }

            return root.GetProperty("tree")
                .EnumerateArray()
                .Where(entry => entry.GetProperty("type").GetString() == "blob")
                .Select(entry => entry.GetProperty("path").GetString()!)
                .ToList();
        }

        /// <summary>Downloads one file, unless <paramref name="skipExisting"/> and it is already there.</summary>
        /// <remarks>
        /// A zero-byte file counts as an interrupted write rather than a completed download, so a
        /// resumed run replaces it.
        /// </remarks>
        public async Task<FileInfo> DownloadAsync(
            string repository,
            string tag,
            string path,
            FileInfo destination,
            bool skipExisting = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(destination);

            destination.Refresh();
            if (skipExisting && destination.Exists && destination.Length > 0)
            {
                return destination;
            }

            destination.Directory?.Create();

            var url = string.Format(
                CultureInfo.InvariantCulture, RawUrl, repository, tag, Uri.EscapeDataString(path).Replace("%2F", "/"));

            var bytes = await this.client.GetByteArrayAsync(url, cancellationToken);
            await File.WriteAllBytesAsync(destination.FullName, bytes, cancellationToken);

            destination.Refresh();

            return destination;
        }

        /// <summary>Fetches the metamodel XMI for a release. Needs no tree listing: the paths are known.</summary>
        public Task<IReadOnlyList<FileInfo>> FetchMetamodelAsync(
            string tag, DirectoryInfo sourcesRoot, bool skipExisting = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourcesRoot);

            var targets = ReleaseInputs.XmiFiles
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => (
                    Path: entry.Key,
                    Destination: new FileInfo(Path.Combine(sourcesRoot.FullName, tag, "xmi", entry.Value))))
                .ToList();

            return this.DownloadManyAsync(Upstream.PilotRepository, tag, targets, skipExisting, cancellationToken);
        }

        /// <summary>Fetches the grammar and textual models, preserving the upstream layout.</summary>
        public async Task<IReadOnlyList<FileInfo>> FetchTextualAsync(
            string tag, DirectoryInfo sourcesRoot, bool skipExisting = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourcesRoot);

            var tree = await this.ListTreeAsync(Upstream.ReleaseRepository, tag, cancellationToken);
            var root = Path.Combine(sourcesRoot.FullName, tag, "textual");

            var targets = ReleaseInputs.SelectTextual(tree)
                .Select(path => (
                    Path: path,
                    Destination: new FileInfo(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)))))
                .ToList();

            return await this.DownloadManyAsync(
                Upstream.ReleaseRepository, tag, targets, skipExisting, cancellationToken);
        }

        /// <summary>
        /// Fetches the specification PDFs into the <b>git-ignored</b> folder.
        /// </summary>
        /// <remarks>
        /// The PDFs are OMG-copyrighted and must never be committed. This only automates the download
        /// the README otherwise asks the user to perform by hand.
        /// </remarks>
        public Task<IReadOnlyList<FileInfo>> FetchSpecificationsAsync(
            string tag, DirectoryInfo sourcesRoot, bool skipExisting = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourcesRoot);

            var targets = ReleaseInputs.SpecificationPdfs
                .Select(path => (
                    Path: path,
                    Destination: new FileInfo(
                        Path.Combine(sourcesRoot.FullName, tag, "specs", Path.GetFileName(path)))))
                .ToList();

            return this.DownloadManyAsync(Upstream.ReleaseRepository, tag, targets, skipExisting, cancellationToken);
        }

        private async Task<IReadOnlyList<FileInfo>> DownloadManyAsync(
            string repository,
            string tag,
            List<(string Path, FileInfo Destination)> targets,
            bool skipExisting,
            CancellationToken cancellationToken)
        {
            var written = new FileInfo[targets.Count];

            await Parallel.ForEachAsync(
                Enumerable.Range(0, targets.Count),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = this.maxConcurrency,
                    CancellationToken = cancellationToken,
                },
                async (index, token) =>
                {
                    var (path, destination) = targets[index];
                    written[index] = await this.DownloadAsync(
                        repository, tag, path, destination, skipExisting, token);
                });

            return written;
        }
    }
}
