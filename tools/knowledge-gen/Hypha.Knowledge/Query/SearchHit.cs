// ------------------------------------------------------------------------------------------------
// <copyright file="SearchHit.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>One search result.</summary>
    /// <param name="Address">The page the hit names, to resolve or cite.</param>
    /// <param name="Title">
    /// What the hit is: the element's summary, the declaration's kind, or the clause title.
    /// </param>
    /// <param name="Match">How closely it matched; lower is closer.</param>
    public sealed record SearchHit(HyphaAddress Address, string Title, SearchMatch Match);
}
