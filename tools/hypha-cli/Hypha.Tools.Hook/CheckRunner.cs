// ------------------------------------------------------------------------------------------------
// <copyright file="CheckRunner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs <c>hypha check --json</c> and captures its output - synchronously, waiting for it to
    /// finish, unlike the detached launch a hook-triggered fetch used to need.
    /// </summary>
    /// <remarks>
    /// <c>check</c> only ever compares local releases against what is offerable upstream - a couple of
    /// GitHub API calls, no fetching or generating - so it is cheap enough to wait for inline within
    /// the hook's own timeout budget (10 minutes by default), with nothing left to poll for later.
    /// </remarks>
    public static class CheckRunner
    {
        /// <summary>Builds the <see cref="ProcessStartInfo"/> for a synchronous, output-capturing run.</summary>
        public static ProcessStartInfo Build(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

            var info = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };

            foreach (var argument in arguments)
            {
                info.ArgumentList.Add(argument);
            }

            return info;
        }

        /// <summary>
        /// Runs the process to completion and returns what it wrote to stdout.
        /// </summary>
        /// <returns>
        /// The captured stdout, or <c>null</c> when the process could not be started, exited with a
        /// non-zero code, or wrote nothing.
        /// </returns>
        public static async Task<string?> RunAndCaptureOutputAsync(
            string fileName, IReadOnlyList<string> arguments, string workingDirectory,
            CancellationToken cancellationToken)
        {
            try
            {
                using var process = Process.Start(Build(fileName, arguments, workingDirectory));
                if (process is null)
                {
                    return null;
                }

                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? output : null;
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or IOException or System.ComponentModel.Win32Exception)
            {
                return null;
            }
        }
    }
}
