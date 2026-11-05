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
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Commands.Api;
    using Hypha.Tools.Hosting;

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
        /// <param name="args">The command line, without the program name: a verb, its options and the global ones.</param>
        /// <returns>
        /// The exit code of the verb that ran; 1 when a <c>--json</c> command line does not parse, after printing the
        /// parse errors as one error document.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            var parseResult = BuildRootCommand().Parse(args);

            // A command line that does not parse never reaches a verb, so System.CommandLine would
            // print its help to standard output. With --json, the caller gets one error document.
            if (parseResult.Errors.Count > 0 && args.Contains(JsonOutput.OptionName, StringComparer.Ordinal))
            {
                await JsonOutput.WriteErrorAsync(string.Join(" ", parseResult.Errors.Select(error => error.Message)));

                return 1;
            }

            return await parseResult.InvokeAsync();
        }

        /// <summary>
        /// Assembles the verbs.
        /// </summary>
        /// <remarks>
        /// Internal so the tests can parse against exactly the command line the tool exposes, rather
        /// than a rebuilt approximation of it.
        /// </remarks>
        /// <returns>
        /// The root command with the global options and every verb, each verb's action composing its services and
        /// running its handler.
        /// </returns>
        internal static RootCommand BuildRootCommand()
        {
            var root = new RootCommand("hypha - fetch the upstream OMG SysML v2 / KerML sources and generate the knowledge base");

            root.Options.Add(GlobalOptions.RepositoryRoot);
            root.Options.Add(GlobalOptions.ToolchainCacheRoot);
            root.Options.Add(GlobalOptions.Token);
            root.Options.Add(GlobalOptions.LogLevel);
            root.Options.Add(GlobalOptions.NoLogo);

            var discover = new DiscoverCommand();
            discover.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new DiscoverCommand.Handler(provider.GetRequiredService<IReleaseDiscovery>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(discover);

            var fetch = new FetchCommand();
            fetch.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new FetchCommand.Handler(provider.GetRequiredService<IReleaseInstaller>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(fetch);

            var generate = new GenerateCommand();
            generate.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new GenerateCommand.Handler(provider.GetServices<IKnowledgeGenerator>(), provider.GetRequiredService<IKnowledgeLayout>())
                    .InvokeAsync(parseResult, cancellationToken),
                parseResult.GetValue(GenerateCommand.Output)));
            root.Add(generate);

            var list = new ListCommand();
            list.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new ListCommand.Handler(provider.GetRequiredService<IKnowledgeLayout>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(list);

            var lookup = new LookupCommand();
            lookup.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new LookupCommand.Handler(provider.GetRequiredService<IKnowledgeLookup>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(lookup);

            var moveWindow = new MoveWindowCommand();
            moveWindow.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new MoveWindowCommand.Handler(provider.GetRequiredService<IReleaseInstaller>(), provider.GetServices<IKnowledgeGenerator>(),
                        provider.GetRequiredService<IKnowledgeLayout>(), provider.GetRequiredService<IReleaseWindowEvictor>(),
                        provider.GetRequiredService<IProcessRunner>(), provider.GetRequiredService<ILogger<MoveWindowCommand.Handler>>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(moveWindow);

            var use = new UseCommand();
            use.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new UseCommand.Handler(provider.GetRequiredService<IKnowledgeLayout>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(use);

            var remove = new RemoveCommand();
            remove.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new RemoveCommand.Handler(provider.GetRequiredService<IReleaseRemover>()).InvokeAsync(parseResult, cancellationToken)));
            root.Add(remove);

            var check = new CheckCommand();
            check.SetAction((parseResult, cancellationToken) => Run(parseResult, provider =>
                new CheckCommand.Handler(provider.GetRequiredService<IReleaseDiscovery>(), provider.GetRequiredService<IKnowledgeLayout>())
                    .InvokeAsync(parseResult, cancellationToken)));
            root.Add(check);

            root.Add(BuildApiCommand());

            return root;
        }

        /// <summary>
        /// <c>hypha api</c> and its verbs: the Systems Modeling API knowledge, versioned on its own
        /// axis and so driven by its own installer, generators and manifest.
        /// </summary>
        private static ApiCommand BuildApiCommand()
        {
            var api = new ApiCommand();

            var discover = new ApiDiscoverCommand();
            discover.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ApiDiscoverCommand.Handler(provider.GetRequiredService<IApiDiscovery>())
                    .InvokeAsync(parseResult, cancellationToken)));
            api.Add(discover);

            var fetch = new ApiFetchCommand();
            fetch.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ApiFetchCommand.Handler(provider.GetRequiredService<IApiInstaller>())
                    .InvokeAsync(parseResult, cancellationToken)));
            api.Add(fetch);

            var generate = new ApiGenerateCommand();
            generate.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new GenerateCommand.Handler(
                        provider.GetServices<IApiKnowledgeGenerator>(),
                        ApiGenerateCommand.Scope(provider.GetRequiredService<IApiLayout>()))
                    .InvokeAsync(parseResult, cancellationToken),
                parseResult.GetValue(ApiGenerateCommand.Output)));
            api.Add(generate);

            var list = new ApiListCommand();
            list.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ApiListCommand.Handler(provider.GetRequiredService<IApiLayout>())
                    .InvokeAsync(parseResult, cancellationToken)));
            api.Add(list);

            var use = new ApiUseCommand();
            use.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ApiUseCommand.Handler(provider.GetRequiredService<IApiLayout>())
                    .InvokeAsync(parseResult, cancellationToken)));
            api.Add(use);

            var remove = new ApiRemoveCommand();
            remove.SetAction((parseResult, cancellationToken) => Run(
                parseResult,
                provider => new ApiRemoveCommand.Handler(provider.GetRequiredService<IApiRemover>())
                    .InvokeAsync(parseResult, cancellationToken)));
            api.Add(remove);

            return api;
        }

        /// <summary>
        /// Composes the services, runs the handler and turns the failures a user can cause into an
        /// exit code and a sentence.
        /// </summary>
        /// <param name="parseResult">The parsed command line of the verb that runs.</param>
        /// <param name="handler">Builds the verb's handler from the composed services and runs it.</param>
        /// <param name="outputRoot">
        /// Where generated artifacts go; <c>null</c> writes them into the repository. Only <c>generate</c> passes one.
        /// </param>
        /// <returns>
        /// The handler's exit code; 130 when cancelled; 1 when composition fails, the upstream cannot be reached or a
        /// knowledge file cannot be read.
        /// </returns>
        private static async Task<int> Run(
            ParseResult parseResult, Func<IServiceProvider, Task<int>> handler, DirectoryInfo? outputRoot = null)
        {
            Banner(parseResult);

            try
            {
                await using var provider = KnowledgeServices.Build(parseResult, outputRoot);

                return await handler(provider);
            }
            catch (OperationCanceledException)
            {
                return await Fail(parseResult, 130, "Cancelled.", "yellow");
            }
            catch (InvalidOperationException exception)
            {
                // Composition failed - almost always "this is not a hypha checkout". A stack trace
                // would bury the one sentence that tells the user what to do about it.
                return await Fail(parseResult, 1, exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return await Fail(parseResult, 1, $"The upstream could not be reached: {exception.Message}",
                    hint: "An unauthenticated GitHub API allows 60 requests an hour; set --token or "
                          + "GITHUB_TOKEN if that is the limit you met.");
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                // A file the verb reads - knowledge/versions.json most often - is corrupt, or is being
                // rewritten by another hypha at this moment.
                return await Fail(parseResult, 1, $"A knowledge file could not be read: {exception.Message}",
                    hint: "If another hypha command is running, try again once it has finished; "
                          + "otherwise fetch or generate the release again.");
            }
        }

        /// <summary>
        /// Reports a failure: as <c>{"error": "..."}</c> when the verb was asked for JSON, so standard
        /// output still holds one document, and as a coloured sentence otherwise.
        /// </summary>
        /// <param name="parseResult">The parsed command line; decides whether the failure is written as JSON.</param>
        /// <param name="exitCode">The exit code to return.</param>
        /// <param name="message">What went wrong, as a sentence for the user.</param>
        /// <param name="colour">The Spectre.Console colour of <paramref name="message"/>; ignored under JSON.</param>
        /// <param name="hint">
        /// What the user can do about it, printed in grey on its own line; <c>null</c> when there is no hint. Under
        /// JSON it is appended to the error message.
        /// </param>
        /// <returns><paramref name="exitCode"/>, unchanged, so a caller can return the result directly.</returns>
        private static async Task<int> Fail(
            ParseResult parseResult, int exitCode, string message, string colour = "red", string? hint = null)
        {
            if (JsonOutput.IsRequested(parseResult))
            {
                await JsonOutput.WriteErrorAsync(hint is null ? message : $"{message} {hint}");

                return exitCode;
            }

            AnsiConsole.MarkupLine($"[{colour}]{Markup.Escape(message)}[/]");

            if (hint is not null)
            {
                AnsiConsole.MarkupLine($"[grey]{Markup.Escape(hint)}[/]");
            }

            return exitCode;
        }

        /// <summary>
        /// Announces which build is running, unless asked not to - and never in front of a JSON
        /// document, which has to be the only thing on standard output.
        /// </summary>
        /// <param name="parseResult">The parsed command line; <c>--no-logo</c> or <c>--json</c> suppresses the banner.</param>
        private static void Banner(ParseResult parseResult)
        {
            if (parseResult.GetValue(GlobalOptions.NoLogo) || JsonOutput.IsRequested(parseResult))
            {
                return;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            AnsiConsole.MarkupLine($"[blue]hypha[/] [grey]{version}[/]");
        }
    }
}
