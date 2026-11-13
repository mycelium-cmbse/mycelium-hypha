// ------------------------------------------------------------------------------------------------
// <copyright file="UpstreamCheck.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>Both version axes against what is offered upstream.</summary>
    /// <param name="Releases">The language releases.</param>
    /// <param name="Api">The Systems Modeling API versions.</param>
    public sealed record UpstreamCheck(UpstreamComparison Releases, UpstreamComparison Api);
}
