// ------------------------------------------------------------------------------------------------
// <copyright file="ClauseEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// The element is treated in this clause.
    /// </summary>
    /// <remarks>
    /// Carries the clause <b>number</b> only - never its title or body. That is the licensing
    /// guarantee that lets this file be committed while <c>knowledge/&lt;tag&gt;/spec</c> stays
    /// git-ignored, and what lets a consumer still name the governing clause when the PDFs are absent.
    /// </remarks>
    /// <param name="Document">The specification tree, <c>kerml</c> or <c>sysml2</c>.</param>
    /// <param name="Clause">The clause number, e.g. <c>8.3.6.3</c>.</param>
    /// <param name="Provenance">Which tier this fact belongs to.</param>
    /// <param name="Method">How the edge was obtained.</param>
    public sealed record ClauseEdge(string Document, string Clause, string Provenance, string Method);
}
