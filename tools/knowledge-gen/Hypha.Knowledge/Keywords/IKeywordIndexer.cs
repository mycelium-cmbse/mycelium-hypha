// ------------------------------------------------------------------------------------------------
// <copyright file="IKeywordIndexer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System;
    using System.Collections.Generic;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Builds one grammar's keyword table: each keyword and operator, the productions that write it and
    /// the metaclasses whose syntax includes it.
    /// </summary>
    public interface IKeywordIndexer
    {
        /// <summary>Builds the keyword table of one grammar.</summary>
        /// <param name="grammar">The grammar's key, such as <c>sysml</c>, written as the <see cref="KeywordEntry.SyntaxFrom"/> of its own entries.</param>
        /// <param name="productions">The grammar's parsed productions, in document order.</param>
        /// <param name="reserved">The keywords its <c>RESERVED_KEYWORD</c> production declares.</param>
        /// <param name="ancestors">
        /// Each metaclass of the metamodel with the names of all its supertypes; nothing outside these is
        /// reported as a metaclass.
        /// </param>
        /// <param name="shared">
        /// The grammar whose productions this one calls without defining them, or <c>null</c> for none.
        /// The literals its reachable productions write are listed too, unless this grammar writes them itself.
        /// </param>
        /// <returns>The grammar's entries, keyed and ordered ordinally by literal, with their counts.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="productions"/>, <paramref name="reserved"/> or <paramref name="ancestors"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="grammar"/> is <c>null</c>, empty or white space.</exception>
        KeywordGrammar Build(
            string grammar,
            IReadOnlyList<Production> productions,
            IReadOnlyList<string> reserved,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors,
            SharedGrammar? shared = null);
    }
}
