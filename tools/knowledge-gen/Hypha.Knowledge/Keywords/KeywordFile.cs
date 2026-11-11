// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>Renders <c>knowledge/&lt;tag&gt;/textual-notation/keywords.json</c>.</summary>
    public static class KeywordFile
    {
        /// <summary>Two-space indent, unescaped non-ASCII, property order taken from the records.</summary>
        /// <remarks>
        /// These settings are the file format, so they are fixed here rather than passed in. The relaxed
        /// encoder keeps an operator such as <c>=&gt;</c> readable instead of writing it as <c>=\u003E</c>.
        /// </remarks>
        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>The source-generated contract, bound to the format above.</summary>
        private static readonly KeywordJsonContext Contract = new(Format);

        /// <summary>Renders the document with LF endings, closing with a single newline.</summary>
        /// <param name="document">The document.</param>
        /// <returns>The file's content.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <see cref="JsonSerializer"/> indents with <see cref="Environment.NewLine"/>, so on Windows it
        /// would otherwise emit CRLF in a file <c>.gitattributes</c> pins to LF.
        /// </remarks>
        public static string Render(KeywordDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return JsonSerializer.Serialize(document, Contract.KeywordDocument)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                + "\n";
        }
    }
}
