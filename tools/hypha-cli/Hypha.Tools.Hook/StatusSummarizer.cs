// ------------------------------------------------------------------------------------------------
// <copyright file="StatusSummarizer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    /// <summary>
    /// Turns <c>hypha sync</c>'s status file into one line of <c>additionalContext</c>, or nothing.
    /// </summary>
    /// <remarks>
    /// Silent by default, matching <c>hooks/check-spec-pdfs.py</c>'s tone: only the phases a user can
    /// do something about, or would otherwise mistake for a hang, get a line.
    /// </remarks>
    public static class StatusSummarizer
    {
        /// <summary>
        /// Summarizes <paramref name="status"/>, or returns <c>null</c> when there is nothing worth
        /// saying - no status yet, an unrecognised schema version, or a quiet phase
        /// (up-to-date/done/checking/pruning).
        /// </summary>
        /// <param name="logPath">Where to point a user when the phase is <c>failed</c>.</param>
        public static string? Summarize(HookSyncStatus? status, string logPath)
        {
            if (status is null || status.SchemaVersion != HookSyncStatus.KnownSchemaVersion)
            {
                return null;
            }

            var tag = status.TargetTag ?? "the newest release";

            return status.Phase switch
            {
                "fetching" when status.Fetch is { } fetch =>
                    $"Hypha: downloading {tag} sources - {fetch.Kind} {fetch.Done}/{fetch.Total} "
                    + $"({Percent(fetch.Done, fetch.Total)}%).",
                "generating" when status.Generate is { } generate =>
                    $"Hypha: generating knowledge for {tag} - {generate.Generator} "
                    + $"({generate.Index}/{generate.Total}).",
                "failed" =>
                    $"Hypha: sync for {tag} failed"
                    + (status.Error?.Message is { Length: > 0 } message ? $" ({message})" : string.Empty)
                    + $" - see {logPath} for details.",
                _ => null,
            };
        }

        private static int Percent(int done, int total) => total > 0 ? done * 100 / total : 0;
    }
}
