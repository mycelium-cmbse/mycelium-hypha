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
}
