// ------------------------------------------------------------------------------------------------
// <copyright file="IApiDiscovery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Finds the API versions the upstreams offer.
    /// </summary>
    public interface IApiDiscovery
    {
        /// <summary>The offerable API versions, newest first.</summary>
        /// <param name="cancellationToken">Cancels the tag listing.</param>
        /// <returns>The offerable API versions, newest first.</returns>
        Task<IReadOnlyList<string>> AvailableAsync(CancellationToken cancellationToken = default);
    }
}
