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
    /// It only compares: it fetches, generates and writes nothing. It asks GitHub, whose anonymous API allows
    /// 60 requests an hour, so each repository's tags are listed once per check.
    /// </remarks>
    public interface IReleaseCheck
    {
        /// <summary>Compares the installed language releases with the ones offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>The comparison.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        /// <remarks>Lists the Release and Pilot-Implementation repositories' tags.</remarks>
        Task<UpstreamComparison> CheckReleasesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Compares the installed language releases and Systems Modeling API versions with the ones offered
        /// upstream, listing each repository's tags once for both.
        /// </summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>Both comparisons.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        /// <remarks>Lists the Release, Pilot-Implementation and API-Services repositories' tags, once each.</remarks>
        Task<UpstreamCheck> CheckReleasesAndApiAsync(CancellationToken cancellationToken = default);
    }
}
