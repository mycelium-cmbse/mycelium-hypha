// ------------------------------------------------------------------------------------------------
// <copyright file="MigrateCommand.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Migration;

    using Spectre.Console;

    /// <summary>
    /// Copies the releases the Claude plugin and the VS Code extension installed in their own folders into
    /// <c>HYPHA_HOME</c>, on request.
    /// </summary>
    /// <remarks>
    /// <c>hypha</c> does this once on its own, the first time it reads an empty <c>HYPHA_HOME</c>; this verb
    /// runs it again, adding whatever <c>HYPHA_HOME</c> still lacks, or copies from a folder named with
    /// <c>--from</c>.
    /// </remarks>
    public sealed class MigrateCommand : Command
    {
        /// <summary>The client folders to copy from instead of the ones found.</summary>
        public static readonly Option<DirectoryInfo[]> From =
            new("--from")
            {
                Description = "A folder holding knowledge/ to copy from, instead of the Claude plugin's and the VS Code extension's; repeatable.",
                AllowMultipleArgumentsPerToken = true,
            };

        /// <summary>Initializes a new instance of the <see cref="MigrateCommand"/> class.</summary>
        public MigrateCommand()
            : base("migrate", "Copy the releases the Claude plugin and the VS Code extension installed into HYPHA_HOME")
        {
            this.Options.Add(From);
        }

        /// <summary>Runs the <see cref="MigrateCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Copies the releases.</summary>
            private readonly IKnowledgeMigration migration;

            /// <summary>Initializes a new instance of the <see cref="Handler"/> class.</summary>
            /// <param name="migration">Copies the releases.</param>
            /// <exception cref="ArgumentNullException"><paramref name="migration"/> is <c>null</c>.</exception>
            public Handler(IKnowledgeMigration migration)
            {
                ArgumentNullException.ThrowIfNull(migration);

                this.migration = migration;
            }

            /// <summary>Copies what <c>HYPHA_HOME</c> lacks and says what it copied.</summary>
            /// <param name="parseResult">The parsed command line.</param>
            /// <param name="cancellationToken">Cancels the copy between files.</param>
            /// <returns>0, whether or not anything was copied.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            /// <exception cref="Hypha.Knowledge.Releases.ReleaseOperationLockException">Another hypha is changing releases.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var result = await this.migration.MigrateAsync(parseResult.GetValue(From), cancellationToken);

                if (result.Copied.Count == 0)
                {
                    AnsiConsole.MarkupLine($"Nothing to copy into [bold]{Markup.Escape(result.Home.FullName)}[/]: it has every release the clients installed.");

                    return 0;
                }

                foreach (var copied in result.Copied)
                {
                    AnsiConsole.MarkupLine(
                        $"Copied {(copied.Kind == MigratedVersion.Api ? "API version" : "release")} [bold]{Markup.Escape(copied.Tag)}[/] "
                        + $"from {Markup.Escape(copied.From)}.");
                }

                AnsiConsole.MarkupLine($"[grey]Into {Markup.Escape(result.Home.FullName)}.[/]");

                return 0;
            }
        }
    }
}
