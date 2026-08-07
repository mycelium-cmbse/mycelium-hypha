// ------------------------------------------------------------------------------------------------
// <copyright file="DiscoverCommand.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Releases;

    using Spectre.Console;

    /// <summary>
    /// Lists the releases that can be installed: release-shaped tags present in both upstreams.
    /// </summary>
    public sealed class DiscoverCommand : Command
    {
        /// <summary>How many releases to show.</summary>
        public static readonly Option<int> Limit =
            new("--limit")
            {
                Description = "How many releases to list, newest first. 0 lists them all.",
                DefaultValueFactory = _ => 10,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoverCommand"/> class.
        /// </summary>
        public DiscoverCommand()
            : base("discover", "List the upstream releases hypha can be generated for")
        {
            this.Options.Add(Limit);
        }

        /// <summary>Runs the <see cref="DiscoverCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReleaseDiscovery discovery;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IReleaseDiscovery discovery)
            {
                ArgumentNullException.ThrowIfNull(discovery);

                this.discovery = discovery;
            }

            /// <summary>Lists the available releases.</summary>
            /// <returns>0 when at least one release was found, 1 when none was.</returns>
            public async Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var limit = parseResult.GetValue(Limit);
                var available = await this.discovery.AvailableAsync(cancellationToken);

                if (available.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No release is offered by both upstreams.[/]");

                    return 1;
                }

                var shown = limit > 0 && limit < available.Count ? limit : available.Count;

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("release");

                for (var index = 0; index < shown; index++)
                {
                    table.AddRow(Markup.Escape(available[index]));
                }

                AnsiConsole.Write(table);

                if (shown < available.Count)
                {
                    AnsiConsole.MarkupLine(
                        $"[grey]{available.Count - shown} older releases not shown; --limit 0 lists them all.[/]");
                }

                return 0;
            }
        }
    }
}
