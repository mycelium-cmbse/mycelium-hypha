// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubRelease.cs" company="Starion Group S.A.">
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
    /// The parts of the GitHub Releases API response this hook reads.
    /// </summary>
    /// <param name="Body">
    /// The release notes - specifically the checksum table <c>release.yml</c>'s "Record the
    /// checksums" step writes into it, which is the only place a SHA256 for each asset is published.
    /// </param>
    /// <param name="Assets">The files attached to the release.</param>
    public sealed record GitHubRelease(
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("assets")] IReadOnlyList<GitHubReleaseAsset>? Assets);

    /// <summary>One file attached to a GitHub release.</summary>
    /// <param name="Name">The asset's file name, e.g. <c>hypha-1.2.0-win-x64.zip</c>.</param>
    /// <param name="BrowserDownloadUrl">Where to download it from.</param>
    public sealed record GitHubReleaseAsset(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("browser_download_url")] string? BrowserDownloadUrl);
}
