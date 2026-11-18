// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Resolves a tag to the commit it points at, for the manifest's provenance record.
    /// </summary>
    public sealed class CommitResolver : ICommitResolver
    {
        /// <summary>Relative to the API base address, so the host stays configurable.</summary>
        private const string CommitPath = "repos/{0}/commits/{1}";

        /// <summary>The client commits are requested with, already carrying its headers and credential.</summary>
        private readonly HttpClient client;

        /// <summary>The API host commits are requested from.</summary>
        private readonly Uri apiBaseAddress;

        /// <summary>
        /// The commits resolved so far, by repository and tag. A release tag is not moved once published,
        /// so installing the same release again, to add its specification text, asks GitHub no more.
        /// </summary>
        private readonly ConcurrentDictionary<(string Repository, string Tag), string> resolved = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitResolver"/> class.
        /// </summary>
        /// <param name="client">
        /// A client already configured with the headers and credential to use, normally the same one
        /// given to <see cref="ReleaseDiscovery"/>.
        /// </param>
        /// <param name="apiBaseAddress">The API host; defaults to the public GitHub API.</param>
        public CommitResolver(HttpClient client, Uri? apiBaseAddress = null)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.client = client;
            this.apiBaseAddress = apiBaseAddress ?? client.BaseAddress ?? Upstream.DefaultApiBaseAddress;
        }

        /// <summary>The commit SHA <paramref name="tag"/> resolves to in <paramref name="repository"/>.</summary>
        public async Task<string> ResolveAsync(
            string repository, string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repository);
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            if (this.resolved.TryGetValue((repository, tag), out var known))
            {
                return known;
            }

            var path = string.Format(CultureInfo.InvariantCulture, CommitPath, repository, tag);
            var payload = await this.client.GetStringAsync(new Uri(this.apiBaseAddress, path), cancellationToken);

            using var document = JsonDocument.Parse(payload);

            var sha = document.RootElement.GetProperty("sha").GetString()
                      ?? throw new InvalidOperationException($"no commit sha for {repository}@{tag}");

            this.resolved[(repository, tag)] = sha;

            return sha;
        }

        /// <summary>Resolves both upstreams for one tag into an <see cref="InstalledVersion"/>.</summary>
        public async Task<InstalledVersion> ResolveVersionAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            var release = await this.ResolveAsync(Upstream.ReleaseRepository, tag, cancellationToken);
            var pilot = await this.ResolveAsync(Upstream.PilotRepository, tag, cancellationToken);

            return new InstalledVersion(
                tag,
                new UpstreamReference(Upstream.ReleaseRepository, release),
                new UpstreamReference(Upstream.PilotRepository, pilot));
        }
    }
}
