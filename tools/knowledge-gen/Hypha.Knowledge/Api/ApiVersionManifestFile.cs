// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionManifestFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>
    /// Reads and writes <c>knowledge/api/versions.json</c>, deterministically: fixed serializer
    /// options, LF endings, a trailing newline, and UTF-8 without a byte-order mark.
    /// </summary>
    public static class ApiVersionManifestFile
    {
        /// <summary>The fixed serializer options the manifest is written and read with.</summary>
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>UTF-8 without a byte-order mark, the encoding the manifest is written in.</summary>
        private static readonly UTF8Encoding Utf8WithoutBom = new(false);

        /// <summary>Renders the manifest deterministically.</summary>
        /// <param name="manifest">The manifest to render.</param>
        /// <returns>The JSON text, with LF endings and a trailing newline.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="manifest"/> is <c>null</c>.</exception>
        public static string Render(ApiVersionManifest manifest)
        {
            ArgumentNullException.ThrowIfNull(manifest);

            return JsonSerializer.Serialize(manifest, SerializerOptions).Replace("\r\n", "\n") + "\n";
        }

        /// <summary>Writes the manifest to <paramref name="path"/>, creating its folder.</summary>
        /// <param name="manifest">The manifest to write.</param>
        /// <param name="path">The file to write it to; an existing file is overwritten.</param>
        /// <returns>The file written.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="manifest"/> is <c>null</c>.</exception>
        public static FileInfo Write(ApiVersionManifest manifest, string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var file = new FileInfo(path);
            file.Directory?.Create();
            File.WriteAllText(file.FullName, Render(manifest), Utf8WithoutBom);

            return file;
        }

        /// <summary>Reads a manifest if it exists, otherwise <c>null</c>.</summary>
        /// <param name="path">The manifest file.</param>
        /// <returns>The manifest, or <c>null</c> when <paramref name="path"/> does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">The file is not a readable manifest.</exception>
        public static ApiVersionManifest? ReadIfPresent(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ApiVersionManifest>(File.ReadAllText(path), SerializerOptions)
                   ?? throw new InvalidOperationException($"not a readable API version manifest: {path}");
        }
    }
}
