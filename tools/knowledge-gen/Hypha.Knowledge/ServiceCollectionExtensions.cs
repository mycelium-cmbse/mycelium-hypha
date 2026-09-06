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
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.Releases;
    using Hypha.Knowledge.TextualNotation;
    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Registers the knowledge services, so the CLI (see #81) composes them in one place instead of
    /// constructing them by hand at each call site.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>The name of the typed client every upstream read goes through.</summary>
        public const string UpstreamClientName = "hypha-upstream";

        /// <summary>
        /// Adds everything needed to fetch a release and generate the knowledge base from it, over a
        /// single resilient client.
        /// </summary>
        /// <param name="services">The collection to add to.</param>
        /// <param name="configure">
        /// Adjusts <see cref="HyphaKnowledgeOptions"/>. Every setting has a working default, so a
        /// caller inside the repository can omit this entirely.
        /// </param>
        public static IServiceCollection AddHyphaKnowledge(
            this IServiceCollection services, Action<HyphaKnowledgeOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var options = new HyphaKnowledgeOptions();
            configure?.Invoke(options);
            options.Validate();

            services.AddSingleton(options);

            // No providers, so nothing is written unless the host adds one. It is the seam #77 needs
            // to show install-time progress, and it keeps ILogger<T> resolvable everywhere else.
            services.AddLogging();

            var baseAddress = options.ApiBaseAddress;
            var credential = string.IsNullOrWhiteSpace(options.Token)
                ? GitHubToken.FromEnvironment()
                : options.Token;

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

            services.AddSingleton<IReleaseDiscovery>(provider => new ReleaseDiscovery(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                options.Token,
                null,
                baseAddress));

            services.AddSingleton<ICommitResolver>(provider => new CommitResolver(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                baseAddress));

            services.AddSingleton<IReleaseFetcher>(provider => new ReleaseFetcher(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                baseAddress,
                options.MaxDownloadConcurrency));

            services.AddSingleton<IReleaseInstaller, ReleaseInstaller>();
            services.AddSingleton<IReleaseWindowEvictor, ReleaseWindowEvictor>();
            services.AddSingleton<IReleaseRemover, ReleaseRemover>();

            // The grammar services are pure functions of the grammar text, so one instance serves
            // every caller.
            services.AddSingleton<IGrammarParser, GrammarParser>();
            services.AddSingleton<IGrammarLinks, GrammarLinks>();
            services.AddSingleton<IGrammarReference>(_ => new GrammarReference(options.RawContentBaseAddress));

            services.AddSingleton<ISurfaceForms, SurfaceForms>();
            services.AddSingleton<INotationRenderer, NotationRenderer>();
            services.AddSingleton<IModelCatalog, ModelCatalog>();

            services.AddSingleton<IDeclarationScanner, DeclarationScanner>();
            services.AddSingleton<IModelLibraryRenderer, ModelLibraryRenderer>();

            services.AddSingleton<ICrossReferenceBuilder, CrossReferenceBuilder>();
            services.AddSingleton<IKnowledgeReader, KnowledgeReader>();

            // The one seam for shelling out - move-window's spec-extract/metamodel-gen steps and the
            // SpecGenerator's on-demand `uv` invocation both go through this.
            services.AddSingleton<IProcessRunner, SystemProcessRunner>();

            services.AddSingleton<IUvProvisioner>(provider => new UvProvisioner(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                provider.GetRequiredService<ILogger<UvProvisioner>>()));

            // Registered as a collection: a full run is a loop over Order, not a list of calls each
            // caller has to keep in step. The metamodel generator joins them via AddHyphaMetamodelGen.
            services.AddSingleton<IKnowledgeGenerator, GrammarReferenceGenerator>();
            services.AddSingleton<IKnowledgeGenerator, TextualNotationGenerator>();
            services.AddSingleton<IKnowledgeGenerator, ModelLibraryGenerator>();
            services.AddSingleton<IKnowledgeGenerator, SpecGenerator>();
            services.AddSingleton<IKnowledgeGenerator, CrossReferenceGenerator>();

            services.AddSingleton<IKnowledgeLayout>(_ =>
            {
                var root = options.RepositoryRoot
                           ?? KnowledgeLayout.Discover()?.Root
                           ?? throw new InvalidOperationException(
                               "no repository root was configured and none could be discovered above "
                               + $"{AppContext.BaseDirectory}; set RepositoryRoot on HyphaKnowledgeOptions");

                return new KnowledgeLayout(root, options.OutputRoot);
            });

            return services;
        }
    }
}
