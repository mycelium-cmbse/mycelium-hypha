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
        /// <param name="productions">The grammar's parsed productions, in document order.</param>
        /// <param name="reserved">The keywords its <c>RESERVED_KEYWORD</c> production declares.</param>
        /// <param name="ancestors">
        /// Each metaclass of the metamodel with the names of all its supertypes; nothing outside these is
        /// reported as a metaclass.
        /// </param>
        /// <returns>The grammar's entries, keyed and ordered ordinally by literal, with their counts.</returns>
        /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
        KeywordGrammar Build(
            IReadOnlyList<Production> productions,
            IReadOnlyList<string> reserved,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors);
    }
}
