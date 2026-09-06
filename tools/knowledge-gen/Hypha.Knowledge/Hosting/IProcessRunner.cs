// ------------------------------------------------------------------------------------------------
// <copyright file="IProcessRunner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Hosting
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs an external command and waits for it, for the verbs that have to shell out - the
    /// spec-extract and metamodel-gen steps <c>move-window</c> drives, and <c>uv</c> for on-demand
    /// spec extraction, have no in-process seam.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>Runs <paramref name="fileName"/> and waits for it to exit.</summary>
        /// <param name="fileName">The executable to run.</param>
        /// <param name="arguments">The command-line arguments, as one already-escaped string.</param>
        /// <param name="workingDirectory">Where the process runs from.</param>
        /// <returns>The process's exit code.</returns>
        Task<int> RunAsync(
            string fileName,
            string arguments,
            DirectoryInfo workingDirectory,
            CancellationToken cancellationToken = default);
    }
}
