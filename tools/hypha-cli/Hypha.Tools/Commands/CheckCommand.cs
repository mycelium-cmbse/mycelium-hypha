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
    using System.CommandLine;
    using System.Linq;
    using System.Text.Json;
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
            new("--json")
            {
                Description = "Print one line of JSON instead of a table - for scripts and the hook.",
                DefaultValueFactory = _ => false,
            };

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
            private readonly IReleaseDiscovery discovery;
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(IReleaseDiscovery discovery, IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(discovery);
                ArgumentNullException.ThrowIfNull(layout);

                this.discovery = discovery;
                this.layout = layout;
            }

            /// <summary>Reports the comparison.</summary>
            /// <returns>0. This command never fails for want of anything to compare.</returns>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var available = await this.discovery.AvailableAsync(cancellationToken);

                var result = new CheckResult(
                    this.layout.InstalledTags,
                    this.layout.DefaultTag,
                    [.. available.Take(CheckResult.MaxAvailableOnline)]);

                if (parseResult.GetValue(Json))
                {
                    // Straight to the underlying writer, not AnsiConsole.Write(IRenderable): a Text
                    // widget wraps to the console width, which would break a long line of JSON across
                    // several lines - exactly what the hook parsing stdout as one line cannot survive.
                    // Still goes through AnsiConsole.Console, so a test that redirects it (as
                    // RecordedConsole does) can capture this the same way it captures everything else.
                    AnsiConsole.Console.Profile.Out.Writer.Write(JsonSerializer.Serialize(result));

                    return 0;
                }

                Report(result);

                return 0;
            }

            private static void Report(CheckResult result)
            {
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
                        result.InstalledTags.Contains(tag, StringComparer.Ordinal) ? "yes" : string.Empty,
                        string.Equals(tag, result.DefaultTag, StringComparison.Ordinal) ? "yes" : string.Empty);
                }

                AnsiConsole.Write(table);

                var newest = result.AvailableOnline.FirstOrDefault();
                if (newest is not null && !result.InstalledTags.Contains(newest, StringComparer.Ordinal))
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]{Markup.Escape(newest)}[/] is newer than anything installed. "
                        + $"Run 'hypha fetch --tag {Markup.Escape(newest)}' to get it.");
                }
            }
        }
    }
}
