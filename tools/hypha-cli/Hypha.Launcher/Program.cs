// ------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    using Hypha.Launcher.Provisioning;

    /// <summary>
    /// <c>hypha-launch</c>: the command every client package starts. It finds or provisions the matching
    /// <c>hypha</c>, then runs <c>hypha serve</c> on the same standard input and output, so the client
    /// talks to the server directly.
    /// </summary>
    public static class Program
    {
        /// <summary>Starts the server, or the fallback server when there is no <c>hypha</c> to start.</summary>
        /// <param name="args">Passed on to <c>hypha serve</c>, such as <c>--log-level Debug</c>.</param>
        /// <returns><c>hypha serve</c>'s exit code; 0 once the fallback server's client has gone.</returns>
        public static async Task<int> Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            LaunchTarget target;
            using (var client = CliProvisioner.CreateClient())
            {
                target = await new Launch(Environment.GetEnvironmentVariable, Environment.GetFolderPath, new CliProvisioner(client))
                    .FindAsync(new DirectoryInfo(AppContext.BaseDirectory), RuntimeInformation.RuntimeIdentifier);
            }

            var failure = target.Failure;
            if (target.Executable is { } executable)
            {
                try
                {
                    return await ServeAsync(executable, args);
                }
                catch (Win32Exception exception)
                {
                    // Nothing has reached the client yet, so the fallback server can still answer it.
                    failure = $"{executable.FullName} could not be started ({exception.Message}). "
                              + $"Reinstall the package, or set {Launch.BinVariable} to a hypha that runs on this machine.";
                }
            }

            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var input = new StreamReader(Console.OpenStandardInput(), encoding);
            await using var output = new StreamWriter(Console.OpenStandardOutput(), encoding);

            await Console.Error.WriteLineAsync($"hypha-launch: {failure}");
            await FallbackServer.RunAsync(input, output, failure!, Version);

            return 0;
        }

        /// <summary>Gets the launcher's version, as its build stamped it.</summary>
        private static string Version =>
            typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0";

        /// <summary>
        /// Runs <c>hypha serve</c>, which inherits the launcher's standard input, output and error, and waits
        /// for it: the client closing standard input ends it.
        /// </summary>
        /// <param name="executable">The <c>hypha</c> executable.</param>
        /// <param name="args">Passed on after <c>serve</c>.</param>
        /// <returns>Its exit code.</returns>
        /// <exception cref="Win32Exception">The executable could not be started.</exception>
        private static async Task<int> ServeAsync(FileInfo executable, string[] args)
        {
            var start = new ProcessStartInfo(executable.FullName) { UseShellExecute = false };
            start.ArgumentList.Add("serve");
            foreach (var argument in args)
            {
                start.ArgumentList.Add(argument);
            }

            using var serve = Process.Start(start)
                ?? throw new InvalidOperationException($"{executable.FullName} did not start");

            await serve.WaitForExitAsync();

            return serve.ExitCode;
        }
    }
}
