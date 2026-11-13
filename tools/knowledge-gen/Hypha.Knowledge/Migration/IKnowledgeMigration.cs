// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeMigration.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Copies the releases and API versions the Claude plugin and the VS Code extension installed in their
    /// own folders into <c>HYPHA_HOME</c>, so moving to the one shared folder fetches and generates nothing
    /// again.
    /// </summary>
    /// <remarks>
    /// It copies rather than moves: the plugin and the extension keep answering from their own folders
    /// until they use <c>HYPHA_HOME</c> themselves. Each release folder is copied beside its target under a
    /// temporary name and renamed into place, and the manifests are written last, so an interrupted
    /// migration leaves nothing half copied that a reader would see.
    /// </remarks>
    public interface IKnowledgeMigration
    {
        /// <summary>
        /// Migrates once, on its own: only when the knowledge is read from <c>HYPHA_HOME</c>, it has no release
        /// installed yet, and no migration ran before. A marker in <c>HYPHA_HOME</c> records that one did.
        /// </summary>
        /// <param name="cancellationToken">Cancels the migration between files.</param>
        /// <returns>What it copied, or why it did not look.</returns>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="IOException">A folder could not be copied; nothing is recorded, so the next run tries again.</exception>
        Task<MigrationResult> MigrateOnceAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Migrates on request: copies every release and API version <c>HYPHA_HOME</c> does not have yet,
        /// whatever it already holds and whether a migration ran before.
        /// </summary>
        /// <param name="from">
        /// The client folders to copy from, each holding a <c>knowledge/</c>; <c>null</c> or empty for the
        /// ones <see cref="ILegacyInstallLocator"/> finds.
        /// </param>
        /// <param name="cancellationToken">Cancels the migration between files.</param>
        /// <returns>What it copied.</returns>
        /// <exception cref="Releases.ReleaseOperationLockException">Another hypha is changing releases in <c>HYPHA_HOME</c>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="IOException">A folder could not be copied.</exception>
        Task<MigrationResult> MigrateAsync(IReadOnlyList<DirectoryInfo>? from = null, CancellationToken cancellationToken = default);
    }
}
