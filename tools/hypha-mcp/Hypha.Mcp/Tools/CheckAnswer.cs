// ------------------------------------------------------------------------------------------------
// <copyright file="CheckAnswer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using Hypha.Knowledge.Releases;

    /// <summary>What <c>hypha_check_releases</c> found.</summary>
    /// <param name="Releases">The language releases, installed and offered.</param>
    /// <param name="Api">The Systems Modeling API versions, installed and offered; <c>null</c> when not asked for.</param>
    internal sealed record CheckAnswer(UpstreamComparison Releases, UpstreamComparison? Api);
}
