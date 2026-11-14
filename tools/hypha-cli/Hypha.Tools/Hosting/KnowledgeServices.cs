// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeServices.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.IO;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Tools.Commands;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Serilog;
    using Serilog.Events;
    using Serilog.Extensions.Logging;

    using Spectre.Console;

    /// <summary>
    /// Composes the object graph a verb runs against.
    /// </summary>
    /// <remarks>
    /// Built <b>after</b> parsing rather than at start-up, because the flags that decide the
    /// composition (the repository root, the output root, the token) are themselves parsed. The
    /// registrations are the libraries' own: this adds nothing but the console log and the values the
    /// user typed.
    /// </remarks>
    public static class KnowledgeServices
    {
        /// <summary>
        /// Builds a provider for one invocation.
        /// </summary>
        /// <param name="parseResult">The parsed command line.</param>
        /// <param name="outputRoot">
        /// Where generated artifacts go; <c>null</c> writes them into the repository, which is the
        /// normal run.
        /// </param>
        /// <param name="configure">Adds the verb's own services, such as the MCP server; <c>null</c> adds none.</param>
        /// <returns>
        /// A provider holding the knowledge and metamodel-generation services plus the console log; the caller owns it
        /// and disposes it once the verb has run.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
        public static ServiceProvider Build(
            ParseResult parseResult, DirectoryInfo? outputRoot = null, Action<IServiceCollection>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(parseResult);

            var root = parseResult.GetValue(GlobalOptions.RepositoryRoot) ?? DiscoverRoot();
            var token = parseResult.GetValue(GlobalOptions.Token);
            var level = parseResult.GetValue(GlobalOptions.LogLevel);

            var services = new ServiceCollection();

            services
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = root;
                    options.OutputRoot = outputRoot;
                    options.ToolchainCacheRoot = parseResult.GetValue(GlobalOptions.ToolchainCacheRoot);
                    options.Token = token;

                    // Several MCP servers and the VS Code extension share one knowledge folder, so a
                    // release tool waits for another's change to finish rather than refusing at once.
                    options.ReleaseLockWait = ServeCommand.Owns(parseResult) ? ServeCommand.ReleaseLockWait : TimeSpan.Zero;
                })
                .AddHyphaMetamodel();

            configure?.Invoke(services);

            // With --json, standard output carries the JSON document and nothing else, and under serve
            // it carries the MCP protocol, so every log line goes to standard error instead.
            var json = JsonOutput.IsRequested(parseResult) || ServeCommand.Owns(parseResult);

            // On a terminal the fetches and generations show a progress display (ProgressDisplay), so
            // the log is written through the same console, which places each line above the bars.
            var terminal = !json && AnsiConsole.Profile.Capabilities.Interactive;

            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                var configuration = new LoggerConfiguration().MinimumLevel.Is(level);

                // A caller reading the JSON may never read standard error (a script or another tool may
                // not), and once that pipe's buffer fills, the next log line blocks and the verb never
                // exits. On a terminal, the progress bars already count every request, and hundreds of
                // lines would push them off the screen. So in both the chattiest categories (a line per
                // request, more on every retry) only report problems, unless a terminal user asked for
                // Debug or more.
                if (json || (terminal && level >= LogEventLevel.Information))
                {
                    var quiet = (LogEventLevel)Math.Max((int)level, (int)LogEventLevel.Warning);

                    foreach (var category in RequestCategories)
                    {
                        configuration = configuration.MinimumLevel.Override(category, quiet);
                    }
                }

                configuration = terminal
                    ? configuration.WriteTo.Sink(new AnsiConsoleSink(OutputTemplate))
                    : configuration.WriteTo.Console(
                        outputTemplate: OutputTemplate,
                        standardErrorFromLevel: json ? LogEventLevel.Verbose : null);

                var logger = configuration.CreateLogger();

                builder.AddProvider(new SerilogLoggerProvider(logger, dispose: true));
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// The log categories that write at <c>Information</c> for every HTTP request and retry, which
        /// <c>--json</c> and a terminal hold to warnings and errors.
        /// </summary>
        public static readonly IReadOnlyList<string> RequestCategories =
            ["System.Net.Http", "Microsoft.Extensions.Http", "Polly"];

        /// <summary>How each console log line is rendered.</summary>
        private const string OutputTemplate = "[{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Finds the repository by walking up from the working directory.
        /// </summary>
        /// <remarks>
        /// The working directory, not the assembly's: installed as a <c>dotnet tool</c> the assembly
        /// sits under the user's tool store, which is nowhere near their checkout.
        /// </remarks>
        /// <returns>
        /// The discovered root, or <c>null</c> to let <c>AddHyphaKnowledge</c> choose: a set
        /// <c>HYPHA_HOME</c>, then the checkout above the assembly, then the home folder's default
        /// location (<see cref="HyphaHome"/>).
        /// </returns>
        private static DirectoryInfo? DiscoverRoot() =>
            KnowledgeLayout.Discover(new DirectoryInfo(Directory.GetCurrentDirectory()))?.Root;
    }
}
