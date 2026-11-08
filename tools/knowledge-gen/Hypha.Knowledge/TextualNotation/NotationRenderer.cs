// ------------------------------------------------------------------------------------------------
// <copyright file="NotationRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Renders the textual-notation pages for one release.
    /// </summary>
    /// <remarks>
    /// Nothing here is hand-written: every model shipped at the tag becomes a page, the keywords come
    /// from that tag's grammar, and the element links are derived from the metamodel naming
    /// convention. The models differ between releases, so this is generated per tag like everything
    /// else.
    /// </remarks>
    public sealed partial class NotationRenderer : INotationRenderer
    {
        /// <summary>Matches a run of characters that cannot appear in a slug.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex("[^a-z0-9]+")]
        private static partial Regex NonSlug();

        /// <summary>
        /// A deterministic, flat file name for a model's example page: the relative path without its
        /// extension, lowercased, with every run of characters other than <c>a-z</c> and <c>0-9</c> replaced
        /// by <c>-</c>. Two paths can slugify alike (<c>a b.sysml</c> and <c>a-b.sysml</c>); the textual
        /// notation generator refuses such a release rather than overwrite one page with the other.
        /// </summary>
        /// <param name="sourcePath">The model's path relative to the textual sources, with <c>/</c> separators.</param>
        /// <returns>The page's file name, ending in <c>.md</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="sourcePath"/> is <c>null</c>, empty or white space.</exception>
        /// <remarks>
        /// The sources are nested directories with spaces and dots in their names, and the pages sit
        /// in one flat folder, so the whole relative path is slugified rather than just the file name:
        /// two models called <c>Example.sysml</c> in different folders must not collide.
        /// </remarks>
        public string ExampleFileName(string sourcePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

            var withoutExtension = StripExtension(sourcePath);

            return NonSlug().Replace(withoutExtension.ToLowerInvariant(), "-").Trim('-') + ".md";
        }

        /// <summary>
        /// Renders one model as a page: front matter, the verbatim source in a fenced block, and a link to
        /// each element it declares.
        /// </summary>
        /// <param name="sourcePath">The model's path relative to the textual sources.</param>
        /// <param name="modelText">The model, embedded verbatim apart from newline normalisation.</param>
        /// <param name="elements">The metaclasses the model declares.</param>
        /// <param name="language"><c>KerML</c> or <c>SysML</c>.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourcePath"/> or <paramref name="language"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelText"/> or <paramref name="elements"/> is <c>null</c>.
        /// </exception>
        public string RenderExample(
            string sourcePath, string modelText, IReadOnlyList<string> elements, string language)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
            ArgumentNullException.ThrowIfNull(modelText);
            ArgumentNullException.ThrowIfNull(elements);
            ArgumentException.ThrowIfNullOrWhiteSpace(language);

            var name = Stem(sourcePath);
            var body = modelText.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');

            var lines = new List<string>
            {
                "---",
                $"name: {name}",
                "kind: example",
                $"language: {language}",
                $"source: {sourcePath}",
                $"elements: [{string.Join(", ", elements)}]",
                "license: EPL-2.0",
                "---",
                string.Empty,
                $"# {name}",
                string.Empty,
                $"Verbatim {language} model from `{sourcePath}`"
                + " (EPL-2.0; see [NOTICE](../../../NOTICE)).",
                string.Empty,
                $"```{language.ToLowerInvariant()}",
                body,
                "```",
                string.Empty,
            };

            if (elements.Count > 0)
            {
                lines.Add("## Elements");
                lines.Add(string.Empty);
                lines.AddRange(elements.Select(element =>
                    $"- [{element}](../metamodel/elements/{element}.md)"));
                lines.Add(string.Empty);
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Renders the per-release index: each grammar's reserved keywords, in grammar-name order, then a link
        /// to every generated example.
        /// </summary>
        /// <param name="tag">The release these pages were generated for.</param>
        /// <param name="examples">The generated pages, in the order they should be listed.</param>
        /// <param name="keywords">Each grammar's reserved keywords, keyed by grammar name.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="examples"/> or <paramref name="keywords"/> is <c>null</c>.
        /// </exception>
        public string RenderIndex(
            string tag,
            IReadOnlyList<ExampleEntry> examples,
            IReadOnlyDictionary<string, IReadOnlyList<string>> keywords)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            ArgumentNullException.ThrowIfNull(examples);
            ArgumentNullException.ThrowIfNull(keywords);

            var lines = new List<string>
            {
                "---",
                $"tag: {tag}",
                "kind: textual-notation-index",
                $"examples: {examples.Count.ToString(CultureInfo.InvariantCulture)}",
                "---",
                string.Empty,
                $"# SysML v2 / KerML textual notation — {tag}",
                string.Empty,
                "> Generated by `tools/knowledge-gen` — do not edit by hand; re-run the generator.",
                string.Empty,
                "Every model shipped with this release, copied verbatim, plus the reserved keywords read",
                "from this release's grammar. Element links are derived from the metamodel naming",
                "convention (`PartDefinition` → `part def`, `PartUsage` → `part`).",
                string.Empty,
            };

            foreach (var grammar in keywords.Keys.Order(StringComparer.Ordinal))
            {
                var words = keywords[grammar];

                lines.Add($"## {grammar} keywords ({words.Count.ToString(CultureInfo.InvariantCulture)})");
                lines.Add(string.Empty);
                lines.Add("`" + string.Join("`, `", words) + "`");
                lines.Add(string.Empty);
            }

            lines.Add("## Examples");
            lines.Add(string.Empty);
            lines.AddRange(examples.Select(example =>
                $"- [{Stem(example.SourcePath)}](examples/{example.FileName}) — `{example.SourcePath}`"));
            lines.Add(string.Empty);

            return string.Join("\n", lines);
        }

        /// <summary>The model's file name without its extension.</summary>
        /// <param name="sourcePath">The model's relative path.</param>
        /// <returns>The last path segment, without its final extension.</returns>
        private static string Stem(string sourcePath)
        {
            var lastSegment = sourcePath[(sourcePath.LastIndexOf('/') + 1)..];
            var dot = lastSegment.LastIndexOf('.');

            return dot < 0 ? lastSegment : lastSegment[..dot];
        }

        /// <summary>The path with only the final extension removed, separators intact.</summary>
        /// <param name="sourcePath">The model's relative path.</param>
        /// <returns>The path without the extension of its last segment.</returns>
        private static string StripExtension(string sourcePath)
        {
            var dot = sourcePath.LastIndexOf('.');
            var separator = sourcePath.LastIndexOf('/');

            return dot > separator ? sourcePath[..dot] : sourcePath;
        }
    }
}
