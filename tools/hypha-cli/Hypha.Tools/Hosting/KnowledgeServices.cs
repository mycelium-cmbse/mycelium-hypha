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
    using System.CommandLine;
    using System.IO;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Layout;
    using Hypha.MetamodelGen;
    using Hypha.Tools.Commands;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Serilog;
    using Serilog.Extensions.Logging;

    /// <summary>
    /// Composes the object graph a verb runs against.
    /// </summary>
    /// <remarks>
    /// Built <b>after</b> parsing rather than at start-up, because the flags that decide the
    /// composition - the repository root, the output root, the token - are themselves parsed. The
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
                    options.Token = token;
                })
                .AddHyphaMetamodelGen();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                var logger = new LoggerConfiguration()
                    .MinimumLevel.Is(level)
                    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                builder.AddProvider(new SerilogLoggerProvider(logger, dispose: true));
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });

            return services.BuildServiceProvider();
        }

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
