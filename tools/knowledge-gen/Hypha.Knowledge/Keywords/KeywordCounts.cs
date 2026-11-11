// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordCounts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    /// <summary>How many entries of each kind one grammar has.</summary>
    /// <param name="Reserved">The keywords the grammar's <c>RESERVED_KEYWORD</c> production declares.</param>
    /// <param name="Contextual">The other words written as literals.</param>
    /// <param name="Operators">The symbols written as literals, plain punctuation left out.</param>
    public sealed record KeywordCounts(int Reserved, int Contextual, int Operators);
}
