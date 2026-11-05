// ------------------------------------------------------------------------------------------------
// <copyright file="IOpenApiRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>
    /// Renders the REST/HTTP PSM knowledge pages.
    /// </summary>
    public interface IOpenApiRenderer
    {
        /// <summary>The file a schema's page is written to, e.g. <c>Commit.md</c>.</summary>
        string SchemaFileName(string schemaName);

        /// <summary>One schema's page: its members with their required and nullable status.</summary>
        string RenderSchema(OpenApiModel model, OpenApiSchema schema);

        /// <summary>Every endpoint, grouped as the document groups them.</summary>
        string RenderEndpoints(OpenApiModel model);

        /// <summary>The index page for one API version.</summary>
        string RenderIndex(string tag, OpenApiModel model);
    }
}
