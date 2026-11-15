// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubReleaseAsset.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Provisioning
{
    using System.Text.Json.Serialization;

    /// <summary>One file attached to a GitHub release.</summary>
    /// <param name="Name">The asset's file name, e.g. <c>hypha-1.2.0-win-x64.zip</c>.</param>
    /// <param name="BrowserDownloadUrl">Where to download it from.</param>
    public sealed record GitHubReleaseAsset(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("browser_download_url")] string? BrowserDownloadUrl);
}
