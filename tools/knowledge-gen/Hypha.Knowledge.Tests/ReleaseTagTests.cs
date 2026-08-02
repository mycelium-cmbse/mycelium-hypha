// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseTagTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for the release-tag shape rules, using the tag forms both upstreams really publish.
    /// </summary>
    [TestFixture]
    public class ReleaseTagTests
    {
        [TestCase("2026-05")]
        [TestCase("2020-10")]
        [TestCase("2025-09.1")]
        [TestCase("2023-07.1")]
        public void Accepts_release_shaped_tags(string tag)
        {
            Assert.That(ReleaseTag.IsRelease(tag), Is.True);
        }

        [TestCase("2026-05-pre")]
        [TestCase("2021-05a")]
        [TestCase("2021-08-internal")]
        [TestCase("2021-02b")]
        [TestCase("main")]
        [TestCase("")]
        [TestCase(null)]
        public void Rejects_pre_releases_and_oddities(string? tag)
        {
            Assert.That(ReleaseTag.IsRelease(tag), Is.False);
        }

        [Test]
        public void Rejects_non_ascii_digits()
        {
            // .NET's \d is Unicode-aware; the pattern uses explicit [0-9] so this cannot slip through.
            Assert.That(ReleaseTag.IsRelease("٢٠٢٦-٠٥"), Is.False);
        }

        [Test]
        public void Sorts_chronologically_not_lexically()
        {
            var ordered = new[] { "2025-09", "2026-01", "2025-12" }
                .OrderBy(ReleaseTag.SortKey)
                .ToList();

            Assert.That(ordered, Is.EqualTo(new[] { "2025-09", "2025-12", "2026-01" }));
        }

        [Test]
        public void Point_release_sorts_after_its_base_tag()
        {
            var ordered = new[] { "2025-09.1", "2025-09" }.OrderBy(ReleaseTag.SortKey).ToList();

            Assert.That(ordered, Is.EqualTo(new[] { "2025-09", "2025-09.1" }));
        }

        [Test]
        public void Sort_key_rejects_a_tag_that_is_not_a_release()
        {
            var exception = Assert.Throws<ArgumentException>(() => ReleaseTag.SortKey("2026-05-pre"));

            Assert.That(exception!.Message, Does.Contain("not a release tag"));
        }
    }
}
