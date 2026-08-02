// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCatalogTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for working out which releases are offerable, on the tag sets the upstreams really have.
    /// </summary>
    [TestFixture]
    public class ReleaseCatalogTests
    {
        private static readonly string[] ReleaseShaped =
        [
            "2026-05", "2026-04", "2026-03", "2025-09.1", "2025-09", "2025-07", "2020-10",
        ];

        /// <summary>Release-shaped, but published only by the Pilot repository.</summary>
        private static readonly string[] PilotOnlyButValid = ["2024-08", "2023-01"];

        /// <summary>Not release-shaped at all: pre-releases, internal drops, letter revisions.</summary>
        private static readonly string[] NonRelease =
        [
            "2026-05-pre", "2021-05a", "2021-08-internal", "2021-02b",
        ];

        [Test]
        public void Available_is_the_intersection_newest_first()
        {
            var versions = ReleaseCatalog.Available(ReleaseShaped, ReleaseShaped);

            Assert.Multiple(() =>
            {
                Assert.That(versions[0], Is.EqualTo("2026-05"));
                Assert.That(versions[^1], Is.EqualTo("2020-10"));
                Assert.That(versions, Is.EqualTo(versions.OrderByDescending(ReleaseTag.SortKey).ToList()));
            });
        }

        [Test]
        public void A_tag_missing_from_the_pilot_repository_is_not_offered()
        {
            // 2023-07.1 exists only in the Release repo; without a metamodel there is no version.
            var versions = ReleaseCatalog.Available(["2026-05", "2023-07.1"], ["2026-05"]);

            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public void A_release_shaped_tag_missing_from_the_release_repository_is_not_offered()
        {
            var pilot = new List<string> { "2026-05" };
            pilot.AddRange(PilotOnlyButValid);

            var versions = ReleaseCatalog.Available(["2026-05"], pilot);

            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public void Pre_releases_are_excluded_even_when_present_in_both()
        {
            var tags = new List<string> { "2026-05" };
            tags.AddRange(NonRelease);

            var versions = ReleaseCatalog.Available(tags, tags);

            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public void Duplicate_tags_are_collapsed()
        {
            var versions = ReleaseCatalog.Available(["2026-05", "2026-05"], ["2026-05"]);

            Assert.That(versions, Has.Count.EqualTo(1));
        }

        [Test]
        public void Latest_takes_the_rolling_window()
        {
            var versions = ReleaseCatalog.Available(ReleaseShaped, ReleaseShaped);

            Assert.That(ReleaseCatalog.Latest(versions, 2), Is.EqualTo(new[] { "2026-05", "2026-04" }));
        }

        [Test]
        public void Latest_tolerates_asking_for_more_than_exist()
        {
            Assert.That(ReleaseCatalog.Latest(["2026-05"], 5), Is.EqualTo(new[] { "2026-05" }));
        }
    }
}
