// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseDiscovery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Finds the releases hypha can be generated for.
    /// </summary>
    public interface IReleaseDiscovery
    {
        /// <summary>Every tag name in a repository, following pagination.</summary>
        Task<IReadOnlyList<string>> FetchTagsAsync(
            string repository, CancellationToken cancellationToken = default);

        /// <summary>
        /// The offerable versions: release-shaped tags present in <b>both</b> upstreams, newest first.
        /// </summary>
        Task<IReadOnlyList<string>> AvailableAsync(CancellationToken cancellationToken = default);
    }
}
