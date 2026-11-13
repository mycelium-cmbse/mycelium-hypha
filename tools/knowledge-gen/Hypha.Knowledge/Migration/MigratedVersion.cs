// ------------------------------------------------------------------------------------------------
// <copyright file="MigratedVersion.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    /// <summary>One release or API version the migration copied into <c>HYPHA_HOME</c>.</summary>
    /// <param name="Kind"><see cref="Release"/> or <see cref="Api"/>.</param>
    /// <param name="Tag">The release or API version.</param>
    /// <param name="From">The client folder it was copied from, as <see cref="LegacyInstall.Origin"/> names it.</param>
    public sealed record MigratedVersion(string Kind, string Tag, string From)
    {
        /// <summary>The <see cref="Kind"/> of a language release.</summary>
        public const string Release = "release";

        /// <summary>The <see cref="Kind"/> of a Systems Modeling API version.</summary>
        public const string Api = "api";
    }
}
