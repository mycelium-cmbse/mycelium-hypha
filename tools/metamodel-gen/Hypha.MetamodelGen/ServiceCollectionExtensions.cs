// ------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen
{
    using System;

    using Hypha.Knowledge.Generation;
    using Hypha.MetamodelGen.Generation;

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
        /// Adds the metamodel model loader and generator. Call after <c>AddHyphaKnowledge</c>, which
        /// registers the layout this depends on.
        /// </summary>
        public static IServiceCollection AddHyphaMetamodelGen(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<IMetamodelModelLoader, MetamodelModelLoader>();
            services.AddSingleton<IKnowledgeGenerator, MetamodelGenerator>();

            return services;
        }
    }
}
