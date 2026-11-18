// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Keywords
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// A grammar whose productions another grammar calls without defining them, as the SysML grammar
    /// calls KerML's <c>OwnedExpression</c>.
    /// </summary>
    /// <param name="Name">The shared grammar's key, such as <c>kerml</c>.</param>
    /// <param name="Productions">Its parsed productions, in document order.</param>
    public sealed record SharedGrammar(string Name, IReadOnlyList<Production> Productions);
}
