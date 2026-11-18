// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Migration
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>What a migration into <c>HYPHA_HOME</c> did.</summary>
    /// <param name="Home">The <c>HYPHA_HOME</c> folder, or the folder read from when <c>HYPHA_HOME</c> cannot be used.</param>
    /// <param name="Searched">The client folders looked in; empty when none was found, or the migration did not look.</param>
    /// <param name="Copied">The releases and API versions copied, in the order copied; empty when none were.</param>
    /// <param name="Skipped">
    /// Why nothing was looked for, such as a migration that already ran; <c>null</c> when the client folders
    /// were looked through.
    /// </param>
    public sealed record MigrationResult(
        DirectoryInfo Home, IReadOnlyList<LegacyInstall> Searched, IReadOnlyList<MigratedVersion> Copied, string? Skipped = null)
    {
        /// <summary>
        /// Gets the releases and API versions found but not copied, because they would answer wrongly;
        /// empty when none were. They are also kept in <see cref="LeftOutFile.FileName"/>.
        /// </summary>
        public IReadOnlyList<LeftOutVersion> LeftOut { get; init; } = [];
    }
}
