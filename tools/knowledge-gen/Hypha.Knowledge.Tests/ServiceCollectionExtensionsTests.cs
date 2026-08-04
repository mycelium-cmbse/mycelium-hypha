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
    using System.Linq;
    using System.Net.Http;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Releases;

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
                Assert.That(provider.GetService<ReleaseDiscovery>(), Is.Not.Null);
                Assert.That(provider.GetService<CommitResolver>(), Is.Not.Null);
                Assert.That(provider.GetService<ReleaseFetcher>(), Is.Not.Null);
            });
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
                .AddHyphaKnowledge("explicit")
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
                .AddHyphaKnowledge(apiBaseAddress: host)
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
                provider.GetRequiredService<ReleaseFetcher>(),
                Is.SameAs(provider.GetRequiredService<ReleaseFetcher>()));
        }
    }
}
