// ------------------------------------------------------------------------------------------------
// <copyright file="DetachedProcessLauncher.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Launches <c>hypha sync</c> detached: this process starts it and returns immediately, without
    /// waiting for it to finish.
    /// </summary>
    public static class DetachedProcessLauncher
    {
        /// <summary>
        /// Builds the <see cref="ProcessStartInfo"/> for a detached launch of <paramref name="fileName"/>.
        /// </summary>
        /// <remarks>
        /// Arguments go through <see cref="ProcessStartInfo.ArgumentList"/>, not a hand-quoted
        /// <see cref="ProcessStartInfo.Arguments"/> string - paths on the user's machine routinely
        /// contain spaces, and .NET's own escaping there is what's actually cross-platform-correct.
        /// <para>
        /// All three standard streams are redirected rather than left inherited. An inherited stdout
        /// handle in the child can outlive this process and delay the harness reading this hook's own
        /// stdout from seeing EOF - even though this process itself has already exited. Nothing needs
        /// to actively drain the redirected streams: <c>hypha sync</c> is launched with
        /// <c>--log-file</c>, so its own diagnostics never reach these pipes in the first place.
        /// </para>
        /// </remarks>
        public static ProcessStartInfo Build(
            string fileName, IReadOnlyList<string> arguments, string workingDirectory)
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

        /// <summary>Starts the process and returns immediately, without waiting for it to exit.</summary>
        /// <returns><c>true</c> when the process started; <c>false</c> on any failure to start it.</returns>
        public static bool Start(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            try
            {
                using var process = Process.Start(Build(fileName, arguments, workingDirectory));

                return process is not null;
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or IOException or System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }
    }
}
