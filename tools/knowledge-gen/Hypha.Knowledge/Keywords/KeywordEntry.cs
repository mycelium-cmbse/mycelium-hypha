// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System.Collections.Generic;

    /// <summary>One keyword or operator: the productions that write it and the metaclasses it introduces.</summary>
    /// <param name="Kind"><see cref="Reserved"/>, <see cref="Contextual"/> or <see cref="Operator"/>.</param>
    /// <param name="Productions">
    /// The productions whose own text writes the literal, ordered ordinally. Empty for a reserved keyword
    /// no production uses.
    /// </param>
    /// <param name="Metaclasses">The metaclasses whose syntax includes the literal, ordered by name.</param>
    public sealed record KeywordEntry(
        string Kind,
        IReadOnlyList<string> Productions,
        IReadOnlyList<KeywordMetaclass> Metaclasses)
    {
        /// <summary>The <see cref="Kind"/> of a keyword the grammar's <c>RESERVED_KEYWORD</c> production declares.</summary>
        public const string Reserved = "reserved";

        /// <summary>The <see cref="Kind"/> of a word written as a literal but not reserved.</summary>
        public const string Contextual = "contextual";

        /// <summary>The <see cref="Kind"/> of a symbol, such as <c>:&gt;&gt;</c> or <c>=&gt;</c>.</summary>
        public const string Operator = "operator";
    }
}
