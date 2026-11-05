// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionManifest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Which API versions a checkout carries, and which one answers by default.
    /// </summary>
    /// <remarks>
    /// Written to <c>knowledge/api/versions.json</c>, apart from the language releases' manifest: the
    /// two are installed, switched and removed independently, and that manifest cannot exist without
    /// a default language release - which an API-only install does not have.
    /// </remarks>
    public sealed record ApiVersionManifest
    {
        /// <summary>The version of the manifest shape itself.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to <c>knowledge/api/</c>.</summary>
        public const string FileName = "versions.json";

        /// <summary>Gets the manifest schema version.</summary>
        [JsonPropertyName("schemaVersion")]
        public required string SchemaVersion { get; init; }

        /// <summary>Gets the API version answered from unless the user names another.</summary>
        [JsonPropertyName("default")]
        public required string Default { get; init; }

        /// <summary>Gets the installed API versions, newest first.</summary>
        [JsonPropertyName("versions")]
        public required IReadOnlyList<InstalledApiVersion> Versions { get; init; }

        /// <summary>The installed tags, newest first.</summary>
        public IReadOnlyList<string> GetTags() => this.Versions.Select(version => version.Tag).ToList();

        /// <summary>
        /// Assembles a manifest, ordering the versions newest first regardless of input order.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="defaultTag"/> is not among <paramref name="versions"/>.
        /// </exception>
        public static ApiVersionManifest Build(string defaultTag, IEnumerable<InstalledApiVersion> versions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultTag);
            ArgumentNullException.ThrowIfNull(versions);

            var ordered = versions
                .OrderByDescending(version => ReleaseTag.SortKey(version.Tag))
                .ToList();

            if (!ordered.Any(version => string.Equals(version.Tag, defaultTag, StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    $"default '{defaultTag}' is not among the installed API versions", nameof(defaultTag));
            }

            return new ApiVersionManifest
            {
                SchemaVersion = CurrentSchemaVersion,
                Default = defaultTag,
                Versions = ordered,
            };
        }
    }
}
