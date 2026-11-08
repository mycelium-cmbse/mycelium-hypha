// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Renders the REST/HTTP PSM knowledge pages, with LF endings throughout.
    /// </summary>
    /// <remarks>
    /// A schema gets a page only when it belongs to the API itself. The document also carries a schema
    /// (and a request twin) for every KerML / SysML metamodel element; those are indexed by name
    /// rather than rendered, since the metamodel knowledge already describes the elements.
    /// </remarks>
    public sealed class OpenApiRenderer : IOpenApiRenderer
    {
        /// <summary>The folder, beside the index, the schema pages are written to.</summary>
        public const string SchemasFolder = "schemas";

        /// <summary>The file every endpoint is listed in.</summary>
        public const string EndpointsFileName = "endpoints.md";

        /// <summary>The group an endpoint without a tag is listed under.</summary>
        private const string Untagged = "Other";

        /// <summary>The file a schema's page is written to: the schema name with <c>.md</c> appended.</summary>
        /// <param name="schemaName">The schema name, as keyed under <c>components/schemas</c>.</param>
        /// <returns>The page's file name, e.g. <c>Commit.md</c>, relative to <see cref="SchemasFolder"/>.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="schemaName"/> is <c>null</c>, empty or white space.
        /// </exception>
        public string SchemaFileName(string schemaName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

            return $"{schemaName}.md";
        }

        /// <summary>
        /// Renders one schema's page: front matter, its members with their type, required and nullable
        /// status and the type they refer to, its variants or values, and the endpoints that use it.
        /// </summary>
        /// <param name="model">
        /// The whole document, for the schemas that have pages and the endpoints that use the schema.
        /// </param>
        /// <param name="schema">The schema to render.</param>
        /// <returns>The page's Markdown, with LF endings.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="model"/> or <paramref name="schema"/> is <c>null</c>.
        /// </exception>
        public string RenderSchema(OpenApiModel model, OpenApiSchema schema)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(schema);

            var pages = Pages(model);
            var page = new StringBuilder();

            page.Append("---\n");
            page.Append("name: ").Append(schema.Name).Append('\n');
            page.Append("id: ").Append(schema.Id).Append('\n');
            page.Append("kind: ").Append(schema.Kind).Append('\n');
            page.Append("role: ").Append(schema.Role).Append('\n');

            if (schema.AdditionalProperties is { } additional)
            {
                page.Append("additionalProperties: ").Append(additional ? "true" : "false").Append('\n');
            }

            page.Append("---\n\n");
            page.Append("# ").Append(schema.Name).Append("\n\n");
            page.Append("REST/HTTP PSM schema · ").Append(schema.Kind).Append(" · ").Append(RoleSentence(schema.Role)).Append('\n');

            if (schema.Properties.Count > 0)
            {
                page.Append("\n## Properties\n\n");
                page.Append("| Property | Type | Required | Nullable | Refers to |\n");
                page.Append("| --- | --- | --- | --- | --- |\n");

                foreach (var property in schema.Properties)
                {
                    page.Append("| ").Append(property.Name)
                        .Append(" | ").Append(Cell(Linked(property.Type, pages, string.Empty)))
                        .Append(" | ").Append(property.Required ? "yes" : "no")
                        .Append(" | ").Append(property.Nullable ? "yes" : "no")
                        .Append(" | ").Append(property.RefersTo is null ? string.Empty : Linked(property.RefersTo, pages, string.Empty))
                        .Append(" |\n");
                }
            }

            if (schema.Variants.Count > 0)
            {
                page.Append("\n## Variants\n\n");

                foreach (var variant in schema.Variants)
                {
                    page.Append("- ").Append(Linked(variant, pages, string.Empty)).Append('\n');
                }
            }

            if (schema.Values.Count > 0)
            {
                page.Append("\n## Values\n\n");

                foreach (var value in schema.Values)
                {
                    page.Append("- `").Append(value).Append("`\n");
                }
            }

            var uses = model.Endpoints
                .SelectMany(endpoint => Uses(endpoint, schema.Name))
                .ToList();

            if (uses.Count > 0)
            {
                page.Append("\n## Used by\n\n");

                foreach (var use in uses)
                {
                    page.Append("- ").Append(use).Append('\n');
                }
            }

            return page.ToString();
        }

        /// <summary>
        /// Renders every endpoint, grouped by its first tag in the order the document declares the tags;
        /// an endpoint without a tag, or with one the document did not declare, follows at the end.
        /// </summary>
        /// <param name="model">The document whose endpoints are rendered.</param>
        /// <returns>The Markdown of <see cref="EndpointsFileName"/>, with LF endings.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <c>null</c>.</exception>
        public string RenderEndpoints(OpenApiModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var pages = Pages(model);
            var page = new StringBuilder();

            page.Append("# ").Append(model.Title).Append(" – endpoints\n\n");
            page.Append("> Generated by `tools/knowledge-gen` from the OpenAPI document - do not edit by hand; re-run the generator.\n\n");
            page.Append(model.Endpoints.Count).Append(" operations on ")
                .Append(model.Endpoints.Select(endpoint => endpoint.Path).Distinct(StringComparer.Ordinal).Count())
                .Append(" paths.\n");

            // Grouped as the document groups them, in the order it declares the groups; an operation
            // without a group, or in one the document did not declare, follows at the end.
            var groups = model.Tags
                .Concat(model.Endpoints.Select(Group).Where(group => !model.Tags.Contains(group, StringComparer.Ordinal))
                    .Distinct(StringComparer.Ordinal).OrderBy(group => group, StringComparer.Ordinal))
                .ToList();

            foreach (var group in groups)
            {
                var endpoints = model.Endpoints
                    .Where(endpoint => string.Equals(Group(endpoint), group, StringComparison.Ordinal))
                    .ToList();

                if (endpoints.Count == 0)
                {
                    continue;
                }

                page.Append("\n## ").Append(group).Append('\n');

                foreach (var endpoint in endpoints)
                {
                    page.Append("\n### ").Append(endpoint.Method).Append(' ').Append(endpoint.Path).Append("\n\n");
                    page.Append('`').Append(endpoint.OperationId).Append('`');

                    if (endpoint.Summary.Length > 0)
                    {
                        page.Append(" – ").Append(endpoint.Summary);
                    }

                    page.Append('\n');

                    if (endpoint.Parameters.Count > 0)
                    {
                        page.Append("\n| Parameter | In | Required | Type | Description |\n");
                        page.Append("| --- | --- | --- | --- | --- |\n");

                        foreach (var parameter in endpoint.Parameters)
                        {
                            page.Append("| ").Append(parameter.Name)
                                .Append(" | ").Append(parameter.In)
                                .Append(" | ").Append(parameter.Required ? "yes" : "no")
                                .Append(" | ").Append(Cell(parameter.Type))
                                .Append(" | ").Append(Cell(parameter.Description))
                                .Append(" |\n");
                        }
                    }

                    if (endpoint.Request is not null)
                    {
                        page.Append("\nRequest body: ")
                            .Append(Linked(endpoint.Request, pages, SchemasFolder + "/")).Append('\n');
                    }

                    if (endpoint.Responses.Count > 0)
                    {
                        page.Append("\n| Status | Description | Body |\n");
                        page.Append("| --- | --- | --- |\n");

                        foreach (var response in endpoint.Responses)
                        {
                            page.Append("| ").Append(response.Status)
                                .Append(" | ").Append(Cell(response.Description))
                                .Append(" | ")
                                .Append(response.Body is null ? string.Empty : Cell(Linked(response.Body, pages, SchemasFolder + "/")))
                                .Append(" |\n");
                        }
                    }
                }
            }

            return page.ToString();
        }

        /// <summary>
        /// Renders the index page for one API version: the document's counts, a table linking every API
        /// schema's page, and the names of the metamodel element schemas, which have no page.
        /// </summary>
        /// <param name="tag">The API version the page is generated for.</param>
        /// <param name="model">The document the page indexes.</param>
        /// <returns>The page's Markdown, with LF endings.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <c>null</c>.</exception>
        public string RenderIndex(string tag, OpenApiModel model)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            ArgumentNullException.ThrowIfNull(model);

            var api = model.Schemas.Where(IsApi).ToList();
            var metamodel = model.Schemas.Where(schema => !IsApi(schema)).ToList();
            var page = new StringBuilder();

            page.Append("# ").Append(model.Title).Append(" – REST/HTTP PSM (API version ").Append(tag).Append(")\n\n");
            page.Append("> Generated by `tools/knowledge-gen` from the OpenAPI document - do not edit by hand; re-run the generator.\n\n");
            page.Append("OpenAPI ").Append(model.OpenApiVersion).Append(", API version ").Append(model.Version).Append(". ")
                .Append(model.Schemas.Count).Append(" schemas (").Append(api.Count).Append(" of the API itself, ")
                .Append(metamodel.Count).Append(" mirroring KerML / SysML elements) and ")
                .Append(model.Endpoints.Count).Append(" operations - see [endpoints](").Append(EndpointsFileName).Append(").\n");

            page.Append("\n## API schemas\n\n");
            page.Append("| Schema | Kind | Role |\n");
            page.Append("| --- | --- | --- |\n");

            foreach (var schema in api)
            {
                page.Append("| [").Append(schema.Name).Append("](").Append(SchemasFolder).Append('/')
                    .Append(this.SchemaFileName(schema.Name)).Append(") | ")
                    .Append(schema.Kind).Append(" | ").Append(schema.Role).Append(" |\n");
            }

            if (metamodel.Count > 0)
            {
                page.Append("\n## Metamodel element schemas\n\n");
                page.Append("These mirror the KerML / SysML metamodel, one schema per element and one for its use in a\n");
                page.Append("request body. They are indexed by name in `index.json` and not rendered as pages: the\n");
                page.Append("metamodel knowledge describes the elements themselves.\n\n");
                page.Append(string.Join(", ", metamodel.Select(schema => $"`{schema.Name}`"))).Append('\n');
            }

            return page.ToString();
        }

        /// <summary>Whether <paramref name="schema"/> belongs to the API itself, and so gets a page.</summary>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> when its namespace is <see cref="OpenApiNames.ApiNamespace"/>.</returns>
        private static bool IsApi(OpenApiSchema schema) =>
            string.Equals(schema.Namespace, OpenApiNames.ApiNamespace, StringComparison.Ordinal);

        /// <summary>The names of the schemas that have a page.</summary>
        /// <param name="model">The document.</param>
        /// <returns>The names of the API schemas.</returns>
        private static HashSet<string> Pages(OpenApiModel model) =>
            model.Schemas.Where(IsApi).Select(schema => schema.Name).ToHashSet(StringComparer.Ordinal);

        /// <summary>The group an endpoint is listed under.</summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns>Its first tag, or <see cref="Untagged"/> when it has none.</returns>
        private static string Group(OpenApiEndpoint endpoint) =>
            endpoint.Tags.Count > 0 && endpoint.Tags[0].Length > 0 ? endpoint.Tags[0] : Untagged;

        /// <summary>
        /// Describes each place <paramref name="endpoint"/> uses <paramref name="schemaName"/>: its request
        /// body and each response whose body names the schema.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="schemaName">The schema looked for.</param>
        /// <returns>The text of one list item per use; none when the endpoint does not use the schema.</returns>
        private static IEnumerable<string> Uses(OpenApiEndpoint endpoint, string schemaName)
        {
            if (endpoint.Request is not null && Mentions(endpoint.Request, schemaName))
            {
                yield return $"`{endpoint.Method} {endpoint.Path}` – request body";
            }

            foreach (var response in endpoint.Responses.Where(
                         response => response.Body is not null && Mentions(response.Body, schemaName)))
            {
                yield return $"`{endpoint.Method} {endpoint.Path}` – response {response.Status}";
            }
        }

        /// <summary>Whether a type notation names <paramref name="schemaName"/> as a whole word.</summary>
        /// <param name="type">The type notation, e.g. <c>Commit[]</c>.</param>
        /// <param name="schemaName">The schema looked for.</param>
        /// <returns><c>true</c> when one of the notation's words is <paramref name="schemaName"/>.</returns>
        private static bool Mentions(string type, string schemaName) =>
            Words(type).Contains(schemaName, StringComparer.Ordinal);

        /// <summary>
        /// Links every schema name in a type notation that has a page, leaving the rest of the notation
        /// (<c>[]</c>, <c>|</c>, parentheses, plain JSON types) as it is.
        /// </summary>
        /// <param name="type">The type notation. A constant or an enumeration literal is returned unchanged.</param>
        /// <param name="pages">The names of the schemas that have a page.</param>
        /// <param name="folder">
        /// The folder the pages are in, relative to the page being rendered, with a trailing <c>/</c>;
        /// empty for the same folder.
        /// </param>
        /// <returns>The notation, with Markdown links.</returns>
        private static string Linked(string type, HashSet<string> pages, string folder)
        {
            // A literal is quoted text, not a type: "Commit" as a constant must not become a link.
            if (type.StartsWith("const ", StringComparison.Ordinal) || type.StartsWith("enum (", StringComparison.Ordinal))
            {
                return type;
            }

            var rendered = new StringBuilder();
            var word = new StringBuilder();

            void Flush()
            {
                if (word.Length == 0)
                {
                    return;
                }

                var name = word.ToString();
                rendered.Append(pages.Contains(name) ? $"[{name}]({folder}{name}.md)" : name);
                word.Clear();
            }

            foreach (var character in type)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    word.Append(character);
                }
                else
                {
                    Flush();
                    rendered.Append(character);
                }
            }

            Flush();

            return rendered.ToString();
        }

        /// <summary>
        /// Splits a type notation into its names, dropping spaces, brackets, parentheses, bars and commas.
        /// </summary>
        /// <param name="type">The type notation.</param>
        /// <returns>The words of the notation, in order.</returns>
        private static IEnumerable<string> Words(string type) =>
            type.Split([' ', '[', ']', '(', ')', '|', ','], StringSplitOptions.RemoveEmptyEntries);

        /// <summary>Keeps a value from breaking the table it is placed in.</summary>
        /// <param name="value">The cell text.</param>
        /// <returns>The text with line breaks replaced by spaces and <c>|</c> escaped.</returns>
        private static string Cell(string value) =>
            value.Replace("\r\n", " ", StringComparison.Ordinal)
                .Replace('\n', ' ')
                .Replace("|", "\\|", StringComparison.Ordinal);

        /// <summary>Phrases a schema's role for the line under its page title.</summary>
        /// <param name="role">One of the <see cref="OpenApiNames"/> roles.</param>
        /// <returns>The phrase, e.g. <c>used in responses</c>.</returns>
        private static string RoleSentence(string role) => role switch
        {
            OpenApiNames.RequestRole => "used in request bodies",
            OpenApiNames.ResponseRole => "used in responses",
            OpenApiNames.BothRoles => "used in request bodies and in responses",
            _ => "not used by any endpoint",
        };
    }
}
