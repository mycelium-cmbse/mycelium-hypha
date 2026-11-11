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
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;

    using Spectre.Console;

    /// <summary>
    /// Advances the default release: fetches it, regenerates and verifies the knowledge base, then
    /// prunes whichever locally-installed releases no longer fit the requested count.
    /// </summary>
    /// <remarks>
    /// A maintainer-only, source-checkout-only convenience, not a way to move a committed rolling
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
            : base("move-window", "Fetch, regenerate and verify a release, then evict the releases that fall outside the window")
        {
            this.Options.Add(Tag);
            this.Options.Add(Keep);
        }

        /// <summary>Runs the <see cref="MoveWindowCommand"/>.</summary>
        public sealed class Handler
        {
            /// <summary>Fetches the release's sources, for the fetch step.</summary>
            private readonly IReleaseInstaller installer;

            /// <summary>Runs every registered release generator, for the generate step.</summary>
            private readonly IKnowledgeBuilder<IKnowledgeGenerator> builder;

            /// <summary>Locates the repository root the steps run in and the knowledge they write.</summary>
            private readonly IKnowledgeLayout layout;

            /// <summary>Removes the installed releases that fall outside the window, for the evict step.</summary>
            private readonly IReleaseWindowEvictor evictor;

            /// <summary>Runs the <c>pytest</c> and <c>dotnet test</c> child processes.</summary>
            private readonly IProcessRunner processes;

            /// <summary>Records each step as it starts and each one that fails.</summary>
            private readonly ILogger<Handler> logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="installer">Installs the release (fetches its sources) for the fetch step.</param>
            /// <param name="builder">Runs the release generators for the generate step.</param>
            /// <param name="layout">The knowledge layout whose root must be a full source checkout.</param>
            /// <param name="evictor">Removes the installed releases that no longer fit within <c>--keep</c>.</param>
            /// <param name="processes">Runs the specification extraction, re-bless and verify child processes.</param>
            /// <param name="logger">Receives the step-by-step progress of the run.</param>
            /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
            public Handler(IReleaseInstaller installer, IKnowledgeBuilder<IKnowledgeGenerator> builder, IKnowledgeLayout layout,
                IReleaseWindowEvictor evictor, IProcessRunner processes, ILogger<Handler> logger)
            {
                ArgumentNullException.ThrowIfNull(installer);
                ArgumentNullException.ThrowIfNull(builder);
                ArgumentNullException.ThrowIfNull(layout);
                ArgumentNullException.ThrowIfNull(evictor);
                ArgumentNullException.ThrowIfNull(processes);
                ArgumentNullException.ThrowIfNull(logger);

                this.installer = installer;
                this.builder = builder;
                this.layout = layout;
                this.evictor = evictor;
                this.processes = processes;
                this.logger = logger;
            }

            /// <summary>Moves the window.</summary>
            /// <param name="parseResult">The parsed command line holding <see cref="Tag"/> and <see cref="Keep"/>.</param>
            /// <param name="cancellationToken">Cancels the steps that are still to run.</param>
            /// <returns>
            /// 0 once every step, including the final verification, has succeeded; 1 when a step
            /// fails; 2 when the command line or the environment is unusable before anything runs.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
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

                if (!await Run(MoveWindowStep.Fetch, () => new FetchCommand.Handler(this.installer).InvokeAsync(
                        ParseAs(new FetchCommand(), $"fetch --tag {tag}"), cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(MoveWindowStep.Generate, () => new GenerateCommand.Handler(this.builder, this.layout).InvokeAsync(
                        ParseAs(new GenerateCommand(), $"generate --tag {tag}"), cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(MoveWindowStep.ExtractSpecifications, () => this.RunSpecExtractAsync(cancellationToken)))
                {
                    return 1;
                }

                if (!await Run(MoveWindowStep.ReblessFixtures, () => this.RunDotnetTestAsync(
                        "tools/metamodel-gen/Hypha.Knowledge.Metamodel.Tests/Hypha.Knowledge.Metamodel.Tests.csproj",
                        "FullyQualifiedName~Bless_expected_files", cancellationToken)))
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

                if (!await Run(MoveWindowStep.Verify, () => this.RunDotnetTestAsync(
                        "tools/hypha-cli/Hypha.Tools.Tests/Hypha.Tools.Tests.csproj",
                        "FullyQualifiedName~KnowledgeRegenerationTests", cancellationToken)))
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
            /// <returns>
            /// A message naming the missing prerequisite and how to provide it, or <c>null</c> when there is none.
            /// </returns>
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
                        + "uv sync --extra dev (or, without uv: python -m venv .venv && "
                        + ". .venv/Scripts/activate && pip install -e .[dev], or the platform equivalent).";
                }

                return null;
            }

            /// <summary>
            /// Runs <c>tools/spec-extract</c>'s test suite through its own virtual environment, which regenerates the
            /// git-ignored specification text from the fetched PDFs.
            /// </summary>
            /// <param name="cancellationToken">Cancels the child process.</param>
            /// <returns>The exit code of <c>pytest</c>: 0 when every test passed.</returns>
            private async Task<int> RunSpecExtractAsync(CancellationToken cancellationToken)
            {
                var specExtract = new DirectoryInfo(Path.Combine(this.layout.Root.FullName, "tools", "spec-extract"));

                return await this.processes.RunAsync(
                    PythonExecutable(specExtract), "-m pytest", specExtract, cancellationToken: cancellationToken);
            }

            /// <summary>Runs <c>dotnet test</c> on one test project, from the repository root.</summary>
            /// <param name="projectPath">The test project, relative to the repository root.</param>
            /// <param name="filter">The <c>--filter</c> expression selecting the tests to run.</param>
            /// <param name="cancellationToken">Cancels the child process.</param>
            /// <returns>The exit code of <c>dotnet test</c>: 0 when every selected test passed.</returns>
            private Task<int> RunDotnetTestAsync(string projectPath, string filter, CancellationToken cancellationToken) =>
                this.processes.RunAsync(
                    "dotnet", $"test {projectPath} --filter \"{filter}\"", this.layout.Root, cancellationToken: cancellationToken);

            /// <summary>
            /// Tells the user which step failed, what had already been done and that nothing is committed yet.
            /// </summary>
            /// <param name="step">The step that failed.</param>
            /// <param name="completed">The steps that completed before it, in the order they ran.</param>
            /// <param name="tag">The release the window was being moved to.</param>
            /// <param name="reason">Why the step failed, when known; <c>null</c> when only its exit code is.</param>
            private static void ReportFailure(MoveWindowStep step, List<MoveWindowStep> completed, string tag, string? reason = null)
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

            /// <summary>The Python interpreter of <c>tools/spec-extract</c>'s virtual environment.</summary>
            /// <param name="specExtract">The <c>tools/spec-extract</c> folder.</param>
            /// <returns>
            /// The path of <c>.venv/Scripts/python.exe</c> on Windows and of <c>.venv/bin/python</c> elsewhere, whether it
            /// exists or not.
            /// </returns>
            private static string PythonExecutable(DirectoryInfo specExtract) =>
                OperatingSystem.IsWindows()
                    ? Path.Combine(specExtract.FullName, ".venv", "Scripts", "python.exe")
                    : Path.Combine(specExtract.FullName, ".venv", "bin", "python");

            /// <summary>
            /// Parses a command line for another verb, so this one can run that verb's handler exactly as the CLI would.
            /// </summary>
            /// <param name="command">A fresh instance of the verb to parse for.</param>
            /// <param name="commandLine">The command line, starting with the verb's name.</param>
            /// <returns>The parse result, under a root command that holds only <paramref name="command"/>.</returns>
            private static ParseResult ParseAs(Command command, string commandLine)
            {
                var root = new RootCommand();
                root.Add(command);

                return root.Parse(commandLine);
            }
        }
    }
}
