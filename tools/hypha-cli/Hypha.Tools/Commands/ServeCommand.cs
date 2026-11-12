// ------------------------------------------------------------------------------------------------
// <copyright file="ServeCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.CommandLine;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;

    using ModelContextProtocol.Server;

    /// <summary>
    /// Runs the hypha MCP server over standard input and output, until the client disconnects.
    /// </summary>
    /// <remarks>
    /// Standard output carries the protocol and nothing else: no banner, and every log line and failure
    /// goes to standard error (<see cref="Owns"/>). An AI client starts it, one server per client, so
    /// every option the other verbs take (<c>--repository-root</c>, <c>--token</c>, <c>--log-level</c>)
    /// applies to it as well.
    /// </remarks>
    public sealed class ServeCommand : Command
    {
        /// <summary>Initializes a new instance of the <see cref="ServeCommand"/> class.</summary>
        public ServeCommand()
            : base("serve", "Run the hypha MCP server over standard input and output, for an AI client to start")
        {
        }

        /// <summary>
        /// Whether <paramref name="parseResult"/> runs this verb, whose standard output belongs to the protocol.
        /// </summary>
        /// <param name="parseResult">The parsed command line.</param>
        /// <returns><c>true</c> when the command that runs is <c>serve</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
        public static bool Owns(ParseResult parseResult)
        {
            ArgumentNullException.ThrowIfNull(parseResult);

            return parseResult.CommandResult.Command is ServeCommand;
        }

        /// <summary>Runs the server.</summary>
        public sealed class Handler
        {
            /// <summary>The server, over the standard input and output transport.</summary>
            private readonly McpServer server;

            /// <summary>Initializes a new instance of the <see cref="Handler"/> class.</summary>
            /// <param name="server">The server, over the standard input and output transport.</param>
            /// <param name="layout">
            /// The knowledge folder the tools answer from. Asking for it resolves it before the server
            /// starts, so a <c>HYPHA_HOME</c> that cannot be used stops <c>serve</c> with its message on
            /// standard error, rather than failing every tool call with no reason the client can see.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="server"/> or <paramref name="layout"/> is <c>null</c>.
            /// </exception>
            public Handler(McpServer server, IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(server);
                ArgumentNullException.ThrowIfNull(layout);

                this.server = server;
            }

            /// <summary>Serves until the client closes standard input or the run is cancelled.</summary>
            /// <param name="cancellationToken">Stops the server.</param>
            /// <returns>0 once the client has disconnected.</returns>
            /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
            public async Task<int> InvokeAsync(CancellationToken cancellationToken = default)
            {
                await this.server.RunAsync(cancellationToken);

                return 0;
            }
        }
    }
}
