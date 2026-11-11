// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordGrammar.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System.Collections.Generic;

    /// <summary>One grammar's keywords and operators.</summary>
    /// <param name="Counts">How many entries of each kind the grammar has.</param>
    /// <param name="Entries">
    /// One entry per literal, keyed by the literal as written in the grammar (<c>ref</c>, <c>:&gt;&gt;</c>)
    /// and ordered ordinally.
    /// </param>
    public sealed record KeywordGrammar(KeywordCounts Counts, IReadOnlyDictionary<string, KeywordEntry> Entries);
}
