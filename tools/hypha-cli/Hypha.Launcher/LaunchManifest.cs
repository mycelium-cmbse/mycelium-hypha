// ------------------------------------------------------------------------------------------------
// <copyright file="LaunchManifest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// <c>bin/hypha-launch.json</c>, beside the launcher in a client package: which <c>hypha</c> it starts.
    /// </summary>
    /// <remarks>
    /// Every package format (the Claude plugin, an Agent Plugin, a Gemini extension) ships the same
    /// <c>bin/</c>, so the launcher reads its version from there rather than from one client's manifest.
    /// The launcher and the <c>hypha</c> it starts come from the same release; nothing else is agreed
    /// between them.
    /// </remarks>
    /// <param name="HyphaVersion">The <c>hypha</c> version to start, the <c>tools-v</c> release it is downloaded from.</param>
    public sealed record LaunchManifest(
        [property: JsonPropertyName("hyphaVersion")] string? HyphaVersion)
    {
        /// <summary>The manifest's file name.</summary>
        public const string FileName = "hypha-launch.json";
    }
}
