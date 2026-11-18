// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.CrossReferences;

    /// <summary>An element that a specification clause treats.</summary>
    /// <param name="Element">The element the clause treats.</param>
    /// <param name="Edge">The document, the clause and the provenance and method of the link.</param>
    public sealed record ClauseUse(string Element, ClauseEdge Edge);
}
