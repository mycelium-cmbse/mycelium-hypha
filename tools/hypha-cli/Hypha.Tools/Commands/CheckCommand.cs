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

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Spectre.Console;

    /// <summary>
    /// Compares what is installed locally against what is offerable upstream - fast, unattended, and
    /// read-only: no fetching, no generating.
    /// </summary>
    /// <remarks>
    /// This is what the plugin's <c>SessionStart</c> hook runs, synchronously, every session: cheap
    /// enough (a couple of GitHub API calls) that it needs no background process, lock file or status
    /// file the way the fetch+generate it used to trigger automatically did. Deciding whether to act
    /// on what it reports is left entirely to the user, through a skill - this command never fetches.
    /// </remarks>
    public sealed class CheckCommand : Command
    {
        /// <summary>Prints machine-readable JSON instead of a table.</summary>
        public static readonly Option<bool> Json =
            JsonOutput.CreateOption("Print one line of JSON instead of a table - for scripts and the hook.");

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
            /// <summary>Asks the upstream which releases can be fetched.</summary>
            private readonly IReleaseDiscovery discovery;

            /// <summary>Reports which releases are installed locally and which one is the default.</summary>
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="discovery">Lists the releases offerable upstream, newest first.</param>
            /// <param name="layout">The knowledge layout whose installed releases are compared against them.</param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="discovery"/> or <paramref name="layout"/> is <c>null</c>.
            /// </exception>
            public Handler(IReleaseDiscovery discovery, IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(discovery);
                ArgumentNullException.ThrowIfNull(layout);

                this.discovery = discovery;
                this.layout = layout;
            }

            /// <summary>Reports the comparison.</summary>
            /// <param name="parseResult">The parsed command line holding this verb's options.</param>
            /// <param name="cancellationToken">Cancels the upstream discovery.</param>
            /// <returns>0. This command never fails for want of anything to compare.</returns>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var available = await this.discovery.AvailableAsync(cancellationToken);

                var result = new CheckResult(
                    this.layout.InstalledTags, this.layout.DefaultTag, [.. available.Take(CheckResult.MaxAvailableOnline)]);

                if (parseResult.GetValue(Json))
                {
                    // One unwrapped line: the hook parses standard output as a single JSON document.
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
