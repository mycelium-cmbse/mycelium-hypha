// ------------------------------------------------------------------------------------------------
// <copyright file="MoveWindowCommand.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.Logging;

    using Spectre.Console;

    /// <summary>
    /// Advances the default release: fetches it, regenerates and verifies the knowledge base, then
    /// prunes whichever locally-installed releases no longer fit the requested count.
    /// </summary>
    /// <remarks>
    /// A maintainer-only, source-checkout-only convenience - not a way to move a committed rolling
    /// window, since nothing per-release is committed to git any more (see the repository root
    /// <c>CLAUDE.md</c>'s "Committed vs git-ignored"). The one verb that is not self-contained: unlike
    /// <c>fetch</c>/<c>generate</c>, its re-bless and verify steps shell out to <c>dotnet test</c>
    /// against the solution's own test projects, and its specification-extraction step shells out to
    /// <c>pytest</c> in <c>tools/spec-extract</c>. It needs a full source checkout with the .NET SDK
    /// and a provisioned <c>tools/spec-extract/.venv</c> - it cannot run from the standalone
    /// distributed binary the other verbs work from.
    /// </remarks>
    public sealed class MoveWindowCommand : Command
    {
        /// <summary>The release to move the window to.</summary>
        public static readonly Option<string> Tag =
            new("--tag", "-t")
            {
                Description = "The release to move the window to, e.g. 2026-06.",
                Required = true,
            };

        /// <summary>How many releases the window holds.</summary>
        public static readonly Option<int> Keep =
            new("--keep")
            {
                Description = "How many releases the window holds; older ones are evicted.",
                DefaultValueFactory = _ => 2,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveWindowCommand"/> class.
        /// </summary>
        public MoveWindowCommand()
            : base(
                "move-window",
                "Fetch, regenerate and verify a release, then evict the releases that fall outside the window")
        {
            this.Options.Add(Tag);
            this.Options.Add(Keep);
        }

        /// <summary>Runs the <see cref="MoveWindowCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReleaseInstaller installer;
            private readonly IReadOnlyList<IKnowledgeGenerator> generators;
            private readonly IKnowledgeLayout layout;
            private readonly IReleaseWindowEvictor evictor;
            private readonly IProcessRunner processes;
            private readonly ILogger<Handler> logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(
                IReleaseInstaller installer,
                IEnumerable<IKnowledgeGenerator> generators,
                IKnowledgeLayout layout,
                IReleaseWindowEvictor evictor,
                IProcessRunner processes,
                ILogger<Handler> logger)
            {
                ArgumentNullException.ThrowIfNull(installer);
                ArgumentNullException.ThrowIfNull(generators);
                ArgumentNullException.ThrowIfNull(layout);
                ArgumentNullException.ThrowIfNull(evictor);
                ArgumentNullException.ThrowIfNull(processes);
                ArgumentNullException.ThrowIfNull(logger);

                this.installer = installer;
                this.generators = [.. generators];
                this.layout = layout;
                this.evictor = evictor;
                this.processes = processes;
                this.logger = logger;
            }

            /// <summary>Moves the window.</summary>
            /// <returns>
            /// 0 once every step, including the final verification, has succeeded; 1 when a step
            /// fails; 2 when the command line or the environment is unusable before anything runs.
            /// </returns>
            public async Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                var tag = parseResult.GetValue(Tag)!;
                var keep = parseResult.GetValue(Keep);

                if (keep < 1)
                {
                    AnsiConsole.MarkupLine("[red]--keep must be at least 1.[/]");

                    return 2;
                }

                var problem = this.PreflightProblem();
                if (problem is not null)
                {
                    AnsiConsole.MarkupLine($"[red]{Markup.Escape(problem)}[/]");

                    return 2;
                }

                var completed = new List<MoveWindowStep>();

                async Task<bool> Run(MoveWindowStep step, Func<Task<int>> action)
                {
                    MoveWindowLog.Step(this.logger, step, tag);

                    var exitCode = await action();
                    if (exitCode != 0)
                    {
                        MoveWindowLog.Failed(this.logger, step, tag, exitCode);
                        ReportFailure(step, completed, tag);

                        return false;
                    }

                    completed.Add(step);

                    return true;
                }

                if (!await Run(
                        MoveWindowStep.Fetch,
                        () => new FetchCommand.Handler(this.installer).InvokeAsync(
                            ParseAs(new FetchCommand(), $"fetch --tag {tag}"), cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(
                        MoveWindowStep.Generate,
                        () => new GenerateCommand.Handler(this.generators, this.layout).InvokeAsync(
                            ParseAs(new GenerateCommand(), $"generate --tag {tag}"), cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(
                        MoveWindowStep.ExtractSpecifications, () => this.RunSpecExtractAsync(cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(
                        MoveWindowStep.ReblessFixtures,
                        () => this.RunDotnetTestAsync(
                            "tools/metamodel-gen/Hypha.MetamodelGen.Tests/Hypha.MetamodelGen.Tests.csproj",
                            "FullyQualifiedName~Bless_expected_files",
                            cancellationToken)))
                {
                    return 1;
                }

                MoveWindowLog.Step(this.logger, MoveWindowStep.Evict, tag);

                IReadOnlyList<string> evicted;

                try
                {
                    evicted = await this.evictor.EvictAsync(keep, cancellationToken);
                }
                catch (InvalidOperationException exception)
                {
                    ReportFailure(MoveWindowStep.Evict, completed, tag, exception.Message);

                    return 1;
                }

                completed.Add(MoveWindowStep.Evict);

                if (!await Run(
                        MoveWindowStep.Verify,
                        () => this.RunDotnetTestAsync(
                            "tools/hypha-cli/Hypha.Tools.Tests/Hypha.Tools.Tests.csproj",
                            "FullyQualifiedName~KnowledgeRegenerationTests",
                            cancellationToken)))
                {
                    if (evicted.Count > 0)
                    {
                        AnsiConsole.MarkupLine(
                            $"[yellow]Already evicted: {Markup.Escape(string.Join(", ", evicted))}. "
                            + "Nothing is committed yet - 'git checkout' restores them if this needs "
                            + "investigating before you commit.[/]");
                    }

                    return 1;
                }

                AnsiConsole.MarkupLine(
                    $"[bold]{Markup.Escape(tag)}[/] is now the default release; window: keep {keep}"
                    + (evicted.Count > 0
                        ? $", evicted {Markup.Escape(string.Join(", ", evicted))}."
                        : "."));

                return 0;
            }

            /// <summary>
            /// What stops this from even starting, or <c>null</c> when the environment looks usable.
            /// </summary>
            /// <remarks>
            /// Checked once, before the first step, so a missing prerequisite fails in milliseconds -
            /// not three multi-minute steps in.
            /// </remarks>
            private string? PreflightProblem()
            {
                var root = this.layout.Root;

                if (!File.Exists(Path.Combine(root.FullName, "mycelium-hypha.sln")))
                {
                    return "move-window needs a full source checkout (mycelium-hypha.sln not found "
                        + $"under {root.FullName}) - it cannot run from the standalone distributed binary.";
                }

                var specExtract = new DirectoryInfo(Path.Combine(root.FullName, "tools", "spec-extract"));
                if (!specExtract.Exists)
                {
                    return $"tools/spec-extract was not found under {root.FullName}.";
                }

                if (!File.Exists(PythonExecutable(specExtract)))
                {
                    return "tools/spec-extract/.venv is not provisioned. From tools/spec-extract, run: "
                        + "python -m venv .venv && . .venv/Scripts/activate && pip install -e .[dev] "
                        + "(or the platform equivalent).";
                }

                return null;
            }

            private async Task<int> RunSpecExtractAsync(CancellationToken cancellationToken)
            {
                var specExtract = new DirectoryInfo(
                    Path.Combine(this.layout.Root.FullName, "tools", "spec-extract"));

                return await this.processes.RunAsync(
                    PythonExecutable(specExtract), "-m pytest", specExtract, cancellationToken);
            }

            private Task<int> RunDotnetTestAsync(
                string projectPath, string filter, CancellationToken cancellationToken) =>
                this.processes.RunAsync(
                    "dotnet", $"test {projectPath} --filter \"{filter}\"", this.layout.Root, cancellationToken);

            private static void ReportFailure(
                MoveWindowStep step, List<MoveWindowStep> completed, string tag, string? reason = null)
            {
                var description = MoveWindowLog.Steps.Single(entry => entry.Step == step).Description;

                AnsiConsole.MarkupLine($"[red]{Markup.Escape(description)} failed for {Markup.Escape(tag)}"
                    + (reason is null ? "." : $": {Markup.Escape(reason)}") + "[/]");

                AnsiConsole.MarkupLine(
                    completed.Count > 0
                        ? "[grey]Already completed: "
                          + $"{Markup.Escape(string.Join(", ", completed))}. Nothing is committed yet - "
                          + "check 'git status', fix the cause, and re-run.[/]"
                        : "[grey]Nothing was changed - check 'git status', fix the cause, and re-run.[/]");
            }

            private static string PythonExecutable(DirectoryInfo specExtract) =>
                OperatingSystem.IsWindows()
                    ? Path.Combine(specExtract.FullName, ".venv", "Scripts", "python.exe")
                    : Path.Combine(specExtract.FullName, ".venv", "bin", "python");

            private static ParseResult ParseAs(Command command, string commandLine)
            {
                var root = new RootCommand();
                root.Add(command);

                return root.Parse(commandLine);
            }
        }
    }
}
