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
        /// <param name="schemaName">The schema name, as keyed under <c>components/schemas</c>.</param>
        /// <returns>The page's file name, relative to the OpenAPI knowledge folder.</returns>
        string SchemaFileName(string schemaName);

        /// <summary>One schema's page: its members with their required and nullable status.</summary>
        /// <param name="model">The whole document, for the endpoints that use the schema.</param>
        /// <param name="schema">The schema to render.</param>
        /// <returns>The page's Markdown.</returns>
        string RenderSchema(OpenApiModel model, OpenApiSchema schema);

        /// <summary>Every endpoint, grouped as the document groups them.</summary>
        /// <param name="model">The document whose endpoints are rendered.</param>
        /// <returns>The page's Markdown.</returns>
        string RenderEndpoints(OpenApiModel model);

        /// <summary>The index page for one API version.</summary>
        /// <param name="tag">The API version the page is generated for.</param>
        /// <param name="model">The document the page indexes.</param>
        /// <returns>The page's Markdown.</returns>
        string RenderIndex(string tag, OpenApiModel model);
    }
}
