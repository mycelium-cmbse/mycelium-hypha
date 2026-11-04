// ------------------------------------------------------------------------------------------------
// <copyright file="LookupResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The outcome of a lookup: one page of the matched entry's generated source, or the names to
    /// choose from.
    /// </summary>
    /// <remarks>
    /// This is the <c>hypha lookup --json</c> contract. Renaming a property is a breaking change for
    /// every host that reads it. The page properties are only present when <see cref="Status"/> is
    /// <see cref="LookupStatus.Found"/>.
    /// </remarks>
    public sealed record LookupResult
    {
        /// <summary>The release the answer comes from.</summary>
        [JsonPropertyName("release")]
        public required string Release { get; init; }

        /// <summary>The index that was searched.</summary>
        [JsonPropertyName("kind")]
        public required KnowledgeKind Kind { get; init; }

        /// <summary>How the name resolved.</summary>
        [JsonPropertyName("status")]
        public required LookupStatus Status { get; init; }

        /// <summary>The index key of the matched entry.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>The absolute path of the generated page the content comes from.</summary>
        [JsonPropertyName("path")]
        public string? Path { get; init; }

        /// <summary>
        /// The line the declaration starts on, for a model-library entry whose declaration could be
        /// located in its package page.
        /// </summary>
        [JsonPropertyName("declarationLine")]
        public int? DeclarationLine { get; init; }

        /// <summary>The first line returned, 1-based.</summary>
        [JsonPropertyName("startLine")]
        public int? StartLine { get; init; }

        /// <summary>The last line returned, 1-based and inclusive.</summary>
        [JsonPropertyName("endLine")]
        public int? EndLine { get; init; }

        /// <summary>The number of lines in the page.</summary>
        [JsonPropertyName("totalLines")]
        public int? TotalLines { get; init; }

        /// <summary>The line to ask for next, or <c>null</c> when the page has been read to its end.</summary>
        [JsonPropertyName("nextLine")]
        public int? NextLine { get; init; }

        /// <summary>The returned lines, joined with <c>\n</c>.</summary>
        [JsonPropertyName("content")]
        public string? Content { get; init; }

        /// <summary>
        /// The names to choose from when the status is <see cref="LookupStatus.Ambiguous"/> or
        /// <see cref="LookupStatus.Candidates"/>, sorted ordinally and capped at
        /// <see cref="KnowledgeLookup.MaxMatches"/>.
        /// </summary>
        [JsonPropertyName("matches")]
        public IReadOnlyList<string> Matches { get; init; } = [];

        /// <summary>How many names matched before the cap.</summary>
        [JsonPropertyName("totalMatches")]
        public int TotalMatches { get; init; }
    }
}
