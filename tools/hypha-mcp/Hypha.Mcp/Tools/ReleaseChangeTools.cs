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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Releases;

    using ModelContextProtocol;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tools that change the installed releases and Systems Modeling API versions: install, switch the
    /// default, remove.
    /// </summary>
    /// <remarks>
    /// Each takes an <c>axis</c>: <c>release</c>, the default, or <c>api</c> for an API version, which has its
    /// own manifest and default. Sharing the three tools keeps the server's tool count down. Each runs in the
    /// server's own process, through <see cref="IReleaseOperations"/> or <see cref="IApiOperations"/>, so it
    /// holds the release-operation lock and waits for another client's change to finish first. The query tools need
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

        /// <summary>Installs, switches and removes API versions under the lock.</summary>
        private readonly IApiOperations apiOperations;

        /// <summary>Finds the newest offerable API version, for an install without a tag.</summary>
        private readonly IApiDiscovery apiDiscovery;

        /// <summary>Initializes a new instance of the <see cref="ReleaseChangeTools"/> class.</summary>
        /// <param name="operations">Installs, switches and removes releases under the lock.</param>
        /// <param name="discovery">Finds the newest offerable release, for an install without a tag.</param>
        /// <param name="apiOperations">Installs, switches and removes API versions under the lock.</param>
        /// <param name="apiDiscovery">Finds the newest offerable API version, for an install without a tag.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ReleaseChangeTools(
            IReleaseOperations operations, IReleaseDiscovery discovery, IApiOperations apiOperations, IApiDiscovery apiDiscovery)
        {
            ArgumentNullException.ThrowIfNull(operations);
            ArgumentNullException.ThrowIfNull(discovery);
            ArgumentNullException.ThrowIfNull(apiOperations);
            ArgumentNullException.ThrowIfNull(apiDiscovery);

            this.operations = operations;
            this.discovery = discovery;
            this.apiOperations = apiOperations;
            this.apiDiscovery = apiDiscovery;
        }

        /// <summary>Fetches a release or an API version and generates its knowledge.</summary>
        /// <param name="tag">The release or API version, or <c>null</c> for the newest offerable one.</param>
        /// <param name="includeSpecifications">Also fetch the specification PDFs, for the specification text.</param>
        /// <param name="makeDefault">Make it the default on its axis once its knowledge is generated.</param>
        /// <param name="axis">Whether <paramref name="tag"/> is a release or an API version.</param>
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
            + "keeps the files it has and generates again. With axis 'api' it installs a Systems Modeling API and "
            + "Services version instead (its PIM, its OpenAPI document and the API specification), which hypha_api "
            + "answers from.")]
        public Task<string> InstallRelease(
            [Description("The release, such as 2026-05, or with axis 'api' the API version, such as 2026-04. Omit it for the newest offered upstream.")] string? tag = null,
            [Description("Also download the OMG specification PDFs and extract their text, which verbatim citations need.")] bool includeSpecifications = true,
            [Description("Make it the default, which every tool answers from unless given a tag (for hypha_api, a version), once its knowledge is generated.")] bool makeDefault = true,
            [Description("'release' (the default) for a SysML v2 / KerML release, or 'api' for a Systems Modeling API version.")] VersionAxis axis = VersionAxis.Release,
            RequestContext<CallToolRequestParams>? context = null,
            CancellationToken cancellationToken = default) =>
            this.InstallAsync(tag, includeSpecifications, makeDefault, axis, Sender(context, cancellationToken), cancellationToken);

        /// <summary>
        /// Fetches a release or an API version and generates its knowledge, sending progress through
        /// <paramref name="send"/>.
        /// </summary>
        /// <param name="tag">The release or API version, or <c>null</c> for the newest offerable one.</param>
        /// <param name="includeSpecifications">Also fetch the specification PDFs, for the specification text.</param>
        /// <param name="makeDefault">Make it the default on its axis once its knowledge is generated.</param>
        /// <param name="axis">Whether <paramref name="tag"/> is a release or an API version.</param>
        /// <param name="send">Sends one progress notification; <c>null</c> sends none.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>What was fetched and what each artifact did, as JSON.</returns>
        /// <exception cref="McpException">The install failed in a way the model can act on; the message says how.</exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        internal Task<string> InstallAsync(
            string? tag,
            bool includeSpecifications,
            bool makeDefault,
            VersionAxis axis,
            Func<ProgressNotificationValue, Task>? send,
            CancellationToken cancellationToken) =>
            Answer.OfAsync(
                async () =>
                {
                    await using var forwarder = new ProgressForwarder(send);

                    var noun = axis == VersionAxis.Api ? "API version" : "release";

                    forwarder.Start(
                        string.IsNullOrWhiteSpace(tag)
                            ? $"Finding the newest {noun} offered upstream, then installing it"
                            : $"Installing {noun} {tag.Trim()}, once any other release change has finished");

                    if (axis == VersionAxis.Api)
                    {
                        var version = string.IsNullOrWhiteSpace(tag) ? await this.NewestApiAsync(cancellationToken) : tag.Trim();

                        var api = await this.apiOperations.InstallAsync(
                            new ApiInstallRequest
                            {
                                Tag = version,
                                IncludeSpecification = includeSpecifications,
                                SkipExisting = true,
                                MakeDefault = makeDefault,
                            },
                            forwarder,
                            cancellationToken);

                        return InstallAnswer.From(api);
                    }

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

        /// <summary>Makes an installed release, or API version, the default.</summary>
        /// <param name="tag">An installed release or API version.</param>
        /// <param name="axis">Whether <paramref name="tag"/> is a release or an API version.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>The new default and the one before it, as JSON.</returns>
        /// <exception cref="McpException">The release is not installed, or another client's change did not finish in time.</exception>
        /// <exception cref="OperationCanceledException">The client cancelled the call.</exception>
        [McpServerTool(Name = "hypha_use_release", Title = "Use release", ReadOnly = false, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Makes an installed SysML v2 / KerML release the default, which every tool answers from unless given a "
            + "tag. Only when the user asks to switch releases; see hypha_release_status for what is installed. With "
            + "axis 'api' it switches the default Systems Modeling API version, which hypha_api answers from.")]
        public Task<string> UseRelease(
            [Description("An installed release, such as 2026-05, or with axis 'api' an installed API version, such as 2026-04.")] string tag,
            [Description("'release' (the default) for a SysML v2 / KerML release, or 'api' for a Systems Modeling API version.")] VersionAxis axis = VersionAxis.Release,
            CancellationToken cancellationToken = default) =>
            Answer.OfAsync(
                async () =>
                {
                    try
                    {
                        var selection = axis == VersionAxis.Api
                            ? await this.apiOperations.UseAsync(tag, cancellationToken)
                            : await this.operations.UseAsync(tag, cancellationToken);

                        return new UseAnswer(selection.Default, selection.Previous, selection.Changed, Axes.Of(axis));
                    }
                    catch (ArgumentException exception)
                    {
                        var install = axis == VersionAxis.Api ? "hypha_install_release with axis 'api'" : "hypha_install_release";

                        throw new McpException($"{exception.Message}. Install it with {install} first.", exception);
                    }
                },
                McpJsonContext.Readable.UseAnswer);

        /// <summary>Removes an installed release, or API version.</summary>
        /// <param name="tag">The installed release or API version to remove.</param>
        /// <param name="force">Remove it even when it is the only one installed on its axis.</param>
        /// <param name="axis">Whether <paramref name="tag"/> is a release or an API version.</param>
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
            + "Only when the user asks to remove that release. With axis 'api' it removes a Systems Modeling API "
            + "version instead; the default API version cannot be removed either.")]
        public Task<string> RemoveRelease(
            [Description("The installed release to remove, such as 2026-02, or with axis 'api' the API version.")] string tag,
            [Description("Remove it even when it is the only one installed, which leaves nothing to answer from.")] bool force = false,
            [Description("'release' (the default) for a SysML v2 / KerML release, or 'api' for a Systems Modeling API version.")] VersionAxis axis = VersionAxis.Release,
            CancellationToken cancellationToken = default) =>
            Answer.OfAsync(
                async () =>
                {
                    if (axis == VersionAxis.Api)
                    {
                        await this.apiOperations.RemoveAsync(tag, force, cancellationToken);
                    }
                    else
                    {
                        await this.operations.RemoveAsync(tag, force, cancellationToken);
                    }

                    return new RemoveAnswer(tag, Axes.Of(axis));
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

        /// <summary>The newest API version offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The newest offerable API version.</returns>
        /// <exception cref="McpException">Nothing is offerable upstream.</exception>
        private async Task<string> NewestApiAsync(CancellationToken cancellationToken)
        {
            var available = await this.apiDiscovery.AvailableAsync(cancellationToken);

            return available.Count > 0
                ? available[0]
                : throw new McpException("No Systems Modeling API version is offered upstream: no API-Services tag is also a release tag.");
        }

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
