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
    /// What a <c>SessionStart</c> hook prints to stdout to add context: the same shape
    /// <c>hooks/check-spec-pdfs.py</c> already emits.
    /// </summary>
    /// <param name="HookSpecificOutput">The event-scoped part of the output (<c>hookSpecificOutput</c>).</param>
    public sealed record HookOutput(
        [property: JsonPropertyName("hookSpecificOutput")] HookSpecificOutput HookSpecificOutput)
    {
        /// <summary>Builds the output for a <c>SessionStart</c> hook.</summary>
        /// <param name="additionalContext">The text added to the session's context.</param>
        /// <returns>The output, its event name <c>SessionStart</c>.</returns>
        public static HookOutput SessionStart(string additionalContext) =>
            new(new HookSpecificOutput("SessionStart", additionalContext));
    }

    /// <summary>The event-scoped part of a hook's output.</summary>
    /// <param name="HookEventName">The hook event the output answers (<c>hookEventName</c>).</param>
    /// <param name="AdditionalContext">The text added to the session's context (<c>additionalContext</c>).</param>
    public sealed record HookSpecificOutput(
        [property: JsonPropertyName("hookEventName")] string HookEventName,
        [property: JsonPropertyName("additionalContext")] string AdditionalContext);
}
