// ------------------------------------------------------------------------------------------------
// <copyright file="ProvisioningJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Provisioning
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract of the GitHub Releases API response, so that reading it stays
    /// NativeAOT-safe in the launcher.
    /// </summary>
    [JsonSerializable(typeof(GitHubRelease))]
    public sealed partial class ProvisioningJsonContext : JsonSerializerContext
    {
    }
}
