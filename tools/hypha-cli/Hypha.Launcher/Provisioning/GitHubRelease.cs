// ------------------------------------------------------------------------------------------------
// <copyright file="GitHubRelease.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Provisioning
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The parts of the GitHub Releases API response the launcher and the hook read.
    /// </summary>
    /// <param name="Body">
    /// The release notes, specifically the checksum table <c>release.yml</c>'s "Record the
    /// checksums" step writes into it, which is the only place a SHA256 for each asset is published.
    /// </param>
    /// <param name="Assets">The files attached to the release.</param>
    public sealed record GitHubRelease(
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("assets")] IReadOnlyList<GitHubReleaseAsset>? Assets);

}
