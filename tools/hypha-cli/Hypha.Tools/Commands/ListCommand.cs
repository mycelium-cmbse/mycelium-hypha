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
    /// Lists the releases this checkout carries, according to <c>knowledge/versions.json</c>.
    /// </summary>
    public sealed class ListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        public ListCommand()
            : base("list", "List the installed releases and which one answers by default")
        {
        }

        /// <summary>Runs the <see cref="ListCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(layout);

                this.layout = layout;
            }

            /// <summary>Lists the installed releases.</summary>
            /// <returns>0 when at least one release is installed, 1 when none is.</returns>
            public Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tags = this.layout.InstalledTags;

                if (tags.Count == 0)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]No release is installed under {Markup.Escape(this.layout.Root.FullName)}.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha fetch --tag <release>' to install one.[/]");

                    return Task.FromResult(1);
                }

                var defaultTag = this.layout.DefaultTag;

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("release");
                table.AddColumn("default");
                table.AddColumn("sources fetched");

                foreach (var tag in tags)
                {
                    table.AddRow(
                        Markup.Escape(tag),
                        string.Equals(tag, defaultTag, StringComparison.Ordinal) ? "yes" : string.Empty,
                        this.layout.Xmi(tag).Exists || this.layout.TextualSources(tag).Exists
                            ? "yes"
                            : "[red]no[/]");
                }

                AnsiConsole.Write(table);

                return Task.FromResult(0);
            }
        }
    }
}
