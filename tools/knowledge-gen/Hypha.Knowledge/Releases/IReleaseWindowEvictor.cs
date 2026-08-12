// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseWindowEvictor.cs" company="Starion Group S.A.">
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
    /// Shrinks the installed releases back to the rolling window: drops the manifest entries and
    /// deletes the on-disk folders of whichever releases no longer fit.
    /// </summary>
    public interface IReleaseWindowEvictor
    {
        /// <summary>
        /// Evicts every installed release beyond the newest <paramref name="keep"/>.
        /// </summary>
        /// <param name="keep">How many releases the window holds.</param>
        /// <returns>The tags evicted, oldest first; empty when the window was not over size.</returns>
        Task<IReadOnlyList<string>> EvictAsync(int keep, CancellationToken cancellationToken = default);
    }
}
