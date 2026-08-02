// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCatalog.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Works out which releases hypha can actually be built for.
    /// </summary>
    public static class ReleaseCatalog
    {
        /// <summary>
        /// The offerable versions: tags present in <b>both</b> upstreams and release-shaped, newest first.
        /// </summary>
        /// <remarks>
        /// This is an intersection rather than an assumption about either side. The repositories are
        /// not in lockstep - some tags exist in only one of them - so a version is offerable only
        /// when both a metamodel and a specification side exist for it.
        /// </remarks>
        public static IReadOnlyList<string> Available(
            IEnumerable<string> releaseTags, IEnumerable<string> pilotTags)
        {
            ArgumentNullException.ThrowIfNull(releaseTags);
            ArgumentNullException.ThrowIfNull(pilotTags);

            var pilot = pilotTags.ToHashSet(StringComparer.Ordinal);

            return releaseTags
                .Where(ReleaseTag.IsRelease)
                .Where(pilot.Contains)
                .Distinct(StringComparer.Ordinal)
                .OrderByDescending(ReleaseTag.SortKey)
                .ToList();
        }

        /// <summary>The newest <paramref name="count"/> versions from an already-ordered list.</summary>
        public static IReadOnlyList<string> Latest(IReadOnlyList<string> versions, int count = 1)
        {
            ArgumentNullException.ThrowIfNull(versions);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            return versions.Take(count).ToList();
        }
    }
}
