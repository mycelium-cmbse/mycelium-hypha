// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Migration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Reads and writes the record of what a migration left out, kept in <c>HYPHA_HOME</c> beside its
    /// marker so that the release status can say why a release a client had is not installed here.
    /// </summary>
    /// <remarks>
    /// The migration runs once, and only says on standard error what it left out, which a client may never
    /// show. The record outlives that run: <see cref="Query.IKnowledgeQuery.Releases"/> reports each entry
    /// until a version of that kind and tag is installed.
    /// </remarks>
    public static class LeftOutFile
    {
        /// <summary>The record's file name, in the root of <c>HYPHA_HOME</c>.</summary>
        public const string FileName = ".migration-left-out.json";

        /// <summary>The options the record is written and read with.</summary>
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>The source-generated contract, with <see cref="SerializerOptions"/>.</summary>
        private static readonly LeftOutJsonContext Contract = new(SerializerOptions);

        /// <summary>Gets the record's file in a home folder.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <returns>The file, whether or not it exists.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="home"/> is <c>null</c>.</exception>
        public static FileInfo In(DirectoryInfo home)
        {
            ArgumentNullException.ThrowIfNull(home);

            return new FileInfo(Path.Combine(home.FullName, FileName));
        }

        /// <summary>Reads what earlier migrations into a home folder left out.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <returns>
        /// The versions left out, or none when there is no record or it cannot be read: the record only
        /// explains, so a damaged one is not worth failing an answer for.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="home"/> is <c>null</c>.</exception>
        public static IReadOnlyList<LeftOutVersion> Read(DirectoryInfo home)
        {
            var file = In(home);
            if (!file.Exists)
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize(File.ReadAllText(file.FullName), Contract.LeftOutRecord)?.Versions ?? [];
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                return [];
            }
        }

        /// <summary>
        /// Adds what a migration left out to the record, replacing an earlier entry of the same kind and
        /// tag; writes nothing when nothing was left out.
        /// </summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="leftOut">What this migration left out.</param>
        /// <exception cref="ArgumentNullException"><paramref name="home"/> or <paramref name="leftOut"/> is <c>null</c>.</exception>
        public static void Add(DirectoryInfo home, IReadOnlyList<LeftOutVersion> leftOut)
        {
            ArgumentNullException.ThrowIfNull(home);
            ArgumentNullException.ThrowIfNull(leftOut);

            if (leftOut.Count == 0)
            {
                return;
            }

            // One entry per kind and tag: the first of this migration's, which replaces an earlier one.
            var added = leftOut.DistinctBy(version => (version.Kind, version.Tag)).ToList();
            var versions = Read(home)
                .Where(earlier => !added.Any(now => now.Kind == earlier.Kind && now.Tag == earlier.Tag))
                .Concat(added)
                .OrderBy(version => version.Kind, StringComparer.Ordinal)
                .ThenBy(version => version.Tag, StringComparer.Ordinal)
                .ToList();

            home.Create();
            ReaderSafeFile.WriteAllText(
                In(home).FullName,
                JsonSerializer.Serialize(new LeftOutRecord(versions), Contract.LeftOutRecord).Replace("\r\n", "\n") + "\n");
        }
    }
}
