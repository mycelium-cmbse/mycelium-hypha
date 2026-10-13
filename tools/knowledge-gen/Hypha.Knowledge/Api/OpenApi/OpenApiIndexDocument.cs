// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiIndexDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    using System.Collections.Generic;

    /// <summary>
    /// <c>knowledge/api/&lt;tag&gt;/openapi/index.json</c>: the machine-readable form of the REST/HTTP PSM
    /// knowledge, and what the API cross-references read.
    /// </summary>
    /// <param name="SchemaVersion">The version of this document's own shape.</param>
    /// <param name="ApiVersion">The API version (the tag) it was generated for.</param>
    /// <param name="Title">The OpenAPI document's title.</param>
    /// <param name="Version">The API version the OpenAPI document declares.</param>
    /// <param name="OpenApi">The OpenAPI specification version the document is written in.</param>
    /// <param name="Counts">How much the document holds.</param>
    /// <param name="Schemas">Every schema, by name.</param>
    /// <param name="Endpoints">Every endpoint, in document order.</param>
    public sealed record OpenApiIndexDocument(
        string SchemaVersion,
        string ApiVersion,
        string Title,
        string Version,
        string OpenApi,
        OpenApiIndexCounts Counts,
        IReadOnlyDictionary<string, OpenApiIndexSchema> Schemas,
        IReadOnlyList<OpenApiEndpoint> Endpoints)
    {
        /// <summary>The version of the document shape.</summary>
        public const string CurrentSchemaVersion = "1.0.0";
    }

    /// <summary>How much an OpenAPI document holds.</summary>
    /// <param name="Schemas">Every schema.</param>
    /// <param name="ApiSchemas">The schemas of the API itself, each of which has a page.</param>
    /// <param name="Endpoints">Every operation.</param>
    public sealed record OpenApiIndexCounts(int Schemas, int ApiSchemas, int Endpoints);

    /// <summary>
    /// One schema in the index.
    /// </summary>
    /// <param name="Id">The schema's <c>$id</c>.</param>
    /// <param name="Namespace">Whether it belongs to the API or mirrors a metamodel element.</param>
    /// <param name="Kind"><c>object</c>, <c>union</c>, <c>enumeration</c>, or the JSON type.</param>
    /// <param name="Role">Whether the endpoints use it in a request body, a response, both or neither.</param>
    /// <param name="File">Its page, relative to the index; <c>null</c> for a schema that has none.</param>
    /// <param name="Properties">Its members; <c>null</c> for a schema that has no page.</param>
    /// <param name="Variants">The alternatives of a union; <c>null</c> for a schema that has no page.</param>
    public sealed record OpenApiIndexSchema(
        string Id,
        string Namespace,
        string Kind,
        string Role,
        string? File,
        IReadOnlyList<OpenApiProperty>? Properties,
        IReadOnlyList<string>? Variants);
}
