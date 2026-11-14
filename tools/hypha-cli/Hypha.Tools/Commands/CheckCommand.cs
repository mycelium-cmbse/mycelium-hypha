// ------------------------------------------------------------------------------------------------
// <copyright file="CheckCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    using Spectre.Console;

    /// <summary>
    /// Compares what is installed locally against what is offerable upstream. It is fast, unattended, and
    /// read-only: no fetching, no generating.
    /// </summary>
    /// <remarks>
    /// The MCP server's <c>hypha_check_releases</c> runs the same check: cheap enough (a couple of
    /// GitHub API calls) to run whenever the user asks about updates. Deciding whether to act on what
    /// it reports is left entirely to the user; this command never fetches.
    /// </remarks>
    public sealed class CheckCommand : Command
    {
        /// <summary>Prints machine-readable JSON instead of a table.</summary>
        public static readonly Option<bool> Json =
            JsonOutput.CreateOption("Print one line of JSON instead of a table, for scripts.");

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckCommand"/> class.
        /// </summary>
        public CheckCommand()
            : base("check", "Compare installed releases against what is offerable upstream")
        {
            this.Options.Add(Json);
        }

        /// <summary>Runs the <see cref="CheckCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Compares the installed releases with the ones offered upstream.</summary>
            private readonly IReleaseCheck check;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="check">Compares the installed releases with the ones offered upstream.</param>
            /// <exception cref="ArgumentNullException"><paramref name="check"/> is <c>null</c>.</exception>
            public Handler(IReleaseCheck check)
            {
                ArgumentNullException.ThrowIfNull(check);

                this.check = check;
            }

            /// <summary>Reports the comparison.</summary>
            /// <param name="parseResult">The parsed command line holding this verb's options.</param>
            /// <param name="cancellationToken">Cancels the upstream discovery.</param>
            /// <returns>0. This command never fails for want of anything to compare.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                // The language releases only: a script may run this often, and an API check would cost it
                // GitHub requests the anonymous rate limit can ill afford.
                var comparison = await this.check.CheckReleasesAsync(cancellationToken);

                var result = new CheckResult(comparison.Installed, comparison.Default, comparison.Offered);

                if (parseResult.GetValue(Json))
                {
                    // One unwrapped line: a script parses standard output as a single JSON document.
                    await JsonOutput.WriteAsync(result);

                    return 0;
                }

                Report(result);

                return 0;
            }

            /// <summary>
            /// Prints the comparison for a person: one table row per installed or offerable release, newest first,
            /// then a hint to fetch the newest upstream release when it is not installed.
            /// </summary>
            /// <param name="result">The comparison to print.</param>
            private static void Report(CheckResult result)
            {
                var installed = new HashSet<string>(result.InstalledTags, StringComparer.Ordinal);

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("release");
                table.AddColumn("installed");
                table.AddColumn("default");

                var every = result.InstalledTags
                    .Concat(result.AvailableOnline)
                    .Distinct(StringComparer.Ordinal)
                    .OrderByDescending(tag => tag, StringComparer.Ordinal);

                foreach (var tag in every)
                {
                    table.AddRow(
                        Markup.Escape(tag),
                        installed.Contains(tag) ? "yes" : string.Empty,
                        string.Equals(tag, result.DefaultTag, StringComparison.Ordinal) ? "yes" : string.Empty);
                }

                AnsiConsole.Write(table);

                var newest = result.AvailableOnline.Count > 0 ? result.AvailableOnline[0] : null;
                if (newest is not null && !installed.Contains(newest))
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]{Markup.Escape(newest)}[/] is newer than anything installed. "
                        + $"Run 'hypha fetch --tag {Markup.Escape(newest)}' to get it.");
                }
            }
        }
    }
}
