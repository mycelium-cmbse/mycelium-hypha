// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>Renders and reads <c>knowledge/&lt;tag&gt;/model-library/index.json</c>.</summary>
    public static class ModelLibraryFile
    {
        /// <summary>
        /// Two-space indent, unescaped non-ASCII, property order taken from the records: the same
        /// format <c>cross-references.json</c> uses, for the same reason: these settings are the file
        /// format, so changing any of them rewrites every committed document.
        /// </summary>
        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>The source-generated contract, bound to the options above.</summary>
        private static readonly ModelLibraryJsonContext Contract = new(Format);

        /// <summary>Renders the document with LF endings, closing with a single newline.</summary>
        public static string Render(ModelLibraryDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return JsonSerializer.Serialize(document, Contract.ModelLibraryDocument)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                + "\n";
        }

        /// <summary>Reads a generated index, or returns <c>null</c> when there is none.</summary>
        /// <param name="path">The index file.</param>
        /// <returns>The document, or <c>null</c> when the file does not exist.</returns>
        /// <exception cref="JsonException">The file is not a model-library index.</exception>
        public static ModelLibraryDocument? ReadIfPresent(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return File.Exists(path)
                ? JsonSerializer.Deserialize(File.ReadAllText(path, Encoding.UTF8), Contract.ModelLibraryDocument)
                : null;
        }
    }
}
