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
    using Hypha.MetamodelGen;
    using Hypha.Tools.Commands;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Serilog;
    using Serilog.Events;
    using Serilog.Extensions.Logging;

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
        /// <returns>
        /// A provider holding the knowledge and metamodel-generation services plus the console log; the caller owns it
        /// and disposes it once the verb has run.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
        public static ServiceProvider Build(ParseResult parseResult, DirectoryInfo? outputRoot = null)
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
                })
                .AddHyphaMetamodelGen();

            // With --json, standard output carries the JSON document and nothing else, so every log
            // line goes to standard error instead.
            var json = JsonOutput.IsRequested(parseResult);

            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                var configuration = new LoggerConfiguration().MinimumLevel.Is(level);

                if (json)
                {
                    // A caller reading the JSON may never read standard error (the SessionStart hook
                    // does not), and once that pipe's buffer fills, the next log line blocks and the
                    // verb never exits. So under --json the chattiest categories (a line per request,
                    // more on every retry) only report problems.
                    var quiet = (LogEventLevel)Math.Max((int)level, (int)LogEventLevel.Warning);

                    foreach (var category in QuietUnderJson)
                    {
                        configuration = configuration.MinimumLevel.Override(category, quiet);
                    }
                }

                var logger = configuration
                    .WriteTo.Console(
                        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        standardErrorFromLevel: json ? LogEventLevel.Verbose : null)
                    .CreateLogger();

                builder.AddProvider(new SerilogLoggerProvider(logger, dispose: true));
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// The log categories that write at <c>Information</c> for every HTTP request and retry, which
        /// <c>--json</c> holds to warnings and errors.
        /// </summary>
        public static readonly IReadOnlyList<string> QuietUnderJson =
            ["System.Net.Http", "Microsoft.Extensions.Http", "Polly"];

        /// <summary>
        /// Finds the repository by walking up from the working directory.
        /// </summary>
        /// <remarks>
        /// The working directory, not the assembly's: installed as a <c>dotnet tool</c> the assembly
        /// sits under the user's tool store, which is nowhere near their checkout.
        /// </remarks>
        /// <returns>
        /// The discovered root, or <c>null</c> to let <c>AddHyphaKnowledge</c> report that there is
        /// none with the message it already has for it.
        /// </returns>
        private static DirectoryInfo? DiscoverRoot() =>
            KnowledgeLayout.Discover(new DirectoryInfo(Directory.GetCurrentDirectory()))?.Root;
    }
}
