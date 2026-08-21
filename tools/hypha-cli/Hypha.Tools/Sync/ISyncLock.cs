// ------------------------------------------------------------------------------------------------
// <copyright file="ISyncLock.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    /// <summary>
    /// Stops two <c>hypha sync</c> runs from working on the same repository at once - two Claude Code
    /// sessions can start close enough together that the hook launches a run for each.
    /// </summary>
    public interface ISyncLock
    {
        /// <summary>
        /// Attempts to take the lock.
        /// </summary>
        /// <returns>
        /// <c>true</c> when the lock is now held by this process; <c>false</c> when another live run
        /// already holds it. A losing caller must not write to the status file - a live run's
        /// in-progress status belongs to the run that is actually making it happen.
        /// </returns>
        bool TryAcquire();

        /// <summary>Releases the lock. Safe to call even when <see cref="TryAcquire"/> never succeeded.</summary>
        void Release();
    }
}
