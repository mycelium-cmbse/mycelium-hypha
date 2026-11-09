// ------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel
{
    using System;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Metamodel.Generation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Registers the metamodel generator, so it joins the same collection the knowledge generators
    /// are resolved from.
    /// </summary>
    /// <remarks>
    /// Separate from <c>AddHyphaKnowledge</c> because this project carries uml4net and Handlebars, and
    /// a caller that only reads the knowledge base should not have to.
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the metamodel model loader and generator, and the Systems Modeling API PIM generator.
        /// Call after <c>AddHyphaKnowledge</c>, which registers the layouts these depend on.
        /// </summary>
        /// <param name="services">The collection to add the services to.</param>
        /// <returns><paramref name="services"/>, so calls can be chained.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddHyphaMetamodel(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<IMetamodelModelLoader, MetamodelModelLoader>();
            services.AddSingleton<IKnowledgeGenerator, MetamodelGenerator>();
            services.AddSingleton<IApiKnowledgeGenerator, PimGenerator>();

            return services;
        }
    }
}
