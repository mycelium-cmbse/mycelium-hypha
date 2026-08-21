// ------------------------------------------------------------------------------------------------
// <copyright file="PluginManifest.cs" company="Starion Group S.A.">
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
    /// The one field of <c>.claude-plugin/plugin.json</c> this hook cares about.
    /// </summary>
    /// <remarks>
    /// Deserialization tolerates every other field in the file - this never round-trips the manifest,
    /// only reads it, so nothing else needs a property here.
    /// </remarks>
    public sealed record PluginManifest(
        [property: JsonPropertyName("hyphaCliVersion")] string? HyphaCliVersion);
}
