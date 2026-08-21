// ------------------------------------------------------------------------------------------------
// <copyright file="HookSyncStatus.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The parts of <c>hypha sync</c>'s status file this hook reads, to summarize into
    /// <c>additionalContext</c>.
    /// </summary>
    /// <remarks>
    /// Deliberately its own type rather than a shared reference to <c>Hypha.Tools.Sync.SyncStatus</c>:
    /// this project must stay free of any dependency on <c>Hypha.Tools</c> (which pulls in
    /// Handlebars.Net/uml4net and is not NativeAOT-safe), so the JSON shape is duplicated on purpose.
    /// The two are kept in sync by convention and by <see cref="SchemaVersion"/> - an unrecognised
    /// version is treated as "nothing to say" rather than guessed at.
    /// </remarks>
    public sealed record HookSyncStatus(
        [property: JsonPropertyName("schemaVersion")] string? SchemaVersion,
        [property: JsonPropertyName("phase")] string? Phase,
        [property: JsonPropertyName("targetTag")] string? TargetTag,
        [property: JsonPropertyName("fetch")] HookFetchProgress? Fetch,
        [property: JsonPropertyName("generate")] HookGenerateProgress? Generate,
        [property: JsonPropertyName("error")] HookSyncError? Error)
    {
        /// <summary>The only schema version this hook knows how to summarize.</summary>
        public const string KnownSchemaVersion = "1.0.0";
    }

    /// <summary>Fetch progress, as reported in the status file.</summary>
    public sealed record HookFetchProgress(
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("done")] int Done,
        [property: JsonPropertyName("total")] int Total);

    /// <summary>Generation progress, as reported in the status file.</summary>
    public sealed record HookGenerateProgress(
        [property: JsonPropertyName("generator")] string? Generator,
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("total")] int Total);

    /// <summary>Why a run failed, as reported in the status file.</summary>
    public sealed record HookSyncError(
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("message")] string? Message);
}
