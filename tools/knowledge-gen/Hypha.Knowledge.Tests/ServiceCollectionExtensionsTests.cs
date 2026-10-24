// ------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Knowledge.TextualNotation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Tests for the composition root - the container has to hand back working services, since the
    /// CLI (see #81) will resolve them rather than construct them.
    /// </summary>
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void Resolves_every_upstream_service()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<IReleaseDiscovery>(), Is.Not.Null);
                Assert.That(provider.GetService<ICommitResolver>(), Is.Not.Null);
                Assert.That(provider.GetService<IReleaseFetcher>(), Is.Not.Null);
            });
        }

        [Test]
        public void Resolves_the_grammar_services_by_their_interfaces()
        {
            // They are resolved rather than constructed so the CLI (#81) composes them once, and so a
            // caller can substitute one - which is also why they are not static despite holding no
            // per-call state.
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<IGrammarParser>(), Is.Not.Null);
                Assert.That(provider.GetService<IGrammarLinks>(), Is.Not.Null);
                Assert.That(provider.GetService<IGrammarReference>(), Is.Not.Null);
            });
        }

        [Test]
        public void Resolves_the_textual_notation_services_by_their_interfaces()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<ISurfaceForms>(), Is.Not.Null);
                Assert.That(provider.GetService<INotationRenderer>(), Is.Not.Null);
                Assert.That(provider.GetService<IModelCatalog>(), Is.Not.Null);
            });
        }

        [Test]
        public void Resolves_the_cross_reference_services_by_their_interfaces()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<ICrossReferenceBuilder>(), Is.Not.Null);
                Assert.That(provider.GetService<IKnowledgeReader>(), Is.Not.Null);
            });
        }

        [Test]
        public void Resolves_the_layout_at_the_root_it_is_given()
        {
            var root = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);

            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.RepositoryRoot = root)
                .BuildServiceProvider();

            Assert.That(
                provider.GetRequiredService<IKnowledgeLayout>().Root.FullName, Is.EqualTo(root.FullName));
        }

        [Test]
        public void The_layout_falls_back_to_discovery()
        {
            // The tests run inside the repository, so the discovered root is the checkout.
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.That(
                provider.GetRequiredService<IKnowledgeLayout>().Root.FullName,
                Is.EqualTo(Repository.Layout!.Root.FullName));
        }

        [Test]
        public void Configures_the_named_client_for_the_github_api()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ServiceCollectionExtensions.UpstreamClientName);

            Assert.Multiple(() =>
            {
                Assert.That(client.BaseAddress, Is.EqualTo(Upstream.DefaultApiBaseAddress));
                Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Does.Contain("mycelium-hypha"));
                Assert.That(
                    client.DefaultRequestHeaders.Accept.Select(header => header.MediaType),
                    Does.Contain("application/vnd.github+json"));
            });
        }

        [Test]
        public void An_explicit_token_authenticates_the_client()
        {
            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.Token = "explicit")
                .BuildServiceProvider();

            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ServiceCollectionExtensions.UpstreamClientName);

            Assert.Multiple(() =>
            {
                Assert.That(client.DefaultRequestHeaders.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(client.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("explicit"));
            });
        }

        [Test]
        public void An_alternate_api_host_reaches_the_services()
        {
            // Sonar S1075 aside, this is what lets the fetch tests point at a stub rather than github.com.
            var host = new Uri("https://ghe.example.invalid/api/v3/");

            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.ApiBaseAddress = host)
                .BuildServiceProvider();

            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ServiceCollectionExtensions.UpstreamClientName);

            Assert.That(client.BaseAddress, Is.EqualTo(host));
        }

        [Test]
        public void The_services_are_shared_so_connections_are_reused()
        {
            // Connection reuse is the whole reason for moving off urllib; a transient fetcher would
            // hand each caller its own client and give that back.
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.That(
                provider.GetRequiredService<IReleaseFetcher>(),
                Is.SameAs(provider.GetRequiredService<IReleaseFetcher>()));
        }

        [Test]
        public void Resolves_the_api_services_by_their_interfaces()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetService<IApiLayout>(), Is.InstanceOf<ApiLayout>());
                Assert.That(provider.GetService<IApiDiscovery>(), Is.InstanceOf<ApiDiscovery>());
                Assert.That(provider.GetService<IDocumentDownloader>(), Is.InstanceOf<HttpDocumentDownloader>());
                Assert.That(provider.GetService<IApiInstaller>(), Is.InstanceOf<ApiInstaller>());
                Assert.That(provider.GetService<IApiRemover>(), Is.InstanceOf<ApiRemover>());
            });
        }

        [Test]
        public void The_api_layout_sits_over_the_roots_the_knowledge_layout_was_given()
        {
            // One pair of roots for both axes: --repository-root and --output mean the same thing to
            // `hypha api generate` as they do to `hypha generate`.
            var root = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            var output = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "elsewhere"));

            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = root;
                    options.OutputRoot = output;
                })
                .BuildServiceProvider();

            var layout = provider.GetRequiredService<IApiLayout>();

            Assert.Multiple(() =>
            {
                Assert.That(
                    layout.Sources("2026-04").FullName,
                    Is.EqualTo(Path.Combine(root.FullName, "sources", "api", "2026-04")));
                Assert.That(
                    layout.Knowledge("2026-04").FullName,
                    Is.EqualTo(Path.Combine(output.FullName, "knowledge", "api", "2026-04")));
            });
        }

        [Test]
        public void The_document_client_carries_no_credentials_even_when_a_token_is_configured()
        {
            // The GitHub token is a credential for one host. The PIM model is read from another.
            using var provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.Token = "explicit")
                .BuildServiceProvider();

            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var documents = factory.CreateClient(ServiceCollectionExtensions.DocumentClientName);

            Assert.Multiple(() =>
            {
                Assert.That(documents.DefaultRequestHeaders.Authorization, Is.Null);
                Assert.That(documents.DefaultRequestHeaders.Contains("Authorization"), Is.False);
                Assert.That(
                    documents.DefaultRequestHeaders.ToString(), Does.Not.Contain("explicit"),
                    "the token must not reach the document host under any header");
                Assert.That(
                    factory.CreateClient(ServiceCollectionExtensions.UpstreamClientName)
                        .DefaultRequestHeaders.Authorization!.Parameter,
                    Is.EqualTo("explicit"),
                    "the same composition still authenticates the upstream client");
            });
        }

        [Test]
        public void The_document_client_identifies_itself_and_is_not_bound_to_the_github_api()
        {
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ServiceCollectionExtensions.DocumentClientName);

            Assert.Multiple(() =>
            {
                Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Does.Contain("mycelium-hypha"));
                Assert.That(client.BaseAddress, Is.Null);
                Assert.That(
                    client.DefaultRequestHeaders.Accept.Select(header => header.MediaType),
                    Does.Not.Contain("application/vnd.github+json"));
            });
        }

        [Test]
        public void No_api_generator_is_registered_among_the_language_generators()
        {
            // An API generator is an IKnowledgeGenerator too, so it could be registered under that
            // type by mistake - and `hypha generate` would then run it for every language release.
            using var provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            Assert.That(
                provider.GetServices<IKnowledgeGenerator>(),
                Has.None.InstanceOf<IApiKnowledgeGenerator>());
        }
    }
}
