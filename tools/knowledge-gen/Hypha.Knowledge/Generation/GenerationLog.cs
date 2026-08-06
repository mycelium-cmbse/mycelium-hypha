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
    /// way - which is what lets #77 render progress without knowing which generator it is watching.
    /// </summary>
    /// <remarks>
    /// Public because the generators no longer all live in this assembly: the metamodel one is in
    /// <c>Hypha.MetamodelGen</c>, and it should report exactly as the others do.
    /// </remarks>
    public static class GenerationLog
    {
        private static readonly Action<ILogger, string, string, Exception?> StartedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(1, nameof(Started)), "Generating {Artifact} for {Tag}");

        private static readonly Action<ILogger, string, string, int, Exception?> FinishedMessage =
            LoggerMessage.Define<string, string, int>(
                LogLevel.Information,
                new EventId(2, nameof(Finished)),
                "Generated {Artifact} for {Tag}: {Files} files");

        private static readonly Action<ILogger, string, string, string, Exception?> SkippedMessage =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Information,
                new EventId(3, nameof(Skipped)),
                "Skipped {Artifact} for {Tag}: {Reason}");

        /// <summary>Work is about to begin.</summary>
        public static void Started(ILogger logger, string artifact, string tag) =>
            StartedMessage(logger, artifact, tag, null);

        /// <summary>The artifact was written.</summary>
        public static void Finished(ILogger logger, string artifact, string tag, int files) =>
            FinishedMessage(logger, artifact, tag, files, null);

        /// <summary>Nothing was written, and why.</summary>
        public static void Skipped(ILogger logger, string artifact, string tag, string reason) =>
            SkippedMessage(logger, artifact, tag, reason, null);

        /// <summary>Reports whichever outcome a result carries.</summary>
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
