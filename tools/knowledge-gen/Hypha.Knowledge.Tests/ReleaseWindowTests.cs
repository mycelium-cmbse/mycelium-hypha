// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseWindowTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for deciding which installed releases fall outside the rolling window.
    /// </summary>
    [TestFixture]
    public class ReleaseWindowTests
    {
        private static InstalledVersion Version(string tag) =>
            new(tag, new UpstreamReference("owner/release", "abc"), new UpstreamReference("owner/pilot", "def"));

        [Test]
        public void The_oldest_releases_beyond_keep_are_evicted()
        {
            var versions = new[] { Version("2026-04"), Version("2026-05"), Version("2026-06") };

            var evicted = ReleaseWindow.Evicted(versions, keep: 2);

            Assert.That(evicted, Is.EqualTo(new[] { "2026-04" }));
        }

        [Test]
        public void Point_releases_sort_after_their_base_tag()
        {
            var versions = new[] { Version("2025-09"), Version("2025-09.1"), Version("2026-05") };

            var evicted = ReleaseWindow.Evicted(versions, keep: 2);

            Assert.That(evicted, Is.EqualTo(new[] { "2025-09" }));
        }

        [Test]
        public void Nothing_is_evicted_when_keep_covers_every_installed_release()
        {
            var versions = new[] { Version("2026-04"), Version("2026-05") };

            Assert.Multiple(() =>
            {
                Assert.That(ReleaseWindow.Evicted(versions, keep: 2), Is.Empty);
                Assert.That(ReleaseWindow.Evicted(versions, keep: 5), Is.Empty);
            });
        }

        [Test]
        public void Evicting_from_an_installed_list_of_one_evicts_nothing_at_keep_one()
        {
            var versions = new[] { Version("2026-05") };

            Assert.That(ReleaseWindow.Evicted(versions, keep: 1), Is.Empty);
        }

        [Test]
        public void A_window_smaller_than_one_is_refused()
        {
            var versions = new[] { Version("2026-05") };

            Assert.Throws<ArgumentOutOfRangeException>(() => ReleaseWindow.Evicted(versions, keep: 0));
        }

        [Test]
        public void Evicted_releases_are_reported_oldest_first()
        {
            var versions = new[]
            {
                Version("2026-06"), Version("2026-05"), Version("2026-04"), Version("2026-03"),
            };

            var evicted = ReleaseWindow.Evicted(versions, keep: 1);

            Assert.That(evicted, Is.EqualTo(new[] { "2026-03", "2026-04", "2026-05" }));
        }
    }
}
