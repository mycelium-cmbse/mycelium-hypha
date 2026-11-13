// ------------------------------------------------------------------------------------------------
// <copyright file="LaunchTarget.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System.IO;

    /// <summary>The <c>hypha</c> to start, or why there is none.</summary>
    /// <param name="Executable">The <c>hypha</c> executable; <c>null</c> when it could not be found or provisioned.</param>
    /// <param name="Version">The version asked for, or <c>null</c> when it is not known.</param>
    /// <param name="Failure">Why there is no executable, and how to fix it, for a person; <c>null</c> when there is one.</param>
    internal sealed record LaunchTarget(FileInfo? Executable, string? Version, string? Failure)
    {
        /// <summary>A <c>hypha</c> to start.</summary>
        /// <param name="executable">The executable.</param>
        /// <param name="version">The version asked for, or <c>null</c> when <c>HYPHA_BIN</c> chose it.</param>
        /// <returns>The target.</returns>
        public static LaunchTarget Found(FileInfo executable, string? version) => new(executable, version, null);

        /// <summary>No <c>hypha</c> to start.</summary>
        /// <param name="failure">Why, and how to fix it.</param>
        /// <param name="version">The version asked for, or <c>null</c> when it is not known.</param>
        /// <returns>The target.</returns>
        public static LaunchTarget Missing(string failure, string? version = null) => new(null, version, failure);
    }
}
