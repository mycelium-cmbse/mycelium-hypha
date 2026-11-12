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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.Query;
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
        /// The name of the client published documents are read through. It carries no credentials: the
        /// upstream client sends the GitHub token, which must not reach another host.
        /// </summary>
        public const string DocumentClientName = "hypha-documents";

        /// <summary>
        /// Adds everything needed to fetch a release and generate the knowledge base from it, over a
        /// single resilient client.
        /// </summary>
        /// <param name="services">The collection to add to.</param>
        /// <param name="configure">
        /// Adjusts <see cref="HyphaKnowledgeOptions"/>. Every setting has a working default, so a
        /// caller inside the repository can omit this entirely.
        /// </param>
        /// <returns><paramref name="services"/>, so calls can be chained.</returns>
        /// <exception cref="ArgumentException">
        /// A setting <paramref name="configure"/> left is not usable (see <see cref="HyphaKnowledgeOptions.Validate"/>).
        /// </exception>
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
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

                    if (!string.IsNullOrWhiteSpace(credential))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential);
                    }
                })

                // Retries use jittered backoff. That matters more than it did when downloads were
                // sequential: a throttled batch of concurrent downloads would otherwise all retry at
                // the same instant, which is how the host came to reset the connection in the first
                // place. The circuit breaker stops us grinding through 300 files once it is refusing.
                .AddStandardResilienceHandler();

            services.AddSingleton<IReleaseDiscovery>(provider => new ReleaseDiscovery(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName), options.Token, null, baseAddress));

            services.AddSingleton<ICommitResolver>(provider => new CommitResolver(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName), baseAddress));

            services.AddSingleton<IReleaseFetcher>(provider => new ReleaseFetcher(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                baseAddress,
                options.MaxDownloadConcurrency));

            services
                .AddHttpClient(
                    DocumentClientName,
                    client => client.DefaultRequestHeaders.UserAgent.ParseAdd("mycelium-hypha"))
                .AddStandardResilienceHandler();

            // The Systems Modeling API is versioned on its own axis: its own layout, manifest and
            // generators, over the same fetcher and commit resolver the language releases use.
            services.AddSingleton<IApiLayout, ApiLayout>();
            services.AddSingleton<IApiDiscovery, ApiDiscovery>();
            services.AddSingleton<IDocumentDownloader>(provider => new HttpDocumentDownloader(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(DocumentClientName)));
            services.AddSingleton<IApiInstaller, ApiInstaller>();
            services.AddSingleton<IApiRemover, ApiRemover>();
            services.AddSingleton<IOpenApiReader, OpenApiReader>();
            services.AddSingleton<IOpenApiRenderer, OpenApiRenderer>();
            services.AddSingleton<IApiKnowledgeGenerator, OpenApiGenerator>();
            services.AddSingleton<IApiKnowledgeGenerator, ApiSpecGenerator>();
            services.AddSingleton<IApiCrossReferenceBuilder, ApiCrossReferenceBuilder>();
            services.AddSingleton<IApiKnowledgeGenerator, ApiCrossReferenceGenerator>();

            services.AddSingleton<IReleaseInstaller, ReleaseInstaller>();
            services.AddSingleton<IReleaseWindowEvictor, ReleaseWindowEvictor>();
            services.AddSingleton<IReleaseRemover, ReleaseRemover>();
            services.AddSingleton<IProcessProbe, SystemProcessProbe>();
            services.AddSingleton<IReleaseOperationLock>(provider => new ReleaseOperationLock(
                provider.GetRequiredService<IKnowledgeLayout>(),
                provider.GetRequiredService<IProcessProbe>(),
                provider.GetRequiredService<ILogger<ReleaseOperationLock>>(),
                options.ReleaseLockWait));

            // The grammar services are pure functions of the grammar text, so one instance serves
            // every caller.
            services.AddSingleton<IGrammarParser, GrammarParser>();
            services.AddSingleton<IGrammarLinks, GrammarLinks>();
            services.AddSingleton<IKeywordIndexer, KeywordIndexer>();
            services.AddSingleton<IGrammarReference>(_ => new GrammarReference(options.RawContentBaseAddress));

            services.AddSingleton<ISurfaceForms, SurfaceForms>();
            services.AddSingleton<INotationRenderer, NotationRenderer>();
            services.AddSingleton<IModelCatalog, ModelCatalog>();

            services.AddSingleton<IDeclarationScanner, DeclarationScanner>();
            services.AddSingleton<IModelLibraryRenderer, ModelLibraryRenderer>();

            services.AddSingleton<ICrossReferenceBuilder, CrossReferenceBuilder>();
            services.AddSingleton<IKnowledgeReader, KnowledgeReader>();

            // The one seam for shelling out: move-window's spec-extract/metamodel-gen steps and the
            // SpecGenerator's on-demand `uv` invocation both go through this.
            services.AddSingleton<IProcessRunner, SystemProcessRunner>();

            // The home folder is resolved once, when the first service that needs it is built, so an
            // unset HYPHA_HOME costs nothing to a caller that sets both roots.
            var home = new Lazy<DirectoryInfo>(() => options.Home ?? HyphaHome.Resolve());

            // The toolchain root takes uv's package cache and Python with it, so hypha never shares
            // them with another uv on the machine.
            services.AddSingleton<IUvProvisioner>(provider => new UvProvisioner(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient(UpstreamClientName),
                provider.GetRequiredService<ILogger<UvProvisioner>>(),
                options.ToolchainCacheRoot ?? HyphaHome.Toolchain(home.Value),
                ownsToolchain: true));

            services.AddSingleton<IKnowledgeLookup, KnowledgeLookup>();

            // Singletons on purpose: the one cache holds the documents read until their files change,
            // and both queries share it.
            services.AddSingleton<IDocumentCache, DocumentCache>();
            services.AddSingleton<ISpecificationReader, SpecificationReader>();
            services.AddSingleton<IMetamodelDiffer, MetamodelDiffer>();
            services.AddSingleton<ICrossReferenceDiffer, CrossReferenceDiffer>();
            services.AddSingleton<IKnowledgeQuery, KnowledgeQuery>();
            services.AddSingleton<IApiQuery, ApiQuery>();

            // Registered as a collection: a full run is a loop over Order, not a list of calls each
            // caller has to keep in step. The metamodel generator joins them via AddHyphaMetamodel.
            services.AddSingleton<IKnowledgeGenerator, GrammarReferenceGenerator>();
            services.AddSingleton<IKnowledgeGenerator, TextualNotationGenerator>();
            services.AddSingleton<IKnowledgeGenerator, KeywordGenerator>();
            services.AddSingleton<IKnowledgeGenerator, ModelLibraryGenerator>();
            services.AddSingleton<IKnowledgeGenerator, SpecGenerator>();
            services.AddSingleton<IKnowledgeGenerator, CrossReferenceGenerator>();

            // That loop, for either collection: IKnowledgeBuilder<IKnowledgeGenerator> runs the release
            // generators, IKnowledgeBuilder<IApiKnowledgeGenerator> the API ones.
            services.AddSingleton(typeof(IKnowledgeBuilder<>), typeof(KnowledgeBuilder<>));

            services.AddSingleton<IKnowledgeLayout>(_ =>
            {
                // A home folder the caller or the user chose wins over a checkout found only because
                // of where the assembly sits; that checkout is a maintainer's build, and anyone else
                // uses the home folder's default location.
                var root = options.RepositoryRoot
                           ?? options.Home
                           ?? HyphaHome.Configured()
                           ?? KnowledgeLayout.Discover()?.Root
                           ?? home.Value;

                return new KnowledgeLayout(root, options.OutputRoot);
            });

            return services;
        }
    }
}
