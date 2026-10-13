// ------------------------------------------------------------------------------------------------
// <copyright file="ApiDiscoveryTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Releases;

    using Moq;

    /// <summary>
    /// Suite of tests for <see cref="ApiDiscovery"/>: which repositories it asks, and what it makes of
    /// their answers. The tag listing itself is stubbed - no live calls.
    /// </summary>
    [TestFixture]
    public class ApiDiscoveryTests
    {
        private Mock<IReleaseDiscovery> releases = null!;
        private ApiDiscovery discovery = null!;

        [SetUp]
        public void SetUp()
        {
            this.releases = new Mock<IReleaseDiscovery>();

            this.Tags(ApiUpstream.ServicesRepository, "2026-04", "2025-09");
            this.Tags(Upstream.ReleaseRepository, "2026-05", "2026-04", "2025-09");
            this.Tags(Upstream.PilotRepository, "2026-05", "2026-04", "2025-09");

            this.discovery = new ApiDiscovery(this.releases.Object);
        }

        [Test]
        public async Task The_offerable_versions_are_the_services_tags_that_are_also_a_language_release()
        {
            var available = await this.discovery.AvailableAsync();

            Assert.That(available, Is.EqualTo(new[] { "2026-04", "2025-09" }));
        }

        [Test]
        public async Task All_three_repositories_are_asked_once_each()
        {
            await this.discovery.AvailableAsync();

            this.releases.Verify(
                mock => mock.FetchTagsAsync(ApiUpstream.ServicesRepository, It.IsAny<CancellationToken>()),
                Times.Once);
            this.releases.Verify(
                mock => mock.FetchTagsAsync(Upstream.ReleaseRepository, It.IsAny<CancellationToken>()),
                Times.Once);
            this.releases.Verify(
                mock => mock.FetchTagsAsync(Upstream.PilotRepository, It.IsAny<CancellationToken>()),
                Times.Once);

            this.releases.Verify(
                mock => mock.AvailableAsync(It.IsAny<CancellationToken>()),
                Times.Never,
                "the language releases' own answer is not what is offered here");
        }

        [Test]
        public async Task A_version_the_pilot_repository_lacks_is_not_offered()
        {
            this.Tags(Upstream.PilotRepository, "2026-05", "2025-09");

            var available = await this.discovery.AvailableAsync();

            Assert.That(available, Is.EqualTo(new[] { "2025-09" }));
        }

        [Test]
        public async Task An_untagged_services_repository_offers_nothing()
        {
            this.Tags(ApiUpstream.ServicesRepository);

            var available = await this.discovery.AvailableAsync();

            Assert.That(available, Is.Empty);
        }

        [Test]
        public async Task The_cancellation_token_reaches_every_listing()
        {
            using var source = new CancellationTokenSource();

            await this.discovery.AvailableAsync(source.Token);

            this.releases.Verify(
                mock => mock.FetchTagsAsync(It.IsAny<string>(), source.Token), Times.Exactly(3));
        }

        [Test]
        public void An_unreachable_upstream_is_not_swallowed()
        {
            // An empty list would read as "the upstreams offer nothing", which is a different answer.
            this.releases
                .Setup(mock => mock.FetchTagsAsync(ApiUpstream.ServicesRepository, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("rate limited"));

            Assert.That(() => this.discovery.AvailableAsync(), Throws.InstanceOf<HttpRequestException>());
        }

        [Test]
        public void A_missing_release_discovery_is_refused()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new ApiDiscovery(null!));
        }

        private void Tags(string repository, params string[] tags) =>
            this.releases
                .Setup(mock => mock.FetchTagsAsync(repository, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tags);
    }
}
