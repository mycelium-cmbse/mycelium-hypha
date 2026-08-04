// ------------------------------------------------------------------------------------------------
// <copyright file="VersionManifest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The committed record of which releases a checkout carries, and which one answers by default.
    /// </summary>
    /// <remarks>
    /// Written to <c>knowledge/versions.json</c>. The <b>tag</b> is the identifier throughout: the
    /// model URI inside the XMI is deliberately absent, because it tracks neither the release nor the
    /// content — the 2026-05 metamodel still declares a 2025 URI.
    /// </remarks>
    public sealed record VersionManifest
    {
        /// <summary>The version of the manifest shape itself.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to the knowledge root.</summary>
        public const string FileName = "versions.json";

        /// <summary>Gets the manifest schema version.</summary>
        [JsonPropertyName("schemaVersion")]
        public required string SchemaVersion { get; init; }

        /// <summary>Gets the tag answered from unless the user names another.</summary>
        [JsonPropertyName("default")]
        public required string Default { get; init; }

        /// <summary>Gets the installed releases, newest first.</summary>
        [JsonPropertyName("versions")]
        public required IReadOnlyList<InstalledVersion> Versions { get; init; }

        /// <summary>
        /// The installed tags, newest first.
        /// </summary>
        /// <remarks>
        /// A method rather than a property: it projects a new list on each call, and callers iterate
        /// the releases in loops. Caching it on the record would go stale the moment a <c>with</c>
        /// expression replaced <see cref="Versions"/>.
        /// </remarks>
        public IReadOnlyList<string> GetTags() =>
            this.Versions.Select(version => version.Tag).ToList();

        /// <summary>
        /// Assembles a manifest, ordering the releases newest first regardless of input order.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="defaultTag"/> is not among <paramref name="versions"/>. A default that is
        /// not installed would leave every skill pointing at a directory that does not exist.
        /// </exception>
        public static VersionManifest Build(string defaultTag, IEnumerable<InstalledVersion> versions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultTag);
            ArgumentNullException.ThrowIfNull(versions);

            var ordered = versions
                .OrderByDescending(version => ReleaseTag.SortKey(version.Tag))
                .ToList();

            if (!ordered.Any(version => string.Equals(version.Tag, defaultTag, StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    $"default '{defaultTag}' is not among the installed versions", nameof(defaultTag));
            }

            return new VersionManifest
            {
                SchemaVersion = CurrentSchemaVersion,
                Default = defaultTag,
                Versions = ordered,
            };
        }
    }
}
