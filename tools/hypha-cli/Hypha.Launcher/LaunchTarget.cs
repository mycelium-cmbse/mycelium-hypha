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

    using Hypha.Launcher.Provisioning;

    /// <summary>The <c>hypha</c> to start, the download it still needs, or why there is none.</summary>
    /// <param name="Executable">The <c>hypha</c> executable; <c>null</c> when it is still to be downloaded or there is none.</param>
    /// <param name="Version">The version asked for, or <c>null</c> when it is not known.</param>
    /// <param name="Failure">Why there is no executable, and how to fix it, for a person; <c>null</c> when there is one or it is being downloaded.</param>
    /// <param name="Cache">Where the version is to be downloaded into; <c>null</c> unless <see cref="NeedsDownload"/>.</param>
    internal sealed record LaunchTarget(FileInfo? Executable, string? Version, string? Failure, CacheLayout? Cache = null)
    {
        /// <summary>Gets a value indicating whether the version is known but not cached yet.</summary>
        public bool NeedsDownload => this.Executable is null && this.Failure is null && this.Cache is not null;

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

        /// <summary>A version known but not cached, to be downloaded into <paramref name="cache"/>.</summary>
        /// <param name="version">The version.</param>
        /// <param name="cache">Where it goes.</param>
        /// <returns>The target.</returns>
        public static LaunchTarget Pending(string version, CacheLayout cache) => new(null, version, null, cache);
    }
}
