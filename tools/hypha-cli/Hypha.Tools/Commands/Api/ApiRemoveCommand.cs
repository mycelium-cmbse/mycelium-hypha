// ------------------------------------------------------------------------------------------------
// <copyright file="ApiRemoveCommand.cs" company="Starion Group S.A.">
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
    /// <c>hypha api remove</c> - removes one installed API version's sources and knowledge.
    /// </summary>
    public sealed class ApiRemoveCommand : Command
    {
        /// <summary>The installed API version to remove.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The installed API version to remove.",
                Required = true,
            };

        /// <summary>Removes the last installed API version too.</summary>
        public static readonly Option<bool> Force =
            new("--force")
            {
                Description = "Remove the API version even if it is the only one installed.",
                DefaultValueFactory = _ => false,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRemoveCommand"/> class.
        /// </summary>
        public ApiRemoveCommand()
            : base("remove", "Remove one installed API version's sources and knowledge")
        {
            this.Options.Add(Tag);
            this.Options.Add(Force);
        }

        /// <summary>Runs the command.</summary>
        public sealed class Handler
        {
            private readonly IApiRemover remover;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IApiRemover remover)
            {
                ArgumentNullException.ThrowIfNull(remover);

                this.remover = remover;
            }

            /// <summary>Removes the requested API version.</summary>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var tag = parseResult.GetValue(Tag)!;

                try
                {
                    await this.remover.RemoveAsync(tag, parseResult.GetValue(Force), cancellationToken);
                }
                catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");
                    return 2;
                }

                AnsiConsole.MarkupLine($"Removed API version [bold]{Markup.Escape(tag)}[/].");

                return 0;
            }
        }
    }
}
