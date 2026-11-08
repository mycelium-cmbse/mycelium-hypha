// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Renders the standard-library pages for one release.
    /// </summary>
    /// <remarks>
    /// Same anatomy as <c>NotationRenderer</c>'s example pages (front matter, verbatim source, a list
    /// section), but the list answers a different question: not which metaclasses a file
    /// <i>exercises</i> (that is what an example page's <c>elements</c> section is for), but which
    /// qualified names this file itself <i>declares</i>. The libraries are normative model content a
    /// user's own model specialises, not a worked example. File naming is
    /// <see cref="TextualNotation.INotationRenderer.ExampleFileName"/>, reused by the generator rather
    /// than duplicated here: same slugging, same collision semantics.
    /// </remarks>
    public sealed class ModelLibraryRenderer : IModelLibraryRenderer
    {
        /// <summary>
        /// Renders one standard-library file as a page: front matter listing the qualified names it
        /// declares, the verbatim source in a fenced block, and the declarations with their keywords.
        /// </summary>
        /// <param name="sourcePath">The file's path, starting with <c>sysml.library/</c>.</param>
        /// <param name="modelText">The file's content, embedded verbatim apart from newline normalisation.</param>
        /// <param name="declarations">The declarations found in it, in the order the file declares them.</param>
        /// <param name="language"><c>KerML</c> or <c>SysML</c>.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourcePath"/> or <paramref name="language"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelText"/> or <paramref name="declarations"/> is <c>null</c>.
        /// </exception>
        public string RenderPackage(
            string sourcePath, string modelText, IReadOnlyList<LibraryDeclaration> declarations, string language)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
            ArgumentNullException.ThrowIfNull(modelText);
            ArgumentNullException.ThrowIfNull(declarations);
            ArgumentException.ThrowIfNullOrWhiteSpace(language);

            var name = Stem(sourcePath);
            var body = modelText.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');
            var declared = declarations.Select(declaration => declaration.QualifiedName).ToList();

            var lines = new List<string>
            {
                "---",
                $"name: {name}",
                "kind: model-library-file",
                $"language: {language}",
                $"source: {sourcePath}",
                $"declares: [{string.Join(", ", declared)}]",
                "license: EPL-2.0",
                "---",
                string.Empty,
                $"# {name}",
                string.Empty,
                $"Verbatim {language} standard-library source from `{sourcePath}`"
                + " (EPL-2.0; see [NOTICE](../../../NOTICE)).",
                string.Empty,
                $"```{language.ToLowerInvariant()}",
                body,
                "```",
                string.Empty,
            };

            if (declarations.Count > 0)
            {
                lines.Add("## Declarations");
                lines.Add(string.Empty);
                lines.AddRange(declarations.Select(declaration =>
                    $"- `{declaration.QualifiedName}` — {declaration.Kind}"));
                lines.Add(string.Empty);
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Renders the per-release index page: every standard-library file, grouped by its top-level folder
        /// under <c>sysml.library/</c>, with groups and files in ordinal order.
        /// </summary>
        /// <param name="tag">The release the pages were generated for.</param>
        /// <param name="packages">The generated package pages.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="packages"/> is <c>null</c>.</exception>
        public string RenderIndex(string tag, IReadOnlyList<ModelLibraryPackageEntry> packages)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            ArgumentNullException.ThrowIfNull(packages);

            var lines = new List<string>
            {
                "---",
                $"tag: {tag}",
                "kind: model-library-index",
                $"files: {packages.Count.ToString(CultureInfo.InvariantCulture)}",
                "---",
                string.Empty,
                $"# SysML v2 / KerML standard model library — {tag}",
                string.Empty,
                "> Generated by `tools/knowledge-gen` — do not edit by hand; re-run the generator.",
                string.Empty,
                "Every standard-library file this release ships, copied verbatim, grouped by its",
                "upstream folder. See `index.json` for the qualified-name declaration index.",
                string.Empty,
            };

            foreach (var group in packages.GroupBy(TopLevelFolder).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                lines.Add($"## {group.Key}");
                lines.Add(string.Empty);
                lines.AddRange(group
                    .OrderBy(package => package.SourcePath, StringComparer.Ordinal)
                    .Select(package =>
                        $"- [{Stem(package.SourcePath)}](packages/{package.FileName}) — `{package.SourcePath}`"));
                lines.Add(string.Empty);
            }

            return string.Join("\n", lines);
        }

        /// <summary>The upstream top-level folder right under <c>sysml.library/</c>.</summary>
        /// <param name="package">The package page.</param>
        /// <returns>The folder name, or the whole source path when it has no folder.</returns>
        private static string TopLevelFolder(ModelLibraryPackageEntry package)
        {
            var segments = package.SourcePath.Split('/');

            return segments.Length > 1 ? segments[1] : package.SourcePath;
        }

        /// <summary>The model's file name without its extension.</summary>
        /// <param name="sourcePath">The file's relative path.</param>
        /// <returns>The last path segment, without its final extension.</returns>
        private static string Stem(string sourcePath)
        {
            var lastSegment = sourcePath[(sourcePath.LastIndexOf('/') + 1)..];
            var dot = lastSegment.LastIndexOf('.');

            return dot < 0 ? lastSegment : lastSegment[..dot];
        }
    }
}
