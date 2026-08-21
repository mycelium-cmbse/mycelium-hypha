// ------------------------------------------------------------------------------------------------
// <copyright file="HookJsonContext.cs" company="Starion Group S.A.">
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
    /// Source-generated (de)serialization for every JSON shape this hook touches.
    /// </summary>
    /// <remarks>
    /// NativeAOT trims reflection metadata, so <see cref="System.Text.Json.JsonSerializer"/>'s default
    /// reflection-based mode is not safe here; a source-generated context sidesteps that entirely.
    /// </remarks>
    [JsonSerializable(typeof(PluginManifest))]
    [JsonSerializable(typeof(GitHubRelease))]
    [JsonSerializable(typeof(HookCheckResult))]
    [JsonSerializable(typeof(HookOutput))]
    public sealed partial class HookJsonContext : JsonSerializerContext
    {
    }
}
