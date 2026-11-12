// ------------------------------------------------------------------------------------------------
// <copyright file="VersionManifestFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Reads and writes <c>knowledge/versions.json</c>.
    /// </summary>
    /// <remarks>
    /// Output is deterministic: fixed serializer options, LF endings independent of the host
    /// platform, a trailing newline, and UTF-8 without a byte-order mark.
    /// </remarks>
    public static class VersionManifestFile
    {
        /// <summary>
        /// The fixed serializer options: indented, camel-case property names for the nested records too, and
        /// relaxed escaping so the output stays readable.
        /// </summary>
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,

            // Applies to the nested records too. Without it they would round-trip as "Tag" and
            // "Repo", which neither matches the committed file nor binds when reading it back.
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>The source-generated contract, bound to the options above.</summary>
        private static readonly VersionManifestJsonContext Contract = new(SerializerOptions);

        /// <summary>Renders the manifest deterministically.</summary>
        public static string Render(VersionManifest manifest)
        {
            ArgumentNullException.ThrowIfNull(manifest);

            return JsonSerializer.Serialize(manifest, Contract.VersionManifest).Replace("\r\n", "\n") + "\n";
        }

        /// <summary>Writes the manifest as UTF-8 without a BOM, with LF endings.</summary>
        public static FileInfo Write(VersionManifest manifest, string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var file = new FileInfo(path);

            ReaderSafeFile.WriteAllText(file.FullName, Render(manifest));

            return file;
        }

        /// <summary>Reads a manifest back.</summary>
        /// <exception cref="InvalidOperationException">The file is not a readable manifest.</exception>
        public static VersionManifest Read(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var content = File.ReadAllText(path);

            return JsonSerializer.Deserialize(content, Contract.VersionManifest)
                   ?? throw new InvalidOperationException($"not a readable version manifest: {path}");
        }

        /// <summary>Reads a manifest if it exists, otherwise <c>null</c>.</summary>
        public static VersionManifest? ReadIfPresent(string path) =>
            File.Exists(path) ? Read(path) : null;
    }
}
