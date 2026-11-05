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

        /// <inheritdoc/>
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
