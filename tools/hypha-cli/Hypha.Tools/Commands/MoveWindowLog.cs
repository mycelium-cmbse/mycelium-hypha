// ------------------------------------------------------------------------------------------------
// <copyright file="MoveWindowLog.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;

    using Microsoft.Extensions.Logging;

    /// <summary>The messages <c>move-window</c> reports as it moves through its steps.</summary>
    public static class MoveWindowLog
    {
        /// <summary>Every step, in the order <c>move-window</c> runs them, with the words a user reads.</summary>
        public static readonly (MoveWindowStep Step, string Description)[] Steps =
        [
            (MoveWindowStep.Fetch, "Fetching the release"),
            (MoveWindowStep.Generate, "Regenerating the knowledge base"),
            (MoveWindowStep.ExtractSpecifications, "Extracting specification text (this can take several minutes)"),
            (MoveWindowStep.ReblessFixtures, "Re-blessing the metamodel-gen test fixtures"),
            (MoveWindowStep.Evict, "Evicting releases outside the window"),
            (MoveWindowStep.Verify, "Verifying the regenerated knowledge base"),
        ];

        private static readonly Action<ILogger, int, int, string, string, Exception?> StepMessage =
            LoggerMessage.Define<int, int, string, string>(
                LogLevel.Information, new EventId(1, nameof(Step)),
                "Step {Number}/{Total}: {Description} ({Tag})");

        private static readonly Action<ILogger, string, string, int, Exception?> StepFailedMessage =
            LoggerMessage.Define<string, string, int>(
                LogLevel.Error, new EventId(2, nameof(Failed)),
                "{Description} failed for {Tag} (exit code {ExitCode})");

        /// <summary>A step is about to begin.</summary>
        public static void Step(ILogger logger, MoveWindowStep step, string tag)
        {
            var index = Array.FindIndex(Steps, entry => entry.Step == step);

            StepMessage(logger, index + 1, Steps.Length, Steps[index].Description, tag, null);
        }

        /// <summary>A step did not succeed.</summary>
        public static void Failed(ILogger logger, MoveWindowStep step, string tag, int exitCode)
        {
            var description = Array.Find(Steps, entry => entry.Step == step).Description;

            StepFailedMessage(logger, description, tag, exitCode, null);
        }
    }
}
