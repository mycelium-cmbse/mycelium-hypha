// ------------------------------------------------------------------------------------------------
// <copyright file="ApiTypeEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// One PIM type and what it lines up with.
    /// </summary>
    /// <param name="Kind"><c>class</c>, <c>interface</c>, <c>enumeration</c> or <c>dataType</c>.</param>
    /// <param name="Pim">Its PIM page, relative to the API version's knowledge folder.</param>
    /// <param name="Clauses">The clauses that cover it, in clause order.</param>
    /// <param name="Schema">Its PSM schemas; <c>null</c> when it has none.</param>
    /// <param name="Members">
    /// Its effective PIM attributes and whether each appears in the PSM models; <c>null</c> when it has
    /// no PSM schema to compare against.
    /// </param>
    /// <param name="PsmOnly">
    /// Members of the PSM response model that are not PIM attributes of the type; <c>null</c> when
    /// there is nothing to compare.
    /// </param>
    public sealed record ApiTypeEntry(
        string Kind,
        string Pim,
        IReadOnlyList<ApiClauseEdge> Clauses,
        ApiSchemaEdge? Schema,
        IReadOnlyList<ApiMemberEntry>? Members,
        IReadOnlyList<string>? PsmOnly);
}
