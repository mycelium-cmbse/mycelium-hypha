// ------------------------------------------------------------------------------------------------
// <copyright file="MigrationResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>What a migration into <c>HYPHA_HOME</c> did.</summary>
    /// <param name="Home">The <c>HYPHA_HOME</c> folder.</param>
    /// <param name="Copied">The releases and API versions copied, in the order copied; empty when none were.</param>
    /// <param name="Skipped">
    /// Why nothing was looked for, such as a migration that already ran; <c>null</c> when the client folders
    /// were looked through.
    /// </param>
    public sealed record MigrationResult(DirectoryInfo Home, IReadOnlyList<MigratedVersion> Copied, string? Skipped = null);
}
