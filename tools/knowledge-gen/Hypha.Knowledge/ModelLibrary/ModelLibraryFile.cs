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
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>Renders <c>knowledge/&lt;tag&gt;/model-library/index.json</c>.</summary>
    public static class ModelLibraryFile
    {
        /// <summary>
        /// Two-space indent, unescaped non-ASCII, property order taken from the records - the same
        /// format <c>cross-references.json</c> uses, for the same reason: these settings are the file
        /// format, so changing any of them rewrites every committed document.
        /// </summary>
        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>Renders the document with LF endings, closing with a single newline.</summary>
        public static string Render(ModelLibraryDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return JsonSerializer.Serialize(document, Format).Replace("\r\n", "\n", StringComparison.Ordinal)
                + "\n";
        }
    }
}
