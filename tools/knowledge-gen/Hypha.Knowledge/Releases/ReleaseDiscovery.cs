// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Reads the tags an upstream repository publishes. The only networked part of release discovery;
    /// every selection rule lives in <see cref="ReleaseTag"/> and <see cref="ReleaseCatalog"/> so it
    /// can be tested without GitHub.
    /// </summary>
    public sealed class ReleaseDiscovery : IReleaseDiscovery
    {
        /// <summary>Relative to the API base address, so the host stays configurable.</summary>
        private const string TagsPath = "repos/{0}/tags?per_page={1}&page={2}";

        /// <summary>
        /// The most tags a page holds. A page with fewer is the last one, so no request is spent on the
        /// empty page after it: every request counts against GitHub's limit of 60 an hour without a token.
        /// </summary>
        private const int PerPage = 100;

        /// <summary>Guards against an unbounded loop if the API ever stops returning an empty page.</summary>
        private const int MaxPages = 20;

        /// <summary>The client tags are read with; its lifetime belongs to the caller.</summary>
        private readonly HttpClient client;

        /// <summary>
        /// How long a repository's tags are kept once read, so that a check followed by an install, or by
        /// another check, does not list them again. A server lives for a whole session, and upstream tags
        /// a release at most every few weeks.
        /// </summary>
        public static readonly TimeSpan TagLifetime = TimeSpan.FromMinutes(10);

        /// <summary>The tags read so far, by repository, with when they were read.</summary>
        private readonly ConcurrentDictionary<string, (DateTimeOffset Read, IReadOnlyList<string> Tags)> tags =
            new(StringComparer.Ordinal);

        /// <summary>The clock <see cref="TagLifetime"/> is measured with.</summary>
        private readonly TimeProvider time;

        /// <summary>The API host tags are read from.</summary>
        private readonly Uri apiBaseAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseDiscovery"/> class.
        /// </summary>
        /// <param name="client">The client to read with; its lifetime belongs to the caller.</param>
        /// <param name="token">An optional token; falls back to the environment when blank.</param>
        /// <param name="readEnvironment">
        /// Environment lookup, injected so a test never depends on the ambient environment.
        /// </param>
        /// <param name="apiBaseAddress">
        /// The API host; defaults to <see cref="Upstream.DefaultApiBaseAddress"/>.
        /// </param>
        /// <param name="time">The clock the tags' lifetime is measured with; the system clock when <c>null</c>.</param>
        public ReleaseDiscovery(
            HttpClient client,
            string? token = null,
            Func<string, string?>? readEnvironment = null,
            Uri? apiBaseAddress = null,
            TimeProvider? time = null)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.time = time ?? TimeProvider.System;

            this.client = client;
            this.apiBaseAddress = apiBaseAddress ?? client.BaseAddress ?? Upstream.DefaultApiBaseAddress;

            if (this.client.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                this.client.DefaultRequestHeaders.UserAgent.ParseAdd("mycelium-hypha");
            }

            this.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            // An explicitly supplied but empty token is treated as absent, the same way an exported
            // but empty environment variable is; otherwise the request carries a bare "Bearer".
            var resolved = string.IsNullOrWhiteSpace(token)
                ? GitHubToken.FromEnvironment(readEnvironment)
                : token;
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                this.client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", resolved);
            }
        }

        /// <summary>
        /// Every tag name in <paramref name="repository"/>, following pagination, or the ones read less than
        /// <see cref="TagLifetime"/> ago.
        /// </summary>
        public async Task<IReadOnlyList<string>> FetchTagsAsync(
            string repository, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repository);

            var now = this.time.GetUtcNow();
            if (this.tags.TryGetValue(repository, out var kept) && now - kept.Read < TagLifetime)
            {
                return kept.Tags;
            }

            var names = new List<string>();

            for (var page = 1; page <= MaxPages; page++)
            {
                var path = string.Format(CultureInfo.InvariantCulture, TagsPath, repository, PerPage, page);
                var url = new Uri(this.apiBaseAddress, path);
                var payload = await this.client.GetStringAsync(url, cancellationToken);

                using var document = JsonDocument.Parse(payload);
                var batch = document.RootElement;

                foreach (var entry in batch.EnumerateArray())
                {
                    names.Add(entry.GetProperty("name").GetString()!);
                }

                if (batch.GetArrayLength() < PerPage)
                {
                    break;
                }
            }

            this.tags[repository] = (now, names);

            return names;
        }

        /// <summary>The offerable versions across both upstreams, newest first.</summary>
        public async Task<IReadOnlyList<string>> AvailableAsync(
            CancellationToken cancellationToken = default)
        {
            var release = await this.FetchTagsAsync(Upstream.ReleaseRepository, cancellationToken);
            var pilot = await this.FetchTagsAsync(Upstream.PilotRepository, cancellationToken);

            return ReleaseCatalog.Available(release, pilot);
        }
    }
}
