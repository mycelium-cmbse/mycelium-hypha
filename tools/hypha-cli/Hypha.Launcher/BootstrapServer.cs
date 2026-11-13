// ------------------------------------------------------------------------------------------------
// <copyright file="BootstrapServer.cs" company="Starion Group S.A.">
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
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Serves the client while the first start downloads <c>hypha</c>, then hands the session over to
    /// <c>hypha serve</c>, so the client neither waits for the download nor has to reconnect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A client gives a server a limited time to answer <c>initialize</c>, and a first download of <c>hypha</c>
    /// can take longer. So this answers at once, as <see cref="FallbackServer"/> does while setting up, and
    /// downloads meanwhile. Once <c>hypha</c> is there it starts <c>hypha serve</c>, replays the client's
    /// <c>initialize</c> to it (under an id of its own, whose answer the client never sees), tells the
    /// client that the tools, resources and prompts changed, and from then on passes every line through
    /// unchanged, both ways.
    /// </para>
    /// <para>
    /// When the download fails, it goes on answering as <see cref="FallbackServer"/> does when there is no
    /// <c>hypha</c>, with the reason. The session's <c>instructions</c> stay the ones given at
    /// <c>initialize</c>; a client that reads them anew reconnects and gets the real server's.
    /// </para>
    /// </remarks>
    internal static class BootstrapServer
    {
        /// <summary>The id the replayed <c>initialize</c> goes to <c>hypha serve</c> under; its answer is dropped.</summary>
        public const string ReplayId = "hypha-launch-initialize";

        /// <summary>Serves until the client closes standard input.</summary>
        /// <param name="input">The client's messages, one per line.</param>
        /// <param name="output">Where the answers go, one per line.</param>
        /// <param name="download">The download of <c>hypha</c>, already running; it never throws.</param>
        /// <param name="start">Starts <c>hypha serve</c> with its standard input and output redirected.</param>
        /// <param name="version">The <c>hypha</c> version being downloaded.</param>
        /// <param name="cancellationToken">Stops the server.</param>
        /// <returns><c>hypha serve</c>'s exit code once it has taken over, otherwise 0.</returns>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public static async Task<int> RunAsync(
            TextReader input,
            TextWriter output,
            Task<LaunchTarget> download,
            Func<FileInfo, Process> start,
            string version,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(output);
            ArgumentNullException.ThrowIfNull(download);
            ArgumentNullException.ThrowIfNull(start);
            ArgumentNullException.ThrowIfNull(version);

            var status = $"hypha {version} is being downloaded, which takes up to a minute or so.";
            var settingUp = true;
            string? initialize = null;
            var initialized = false;
            Task<LaunchTarget>? pending = download;
            var read = input.ReadLineAsync(cancellationToken).AsTask();

            while (true)
            {
                var finished = pending is null ? read : await Task.WhenAny(read, pending);

                if (finished == read)
                {
                    if (await read is not { } line)
                    {
                        return 0;
                    }

                    switch (Method(line))
                    {
                        case "initialize":
                            initialize = line;
                            break;
                        case "notifications/initialized":
                            initialized = true;
                            break;
                    }

                    if (FallbackServer.Answer(line, status, version, settingUp) is { } answer)
                    {
                        await Send(output, answer, cancellationToken);
                    }

                    read = input.ReadLineAsync(cancellationToken).AsTask();
                    continue;
                }

                var target = await pending!;
                pending = null;

                if (target.Executable is { } executable)
                {
                    var (serve, failure) = Start(start, executable);
                    if (serve is not null)
                    {
                        return await HandOverAsync(serve, input, output, read, initialize, initialized, cancellationToken);
                    }

                    target = LaunchTarget.Missing(failure!, version);
                }

                // No hypha after all: the reason, from now on, and the lists asked for again so the client
                // sees hypha_status as the one tool that is left.
                status = target.Failure ?? "the download failed";
                settingUp = false;
                await Send(output, Notification("notifications/tools/list_changed"), cancellationToken);
            }
        }

        /// <summary>Starts <c>hypha serve</c>, or says why it could not be started.</summary>
        /// <param name="start">Starts it.</param>
        /// <param name="executable">The downloaded <c>hypha</c>.</param>
        /// <returns>The process, or <c>null</c> and why.</returns>
        private static (Process? Serve, string? Failure) Start(Func<FileInfo, Process> start, FileInfo executable)
        {
            try
            {
                return (start(executable), null);
            }
            catch (Win32Exception exception)
            {
                return (null, $"{executable.FullName} was downloaded but could not be started ({exception.Message}). "
                              + $"Install hypha {Launch.MinimumVersion} or later yourself and set {Launch.BinVariable} to it.");
            }
        }

        /// <summary>The method a line names, or <c>null</c> when it names none or is not a message.</summary>
        /// <param name="line">The line.</param>
        /// <returns>The method, or <c>null</c>.</returns>
        private static string? Method(string line)
        {
            try
            {
                return JsonNode.Parse(line) is JsonObject message && message["method"]?.GetValueKind() == JsonValueKind.String
                    ? message["method"]!.GetValue<string>()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Hands the session over to <c>hypha serve</c>: replays <c>initialize</c>, tells the client what changed,
        /// then passes every line through until the client leaves.
        /// </summary>
        /// <param name="serve">The started <c>hypha serve</c>.</param>
        /// <param name="input">The client's messages.</param>
        /// <param name="output">Where the answers go.</param>
        /// <param name="read">The client line being read, which goes to <c>hypha serve</c> once it arrives.</param>
        /// <param name="initialize">The client's <c>initialize</c>, or <c>null</c> when it has not sent it yet.</param>
        /// <param name="initialized">Whether the client has sent <c>notifications/initialized</c>.</param>
        /// <param name="cancellationToken">Stops the server.</param>
        /// <returns><c>hypha serve</c>'s exit code.</returns>
        private static async Task<int> HandOverAsync(
            Process serve,
            TextReader input,
            TextWriter output,
            Task<string?> read,
            string? initialize,
            bool initialized,
            CancellationToken cancellationToken)
        {
            using (serve)
            {
                var toServe = serve.StandardInput;
                var fromServe = serve.StandardOutput;

                if (initialize is not null && JsonNode.Parse(initialize) is JsonObject replay)
                {
                    replay["id"] = ReplayId;
                    await toServe.WriteAsync(replay.ToJsonString() + "\n");
                    await toServe.FlushAsync(cancellationToken);

                    // Its answer, and anything before it, are for the launcher, not the client.
                    while (await fromServe.ReadLineAsync(cancellationToken) is { } line)
                    {
                        if (IsAnswerTo(line, ReplayId))
                        {
                            break;
                        }
                    }

                    if (initialized)
                    {
                        await toServe.WriteAsync(Notification("notifications/initialized").ToJsonString() + "\n");
                        await toServe.FlushAsync(cancellationToken);
                    }

                    foreach (var changed in new[] { "notifications/tools/list_changed", "notifications/resources/list_changed", "notifications/prompts/list_changed" })
                    {
                        await Send(output, Notification(changed), cancellationToken);
                    }
                }

                var pump = PumpAsync(fromServe, output, cancellationToken);

                while (await read is { } line)
                {
                    await toServe.WriteAsync(line + "\n");
                    await toServe.FlushAsync(cancellationToken);
                    read = input.ReadLineAsync(cancellationToken).AsTask();
                }

                toServe.Close();
                await serve.WaitForExitAsync(cancellationToken);
                await pump;

                return serve.ExitCode;
            }
        }

        /// <summary>Whether a line is the answer to the request with an id.</summary>
        /// <param name="line">The line.</param>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> when it is.</returns>
        private static bool IsAnswerTo(string line, string id)
        {
            try
            {
                return JsonNode.Parse(line) is JsonObject message
                       && message["id"]?.GetValueKind() == JsonValueKind.String
                       && message["id"]!.GetValue<string>() == id;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>Passes every line <c>hypha serve</c> writes on to the client, until it closes its output.</summary>
        /// <param name="fromServe">What <c>hypha serve</c> writes.</param>
        /// <param name="output">The client.</param>
        /// <param name="cancellationToken">Stops the copy.</param>
        /// <returns>A task that completes once <c>hypha serve</c> has closed its output.</returns>
        private static async Task PumpAsync(TextReader fromServe, TextWriter output, CancellationToken cancellationToken)
        {
            while (await fromServe.ReadLineAsync(cancellationToken) is { } line)
            {
                await output.WriteAsync(line + "\n");
                await output.FlushAsync(cancellationToken);
            }
        }

        /// <summary>A notification.</summary>
        /// <param name="method">Its method.</param>
        /// <returns>The notification.</returns>
        private static JsonObject Notification(string method) => new() { ["jsonrpc"] = "2.0", ["method"] = method };

        /// <summary>Writes one message, as a line, and flushes it.</summary>
        /// <param name="output">Where it goes.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">Cancels the flush.</param>
        /// <returns>A task that completes once it is written.</returns>
        private static async Task Send(TextWriter output, JsonObject message, CancellationToken cancellationToken)
        {
            await output.WriteAsync(message.ToJsonString() + "\n");
            await output.FlushAsync(cancellationToken);
        }
    }
}
