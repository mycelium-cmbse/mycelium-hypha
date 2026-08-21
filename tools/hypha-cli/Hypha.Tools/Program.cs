// ------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools
{
    using System;
    using System.CommandLine;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Hosting;
    using Hypha.Tools.Sync;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Spectre.Console;

    /// <summary>
    /// The entry point: generation is driven from here rather than from a test runner.
    /// </summary>
    /// <remarks>
    /// Each verb resolves its handler's dependencies from the composed provider and hands over. The
    /// handlers take their services in the constructor and return an exit code, so they are exercised
    /// directly in the tests without a process or a host.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>Runs the command line application.</summary>
        public static async Task<int> Main(string[] args)
        {
            var root = BuildRootCommand();

            return await root.Parse(args).InvokeAsync();
        }

        /// <summary>
        /// Assembles the verbs.
        /// </summary>
        /// <remarks>
        /// Internal so the tests can parse against exactly the command line the tool exposes, rather
        /// than a rebuilt approximation of it.
        /// </remarks>
        internal static RootCommand BuildRootCommand()
        {
            var root = new RootCommand(
                "hypha - fetch the upstream OMG SysML v2 / KerML sources and generate the knowledge base");

            root.Options.Add(GlobalOptions.RepositoryRoot);
            root.Options.Add(GlobalOptions.Token);
            root.Options.Add(GlobalOptions.LogLevel);
            root.Options.Add(GlobalOptions.NoLogo);

            var discover = new DiscoverCommand();
            discover.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new DiscoverCommand.Handler(
                        provider.GetRequiredService<IReleaseDiscovery>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(discover);

            var fetch = new FetchCommand();
            fetch.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new FetchCommand.Handler(
                        provider.GetRequiredService<IReleaseInstaller>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(fetch);

            var generate = new GenerateCommand();
            generate.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new GenerateCommand.Handler(
                        provider.GetServices<IKnowledgeGenerator>(),
                        provider.GetRequiredService<IKnowledgeLayout>())
                    .InvokeAsync(parseResult, cancellationToken),
                parseResult.GetValue(GenerateCommand.Output)));
            root.Add(generate);

            var list = new ListCommand();
            list.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ListCommand.Handler(
                        provider.GetRequiredService<IKnowledgeLayout>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(list);

            var moveWindow = new MoveWindowCommand();
            moveWindow.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new MoveWindowCommand.Handler(
                        provider.GetRequiredService<IReleaseInstaller>(),
                        provider.GetServices<IKnowledgeGenerator>(),
                        provider.GetRequiredService<IKnowledgeLayout>(),
                        provider.GetRequiredService<IReleaseWindowEvictor>(),
                        provider.GetRequiredService<IProcessRunner>(),
                        provider.GetRequiredService<ILogger<MoveWindowCommand.Handler>>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(moveWindow);

            var sync = new SyncCommand();
            sync.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new SyncCommand.Handler(
                        provider.GetRequiredService<IReleaseDiscovery>(),
                        provider.GetRequiredService<IReleaseInstaller>(),
                        provider.GetServices<IKnowledgeGenerator>(),
                        provider.GetRequiredService<IKnowledgeLayout>(),
                        new SyncStatusWriter(parseResult.GetValue(SyncCommand.StatusFile)!),
                        new FileSyncLock(new FileInfo(
                            parseResult.GetValue(SyncCommand.StatusFile)!.FullName + ".lock")),
                        provider.GetRequiredService<ILogger<SyncCommand.Handler>>())
                    .InvokeAsync(parseResult, cancellationToken),
                logFile: parseResult.GetValue(SyncCommand.LogFile)));
            root.Add(sync);

            return root;
        }

        /// <summary>
        /// Composes the services, runs the handler and turns the failures a user can cause into an
        /// exit code and a sentence.
        /// </summary>
        private static async Task<int> Run(
            ParseResult parseResult,
            Func<IServiceProvider, Task<int>> handler,
            DirectoryInfo? outputRoot = null,
            FileInfo? logFile = null)
        {
            Banner(parseResult);

            try
            {
                await using var provider = KnowledgeServices.Build(parseResult, outputRoot, logFile);

                return await handler(provider);
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");

                return 130;
            }
            catch (InvalidOperationException exception)
            {
                // Composition failed - almost always "this is not a hypha checkout". A stack trace
                // would bury the one sentence that tells the user what to do about it.
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");

                return 1;
            }
            catch (HttpRequestException exception)
            {
                AnsiConsole.MarkupLine($"[red]The upstream could not be reached: {Markup.Escape(exception.Message)}[/]");
                AnsiConsole.MarkupLine(
                    "[grey]An unauthenticated GitHub API allows 60 requests an hour; set --token or "
                    + "GITHUB_TOKEN if that is the limit you met.[/]");

                return 1;
            }
        }

        /// <summary>Announces which build is running, unless asked not to.</summary>
        private static void Banner(ParseResult parseResult)
        {
            if (parseResult.GetValue(GlobalOptions.NoLogo))
            {
                return;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            AnsiConsole.MarkupLine($"[blue]hypha[/] [grey]{version}[/]");
        }
    }
}
