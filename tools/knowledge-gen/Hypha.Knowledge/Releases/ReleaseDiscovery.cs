// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseDiscovery.cs" company="Starion Group S.A.">
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
        private const string TagsPath = "repos/{0}/tags?per_page=100&page={1}";

        /// <summary>Guards against an unbounded loop if the API ever stops returning an empty page.</summary>
        private const int MaxPages = 20;

        private readonly HttpClient client;
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
        public ReleaseDiscovery(
            HttpClient client,
            string? token = null,
            Func<string, string?>? readEnvironment = null,
            Uri? apiBaseAddress = null)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.client = client;
            this.apiBaseAddress = apiBaseAddress ?? client.BaseAddress ?? Upstream.DefaultApiBaseAddress;

            if (this.client.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                this.client.DefaultRequestHeaders.UserAgent.ParseAdd("mycelium-hypha");
            }

            this.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            // An explicitly supplied but empty token is treated as absent, the same way an exported
            // but empty environment variable is - otherwise the request carries a bare "Bearer".
            var resolved = string.IsNullOrWhiteSpace(token)
                ? GitHubToken.FromEnvironment(readEnvironment)
                : token;
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                this.client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", resolved);
            }
        }

        /// <summary>Every tag name in <paramref name="repository"/>, following pagination.</summary>
        public async Task<IReadOnlyList<string>> FetchTagsAsync(
            string repository, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repository);

            var names = new List<string>();

            for (var page = 1; page <= MaxPages; page++)
            {
                var path = string.Format(CultureInfo.InvariantCulture, TagsPath, repository, page);
                var url = new Uri(this.apiBaseAddress, path);
                var payload = await this.client.GetStringAsync(url, cancellationToken);

                using var document = JsonDocument.Parse(payload);
                var batch = document.RootElement;

                if (batch.GetArrayLength() == 0)
                {
                    break;
                }

                foreach (var entry in batch.EnumerateArray())
                {
                    names.Add(entry.GetProperty("name").GetString()!);
                }
            }

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
