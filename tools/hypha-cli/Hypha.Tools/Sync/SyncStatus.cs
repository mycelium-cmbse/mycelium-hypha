// ------------------------------------------------------------------------------------------------
// <copyright file="SyncStatus.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// What <c>hypha sync</c> is doing, or last did - written to disk so the hook that launched it,
    /// detached, can report progress on a later session without waiting for it.
    /// </summary>
    public sealed record SyncStatus
    {
        /// <summary>The shape of this file.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>Gets the schema version.</summary>
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; init; } = CurrentSchemaVersion;

        /// <summary>Gets the process id that wrote this status, for lock staleness checks.</summary>
        [JsonPropertyName("pid")]
        public int Pid { get; init; }

        /// <summary>Gets when this run started.</summary>
        [JsonPropertyName("startedAt")]
        public DateTimeOffset StartedAt { get; init; }

        /// <summary>Gets when this status was last written.</summary>
        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>Gets when this run finished, successfully or not.</summary>
        [JsonPropertyName("finishedAt")]
        public DateTimeOffset? FinishedAt { get; init; }

        /// <summary>Gets which step this run is on, or ended on.</summary>
        [JsonPropertyName("phase")]
        public required string Phase { get; init; }

        /// <summary>Gets the release this run is fetching/generating, once known.</summary>
        [JsonPropertyName("targetTag")]
        public string? TargetTag { get; init; }

        /// <summary>Gets the fetch progress, while <see cref="Phase"/> is fetching.</summary>
        [JsonPropertyName("fetch")]
        public SyncFetchProgress? Fetch { get; init; }

        /// <summary>Gets the generation progress, while <see cref="Phase"/> is generating.</summary>
        [JsonPropertyName("generate")]
        public SyncGenerateProgress? Generate { get; init; }

        /// <summary>Gets why this run failed, or <c>null</c> when it did not.</summary>
        [JsonPropertyName("error")]
        public SyncError? Error { get; init; }

        /// <summary>
        /// Gets the releases that were already installed the first time <c>sync</c> ever ran here.
        /// </summary>
        /// <remarks>
        /// Captured exactly once and carried forward unchanged on every later run, even as
        /// <see cref="Sync.SyncStatus"/> itself is rewritten many times over - see
        /// <c>SyncCommand</c>. Pruning only ever touches <see cref="LocallyAddedTag"/>, never a tag in
        /// this list, so the two committed releases a plugin ships with can never be deleted by
        /// automation.
        /// </remarks>
        [JsonPropertyName("committedBaselineTags")]
        public IReadOnlyList<string>? CommittedBaselineTags { get; init; }

        /// <summary>Gets the one release <c>sync</c> itself most recently added, if any.</summary>
        [JsonPropertyName("locallyAddedTag")]
        public string? LocallyAddedTag { get; init; }
    }

    /// <summary>Fetch progress for one release, across the metamodel and textual inputs.</summary>
    /// <param name="Kind">Which input is downloading, e.g. <c>metamodel</c> or <c>textual</c>.</param>
    /// <param name="Done">How many of <paramref name="Total"/> have completed.</param>
    /// <param name="Total">The full count for <paramref name="Kind"/>.</param>
    public sealed record SyncFetchProgress(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("done")] int Done,
        [property: JsonPropertyName("total")] int Total);

    /// <summary>Generation progress for one release, across the registered generators.</summary>
    /// <param name="Generator">The generator that just ran, e.g. <c>cross-references</c>.</param>
    /// <param name="Index">Its one-based position among <paramref name="Total"/>.</param>
    /// <param name="Total">How many generators run in total.</param>
    public sealed record SyncGenerateProgress(
        [property: JsonPropertyName("generator")] string Generator,
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("total")] int Total);

    /// <summary>Why a <c>sync</c> run failed.</summary>
    /// <param name="Kind">A coarse category: <c>network</c>, <c>disk</c> or <c>unexpected</c>.</param>
    /// <param name="Message">A sentence a user can read, not a stack trace.</param>
    public sealed record SyncError(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("message")] string Message);

    /// <summary>The phases a <c>sync</c> run passes through, in order.</summary>
    public static class SyncPhase
    {
        /// <summary>Working out whether anything needs to happen at all.</summary>
        public const string Checking = "checking";

        /// <summary>Nothing to do: the newest offerable release is already installed.</summary>
        public const string UpToDate = "up-to-date";

        /// <summary>Downloading a release's inputs.</summary>
        public const string Fetching = "fetching";

        /// <summary>Generating the knowledge base for a release.</summary>
        public const string Generating = "generating";

        /// <summary>Removing the previous locally-added release, now that a new one has replaced it.</summary>
        public const string Pruning = "pruning";

        /// <summary>The run finished successfully.</summary>
        public const string Done = "done";

        /// <summary>The run failed; see <see cref="SyncStatus.Error"/>.</summary>
        public const string Failed = "failed";
    }
}
