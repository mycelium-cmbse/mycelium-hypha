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
        /// <summary>
        /// A deterministic, flat file name for a model's example page. Two paths can slugify to the same
        /// name; callers that write the pages must refuse that rather than overwrite one with the other.
        /// </summary>
        /// <param name="sourcePath">The model's path relative to the textual sources, with <c>/</c> separators.</param>
        /// <returns>The page's file name, ending in <c>.md</c>.</returns>
        string ExampleFileName(string sourcePath);

        /// <summary>Renders one model as a page: front matter, the verbatim source, and its elements.</summary>
        /// <param name="sourcePath">The model's path relative to the textual sources.</param>
        /// <param name="modelText">The model's content.</param>
        /// <param name="elements">The metaclasses the model declares.</param>
        /// <param name="language"><c>KerML</c> or <c>SysML</c>.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string RenderExample(
            string sourcePath, string modelText, IReadOnlyList<string> elements, string language);

        /// <summary>Renders the per-release index: the keyword reference and every generated example.</summary>
        /// <param name="tag">The release the pages were generated for.</param>
        /// <param name="examples">The generated pages, in the order they should be listed.</param>
        /// <param name="keywords">Each grammar's reserved keywords, keyed by grammar name.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string RenderIndex(
            string tag,
            IReadOnlyList<ExampleEntry> examples,
            IReadOnlyDictionary<string, IReadOnlyList<string>> keywords);
    }
}
