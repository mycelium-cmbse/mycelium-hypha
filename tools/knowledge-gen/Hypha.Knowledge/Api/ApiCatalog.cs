// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCatalog.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Which API versions can be offered, as a pure function of the upstream tag lists.
    /// </summary>
    public static class ApiCatalog
    {
        /// <summary>
        /// The earliest API version that can be offered.
        /// </summary>
        /// <remarks>
        /// The PIM model is one fixed OMG document (<see cref="ApiUpstream.PimDocument"/>, dated
        /// 2025-02) rather than a file at a tag. Pairing it with an older OpenAPI document and
        /// specification would compare a 2025 PIM against an earlier PSM, and the differences reported
        /// would be version skew, not specification facts. Older tags also predate the files a fetch
        /// reads: several carry no <c>openapi.json</c> at all.
        /// </remarks>
        public const string EarliestVersion = "2025-02";

        /// <summary>
        /// The API-Services tags that are also a language release and not older than
        /// <see cref="EarliestVersion"/>, newest first.
        /// </summary>
        /// <remarks>
        /// The API-Services repository is not tagged for every language release, so this is a subset
        /// of the language releases rather than the same list. The intersection is what makes a
        /// version installable: its specification PDF and the KerML model its PIM refers to are read
        /// from the language repositories at the same tag.
        /// </remarks>
        public static IReadOnlyList<string> Available(
            IEnumerable<string> servicesTags, IEnumerable<string> releaseTags, IEnumerable<string> pilotTags)
        {
            ArgumentNullException.ThrowIfNull(servicesTags);

            var services = servicesTags.ToHashSet(StringComparer.Ordinal);

            return ReleaseCatalog.Available(releaseTags, pilotTags)
                .Where(services.Contains)
                .Where(IsOfferable)
                .ToList();
        }

        /// <summary>
        /// Whether <paramref name="tag"/> is release-shaped and not older than
        /// <see cref="EarliestVersion"/>.
        /// </summary>
        public static bool IsOfferable(string? tag) =>
            ReleaseTag.IsRelease(tag)
            && ReleaseTag.SortKey(tag!).CompareTo(ReleaseTag.SortKey(EarliestVersion)) >= 0;
    }
}
