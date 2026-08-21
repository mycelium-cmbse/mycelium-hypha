// ------------------------------------------------------------------------------------------------
// <copyright file="RemoveCommand.cs" company="Starion Group S.A.">
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
    /// Removes one installed release: its <c>sources/</c> and <c>knowledge/</c> folders, and its
    /// entry in the version manifest.
    /// </summary>
    /// <remarks>
    /// Never automatic - this only ever runs because a user asked for exactly this tag to go, unlike
    /// <c>hypha move-window</c>'s window eviction.
    /// </remarks>
    public sealed class RemoveCommand : Command
    {
        /// <summary>The release to remove.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The installed release to remove.",
                Required = true,
            };

        /// <summary>Removes the last installed release too.</summary>
        public static readonly Option<bool> Force =
            new("--force")
            {
                Description = "Remove the release even if it is the only one installed.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveCommand"/> class.
        /// </summary>
        public RemoveCommand()
            : base("remove", "Remove one installed release's sources and knowledge")
        {
            this.Options.Add(Tag);
            this.Options.Add(Force);
        }

        /// <summary>Runs the <see cref="RemoveCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReleaseRemover remover;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IReleaseRemover remover)
            {
                ArgumentNullException.ThrowIfNull(remover);

                this.remover = remover;
            }

            /// <summary>Removes the release.</summary>
            /// <returns>
            /// 0 when removed; 2 when the tag is not installed, is the default, or is the only
            /// installed release without <c>--force</c>.
            /// </returns>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var tag = parseResult.GetValue(Tag)!;
                var force = parseResult.GetValue(Force);

                try
                {
                    await this.remover.RemoveAsync(tag, force, cancellationToken);
                }
                catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");

                    return 2;
                }

                AnsiConsole.MarkupLine($"Removed [bold]{Markup.Escape(tag)}[/].");

                return 0;
            }
        }
    }
}
