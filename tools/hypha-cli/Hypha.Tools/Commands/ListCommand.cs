// ------------------------------------------------------------------------------------------------
// <copyright file="ListCommand.cs" company="Starion Group S.A.">
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

    using Spectre.Console;

    /// <summary>
    /// Lists the releases this checkout carries, according to <c>knowledge/versions.json</c>, and
    /// which of their artifacts have been generated.
    /// </summary>
    public sealed class ListCommand : Command
    {
        /// <summary>Prints machine-readable JSON instead of a table.</summary>
        public static readonly Option<bool> Json =
            JsonOutput.CreateOption("Print one line of JSON instead of a table - for editors and scripts.");

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        public ListCommand()
            : base("list", "List the installed releases, the default, and what has been generated")
        {
            this.Options.Add(Json);
        }

        /// <summary>Runs the <see cref="ListCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Locates the knowledge folder and reads the installed releases and the default from it.</summary>
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="layout">The knowledge layout whose installed releases are listed.</param>
            /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
            public Handler(IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Lists the installed releases.</summary>
            /// <param name="parseResult">The parsed command line holding this verb's options.</param>
            /// <param name="cancellationToken">Cancels the run before it starts.</param>
            /// <returns>
            /// 0 when at least one release is installed, 1 when none is. With <c>--json</c> always 0:
            /// an empty list is an answer, not a failure.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);
                cancellationToken.ThrowIfCancellationRequested();

                var releases = ReleaseStatus.ReadAll(this.layout);

                if (parseResult.GetValue(Json))
                {
                    await JsonOutput.WriteAsync(new ListResult(this.layout.DefaultTag, releases));

                    return 0;
                }

                if (releases.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]No release is installed under {Markup.Escape(this.layout.Root.FullName)}.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha fetch --tag <release>' to install one.[/]");

                    return 1;
                }

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("release");
                table.AddColumn("default");
                table.AddColumn("sources fetched");
                table.AddColumn("generated");
                table.AddColumn("specification text");

                foreach (var release in releases)
                {
                    table.AddRow(
                        Markup.Escape(release.Tag),
                        release.IsDefault ? "yes" : string.Empty,
                        release.SourcesFetched ? "yes" : "[red]no[/]",
                        release.Artifacts.Core ? "yes" : "[red]incomplete[/]",
                        release.Artifacts.Specification ? "yes" : "[grey]no[/]");
                }

                AnsiConsole.Write(table);

                return 0;
            }
        }
    }
}
