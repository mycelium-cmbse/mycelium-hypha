// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceCounts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>How much a cross-reference document holds.</summary>
    /// <param name="Types">The PIM types.</param>
    /// <param name="TypesWithClause">Those with at least one clause.</param>
    /// <param name="TypesWithSchema">Those with a PSM schema.</param>
    /// <param name="Operations">The PIM service operations.</param>
    /// <param name="OperationsWithEndpoint">Those mapped to at least one endpoint.</param>
    public sealed record ApiCrossReferenceCounts(
        int Types, int TypesWithClause, int TypesWithSchema, int Operations, int OperationsWithEndpoint);
}
