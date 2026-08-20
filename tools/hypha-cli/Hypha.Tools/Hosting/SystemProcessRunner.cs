// ------------------------------------------------------------------------------------------------
// <copyright file="SystemProcessRunner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hosting
{
    using System;
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
    /// Streamed rather than captured-and-printed-at-the-end: the step this exists for (specification
    /// extraction) takes several minutes and prints nothing of its own, so a caller watching the
    /// console needs to see it is making progress, not just that it eventually finished.
    /// </remarks>
    public sealed class SystemProcessRunner : IProcessRunner
    {
        private readonly ILogger<SystemProcessRunner> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemProcessRunner"/> class.
        /// </summary>
        public SystemProcessRunner(ILogger<SystemProcessRunner> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> RunAsync(
            string fileName,
            string arguments,
            DirectoryInfo workingDirectory,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentNullException.ThrowIfNull(workingDirectory);

            var info = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingDirectory.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

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
