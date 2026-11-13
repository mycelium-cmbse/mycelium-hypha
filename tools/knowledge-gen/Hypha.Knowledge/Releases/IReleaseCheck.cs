// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseCheck.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Compares what is installed with what is offered upstream, and says when something newer is: what the
    /// <c>hypha check</c> verb and the MCP server's <c>hypha_check_releases</c> answer.
    /// </summary>
    /// <remarks>
    /// It only compares: it fetches, generates and writes nothing. It asks GitHub, so it is the one
    /// check of the installed releases that reaches the network.
    /// </remarks>
    public interface IReleaseCheck
    {
        /// <summary>Compares the installed language releases with the ones offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>The comparison.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        Task<UpstreamComparison> CheckReleasesAsync(CancellationToken cancellationToken = default);

        /// <summary>Compares the installed Systems Modeling API versions with the ones offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>The comparison.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        Task<UpstreamComparison> CheckApiAsync(CancellationToken cancellationToken = default);
    }
}
