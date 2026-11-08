// ------------------------------------------------------------------------------------------------
// <copyright file="IGrammarReference.cs" company="Starion Group S.A.">
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
    /// Renders the per-release grammar references under <c>knowledge/&lt;tag&gt;/textual-notation/</c>.
    /// </summary>
    public interface IGrammarReference
    {
        /// <summary>Renders the textual grammar reference for one document, grouped by clause.</summary>
        /// <param name="grammar">The grammar's name, <c>KerML</c> or <c>SysML</c>.</param>
        /// <param name="tag">The release the grammar was read at.</param>
        /// <param name="productions">The parsed productions, in document order.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string Render(string grammar, string tag, IReadOnlyList<Production> productions);

        /// <summary>Renders the graphical-notation reference for one release, grouped by clause.</summary>
        /// <param name="tag">The release the grammar was read at.</param>
        /// <param name="repository">The upstream repository the notation images are published from.</param>
        /// <param name="productions">The parsed graphical productions, in document order.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string RenderGraphical(string tag, string repository, IReadOnlyList<Production> productions);
    }
}
