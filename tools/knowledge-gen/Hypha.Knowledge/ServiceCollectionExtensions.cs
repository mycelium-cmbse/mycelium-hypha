// ------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge
{
    using System;
    using System.Net.Http.Headers;

    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Registers the knowledge services, so the CLI (see #81) composes them in one place instead of
    /// constructing them by hand at each call site.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>The name of the typed client every upstream read goes through.</summary>
        public const string UpstreamClientName = "hypha-upstream";

        /// <summary>
        /// Adds release discovery, commit resolution and fetching, over a single resilient client.
        /// </summary>
        /// <param name="services">The collection to add to.</param>
        /// <param name="token">
        /// An optional GitHub token; falls back to <c>GITHUB_TOKEN</c> / <c>GH_TOKEN</c>. The
        /// anonymous API allows 60 requests an hour, which discovery alone can exhaust.
        /// </param>
        /// <param name="apiBaseAddress">The API host; defaults to the public GitHub API.</param>
        public static IServiceCollection AddHyphaKnowledge(
            this IServiceCollection services, string? token = null, Uri? apiBaseAddress = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var baseAddress = apiBaseAddress ?? Upstream.DefaultApiBaseAddress;
            var credential = string.IsNullOrWhiteSpace(token) ? GitHubToken.FromEnvironment() : token;

            services
                .AddHttpClient(UpstreamClientName, client =>
                {
                    client.BaseAddress = baseAddress;
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("mycelium-hypha");
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

                    if (!string.IsNullOrWhiteSpace(credential))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", credential);
                    }
                })

                // Retries use jittered backoff. That matters more than it did when downloads were
                // sequential: a throttled batch of concurrent downloads would otherwise all retry at
                // the same instant, which is how the host came to reset the connection in the first
                // place. The circuit breaker stops us grinding through 300 files once it is refusing.
                .AddStandardResilienceHandler();

            services.AddSingleton(provider => new ReleaseDiscovery(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                token,
                null,
                baseAddress));

            services.AddSingleton(provider => new CommitResolver(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                baseAddress));

            services.AddSingleton(provider => new ReleaseFetcher(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                baseAddress));

            // The grammar services are pure functions of the grammar text, so one instance serves
            // every caller.
            services.AddSingleton<IGrammarParser, GrammarParser>();
            services.AddSingleton<IGrammarLinks, GrammarLinks>();
            services.AddSingleton<IGrammarReference>(_ => new GrammarReference());

            return services;
        }
    }
}
