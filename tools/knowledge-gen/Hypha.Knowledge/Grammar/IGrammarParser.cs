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
        IReadOnlyList<Production> Parse(string text);

        /// <summary>Parses a <c>.kgbnf</c> graphical grammar into productions, in document order.</summary>
        IReadOnlyList<Production> ParseGraphical(string text);

        /// <summary>
        /// The reserved keywords a grammar declares, ordered. Empty when it declares none.
        /// </summary>
        IReadOnlyList<string> ReservedKeywords(string text);
    }
}
