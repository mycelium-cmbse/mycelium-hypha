// ------------------------------------------------------------------------------------------------
// <copyright file="ReadContracts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Text.Json;
    using System.Text.Json.Serialization.Metadata;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// The source-generated contracts the query API reads the generated documents through, set to
    /// refuse a document of the wrong shape rather than read it with members left <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The generators write through their own contracts, which these do not change. A document missing a
    /// member, or holding <c>null</c> where its type does not allow one (an empty object, or a file from
    /// an older format), fails to read here, so a query refuses it instead of failing later on a
    /// <c>null</c>. The Systems Modeling API documents leave members that are <c>null</c> out, so their
    /// constructor parameters cannot all be required; <see cref="DocumentCache"/> checks their top-level
    /// collections instead.
    /// </remarks>
    internal static class ReadContracts
    {
        /// <summary>Gets the contract for <c>metamodel.json</c>, of a release or of an API version's PIM.</summary>
        public static JsonTypeInfo<MetamodelDocument> MetamodelGraph { get; } =
            new MetamodelJsonContext(Strict(requireMembers: true)).MetamodelDocument;

        /// <summary>Gets the contract for the metamodel's <c>index.json</c>.</summary>
        public static JsonTypeInfo<MetamodelLookup> MetamodelIndex { get; } =
            new MetamodelJsonContext(Strict(requireMembers: true)).MetamodelLookup;

        /// <summary>Gets the contract for a release's <c>cross-references.json</c>.</summary>
        public static JsonTypeInfo<CrossReferenceDocument> CrossReferences { get; } =
            new CrossReferenceJsonContext(Strict(requireMembers: true)).CrossReferenceDocument;

        /// <summary>Gets the contract for the model library's <c>index.json</c>.</summary>
        public static JsonTypeInfo<ModelLibraryDocument> ModelLibrary { get; } =
            new ModelLibraryJsonContext(Strict(requireMembers: true)).ModelLibraryDocument;

        /// <summary>Gets the contract for an API version's <c>cross-references.json</c>.</summary>
        public static JsonTypeInfo<ApiCrossReferenceDocument> ApiCrossReferences { get; } =
            new ApiJsonContext(Strict(requireMembers: false)).ApiCrossReferenceDocument;

        /// <summary>The reading options: the file format's camel-case names, and no silent <c>null</c>s.</summary>
        /// <param name="requireMembers">Whether every constructor parameter must be present in the document.</param>
        /// <returns>A new options instance, since each context binds one of its own.</returns>
        private static JsonSerializerOptions Strict(bool requireMembers) => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = requireMembers,
        };
    }
}
