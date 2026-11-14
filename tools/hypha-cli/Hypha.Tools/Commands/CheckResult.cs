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
    /// Scripts read this output, and the plugin's <c>SessionStart</c> hook did until it was removed
    /// (the MCP server's <c>hypha_check_releases</c> answers the same question now). Changing these
    /// property names is a breaking change for anything that parses it.
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
        public const int MaxAvailableOnline = Hypha.Knowledge.Releases.UpstreamComparison.MaxOffered;
    }
}
