// ------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp
{
    using System;
    using System.Reflection;

    using Hypha.Mcp.Prompts;
    using Hypha.Mcp.Resources;
    using Hypha.Mcp.Tools;

    using Microsoft.Extensions.DependencyInjection;

    using ModelContextProtocol.Protocol;

    /// <summary>
    /// Registers the hypha MCP server: its identity, its <see cref="ServerInstructions"/> and its tools.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>The name the server reports to a client.</summary>
        public const string ServerName = "hypha";

        /// <summary>
        /// Adds the hypha MCP server and every one of its tools.
        /// </summary>
        /// <remarks>
        /// The tools read through <c>IKnowledgeQuery</c>, <c>IApiQuery</c> and <c>IKnowledgeLookup</c>,
        /// so the caller registers the knowledge services too (<c>AddHyphaKnowledge</c>). The transport is
        /// left to the caller as well: <c>hypha serve</c> adds the standard input and output one, and a
        /// test can add its own.
        /// </remarks>
        /// <param name="services">The services to add to.</param>
        /// <returns>The server builder, so the caller can add a transport.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
        public static IMcpServerBuilder AddHyphaMcp(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            var version = typeof(ServiceCollectionExtensions).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString()
                ?? "0.0.0";

            return services
                .AddMcpServer(options =>
                {
                    options.ServerInfo = new Implementation
                    {
                        Name = ServerName,
                        Title = "hypha: OMG SysML v2 and KerML knowledge",
                        Version = version,
                    };
                    options.ServerInstructions = ServerInstructions.Text;
                })
                .WithTools<ReleaseTools>()
                .WithTools<MetamodelTools>()
                .WithTools<SyntaxTools>()
                .WithTools<SpecificationTools>()
                .WithTools<LookupTools>()
                .WithTools<ApiTools>()
                .WithTools<ReleaseChangeTools>()
                .WithTools<UpstreamTools>()
                .WithResources<KnowledgeResources>()
                .WithPrompts<KnowledgePrompts>()
                .WithRequestFilters(filters => filters.AddGetPromptFilter(PromptArguments.RequireArguments));
        }
    }
}
