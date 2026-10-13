// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCatalogTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for deciding which API versions to offer. Pure - no network.
    /// </summary>
    [TestFixture]
    public class ApiCatalogTests
    {
        private static readonly string[] LanguageReleases =
        [
            "2026-05", "2026-04", "2026-03", "2025-09.1", "2025-09", "2025-07",
        ];

        [Test]
        public void Available_is_the_api_services_tags_that_are_also_a_language_release()
        {
            // The API-Services repository is tagged far less often than the language repositories.
            var versions = ApiCatalog.Available(["2026-04", "2025-09"], LanguageReleases, LanguageReleases);

            Assert.That(versions, Is.EqualTo(new[] { "2026-04", "2025-09" }));
        }

        [Test]
        public void Available_is_newest_first_whatever_order_the_tags_arrive_in()
        {
            var versions = ApiCatalog.Available(
                ["2025-07", "2026-04", "2025-09.1", "2025-09"], LanguageReleases, LanguageReleases);

            Assert.Multiple(() =>
            {
                Assert.That(versions, Is.EqualTo(new[] { "2026-04", "2025-09.1", "2025-09", "2025-07" }));
                Assert.That(versions, Is.EqualTo(versions.OrderByDescending(ReleaseTag.SortKey).ToList()));
            });
        }

        [Test]
        public void A_services_tag_missing_from_the_release_repository_is_not_offered()
        {
            // Without the Release repository at that tag there is no specification PDF to read.
            var versions = ApiCatalog.Available(["2026-05", "2026-04"], ["2026-05"], ["2026-05", "2026-04"]);

            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public void A_services_tag_missing_from_the_pilot_repository_is_not_offered()
        {
            // Without the Pilot repository at that tag there is no KerML model for the PIM to refer to.
            var versions = ApiCatalog.Available(["2026-05", "2026-04"], ["2026-05", "2026-04"], ["2026-05"]);

            Assert.That(versions, Is.EqualTo(new[] { "2026-05" }));
        }

        [Test]
        public void A_language_release_the_api_services_repository_never_tagged_is_not_offered()
        {
            var versions = ApiCatalog.Available(["2026-04"], LanguageReleases, LanguageReleases);

            Assert.That(versions, Is.EqualTo(new[] { "2026-04" }));
        }

        [Test]
        public void Pre_releases_are_excluded_even_when_present_in_all_three()
        {
            string[] tags = ["2026-04", "2026-05-pre", "2021-05a", "2021-08-internal"];

            Assert.That(ApiCatalog.Available(tags, tags, tags), Is.EqualTo(new[] { "2026-04" }));
        }

        [Test]
        public void Duplicate_tags_are_collapsed()
        {
            var versions = ApiCatalog.Available(["2026-04", "2026-04"], ["2026-04", "2026-04"], ["2026-04"]);

            Assert.That(versions, Has.Count.EqualTo(1));
        }

        [Test]
        public void No_services_tags_means_nothing_is_offered()
        {
            Assert.That(ApiCatalog.Available([], LanguageReleases, LanguageReleases), Is.Empty);
        }

        [Test]
        public void Missing_services_tags_are_refused()
        {
            Assert.Throws<ArgumentNullException>(
                () => ApiCatalog.Available(null!, LanguageReleases, LanguageReleases));
        }
    }
}
