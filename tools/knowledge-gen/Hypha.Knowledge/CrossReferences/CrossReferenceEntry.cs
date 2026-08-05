// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceEntry.cs" company="Starion Group S.A.">
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
    /// Everything the knowledge base knows about where one element is treated.
    /// </summary>
    /// <remarks>
    /// Every element appears, with empty arrays where nothing matched, so a consumer can tell
    /// "nothing links here" from "this element is unknown".
    /// </remarks>
    /// <param name="Kind"><c>class</c>, <c>enumeration</c> or <c>primitiveType</c>.</param>
    /// <param name="Element">Its page, relative to <c>knowledge/&lt;tag&gt;/</c>.</param>
    /// <param name="Clauses">Specification clauses treating it.</param>
    /// <param name="Grammar">Grammar productions building it.</param>
    /// <param name="Features">Metamodel features its syntax populates.</param>
    /// <param name="Examples">Worked examples declaring it.</param>
    public sealed record CrossReferenceEntry(
        string Kind,
        string Element,
        IReadOnlyList<ClauseEdge> Clauses,
        IReadOnlyList<GrammarEdge> Grammar,
        IReadOnlyList<FeatureEdge> Features,
        IReadOnlyList<ExampleEdge> Examples);
}
