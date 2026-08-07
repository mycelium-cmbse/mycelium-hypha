// ------------------------------------------------------------------------------------------------
// <copyright file="GenerateCommand.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Spectre.Console;

    /// <summary>
    /// Regenerates the knowledge base: every artifact for every installed release, or a narrower slice.
    /// </summary>
    /// <remarks>
    /// The artifact names are not listed here. They are the <see cref="IKnowledgeGenerator.Artifact"/>
    /// of whatever is registered, so adding a generator adds its verb without anyone remembering to.
    /// </remarks>
    public sealed class GenerateCommand : Command
    {
        /// <summary>Which artifact to generate; all of them when omitted.</summary>
        public static readonly Argument<string?> Artifact =
            new("artifact")
            {
                Description =
                    "The artifact to generate, e.g. metamodel or cross-references. Every artifact, in "
                    + "dependency order, when omitted.",
                Arity = ArgumentArity.ZeroOrOne,
            };

        /// <summary>Which releases to generate for; every installed one when omitted.</summary>
        public static readonly Option<string[]> Tag =
            new("--tag", "-t")
            {
                Description = "The release to generate for; repeatable. Every installed release when omitted.",
                AllowMultipleArgumentsPerToken = true,
            };

        /// <summary>Where the generated artifacts are written.</summary>
        public static readonly Option<DirectoryInfo?> Output =
            new("--output", "-o")
            {
                Description =
                    "Write the generated artifacts here instead of into the repository. Inputs are "
                    + "still read from the repository.",
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateCommand"/> class.
        /// </summary>
        public GenerateCommand()
            : base("generate", "Generate the knowledge base from the fetched sources")
        {
            this.Arguments.Add(Artifact);
            this.Options.Add(Tag);
            this.Options.Add(Output);
        }

        /// <summary>Runs the <see cref="GenerateCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReadOnlyList<IKnowledgeGenerator> generators;
            private readonly IKnowledgeLayout layout;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="generators">
            /// Every registered generator. Ordered here rather than trusted to arrive ordered: the
            /// cross-references read what the others write.
            /// </param>
            /// <param name="layout">Used to resolve which releases are installed.</param>
            public Handler(IEnumerable<IKnowledgeGenerator> generators, IKnowledgeLayout layout)
            {
                ArgumentNullException.ThrowIfNull(generators);
                ArgumentNullException.ThrowIfNull(layout);

                this.generators = [.. generators.OrderBy(generator => generator.Order)];
                this.layout = layout;
            }

            /// <summary>Generates the requested artifacts.</summary>
            /// <returns>
            /// 0 when something was written, 1 when there was nothing to do or a generator refused,
            /// and 2 when an artifact was named that no generator produces.
            /// </returns>
            public async Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var selected = this.Select(parseResult.GetValue(Artifact));
                if (selected is null)
                {
                    return 2;
                }

                var tags = parseResult.GetValue(Tag) is { Length: > 0 } requested
                    ? requested
                    : [.. this.layout.InstalledTags];

                if (tags.Length == 0)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]No release is installed under {Markup.Escape(this.layout.Root.FullName)}.[/]");
                    AnsiConsole.MarkupLine("[grey]Run 'hypha fetch --tag <release>' first.[/]");

                    return 1;
                }

                var written = 0;
                var skipped = 0;

                foreach (var tag in tags)
                {
                    foreach (var generator in selected)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        GenerationResult result;

                        try
                        {
                            result = await generator.GenerateAsync(tag, cancellationToken);
                        }
                        catch (InvalidOperationException exception)
                        {
                            // The deliberate refusals - a missing metamodel index, a clause title
                            // reaching the cross-references, a slug collision. They mean the output
                            // would be wrong, so the run stops rather than leaving a half-built base.
                            AnsiConsole.MarkupLine(
                                $"[red]{Markup.Escape(generator.Artifact)} refused for "
                                + $"{Markup.Escape(tag)}: {Markup.Escape(exception.Message)}[/]");

                            return 1;
                        }

                        if (result.Outcome == GenerationOutcome.Skipped)
                        {
                            skipped++;
                            continue;
                        }

                        written += result.Written.Count;
                    }
                }

                if (written == 0)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]Nothing was generated: all {skipped} artifacts were skipped for want "
                        + "of their inputs.[/]");
                    AnsiConsole.MarkupLine("[grey]Re-run with --log-level Information to see why.[/]");

                    return 1;
                }

                AnsiConsole.MarkupLine(
                    $"Generated [bold]{written}[/] files across {tags.Length} release(s)"
                    + (skipped > 0 ? $", {skipped} artifacts skipped." : "."));

                return 0;
            }

            /// <summary>
            /// The generators to run, or <c>null</c> when an artifact was named that none produces.
            /// </summary>
            private IReadOnlyList<IKnowledgeGenerator>? Select(string? artifact)
            {
                if (string.IsNullOrWhiteSpace(artifact))
                {
                    return this.generators;
                }

                var match = this.generators.FirstOrDefault(
                    generator => string.Equals(generator.Artifact, artifact, StringComparison.Ordinal));

                if (match is not null)
                {
                    return [match];
                }

                var known = string.Join(", ", this.generators.Select(generator => generator.Artifact));

                AnsiConsole.MarkupLine($"[red]No generator produces '{Markup.Escape(artifact)}'.[/]");
                AnsiConsole.MarkupLine($"[grey]Known artifacts: {Markup.Escape(known)}.[/]");

                return null;
            }
        }
    }
}
