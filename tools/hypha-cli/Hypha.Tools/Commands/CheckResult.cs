// ------------------------------------------------------------------------------------------------
// <copyright file="CheckResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// What <c>hypha check --json</c> prints: local vs. online releases, with no fetching or
    /// generating done to produce it.
    /// </summary>
    /// <remarks>
    /// This is the one piece of machine-readable output in the CLI, read by the plugin's
    /// <c>SessionStart</c> hook (<c>Hypha.Tools.Hook</c>, which keeps its own equivalent shape rather
    /// than referencing this type directly - see that project's <c>HookCheckResult</c> and the note on
    /// why it stays dependency-free). Changing these property names is a breaking change for that
    /// parser.
    /// </remarks>
    /// <param name="InstalledTags">Locally installed releases, newest first; empty if none.</param>
    /// <param name="DefaultTag">The release every skill answers from by default, or <c>null</c>.</param>
    /// <param name="AvailableOnline">
    /// The newest offerable releases upstream, newest first, capped at <see cref="MaxAvailableOnline"/>.
    /// </param>
    public sealed record CheckResult(
        [property: JsonPropertyName("installedTags")] IReadOnlyList<string> InstalledTags,
        [property: JsonPropertyName("defaultTag")] string? DefaultTag,
        [property: JsonPropertyName("availableOnline")] IReadOnlyList<string> AvailableOnline)
    {
        /// <summary>How many upstream releases <see cref="AvailableOnline"/> ever carries.</summary>
        public const int MaxAvailableOnline = 5;
    }
}
