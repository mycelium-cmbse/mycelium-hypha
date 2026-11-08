// ------------------------------------------------------------------------------------------------
// <copyright file="IProcessProbe.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Hosting
{
    /// <summary>
    /// Tells whether a process is still running on this machine, so a lock left behind by a process
    /// that is gone can be told apart from one whose owner is still at work.
    /// </summary>
    public interface IProcessProbe
    {
        /// <summary>Whether the process with <paramref name="processId"/> is still running.</summary>
        /// <param name="processId">The operating system's process id.</param>
        /// <returns>
        /// <c>false</c> only when the process is known to be gone; a process that exists but cannot be
        /// inspected counts as running, so its lock is never taken from it.
        /// </returns>
        bool IsRunning(int processId);
    }
}
