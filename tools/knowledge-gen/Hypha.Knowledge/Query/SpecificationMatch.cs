// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationMatch.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>One clause whose text contains what a specification search looked for.</summary>
    /// <param name="Address">The clause, to read in full or cite.</param>
    /// <param name="Title">The clause title.</param>
    /// <param name="Occurrences">How many times the clause's text contains it.</param>
    /// <param name="Excerpt">
    /// The text around the first occurrence, with its white space collapsed, to judge whether the clause
    /// is the one wanted; read the clause itself before quoting it.
    /// </param>
    public sealed record SpecificationMatch(HyphaAddress Address, string Title, int Occurrences, string Excerpt);
}
