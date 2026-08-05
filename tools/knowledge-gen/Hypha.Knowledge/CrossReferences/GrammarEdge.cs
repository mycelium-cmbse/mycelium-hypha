// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// A grammar production that builds the element.
    /// </summary>
    /// <param name="Grammar">The grammar, <c>kerml</c> or <c>sysml</c>.</param>
    /// <param name="Production">The production's name.</param>
    /// <param name="Provenance">Which tier this fact belongs to.</param>
    /// <param name="Method">
    /// <c>declared-production</c> when the grammar states what the production builds, and
    /// <c>production-name</c> when the production simply carries the element's own name.
    /// </param>
    public sealed record GrammarEdge(string Grammar, string Production, string Provenance, string Method);
}
