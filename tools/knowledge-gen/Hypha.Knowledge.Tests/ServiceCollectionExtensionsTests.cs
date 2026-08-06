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
    using Hypha.Knowledge.CrossReferences;
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
    }
}
