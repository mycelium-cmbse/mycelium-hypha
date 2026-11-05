// ------------------------------------------------------------------------------------------------
// <copyright file="ListResult.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// What <c>hypha list --json</c> prints: every installed release, the default, and which
    /// artifacts of each have been generated.
    /// </summary>
    /// <remarks>
    /// A host reports readiness from this rather than probing the knowledge folders itself, so the
    /// layout stays the CLI's business. Renaming a property is a breaking change for every host.
    /// </remarks>
    /// <param name="DefaultTag">The release lookups answer from when none is named, or <c>null</c>.</param>
    /// <param name="Releases">The installed releases, newest first; empty when none is installed.</param>
    public sealed record ListResult(
        [property: JsonPropertyName("defaultTag")] string? DefaultTag,
        [property: JsonPropertyName("releases")] IReadOnlyList<ReleaseStatus> Releases);
}
