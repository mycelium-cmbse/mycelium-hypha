// ------------------------------------------------------------------------------------------------
// <copyright file="SystemProcessRunner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Runs a child process with <see cref="System.Diagnostics.Process"/>, streaming its output
    /// through <see cref="ILogger"/> line by line as it runs.
    /// </summary>
    /// <remarks>
    /// Streamed rather than captured-and-printed-at-the-end: the steps this exists for (specification
    /// extraction, provisioning <c>uv</c>) take from several seconds to several minutes and print
    /// nothing of their own, so a caller watching the console needs to see it is making progress, not
    /// just that it eventually finished.
    /// </remarks>
    public sealed class SystemProcessRunner : IProcessRunner
    {
        /// <summary>
        /// The log the child process's output is streamed to: standard output as information, standard error as warnings.
        /// </summary>
        private readonly ILogger<SystemProcessRunner> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemProcessRunner"/> class.
        /// </summary>
        /// <param name="logger">
        /// The log the child process's output is streamed to: standard output as information, standard error as warnings.
        /// </param>
        public SystemProcessRunner(ILogger<SystemProcessRunner> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            this.logger = logger;
        }

        /// <summary>
        /// Runs <paramref name="fileName"/> without a shell and waits for it to exit, logging each line of its
        /// standard output as information and each line of its standard error as a warning while it runs.
        /// </summary>
        /// <param name="fileName">The executable to run.</param>
        /// <param name="arguments">The command-line arguments, as one already-escaped string.</param>
        /// <param name="workingDirectory">Where the process runs from.</param>
        /// <param name="environment">
        /// Environment variables to set for the process, on top of this process's own; <c>null</c>
        /// sets none.
        /// </param>
        /// <param name="cancellationToken">Kills the process and its whole process tree, then rethrows.</param>
        /// <returns>The process's exit code.</returns>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="arguments"/> or <paramref name="workingDirectory"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">The executable could not be started.</exception>
        public async Task<int> RunAsync(string fileName, string arguments, DirectoryInfo workingDirectory,
            IReadOnlyDictionary<string, string>? environment = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentNullException.ThrowIfNull(workingDirectory);

            var info = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingDirectory.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // The child gets an input of its own, closed at once below, never this process's: under
                // `hypha serve` that is the pipe the MCP client writes its requests to. A child that
                // inherits it can take a request meant for the server, and on Windows a Python child
                // hangs at start-up while the server is waiting to read from the same pipe.
                RedirectStandardInput = true,
                UseShellExecute = false,
            };

            foreach (var (name, value) in environment ?? new Dictionary<string, string>())
            {
                info.Environment[name] = value;
            }

            using var process = new Process { StartInfo = info, EnableRaisingEvents = true };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    this.logger.LogInformation("{Line}", e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    this.logger.LogWarning("{Line}", e.Data);
                }
            };

            process.Start();
            process.StandardInput.Close();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw;
            }

            return process.ExitCode;
        }
    }
}
