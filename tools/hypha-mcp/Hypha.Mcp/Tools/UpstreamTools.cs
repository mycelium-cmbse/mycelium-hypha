// ------------------------------------------------------------------------------------------------
// <copyright file="UpstreamTools.cs" company="Starion Group S.A.">
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
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tool that compares what is installed with what is offered upstream: what the plugin's
    /// <c>SessionStart</c> hook told the user before the MCP server, now asked by the model instead.
    /// </summary>
    /// <remarks>
    /// The one read-only tool that reaches the network, so it is marked open-world, apart from the query
    /// tools that answer from this machine alone.
    /// </remarks>
    [McpServerToolType]
    public sealed class UpstreamTools
    {
        /// <summary>Compares the installed releases with the ones offered upstream.</summary>
        private readonly IReleaseCheck check;

        /// <summary>Initializes a new instance of the <see cref="UpstreamTools"/> class.</summary>
        /// <param name="check">Compares the installed releases with the ones offered upstream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="check"/> is <c>null</c>.</exception>
        public UpstreamTools(IReleaseCheck check)
        {
            ArgumentNullException.ThrowIfNull(check);

            this.check = check;
        }

        /// <summary>Compares what is installed with what is offered upstream.</summary>
        /// <param name="api">Also compare the Systems Modeling API versions.</param>
        /// <param name="cancellationToken">Cancelled when the client cancels the call.</param>
        /// <returns>The comparisons, as JSON.</returns>
        /// <exception cref="McpException">GitHub could not be reached; the message says how to fix it.</exception>
        [McpServerTool(Name = "hypha_check_releases", Title = "Check for newer releases", ReadOnly = true, Idempotent = true, OpenWorld = true, Destructive = false)]
        [Description(
            "Compares the SysML v2 / KerML releases, and the Systems Modeling API versions, installed on this machine "
            + "with the ones offered upstream on GitHub. For each, 'newer' names the newest offered one that is newer than "
            + "everything installed, or is null (for the API, also when no API version is installed). Asks GitHub, so call "
            + "it when the user asks about updates or once at the start of a session. When releases.newer is set, tell the "
            + "user and offer hypha_install_release; when api.newer is, offer hypha_install_release with axis 'api'.")]
        public Task<string> CheckReleases(
            [Description("Also compare the Systems Modeling API versions.")] bool api = true,
            CancellationToken cancellationToken = default) =>
            Answer.OfAsync(
                async () =>
                {
                    if (!api)
                    {
                        return new CheckAnswer(await this.check.CheckReleasesAsync(cancellationToken), null);
                    }

                    var both = await this.check.CheckReleasesAndApiAsync(cancellationToken);

                    return new CheckAnswer(both.Releases, both.Api);
                },
                McpJsonContext.Readable.CheckAnswer);
    }
}
