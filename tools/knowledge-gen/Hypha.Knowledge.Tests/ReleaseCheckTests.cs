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
        public async Task The_API_versions_are_compared_with_the_offered_API_versions()
        {
            var api = new Mock<IApiDiscovery>();
            api.Setup(discovery => discovery.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(["2026-04", "2025-09"]);
            var apiLayout = new Mock<IApiLayout>();
            apiLayout.Setup(layout => layout.InstalledTags).Returns(["2025-09"]);
            apiLayout.Setup(layout => layout.DefaultTag).Returns("2025-09");
            var releases = new Mock<IReleaseDiscovery>(MockBehavior.Strict);

            var comparison = await new ReleaseCheck(releases.Object, api.Object, Mock.Of<IKnowledgeLayout>(), apiLayout.Object).CheckApiAsync();

            Assert.Multiple(() =>
            {
                Assert.That(comparison.Newer, Is.EqualTo("2026-04"));
                Assert.That(comparison.Default, Is.EqualTo("2025-09"));
            });
            releases.VerifyNoOtherCalls();
        }

        private static ReleaseCheck Check(IReadOnlyList<string> installed, string? defaultTag, IReadOnlyList<string> offered)
        {
            var discovery = new Mock<IReleaseDiscovery>();
            discovery.Setup(releases => releases.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offered);
            var layout = new Mock<IKnowledgeLayout>();
            layout.Setup(knowledge => knowledge.InstalledTags).Returns(installed);
            layout.Setup(knowledge => knowledge.DefaultTag).Returns(defaultTag);

            return new ReleaseCheck(discovery.Object, Mock.Of<IApiDiscovery>(MockBehavior.Strict), layout.Object, Mock.Of<IApiLayout>());
        }
    }
}
