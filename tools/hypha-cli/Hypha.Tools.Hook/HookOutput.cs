// ------------------------------------------------------------------------------------------------
// <copyright file="HookOutput.cs" company="Starion Group S.A.">
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
    /// What a <c>SessionStart</c> hook prints to stdout to add context - the same shape
    /// <c>hooks/check-spec-pdfs.py</c> already emits.
    /// </summary>
    public sealed record HookOutput(
        [property: JsonPropertyName("hookSpecificOutput")] HookSpecificOutput HookSpecificOutput)
    {
        /// <summary>Builds the output for a <c>SessionStart</c> hook.</summary>
        public static HookOutput SessionStart(string additionalContext) =>
            new(new HookSpecificOutput("SessionStart", additionalContext));
    }

    /// <summary>The event-scoped part of a hook's output.</summary>
    public sealed record HookSpecificOutput(
        [property: JsonPropertyName("hookEventName")] string HookEventName,
        [property: JsonPropertyName("additionalContext")] string AdditionalContext);
}
