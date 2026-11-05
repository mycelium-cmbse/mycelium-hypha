// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiModel.cs" company="Starion Group S.A.">
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
    /// What hypha keeps of the REST/HTTP PSM: the schemas and the endpoints of the OpenAPI document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <param name="Version">The API version the document declares.</param>
    /// <param name="OpenApiVersion">The OpenAPI specification version the document is written in.</param>
    /// <param name="Tags">The endpoint groups, in the order the document declares them.</param>
    /// <param name="Schemas">Every schema, ordered by name.</param>
    /// <param name="Endpoints">Every endpoint, in document order.</param>
    public sealed record OpenApiModel(
        string Title,
        string Version,
        string OpenApiVersion,
        IReadOnlyList<string> Tags,
        IReadOnlyList<OpenApiSchema> Schemas,
        IReadOnlyList<OpenApiEndpoint> Endpoints);
}
