// ------------------------------------------------------------------------------------------------
// <copyright file="IGrammarParser.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Grammar
{
    using System.Collections.Generic;

    /// <summary>
    /// Parses the KerML / SysML BNF into productions.
    /// </summary>
    public interface IGrammarParser
    {
        /// <summary>Parses a <c>.kebnf</c> grammar into productions, in document order.</summary>
        /// <param name="text">The grammar's content.</param>
        /// <returns>The productions, each with its clause attribution and feature assignments.</returns>
        IReadOnlyList<Production> Parse(string text);

        /// <summary>Parses a <c>.kgbnf</c> graphical grammar into productions, in document order.</summary>
        /// <param name="text">The graphical grammar's content.</param>
        /// <returns>The productions, each with its clause attribution and image references.</returns>
        IReadOnlyList<Production> ParseGraphical(string text);

        /// <summary>
        /// The reserved keywords a grammar declares, ordered. Empty when it declares none.
        /// </summary>
        /// <param name="text">The grammar's content.</param>
        /// <returns>The keywords of its <c>RESERVED_KEYWORD</c> production.</returns>
        IReadOnlyList<string> ReservedKeywords(string text);
    }
}
