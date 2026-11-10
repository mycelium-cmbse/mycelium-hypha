// ------------------------------------------------------------------------------------------------
// <copyright file="ResolvedAddress.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>The generated page an address names.</summary>
    /// <param name="Address">The address.</param>
    /// <param name="Title">
    /// What the page is about: the element's summary, the declaration's kind, or the clause title.
    /// </param>
    /// <param name="Page">The page, relative to the output root (the folder holding <c>knowledge/</c>), with <c>/</c> separators.</param>
    public sealed record ResolvedAddress(HyphaAddress Address, string Title, string Page);
}
