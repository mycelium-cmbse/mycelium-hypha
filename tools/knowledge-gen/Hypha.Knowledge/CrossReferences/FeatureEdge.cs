// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// A metamodel feature that the element's syntax populates, and how.
    /// </summary>
    /// <remarks>
    /// The only link in the knowledge base between the notation and the features it fills. The same
    /// feature legitimately appears under both grammars - SysML re-declares many KerML productions -
    /// which is one fact stated twice, not a duplicate to collapse.
    /// </remarks>
    /// <param name="Grammar">The grammar the assignment was read from.</param>
    /// <param name="Feature">The metamodel feature.</param>
    /// <param name="Operator"><c>=</c> sets a value, <c>+=</c> adds, <c>?=</c> is a boolean flag.</param>
    /// <param name="Productions">The productions carrying the assignment.</param>
    /// <param name="Provenance">Which tier this fact belongs to.</param>
    /// <param name="Method">How the edge was obtained.</param>
    public sealed record FeatureEdge(
        string Grammar,
        string Feature,
        string Operator,
        IReadOnlyList<string> Productions,
        string Provenance,
        string Method);
}
