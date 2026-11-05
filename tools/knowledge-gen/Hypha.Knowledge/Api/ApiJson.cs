// ------------------------------------------------------------------------------------------------
// <copyright file="ApiJson.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The one JSON format the generated API documents are written in and read back with.
    /// </summary>
    /// <remarks>
    /// Two-space indent, camel-case names, unescaped non-ASCII, absent rather than <c>null</c> members,
    /// LF endings and a single closing newline. These settings are the file format: changing one
    /// rewrites every generated document.
    /// </remarks>
    public static class ApiJson
    {
        private static readonly JsonSerializerOptions Format = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>Renders <paramref name="document"/> deterministically.</summary>
        public static string Render<T>(T document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return JsonSerializer.Serialize(document, Format).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
        }

        /// <summary>Reads a document back.</summary>
        /// <exception cref="InvalidOperationException">The text is not such a document.</exception>
        public static T Read<T>(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            return JsonSerializer.Deserialize<T>(json, Format)
                   ?? throw new InvalidOperationException($"not a readable {typeof(T).Name}");
        }
    }
}
