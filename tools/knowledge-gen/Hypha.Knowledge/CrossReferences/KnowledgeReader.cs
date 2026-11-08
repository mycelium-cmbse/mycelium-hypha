// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Reads the generated knowledge trees from disk.
    /// </summary>
    /// <remarks>
    /// The only part of the cross-reference layer that touches the file system; everything the
    /// builder does is a pure function of what this returns.
    /// </remarks>
    public sealed partial class KnowledgeReader : IKnowledgeReader
    {
        /// <summary>
        /// Matches an example page's <c>elements: [...]</c> front-matter line, capturing the list's
        /// <c>items</c>.
        /// </summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^elements:[ \t]*\[(?<items>[^\]]*)\][ \t]*$", RegexOptions.Multiline)]
        private static partial Regex ElementsFrontMatter();

        /// <summary>
        /// Reads a release's metamodel index: the name, <c>kind</c> and <c>file</c> of every entry under
        /// <c>entries</c>, and the index's <c>modelVersionUri</c>.
        /// </summary>
        /// <param name="index">The release's <c>metamodel/index.json</c>.</param>
        /// <returns>The elements, in index order, and the model URI.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="index"/> is <c>null</c>.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="index"/> does not exist.</exception>
        /// <exception cref="JsonException"><paramref name="index"/> is not valid JSON.</exception>
        /// <exception cref="KeyNotFoundException">The index lacks a property it is read for.</exception>
        public (IReadOnlyList<ElementRecord> Elements, string ModelVersionUri) ReadElements(FileInfo index)
        {
            ArgumentNullException.ThrowIfNull(index);

            using var document = JsonDocument.Parse(File.ReadAllText(index.FullName, Encoding.UTF8));
            var root = document.RootElement;

            var elements = root.GetProperty("entries")
                .EnumerateObject()
                .Select(entry => new ElementRecord(
                    entry.Name,
                    entry.Value.GetProperty("kind").GetString()!,
                    entry.Value.GetProperty("file").GetString()!))
                .ToList();

            return (elements, root.GetProperty("modelVersionUri").GetString()!);
        }

        /// <summary>
        /// Reads a specification clause catalog: the <c>title</c> of every entry under <c>entries</c>, by
        /// clause number, and the catalog's <c>document</c> and <c>version</c>.
        /// </summary>
        /// <param name="index">A specification's <c>spec/&lt;document&gt;/index.json</c>.</param>
        /// <returns>The clause titles, keyed ordinally, and the document they belong to.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="index"/> is <c>null</c>.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="index"/> does not exist.</exception>
        /// <exception cref="JsonException"><paramref name="index"/> is not valid JSON.</exception>
        /// <exception cref="KeyNotFoundException">The catalog lacks a property it is read for.</exception>
        public (IReadOnlyDictionary<string, string> Titles, SpecificationDocument Document) ReadClauseTitles(
            FileInfo index)
        {
            ArgumentNullException.ThrowIfNull(index);

            using var document = JsonDocument.Parse(File.ReadAllText(index.FullName, Encoding.UTF8));
            var root = document.RootElement;

            var titles = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var entry in root.GetProperty("entries").EnumerateObject())
            {
                titles[entry.Name] = entry.Value.GetProperty("title").GetString()!;
            }

            return (
                titles,
                new SpecificationDocument(
                    root.GetProperty("document").GetString()!,
                    root.GetProperty("version").GetString()!));
        }

        /// <summary>
        /// Reads the <c>elements:</c> front matter of every <c>*.md</c> page in <paramref name="examples"/>,
        /// keyed by <paramref name="prefix"/> plus the page's file name.
        /// </summary>
        /// <param name="examples">The release's <c>textual-notation/examples</c> folder.</param>
        /// <param name="prefix">Prepended to each page's file name to form its key.</param>
        /// <returns>
        /// The metaclasses each example declares, unquoted, keyed ordinally by page path. Empty when the
        /// folder does not exist; a page without the front-matter line is left out.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="examples"/> or <paramref name="prefix"/> is <c>null</c>.
        /// </exception>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> ReadExamples(
            DirectoryInfo examples, string prefix)
        {
            ArgumentNullException.ThrowIfNull(examples);
            ArgumentNullException.ThrowIfNull(prefix);

            var found = new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            if (!examples.Exists)
            {
                return found;
            }

            foreach (var page in examples.EnumerateFiles("*.md"))
            {
                var match = ElementsFrontMatter().Match(File.ReadAllText(page.FullName, Encoding.UTF8));

                if (!match.Success)
                {
                    continue;
                }

                found[prefix + page.Name] = match.Groups["items"].Value
                    .Split(',')
                    .Select(item => item.Trim().Trim('"', '\''))
                    .Where(item => item.Length > 0)
                    .ToList();
            }

            return found;
        }
    }
}
