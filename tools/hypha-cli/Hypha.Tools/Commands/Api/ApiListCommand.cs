// ------------------------------------------------------------------------------------------------
// <copyright file="ApiListCommand.cs" company="Starion Group S.A.">
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
    /// <c>hypha api list</c> - shows the installed API versions and which one answers by default.
    /// </summary>
    public sealed class ApiListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiListCommand"/> class.
        /// </summary>
        public ApiListCommand()
            : base("list", "List the installed API versions and which one answers by default")
        {
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            private readonly IApiLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IApiLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Lists the installed API versions.</summary>
            public Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tags = this.layout.InstalledTags;

                if (tags.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No API version is installed.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha api fetch --tag <version>' to install one.[/]");
                    return Task.FromResult(1);
                }

                var defaultTag = this.layout.DefaultTag;

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("API version");
                table.AddColumn("default");
                table.AddColumn("sources fetched");
                table.AddColumn("knowledge generated");

                foreach (var tag in tags)
                {
                    table.AddRow(
                        Markup.Escape(tag),
                        string.Equals(tag, defaultTag, StringComparison.Ordinal) ? "yes" : string.Empty,
                        this.layout.Sources(tag).Exists ? "yes" : "[red]no[/]",
                        this.layout.Knowledge(tag).Exists ? "yes" : "[red]no[/]");
                }

                AnsiConsole.Write(table);

                return Task.FromResult(0);
            }
        }
    }
}
