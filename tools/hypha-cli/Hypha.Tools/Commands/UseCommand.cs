// ------------------------------------------------------------------------------------------------
// <copyright file="UseCommand.cs" company="Starion Group S.A.">
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
    /// Switches which installed release every skill answers from by default.
    /// </summary>
    public sealed class UseCommand : Command
    {
        /// <summary>The release to make the default.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The installed release to answer from by default.",
                Required = true,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="UseCommand"/> class.
        /// </summary>
        public UseCommand()
            : base("use", "Switch which installed release the plugin answers from by default")
        {
            this.Options.Add(Tag);
        }

        /// <summary>Runs the <see cref="UseCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Switches the default release.</summary>
            private readonly IReleaseSelector selector;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="selector">Switches the default release.</param>
            /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <c>null</c>.</exception>
            public Handler(IReleaseSelector selector)
            {
                ArgumentNullException.ThrowIfNull(selector);

                this.selector = selector;
            }

            /// <summary>Switches the default release.</summary>
            /// <param name="parseResult">The parsed command line.</param>
            /// <param name="cancellationToken">Cancels the switch; observed before the manifest is read.</param>
            /// <returns>0 when switched; 2 when the tag is not installed.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
            public Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                cancellationToken.ThrowIfCancellationRequested();

                var tag = parseResult.GetValue(Tag)!;

                ReleaseSelection selection;
                try
                {
                    selection = this.selector.Use(tag);
                }
                catch (ArgumentException exception)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha fetch --tag <release>' first.[/]");

                    return Task.FromResult(2);
                }

                AnsiConsole.MarkupLine(
                    selection.Changed
                        ? $"[bold]{Markup.Escape(tag)}[/] is now the default release [grey](was {Markup.Escape(selection.Previous)})[/]."
                        : $"[bold]{Markup.Escape(tag)}[/] is already the default.");

                return Task.FromResult(0);
            }
        }
    }
}
