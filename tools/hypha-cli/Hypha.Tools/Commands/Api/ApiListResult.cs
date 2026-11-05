// ------------------------------------------------------------------------------------------------
// <copyright file="ApiListResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands.Api
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using Hypha.Knowledge.Api;

    /// <summary>
    /// What <c>hypha api list --json</c> prints: every installed Systems Modeling API version, the
    /// default, and which artifacts of each have been generated.
    /// </summary>
    /// <remarks>
    /// The same shape as <c>hypha list --json</c>, with <c>versions</c> in place of <c>releases</c>, so a
    /// host reads both version axes the same way. Renaming a property is a breaking change for every host.
    /// </remarks>
    /// <param name="DefaultTag">The API version lookups answer from when none is named, or <c>null</c>.</param>
    /// <param name="Versions">The installed API versions, newest first; empty when none is installed.</param>
    public sealed record ApiListResult(
        [property: JsonPropertyName("defaultTag")] string? DefaultTag,
        [property: JsonPropertyName("versions")] IReadOnlyList<ApiVersionStatus> Versions);
}
