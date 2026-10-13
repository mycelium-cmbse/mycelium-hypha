// ------------------------------------------------------------------------------------------------
// <copyright file="ApiOperationEntry.cs" company="Starion Group S.A.">
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
    /// One PIM service operation and what it maps to.
    /// </summary>
    /// <param name="Service">The service that owns it.</param>
    /// <param name="Clauses">The clauses that cover it.</param>
    /// <param name="Endpoints">The endpoints the specification maps it to on a single table row.</param>
    /// <param name="MappingClause">
    /// The clause holding the PIM-to-PSM endpoint mapping, when the operation is named there. An
    /// operation with this set and no endpoints is mapped in prose that spans several rows: read the
    /// clause.
    /// </param>
    public sealed record ApiOperationEntry(
        string Service,
        IReadOnlyList<ApiClauseEdge> Clauses,
        IReadOnlyList<ApiEndpointEdge> Endpoints,
        string? MappingClause);
}
