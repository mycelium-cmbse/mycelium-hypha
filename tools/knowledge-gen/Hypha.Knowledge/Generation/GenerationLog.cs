// ------------------------------------------------------------------------------------------------
// <copyright file="GenerationLog.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The messages a generator reports, defined once so every generator says the same thing the same
    /// way, which is what lets #77 render progress without knowing which generator it is watching.
    /// </summary>
    /// <remarks>
    /// Public because the generators no longer all live in this assembly: the metamodel one is in
    /// <c>Hypha.Knowledge.Metamodel</c>, and it should report exactly as the others do.
    /// </remarks>
    public static class GenerationLog
    {
        /// <summary>The compiled <c>Generating {Artifact} for {Tag}</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, string, Exception?> StartedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(1, nameof(Started)), "Generating {Artifact} for {Tag}");

        /// <summary>The compiled <c>Generated {Artifact} for {Tag}: {Files} files</c> message (event 2, information).</summary>
        private static readonly Action<ILogger, string, string, int, Exception?> FinishedMessage =
            LoggerMessage.Define<string, string, int>(
                LogLevel.Information,
                new EventId(2, nameof(Finished)),
                "Generated {Artifact} for {Tag}: {Files} files");

        /// <summary>The compiled <c>Skipped {Artifact} for {Tag}: {Reason}</c> message (event 3, information).</summary>
        private static readonly Action<ILogger, string, string, string, Exception?> SkippedMessage =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Information,
                new EventId(3, nameof(Skipped)),
                "Skipped {Artifact} for {Tag}: {Reason}");

        /// <summary>
        /// The compiled warning that the cross-references are generated without clause-title edges (event 4,
        /// warning).
        /// </summary>
        private static readonly Action<ILogger, string, string, Exception?> DegradedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(4, nameof(Degraded)),
                "Generating {Artifact} for {Tag} without clause-title edges: no specification clause "
                + "catalog and no previous document to carry matches from");

        /// <summary>Work is about to begin.</summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="artifact">The artifact being generated, e.g. <c>metamodel</c>.</param>
        /// <param name="tag">The release it is generated for.</param>
        public static void Started(ILogger logger, string artifact, string tag) =>
            StartedMessage(logger, artifact, tag, null);

        /// <summary>The artifact was written.</summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="artifact">The artifact that was generated.</param>
        /// <param name="tag">The release it was generated for.</param>
        /// <param name="files">How many files were written.</param>
        public static void Finished(ILogger logger, string artifact, string tag, int files) =>
            FinishedMessage(logger, artifact, tag, files, null);

        /// <summary>Nothing was written, and why.</summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="artifact">The artifact that was skipped.</param>
        /// <param name="tag">The release it was skipped for.</param>
        /// <param name="reason">Why nothing was written.</param>
        public static void Skipped(ILogger logger, string artifact, string tag, string reason) =>
            SkippedMessage(logger, artifact, tag, reason, null);

        /// <summary>The artifact is about to be written with less than its full quality.</summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="artifact">The artifact being generated.</param>
        /// <param name="tag">The release it is generated for.</param>
        public static void Degraded(ILogger logger, string artifact, string tag) =>
            DegradedMessage(logger, artifact, tag, null);

        /// <summary>Reports whichever outcome a result carries.</summary>
        /// <param name="result">The result to report.</param>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="artifact">The artifact the result is for.</param>
        /// <param name="tag">The release the result is for.</param>
        /// <returns><paramref name="result"/>, unchanged, so a generator can report and return in one expression.</returns>
        public static GenerationResult Report(
            this GenerationResult result, ILogger logger, string artifact, string tag)
        {
            if (result.Outcome == GenerationOutcome.Skipped)
            {
                Skipped(logger, artifact, tag, result.Reason ?? "no reason given");
            }
            else
            {
                Finished(logger, artifact, tag, result.Written.Count);
            }

            return result;
        }
    }
}
