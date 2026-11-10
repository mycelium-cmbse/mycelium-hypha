// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationClause.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>One clause of a specification, with its verbatim text.</summary>
    /// <param name="Document">The specification, as the knowledge keys it: <c>kerml</c>, <c>sysml2</c>, or <c>api</c>.</param>
    /// <param name="Clause">The clause number, such as <c>7.4.2</c>.</param>
    /// <param name="Title">The clause title.</param>
    /// <param name="Pages">The pages of the PDF the clause spans.</param>
    /// <param name="Normative">Whether the clause is normative.</param>
    /// <param name="Text">
    /// The clause's text, verbatim OMG specification text without the page's front matter. It must not
    /// be redistributed; quote it in an answer, never commit it.
    /// </param>
    public sealed record SpecificationClause(
        string Document, string Clause, string Title, string Pages, bool Normative, string Text);
}
