// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Moq;

    [TestFixture]
    public class ReleaseCheckTests
    {
        [Test]
        public async Task A_release_newer_than_everything_installed_is_named()
        {
            var comparison = await Check(installed: ["2026-05", "2026-02"], defaultTag: "2026-05", offered: ["2026-09", "2026-07", "2026-05"])
                .CheckReleasesAsync();

            Assert.Multiple(() =>
            {
                Assert.That(comparison.Newer, Is.EqualTo("2026-09"));
                Assert.That(comparison.Installed, Is.EqualTo(new[] { "2026-05", "2026-02" }));
                Assert.That(comparison.Default, Is.EqualTo("2026-05"));
                Assert.That(comparison.Offered, Is.EqualTo(new[] { "2026-09", "2026-07", "2026-05" }));
            });
        }

        [Test]
        public async Task With_the_newest_installed_nothing_is_newer_even_when_the_default_is_older()
        {
            var comparison = await Check(installed: ["2026-02", "2026-09"], defaultTag: "2026-02", offered: ["2026-09", "2026-07"])
                .CheckReleasesAsync();

            Assert.Multiple(() =>
            {
                Assert.That(comparison.Newer, Is.Null);
                Assert.That(comparison.Installed, Is.EqualTo(new[] { "2026-09", "2026-02" }), "newest first");
            });
        }

        [Test]
        public async Task With_nothing_installed_the_newest_offered_release_is_the_newer_one()
        {
            var comparison = await Check(installed: [], defaultTag: null, offered: ["2026-07", "2026-09"]).CheckReleasesAsync();

            Assert.Multiple(() =>
            {
                Assert.That(comparison.Newer, Is.EqualTo("2026-09"));
                Assert.That(comparison.Offered, Is.EqualTo(new[] { "2026-09", "2026-07" }), "newest first, whatever upstream's order");
            });
        }

        [Test]
        public async Task A_point_release_is_newer_than_its_base_release()
        {
            var comparison = await Check(installed: ["2026-09"], defaultTag: "2026-09", offered: ["2026-09.1", "2026-09"]).CheckReleasesAsync();

            Assert.That(comparison.Newer, Is.EqualTo("2026-09.1"));
        }

        [Test]
        public async Task At_most_five_offered_releases_are_listed()
        {
            var comparison = await Check(
                    installed: [], defaultTag: null, offered: ["2026-09", "2026-07", "2026-05", "2026-04", "2026-02", "2025-11", "2025-09"])
                .CheckReleasesAsync();

            Assert.That(comparison.Offered, Has.Count.EqualTo(UpstreamComparison.MaxOffered));
        }

        [Test]
        public async Task An_installed_tag_that_is_not_a_release_is_listed_but_never_ranked()
        {
            var comparison = await Check(installed: ["main", "2026-05"], defaultTag: "2026-05", offered: ["2026-07"]).CheckReleasesAsync();

            Assert.Multiple(() =>
            {
                Assert.That(comparison.Installed, Is.EqualTo(new[] { "2026-05", "main" }));
                Assert.That(comparison.Newer, Is.EqualTo("2026-07"));
            });
        }

        [Test]
        public async Task Both_axes_are_compared_from_one_listing_of_each_repository()
        {
            var discovery = new Mock<IReleaseDiscovery>(MockBehavior.Strict);
            discovery.Setup(d => d.FetchTagsAsync(Upstream.ReleaseRepository, It.IsAny<CancellationToken>())).ReturnsAsync(["2026-09", "2026-05", "2026-04"]);
            discovery.Setup(d => d.FetchTagsAsync(Upstream.PilotRepository, It.IsAny<CancellationToken>())).ReturnsAsync(["2026-09", "2026-05", "2026-04"]);
            discovery.Setup(d => d.FetchTagsAsync(ApiUpstream.ServicesRepository, It.IsAny<CancellationToken>())).ReturnsAsync(["2026-04", "2025-09"]);
            var layout = Layout(["2026-05"], "2026-05");
            var apiLayout = new Mock<IApiLayout>();
            apiLayout.Setup(api => api.InstalledTags).Returns(["2025-09"]);
            apiLayout.Setup(api => api.DefaultTag).Returns("2025-09");

            var check = await new ReleaseCheck(discovery.Object, layout, apiLayout.Object).CheckReleasesAndApiAsync();

            Assert.Multiple(() =>
            {
                Assert.That(check.Releases.Newer, Is.EqualTo("2026-09"));
                Assert.That(check.Api.Newer, Is.EqualTo("2026-04"));
                Assert.That(check.Api.Default, Is.EqualTo("2025-09"));
            });

            // Each repository once, for both axes: the anonymous GitHub API allows 60 requests an hour.
            discovery.Verify(d => d.FetchTagsAsync(Upstream.ReleaseRepository, It.IsAny<CancellationToken>()), Times.Once);
            discovery.Verify(d => d.FetchTagsAsync(Upstream.PilotRepository, It.IsAny<CancellationToken>()), Times.Once);
            discovery.Verify(d => d.FetchTagsAsync(ApiUpstream.ServicesRepository, It.IsAny<CancellationToken>()), Times.Once);
            discovery.VerifyNoOtherCalls();
        }

        [Test]
        public async Task With_no_API_version_installed_none_is_reported_as_newer()
        {
            var discovery = new Mock<IReleaseDiscovery>();
            discovery.Setup(d => d.FetchTagsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(["2026-04"]);
            var apiLayout = new Mock<IApiLayout>();
            apiLayout.Setup(api => api.InstalledTags).Returns([]);

            var check = await new ReleaseCheck(discovery.Object, Layout([], null), apiLayout.Object).CheckReleasesAndApiAsync();

            Assert.Multiple(() =>
            {
                Assert.That(check.Api.Offered, Is.EqualTo(new[] { "2026-04" }), "still listed, for a user who asks");
                Assert.That(check.Api.Newer, Is.Null, "only some users want the API; it is not offered as an update");
                Assert.That(check.Releases.Newer, Is.EqualTo("2026-04"), "every question needs a language release");
            });
        }

        private static IKnowledgeLayout Layout(IReadOnlyList<string> installed, string? defaultTag)
        {
            var layout = new Mock<IKnowledgeLayout>();
            layout.Setup(knowledge => knowledge.InstalledTags).Returns(installed);
            layout.Setup(knowledge => knowledge.DefaultTag).Returns(defaultTag);

            return layout.Object;
        }

        private static ReleaseCheck Check(IReadOnlyList<string> installed, string? defaultTag, IReadOnlyList<string> offered)
        {
            var discovery = new Mock<IReleaseDiscovery>();
            discovery.Setup(releases => releases.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offered);

            return new ReleaseCheck(discovery.Object, Layout(installed, defaultTag), Mock.Of<IApiLayout>());
        }
    }
}
