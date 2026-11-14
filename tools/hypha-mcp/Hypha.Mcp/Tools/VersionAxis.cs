// ------------------------------------------------------------------------------------------------
// <copyright file="VersionAxis.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Which kind of version a release tool installs, switches or removes: a SysML v2 / KerML release, or
    /// a Systems Modeling API version, which is versioned apart from the releases.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<VersionAxis>))]
    public enum VersionAxis
    {
        /// <summary>A SysML v2 / KerML language release, such as 2026-05.</summary>
        [JsonStringEnumMemberName("release")]
        Release,

        /// <summary>A Systems Modeling API and Services version, such as 2026-04.</summary>
        [JsonStringEnumMemberName("api")]
        Api,
    }
}
