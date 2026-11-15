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
    /// <remarks>
    /// Three ways, depending on what is there. A <c>hypha</c> that is cached (or named by <c>HYPHA_BIN</c>)
    /// inherits the launcher's standard input and output, and the launcher only waits for it. A first start
    /// answers the client at once and downloads meanwhile, then hands over (<see cref="BootstrapServer"/>).
    /// Without a <c>hypha</c> to start, the launcher answers itself, saying why (<see cref="FallbackServer"/>).
    /// Whatever goes wrong before the client is answered ends in one of those, never in a crash: a client
    /// shows a server that dies at its start only as "failed to start".
    /// </remarks>
    public static class Program
    {
        /// <summary>Starts the server, or the launcher's own server when there is no <c>hypha</c> to start yet.</summary>
        /// <param name="args">Passed on to <c>hypha serve</c>, such as <c>--log-level Debug</c>.</param>
        /// <returns><c>hypha serve</c>'s exit code; 0 once the launcher's own server's client has gone.</returns>
        public static async Task<int> Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            var rid = RuntimeInformation.RuntimeIdentifier;
            using var client = CliProvisioner.CreateClient();
            var launch = new Launch(Environment.GetEnvironmentVariable, Environment.GetFolderPath, new CliProvisioner(client));

            LaunchTarget located;
            try
            {
                located = launch.Locate(new DirectoryInfo(AppContext.BaseDirectory), rid);
            }
            catch (Exception exception)
            {
                located = LaunchTarget.Missing($"the launcher could not look for hypha ({exception.GetType().Name}: {exception.Message}). Reinstall the package, or set {Launch.BinVariable} to an installed hypha {Launch.MinimumVersion} or later.");
            }

            var version = located.Version ?? LauncherVersion;
            var failure = located.Failure;

            if (located.Executable is { } executable)
            {
                try
                {
                    return await ServeAsync(executable, args);
                }
                catch (Win32Exception exception)
                {
                    // Nothing has reached the client yet, so the launcher can still answer it.
                    failure = $"{executable.FullName} could not be started ({exception.Message}). "
                              + $"Reinstall the package, or set {Launch.BinVariable} to a hypha {Launch.MinimumVersion} or later that runs on this machine.";
                }
            }

            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var input = new StreamReader(Console.OpenStandardInput(), encoding);
            await using var output = new StreamWriter(Console.OpenStandardOutput(), encoding);

            if (located.NeedsDownload)
            {
                await Console.Error.WriteLineAsync($"hypha-launch: downloading hypha {version} for {rid}, then handing over to it");

                return await BootstrapServer.RunAsync(
                    input, output, launch.DownloadAsync(located, rid), downloaded => StartRedirected(downloaded, args, encoding), version);
            }

            await Console.Error.WriteLineAsync($"hypha-launch: {failure}");
            await FallbackServer.RunAsync(input, output, failure!, version);

            return 0;
        }

        /// <summary>Gets the launcher's own version, reported when the package names no <c>hypha</c> version.</summary>
        private static string LauncherVersion =>
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
            using var serve = Process.Start(Serve(executable, args))
                ?? throw new InvalidOperationException($"{executable.FullName} did not start");

            await serve.WaitForExitAsync();

            return serve.ExitCode;
        }

        /// <summary>
        /// Starts <c>hypha serve</c> with its standard input and output redirected, for the bootstrap server to
        /// hand the session over to; its standard error stays the launcher's.
        /// </summary>
        /// <param name="executable">The <c>hypha</c> executable.</param>
        /// <param name="args">Passed on after <c>serve</c>.</param>
        /// <param name="encoding">The protocol's encoding, UTF-8 without a byte order mark.</param>
        /// <returns>The started process.</returns>
        /// <exception cref="Win32Exception">The executable could not be started.</exception>
        private static Process StartRedirected(FileInfo executable, string[] args, Encoding encoding)
        {
            var start = Serve(executable, args);
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;
            start.StandardInputEncoding = encoding;
            start.StandardOutputEncoding = encoding;

            return Process.Start(start) ?? throw new Win32Exception($"{executable.FullName} did not start");
        }

        /// <summary>The start of <c>hypha serve</c> with the launcher's arguments.</summary>
        /// <param name="executable">The <c>hypha</c> executable.</param>
        /// <param name="args">Passed on after <c>serve</c>.</param>
        /// <returns>The start information.</returns>
        private static ProcessStartInfo Serve(FileInfo executable, string[] args)
        {
            var start = new ProcessStartInfo(executable.FullName) { UseShellExecute = false };
            start.ArgumentList.Add("serve");
            foreach (var argument in args)
            {
                start.ArgumentList.Add(argument);
            }

            return start;
        }
    }
}
