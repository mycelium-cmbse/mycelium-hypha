// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseTag.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The shape of an upstream release tag, and how tags order relative to one another.
    /// </summary>
    /// <remarks>
    /// Upstream publishes more than plain <c>YYYY-MM</c>: point releases (<c>2025-09.1</c>) are real
    /// releases and are accepted, while pre-releases (<c>2026-05-pre</c>), internal drops
    /// (<c>2021-08-internal</c>) and letter revisions (<c>2021-05a</c>) are not offered.
    /// </remarks>
    public static partial class ReleaseTag
    {
        /// <summary>
        /// Matches an offerable tag. The digit classes are written out rather than using <c>\d</c>:
        /// in .NET <c>\d</c> is Unicode-aware and would accept non-ASCII digits.
        /// </summary>
        [GeneratedRegex(@"^(?<year>[0-9]{4})-(?<month>[0-9]{2})(?:\.(?<point>[0-9]+))?$")]
        private static partial Regex TagPattern();

        /// <summary>True when <paramref name="tag"/> is a tag hypha will offer as a version.</summary>
        public static bool IsRelease(string? tag) =>
            !string.IsNullOrEmpty(tag) && TagPattern().IsMatch(tag);

        /// <summary>
        /// The chronological ordering key for a release tag; a point release sorts after its base tag.
        /// </summary>
        /// <exception cref="ArgumentException">The tag is not an offerable release tag.</exception>
        public static (int Year, int Month, int Point) SortKey(string tag)
        {
            ArgumentNullException.ThrowIfNull(tag);

            var match = TagPattern().Match(tag);
            if (!match.Success)
            {
                throw new ArgumentException($"not a release tag: {tag}", nameof(tag));
            }

            var point = match.Groups["point"];

            return (
                int.Parse(match.Groups["year"].Value),
                int.Parse(match.Groups["month"].Value),
                point.Success ? int.Parse(point.Value) : 0);
        }
    }
}
