// ------------------------------------------------------------------------------------------------
// <copyright file="ProductionUse.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.CrossReferences;

    /// <summary>An element that a grammar production builds.</summary>
    /// <param name="Element">The element the production builds.</param>
    /// <param name="Edge">The grammar, the production and the provenance of the link.</param>
    public sealed record ProductionUse(string Element, GrammarEdge Edge);
}
