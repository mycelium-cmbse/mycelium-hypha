// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationClauseEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Specification
{
    /// <summary>One clause in a specification's index.</summary>
    /// <param name="Title">The clause title, as the specification prints it.</param>
    /// <param name="Pages">The pages of the PDF the clause spans, such as <c>31-32</c>.</param>
    /// <param name="Normative">Whether the clause is normative.</param>
    /// <param name="File">The page with the clause's text, relative to the specification's folder.</param>
    public sealed record SpecificationClauseEntry(string Title, string Pages, bool Normative, string File);
}
