// ------------------------------------------------------------------------------------------------
// <copyright file="INotationRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System.Collections.Generic;

    /// <summary>
    /// Renders <c>knowledge/&lt;tag&gt;/textual-notation/</c> from a release's own models and grammar.
    /// </summary>
    public interface INotationRenderer
    {
        /// <summary>A deterministic, flat, unique file name for a model's example page.</summary>
        string ExampleFileName(string sourcePath);

        /// <summary>Renders one model as a page: front matter, the verbatim source, and its elements.</summary>
        string RenderExample(
            string sourcePath, string modelText, IReadOnlyList<string> elements, string language);

        /// <summary>Renders the per-release index: the keyword reference and every generated example.</summary>
        string RenderIndex(
            string tag,
            IReadOnlyList<ExampleEntry> examples,
            IReadOnlyDictionary<string, IReadOnlyList<string>> keywords);
    }
}
