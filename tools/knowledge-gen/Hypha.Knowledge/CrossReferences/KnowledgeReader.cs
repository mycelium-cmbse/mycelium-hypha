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
        [GeneratedRegex(@"^elements:[ \t]*\[(?<items>[^\]]*)\][ \t]*$", RegexOptions.Multiline)]
        private static partial Regex ElementsFrontMatter();

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
