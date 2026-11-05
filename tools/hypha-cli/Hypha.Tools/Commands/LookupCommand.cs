// ------------------------------------------------------------------------------------------------
// <copyright file="LookupCommand.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Lookup;

    using Spectre.Console;

    /// <summary>
    /// Resolves a metamodel or model-library name in an installed release and prints one page of its
    /// generated source. Read-only: never fetches or generates.
    /// </summary>
    public sealed class LookupCommand : Command
    {
        /// <summary>Which index to search.</summary>
        public static readonly Option<string> Kind =
            new Option<string>("--kind", "-k")
            {
                Description = "metamodel or model-library.",
                Required = true,
            }.AcceptOnlyFromAmong("metamodel", "model-library");

        /// <summary>The name to look up.</summary>
        public static readonly Option<string> ElementName =
            new("--name", "-n")
            {
                Description = "A short or qualified name, e.g. PartUsage, Real or ScalarValues::Real.",
                Required = true,
            };

        /// <summary>The release; the default release when omitted.</summary>
        public static readonly Option<string?> Tag =
            new("--tag", "-t")
            {
                Description = "The installed release to answer from. The default release when omitted.",
            };

        /// <summary>The first line of the page to print.</summary>
        public static readonly Option<int?> StartLine =
            new("--start-line")
            {
                Description =
                    "The first line to print, 1-based. Defaults to the declaration for a model-library "
                    + "name and to the top of the page for a metamodel name.",
            };

        /// <summary>How many lines to print.</summary>
        public static readonly Option<int> MaxLines =
            new("--max-lines")
            {
                Description = $"How many lines to print, at most {LookupRequest.LargestPage}.",
                DefaultValueFactory = _ => LookupRequest.DefaultMaxLines,
            };

        /// <summary>Prints machine-readable JSON instead of the page.</summary>
        public static readonly Option<bool> Json =
            JsonOutput.CreateOption("Print one line of JSON - for editors, language-model tools and scripts.");

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupCommand"/> class.
        /// </summary>
        public LookupCommand()
            : base("lookup", "Print the generated page of a metamodel element or model-library declaration")
        {
            this.Options.Add(Kind);
            this.Options.Add(ElementName);
            this.Options.Add(Tag);
            this.Options.Add(StartLine);
            this.Options.Add(MaxLines);
            this.Options.Add(Json);
        }

        /// <summary>Runs the <see cref="LookupCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Resolves the name against the release's index and reads the page.</summary>
            private readonly IKnowledgeLookup lookup;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="lookup">Resolves names in the installed releases' indexes and pages their generated source.</param>
            /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c>.</exception>
            public Handler(IKnowledgeLookup lookup)
            {
                ArgumentNullException.ThrowIfNull(lookup);

                this.lookup = lookup;
            }

            /// <summary>Looks the name up and prints the result.</summary>
            /// <param name="parseResult">The parsed command line holding this verb's options.</param>
            /// <param name="cancellationToken">Cancels the run before it starts.</param>
            /// <returns>
            /// 0 when the lookup ran, whatever the name resolved to - the status says that. 1 when it
            /// could not run: the release is not installed or generated, or the request is invalid.
            /// </returns>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);
                cancellationToken.ThrowIfCancellationRequested();

                var json = parseResult.GetValue(Json);
                var kind = parseResult.GetValue(Kind) == "model-library" ? KnowledgeKind.ModelLibrary : KnowledgeKind.Metamodel;
                var request = new LookupRequest(kind, parseResult.GetValue(ElementName)!, parseResult.GetValue(Tag),
                    parseResult.GetValue(StartLine), parseResult.GetValue(MaxLines));

                LookupResult result;

                try
                {
                    result = this.lookup.Lookup(request);
                }
                catch (Exception exception) when (exception is KnowledgeLookupException or ArgumentException)
                {
                    if (json)
                    {
                        await JsonOutput.WriteErrorAsync(exception.Message);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");
                    }

                    return 1;
                }

                if (json)
                {
                    await JsonOutput.WriteAsync(result);
                }
                else
                {
                    await Report(result);
                }

                return 0;
            }

            /// <summary>
            /// Prints a lookup result for a person: the page with a heading line when the name was found, otherwise
            /// what the index held instead – nothing, or the candidates to choose from.
            /// </summary>
            /// <param name="result">The result to print; its <see cref="LookupResult.Status"/> decides the form.</param>
            private static async Task Report(LookupResult result)
            {
                switch (result.Status)
                {
                    case LookupStatus.Found:
                        AnsiConsole.MarkupLine(
                            $"[blue]{Markup.Escape(result.Name!)}[/] [grey]({Markup.Escape(result.Release)}, "
                            + $"lines {result.StartLine}-{result.EndLine} of {result.TotalLines}, "
                            + $"{Markup.Escape(result.Path!)})[/]");

                        // The page is printed as is: it is Markdown and source text, not markup.
                        await AnsiConsole.Console.Profile.Out.Writer.WriteLineAsync(result.Content);

                        if (result.NextLine is { } next)
                        {
                            AnsiConsole.MarkupLine($"[grey]More follows: --start-line {next}[/]");
                        }

                        return;

                    case LookupStatus.NotFound:
                        AnsiConsole.MarkupLine($"[yellow]Nothing in the {Markup.Escape(result.Release)} index matches that name.[/]");
                        return;

                    default:
                        AnsiConsole.MarkupLine(
                            result.Status == LookupStatus.Ambiguous
                                ? "[yellow]Several entries match; name one of them:[/]"
                                : "[yellow]No exact match. Did you mean one of these?[/]");

                        foreach (var match in result.Matches)
                        {
                            AnsiConsole.MarkupLine($"  {Markup.Escape(match)}");
                        }

                        if (result.TotalMatches > result.Matches.Count)
                        {
                            AnsiConsole.MarkupLine($"[grey]... and {result.TotalMatches - result.Matches.Count} more.[/]");
                        }

                        return;
                }
            }
        }
    }
}
