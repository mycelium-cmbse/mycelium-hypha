// ------------------------------------------------------------------------------------------------
// <copyright file="ISyncStatusWriter.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    /// <summary>
    /// Reads and writes the status file <c>hypha sync</c> uses to report its own progress.
    /// </summary>
    public interface ISyncStatusWriter
    {
        /// <summary>Reads the status file, or <c>null</c> when there is none yet.</summary>
        SyncStatus? Load();

        /// <summary>Writes the status file atomically, so a concurrent reader never sees a half-write.</summary>
        void Write(SyncStatus status);
    }
}
