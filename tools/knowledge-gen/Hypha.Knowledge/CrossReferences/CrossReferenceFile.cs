// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceFile.cs" company="Starion Group S.A.">
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
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>
    /// Reads and writes <c>knowledge/&lt;tag&gt;/cross-references.json</c>.
    /// </summary>
    public static class CrossReferenceFile
    {
        /// <summary>UTF-8 without a BOM; the knowledge base is pinned to LF by <c>.gitattributes</c>.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Two-space indent, unescaped non-ASCII, property order taken from the records.
        /// </summary>
        /// <remarks>
        /// These settings are the file format. Changing any of them rewrites every committed
        /// cross-reference document, so they are fixed here rather than passed in.
        /// </remarks>
        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>Renders the document with LF endings, closing with a single newline.</summary>
        /// <remarks>
        /// The normalisation is not decorative: <see cref="JsonSerializer"/> indents with
        /// <see cref="Environment.NewLine"/>, so on Windows it would otherwise emit CRLF and rewrite
        /// every line of a file <c>.gitattributes</c> pins to LF.
        /// </remarks>
        public static string Render(CrossReferenceDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return JsonSerializer.Serialize(document, Format).Replace("\r\n", "\n", StringComparison.Ordinal)
                + "\n";
        }

        /// <summary>Writes the rendered document, creating the directory if it is missing.</summary>
        public static FileInfo Write(CrossReferenceDocument document, string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var file = new FileInfo(path);
            file.Directory?.Create();

            File.WriteAllText(file.FullName, Render(document), Utf8NoBom);
            file.Refresh();

            return file;
        }

        /// <summary>Reads a previously generated document, or <c>null</c> when there is none.</summary>
        public static CrossReferenceDocument? ReadIfPresent(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return File.Exists(path)
                ? JsonSerializer.Deserialize<CrossReferenceDocument>(
                    File.ReadAllText(path, Encoding.UTF8), Format)
                : null;
        }

        /// <summary>
        /// The title-matched clause edges a previous run recorded, per element.
        /// </summary>
        /// <remarks>
        /// This is what lets the file be rebuilt without the OMG PDFs. Only <c>exact-title-match</c>
        /// edges are recovered: the grammar-stated ones are recomputed from the committed
        /// <c>.kebnf</c> on every run, and a clause found <b>both</b> ways is recorded as stated, so
        /// re-merging reproduces it exactly.
        /// </remarks>
        public static IReadOnlyDictionary<string, IReadOnlyList<ClauseEdge>> TitleMatchedEdges(
            CrossReferenceDocument? document)
        {
            var recovered = new Dictionary<string, IReadOnlyList<ClauseEdge>>(StringComparer.Ordinal);

            if (document is null)
            {
                return recovered;
            }

            foreach (var (element, entry) in document.Entries)
            {
                var titleMatched = entry.Clauses
                    .Where(edge => string.Equals(
                        edge.Method, CrossReferenceBuilder.ExactTitleMatch, StringComparison.Ordinal))
                    .ToList();

                if (titleMatched.Count > 0)
                {
                    recovered[element] = titleMatched;
                }
            }

            return recovered;
        }
    }
}
