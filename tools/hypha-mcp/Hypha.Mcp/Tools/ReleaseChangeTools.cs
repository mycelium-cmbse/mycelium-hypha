// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseChangeTools.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tools that change the installed releases: install, switch the default, remove.
    /// </summary>
    /// <remarks>
    /// Each runs in the server's own process, through <see cref="IReleaseOperations"/>, so it holds the
    /// release-operation lock and waits for another client's change to finish first. The query tools need
    /// no telling afterwards: they read the version manifest on every call, and read a generated file
    /// again once it has changed on disk.
    /// </remarks>
    [McpServerToolType]
    public sealed class ReleaseChangeTools
    {
        /// <summary>Installs, switches and removes releases under the lock.</summary>
        private readonly IReleaseOperations operations;

        /// <summary>Finds the newest offerable release, for an install without a tag.</summary>
        private readonly IReleaseDiscovery discovery;

        /// <summary>Initializes a new instance of the <see cref="ReleaseChangeTools"/> class.</summary>
        /// <param name="operations">Installs, switches and removes releases under the lock.</param>
        /// <param name="discovery">Finds the newest offerable release, for an install without a tag.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ReleaseChangeTools(IReleaseOperations operations, IReleaseDiscovery discovery)
        {
            ArgumentNullException.ThrowIfNull(operations);
            ArgumentNullException.ThrowIfNull(discovery);

            this.operations = operations;
            this.discovery = discovery;
        }

        /// <summary>Fetches a release and generates its knowledge.</summary>
        /// <param name="tag">The release, or <c>null</c> for the newest offerable one.</param>
        /// <param name="includeSpecifications">Also fetch the specification PDFs, for the specification text.</param>
        /// <param name="makeDefault">Make the release the default once fetched.</param>
        /// <param name="context">
        /// The call, bound by the SDK and not part of the tool's schema: its progress token, and the server
        /// to send progress notifications through. <c>null</c> sends none.
        /// </param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>What was fetched and what each artifact did, as JSON.</returns>
        /// <exception cref="McpException">
        /// The tag is not an offerable release, nothing is offerable upstream, another client's change did
        /// not finish in time, or the upstream could not be reached; the message says which.
        /// </exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        [McpServerTool(Name = "hypha_install_release", Title = "Install release", ReadOnly = false, Idempotent = true, OpenWorld = true, Destructive = false)]
        [Description(
            "Installs a SysML v2 / KerML release on this machine: downloads its sources from the OMG GitHub "
            + "repositories and generates the knowledge every other hypha tool answers from (metamodel, textual "
            + "notation, model library, specification text, cross-references). Takes minutes and reports progress. "
            + "Only when the user asks to install or update a release. Installing one that is already installed "
            + "keeps the files it has and generates again.")]
        public Task<string> InstallRelease(
            [Description("The release, such as 2026-05. Omit it for the newest release offered upstream.")] string? tag = null,
            [Description("Also download the OMG specification PDFs and extract their text, which verbatim citations need.")] bool includeSpecifications = true,
            [Description("Make it the default release, which every tool answers from unless given a tag, once its knowledge is generated.")] bool makeDefault = true,
            RequestContext<CallToolRequestParams>? context = null,
            CancellationToken cancellationToken = default) =>
            this.InstallAsync(tag, includeSpecifications, makeDefault, Sender(context, cancellationToken), cancellationToken);

        /// <summary>Fetches a release and generates its knowledge, sending progress through <paramref name="send"/>.</summary>
        /// <param name="tag">The release, or <c>null</c> for the newest offerable one.</param>
        /// <param name="includeSpecifications">Also fetch the specification PDFs, for the specification text.</param>
        /// <param name="makeDefault">Make the release the default once fetched.</param>
        /// <param name="send">Sends one progress notification; <c>null</c> sends none.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>What was fetched and what each artifact did, as JSON.</returns>
        /// <exception cref="McpException">The install failed in a way the model can act on; the message says how.</exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        internal Task<string> InstallAsync(
            string? tag,
            bool includeSpecifications,
            bool makeDefault,
            Func<ProgressNotificationValue, Task>? send,
            CancellationToken cancellationToken) =>
            Answer.OfAsync(
                async () =>
                {
                    await using var forwarder = new ProgressForwarder(send);

                    forwarder.Start(
                        string.IsNullOrWhiteSpace(tag)
                            ? "Finding the newest release offered upstream, then installing it"
                            : $"Installing {tag.Trim()}, once any other release change has finished");

                    var release = string.IsNullOrWhiteSpace(tag) ? await this.NewestAsync(cancellationToken) : tag.Trim();

                    var setup = await this.operations.InstallAsync(
                        new ReleaseInstallRequest
                        {
                            Tag = release,
                            IncludeSpecifications = includeSpecifications,
                            SkipExisting = true,
                            MakeDefault = makeDefault,
                        },
                        forwarder,
                        cancellationToken);

                    return InstallAnswer.From(setup);
                },
                McpJsonContext.Readable.InstallAnswer);

        /// <summary>Makes an installed release the default.</summary>
        /// <param name="tag">An installed release.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>The new default and the one before it, as JSON.</returns>
        /// <exception cref="McpException">The release is not installed, or another client's change did not finish in time.</exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        [McpServerTool(Name = "hypha_use_release", Title = "Use release", ReadOnly = false, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Makes an installed SysML v2 / KerML release the default, which every tool answers from unless given a "
            + "tag. Only when the user asks to switch releases; see hypha_release_status for what is installed.")]
        public Task<string> UseRelease(
            [Description("An installed release, such as 2026-05.")] string tag,
            CancellationToken cancellationToken = default) =>
            Answer.OfAsync(
                async () =>
                {
                    try
                    {
                        var selection = await this.operations.UseAsync(tag, cancellationToken);

                        return new UseAnswer(selection.Default, selection.Previous, selection.Changed);
                    }
                    catch (ArgumentException exception)
                    {
                        throw new McpException($"{exception.Message}. Install it with hypha_install_release first.", exception);
                    }
                },
                McpJsonContext.Readable.UseAnswer);

        /// <summary>Removes an installed release.</summary>
        /// <param name="tag">The installed release to remove.</param>
        /// <param name="force">Remove it even when it is the only installed release.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>The release removed, as JSON.</returns>
        /// <exception cref="McpException">
        /// The release is not installed, is the default, is the only one and <paramref name="force"/> was not
        /// given, or another client's change did not finish in time.
        /// </exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        [McpServerTool(Name = "hypha_remove_release", Title = "Remove release", ReadOnly = false, Idempotent = false, OpenWorld = false, Destructive = true)]
        [Description(
            "Deletes an installed SysML v2 / KerML release from this machine: its downloaded sources and its "
            + "generated knowledge. The default release cannot be removed; switch with hypha_use_release first. "
            + "Only when the user asks to remove that release.")]
        public Task<string> RemoveRelease(
            [Description("The installed release to remove, such as 2026-02.")] string tag,
            [Description("Remove it even when it is the only installed release, which leaves nothing to answer from.")] bool force = false,
            CancellationToken cancellationToken = default) =>
            Answer.OfAsync(
                async () =>
                {
                    await this.operations.RemoveAsync(tag, force, cancellationToken);

                    return new RemoveAnswer(tag);
                },
                McpJsonContext.Readable.RemoveAnswer);

        /// <summary>
        /// Sends a progress notification for the call, awaiting it, so that the notifications reach the
        /// client in the order they were sent; <c>null</c> when the client asked for no progress.
        /// </summary>
        /// <param name="context">The call.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>The sender, or <c>null</c>.</returns>
        private static Func<ProgressNotificationValue, Task>? Sender(RequestContext<CallToolRequestParams>? context, CancellationToken cancellationToken) =>
            context?.Params?.ProgressToken is { } token
                ? async value => await context.Server.NotifyProgressAsync(token, value, cancellationToken: cancellationToken)
                : null;

        /// <summary>The newest release offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The newest offerable tag.</returns>
        /// <exception cref="McpException">Nothing is offerable upstream.</exception>
        private async Task<string> NewestAsync(CancellationToken cancellationToken)
        {
            var available = await this.discovery.AvailableAsync(cancellationToken);

            return available.Count > 0
                ? available[0]
                : throw new McpException("No release is offered upstream: no tag is in both the SysML v2 Release and Pilot Implementation repositories.");
        }
    }
}
