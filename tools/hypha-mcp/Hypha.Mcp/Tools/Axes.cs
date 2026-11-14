// ------------------------------------------------------------------------------------------------
// <copyright file="Axes.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    /// <summary>The names of the two version axes, as the release tools' answers write them.</summary>
    internal static class Axes
    {
        /// <summary>A SysML v2 / KerML language release.</summary>
        public const string Release = "release";

        /// <summary>A Systems Modeling API version.</summary>
        public const string Api = "api";

        /// <summary>The name an axis is written with.</summary>
        /// <param name="axis">The axis.</param>
        /// <returns><see cref="Release"/> or <see cref="Api"/>.</returns>
        public static string Of(VersionAxis axis) => axis == VersionAxis.Api ? Api : Release;
    }
}
