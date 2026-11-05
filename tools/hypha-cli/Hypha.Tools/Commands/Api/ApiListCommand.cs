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
    /// <c>hypha api list</c> - shows the installed API versions, which one answers by default, and
    /// which of their artifacts have been generated.
    /// </summary>
    public sealed class ApiListCommand : Command
    {
        /// <summary>Prints machine-readable JSON instead of a table.</summary>
        public static readonly Option<bool> Json =
            JsonOutput.CreateOption("Print one line of JSON instead of a table - for editors and scripts.");

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiListCommand"/> class.
        /// </summary>
        public ApiListCommand()
            : base("list", "List the installed API versions, the default, and what has been generated")
        {
            this.Options.Add(Json);
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            /// <summary>Locates the API knowledge folder and reads the installed versions and the default from it.</summary>
            private readonly IApiLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="layout">The API layout whose installed versions are listed.</param>
            /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
            public Handler(IApiLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Lists the installed API versions.</summary>
            /// <param name="parseResult">The parsed command line holding this verb's options.</param>
            /// <param name="cancellationToken">Cancels the run before it starts.</param>
            /// <returns>
            /// 0 when at least one API version is installed, 1 when none is. With <c>--json</c> always 0:
            /// an empty list is an answer, not a failure.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);
                cancellationToken.ThrowIfCancellationRequested();

                var versions = ApiVersionStatus.ReadAll(this.layout);

                if (parseResult.GetValue(Json))
                {
                    await JsonOutput.WriteAsync(new ApiListResult(this.layout.DefaultTag, versions));

                    return 0;
                }

                if (versions.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No API version is installed.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha api fetch --tag <version>' to install one.[/]");

                    return 1;
                }

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("API version");
                table.AddColumn("default");
                table.AddColumn("sources fetched");
                table.AddColumn("knowledge generated");
                table.AddColumn("specification text");

                foreach (var version in versions)
                {
                    table.AddRow(
                        Markup.Escape(version.Tag),
                        version.IsDefault ? "yes" : string.Empty,
                        version.SourcesFetched ? "yes" : "[red]no[/]",
                        version.Artifacts.Core ? "yes" : "[red]incomplete[/]",
                        version.Artifacts.Specification ? "yes" : "[grey]no[/]");
                }

                AnsiConsole.Write(table);

                return 0;
            }
        }
    }
}
