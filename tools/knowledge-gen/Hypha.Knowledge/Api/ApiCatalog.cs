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
        /// The API-Services tags that are also a language release, newest first.
        /// </summary>
        /// <remarks>
        /// The API-Services repository is tagged far less often than the language repositories, so
        /// this is a subset of the language releases rather than the same list. The intersection is
        /// what makes a version installable: its specification PDF and the KerML model its PIM refers
        /// to are read from the language repositories at the same tag.
        /// </remarks>
        public static IReadOnlyList<string> Available(
            IEnumerable<string> servicesTags, IEnumerable<string> releaseTags, IEnumerable<string> pilotTags)
        {
            ArgumentNullException.ThrowIfNull(servicesTags);

            var services = servicesTags.ToHashSet(StringComparer.Ordinal);

            return ReleaseCatalog.Available(releaseTags, pilotTags).Where(services.Contains).ToList();
        }
    }
}
