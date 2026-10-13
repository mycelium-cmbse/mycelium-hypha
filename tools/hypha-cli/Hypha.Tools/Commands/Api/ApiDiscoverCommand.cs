// ------------------------------------------------------------------------------------------------
// <copyright file="ApiDiscoverCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands.Api
{
    using System;
    using System.CommandLine;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;

    using Spectre.Console;

    /// <summary>
    /// <c>hypha api discover</c> - lists the API versions the upstreams offer.
    /// </summary>
    public sealed class ApiDiscoverCommand : Command
    {
        /// <summary>How many versions to list.</summary>
        public static readonly Option<int> Limit =
            new("--limit")
            {
                Description = "How many API versions to list, newest first. 0 lists them all.",
                DefaultValueFactory = _ => 10,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDiscoverCommand"/> class.
        /// </summary>
        public ApiDiscoverCommand()
            : base("discover", "List the upstream API versions hypha can be generated for")
        {
            this.Options.Add(Limit);
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            private readonly IApiDiscovery discovery;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IApiDiscovery discovery)
            {
                ArgumentNullException.ThrowIfNull(discovery);

                this.discovery = discovery;
            }

            /// <summary>Lists the offerable API versions.</summary>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var limit = parseResult.GetValue(Limit);
                var available = await this.discovery.AvailableAsync(cancellationToken);

                if (available.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No API version is offered by the upstreams.[/]");
                    return 1;
                }

                var shown = limit > 0 && limit < available.Count ? limit : available.Count;

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("API version");

                for (var index = 0; index < shown; index++)
                {
                    table.AddRow(Markup.Escape(available[index]));
                }

                AnsiConsole.Write(table);

                if (shown < available.Count)
                {
                    AnsiConsole.MarkupLine(
                        $"[grey]{available.Count - shown} older API versions not shown; --limit 0 lists them all.[/]");
                }

                return 0;
            }
        }
    }
}
