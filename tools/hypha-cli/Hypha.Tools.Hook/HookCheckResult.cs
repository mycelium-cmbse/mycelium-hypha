// ------------------------------------------------------------------------------------------------
// <copyright file="HookCheckResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The parts of <c>hypha check --json</c>'s output this hook reads.
    /// </summary>
    /// <remarks>
    /// Deliberately its own type rather than a shared reference to
    /// <c>Hypha.Tools.Commands.CheckResult</c>: this project must stay free of any dependency on
    /// <c>Hypha.Tools</c> (which pulls in Handlebars.Net/uml4net and is not NativeAOT-safe), so the
    /// JSON shape is duplicated on purpose and kept in sync with it by convention.
    /// </remarks>
    /// <param name="InstalledTags">
    /// The locally installed releases, newest first (<c>installedTags</c>), or <c>null</c> when the
    /// output omits them.
    /// </param>
    /// <param name="DefaultTag">The default release (<c>defaultTag</c>), or <c>null</c> when none is set.</param>
    /// <param name="AvailableOnline">
    /// The newest offerable releases upstream, newest first (<c>availableOnline</c>), or <c>null</c>
    /// when the output omits them.
    /// </param>
    public sealed record HookCheckResult(
        [property: JsonPropertyName("installedTags")] IReadOnlyList<string>? InstalledTags,
        [property: JsonPropertyName("defaultTag")] string? DefaultTag,
        [property: JsonPropertyName("availableOnline")] IReadOnlyList<string>? AvailableOnline);
}
