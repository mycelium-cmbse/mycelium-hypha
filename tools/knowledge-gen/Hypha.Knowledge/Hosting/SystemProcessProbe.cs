// ------------------------------------------------------------------------------------------------
// <copyright file="SystemProcessProbe.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Hosting
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Asks the operating system whether a process is still running.
    /// </summary>
    /// <remarks>
    /// The same rule the VS Code extension applies with <c>process.kill(pid, 0)</c>: a process that
    /// does not exist is gone, and one that exists but may not be inspected is running.
    /// </remarks>
    public sealed class SystemProcessProbe : IProcessProbe
    {
        /// <summary>
        /// Whether the process with <paramref name="processId"/> is still running, asked of the operating
        /// system through <see cref="Process.GetProcessById(int)"/>.
        /// </summary>
        /// <param name="processId">The operating system's process id.</param>
        /// <returns>
        /// <c>false</c> for an id that is not positive, for an id no process has, and for a process that has
        /// exited; <c>true</c> otherwise, including for a process that exists but may not be inspected, so
        /// its lock is never taken from it.
        /// </returns>
        public bool IsRunning(int processId)
        {
            if (processId <= 0)
            {
                return false;
            }

            try
            {
                using var process = Process.GetProcessById(processId);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                // No process has this id.
                return false;
            }
            catch (InvalidOperationException)
            {
                // The process exited while it was being looked at.
                return false;
            }
            catch (Win32Exception)
            {
                // It exists, but belongs to someone this process may not inspect.
                return true;
            }
        }
    }
}
