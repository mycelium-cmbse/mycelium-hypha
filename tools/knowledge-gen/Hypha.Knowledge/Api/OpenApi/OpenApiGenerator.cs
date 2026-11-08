// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiGenerator.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/api/&lt;tag&gt;/openapi/</c>: the REST/HTTP PSM: one page per API schema with
    /// its required and nullable members, every endpoint, and the index.
    /// </summary>
    public sealed class OpenApiGenerator : IApiKnowledgeGenerator
    {
        /// <summary>Where the OpenAPI document is read from and the knowledge is written to.</summary>
        private readonly IApiLayout layout;

        /// <summary>Reads the schemas and endpoints out of the OpenAPI document.</summary>
        private readonly IOpenApiReader reader;

        /// <summary>Renders the schema pages, the endpoint list and the index page.</summary>
        private readonly IOpenApiRenderer renderer;

        /// <summary>The logger progress and outcomes are reported to.</summary>
        private readonly ILogger<OpenApiGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator"/> class.
        /// </summary>
        /// <param name="layout">Where the OpenAPI document is read from and the knowledge is written to.</param>
        /// <param name="reader">Reads the schemas and endpoints out of the OpenAPI document.</param>
        /// <param name="renderer">Renders the schema pages, the endpoint list and the index page.</param>
        /// <param name="logger">The logger progress and outcomes are reported to.</param>
        public OpenApiGenerator(
            IApiLayout layout, IOpenApiReader reader, IOpenApiRenderer renderer, ILogger<OpenApiGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.reader = reader;
            this.renderer = renderer;
            this.logger = logger;
        }

        /// <summary>Gets what this generator produces: <c>openapi</c>. Used when reporting progress.</summary>
        public string Artifact => "openapi";

        /// <summary>
        /// Gets where this generator sits in a full API run: <c>20</c>, after the PIM and before the
        /// specification text and the API cross-references, which read the index it writes.
        /// </summary>
        public int Order => 20;

        /// <summary>
        /// Generates <c>knowledge/api/&lt;tag&gt;/openapi/</c> from the fetched OpenAPI document: one page
        /// per API schema under <see cref="OpenApiRenderer.SchemasFolder"/> (stale pages are deleted
        /// first), <see cref="OpenApiRenderer.EndpointsFileName"/>, <c>index.md</c> and
        /// <c>index.json</c>.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <param name="cancellationToken">Cancels reading the document and writing the files.</param>
        /// <returns>
        /// The files written, or <see cref="GenerationOutcome.Skipped"/> when no OpenAPI document was
        /// fetched for <paramref name="tag"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// The document is not readable OpenAPI, a schema name cannot be used as a page name, or two
        /// schema names differ only by case.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var source = this.layout.OpenApi(tag);

            if (!source.Exists)
            {
                return GenerationResult.Skipped(
                        $"no OpenAPI document fetched for API version {tag} at {source.FullName}; "
                        + $"run 'hypha api fetch --tag {tag}'")
                    .Report(this.logger, this.Artifact, tag);
            }

            var model = this.reader.Read(await File.ReadAllTextAsync(source.FullName, cancellationToken));

            // The pages land in one flat folder under the schema's own name. A name that is not a
            // plain file name could write outside it, and two that differ only by case would be one
            // file on a case-insensitive disk: two entries in the index, one page.
            var paged = model.Schemas
                .Where(schema => string.Equals(schema.Namespace, OpenApiNames.ApiNamespace, StringComparison.Ordinal))
                .Select(schema => schema.Name)
                .ToList();

            if (paged.FirstOrDefault(name => !IsPageName(name)) is { } unusable)
            {
                throw new InvalidOperationException(
                    $"schema '{unusable}' of API version {tag} cannot be used as a page name");
            }

            if (paged.Distinct(StringComparer.OrdinalIgnoreCase).Count() != paged.Count)
            {
                throw new InvalidOperationException(
                    $"two schemas of API version {tag} have names that differ only by case");
            }

            var output = this.layout.OpenApiKnowledge(tag);
            var schemas = new DirectoryInfo(Path.Combine(output.FullName, OpenApiRenderer.SchemasFolder));
            schemas.Create();

            // Stale pages go first: a schema removed upstream would otherwise linger forever.
            foreach (var stale in schemas.EnumerateFiles("*.md"))
            {
                stale.Delete();
            }

            var written = new List<FileInfo>();
            var entries = new SortedDictionary<string, OpenApiIndexSchema>(StringComparer.Ordinal);

            foreach (var schema in model.Schemas)
            {
                var hasPage = string.Equals(schema.Namespace, OpenApiNames.ApiNamespace, StringComparison.Ordinal);
                var fileName = this.renderer.SchemaFileName(schema.Name);

                if (hasPage)
                {
                    written.Add(await KnowledgeFile.WriteAsync(
                        Path.Combine(schemas.FullName, fileName),
                        this.renderer.RenderSchema(model, schema),
                        cancellationToken));
                }

                entries[schema.Name] = new OpenApiIndexSchema(
                    schema.Id,
                    schema.Namespace,
                    schema.Kind,
                    schema.Role,
                    hasPage ? $"{OpenApiRenderer.SchemasFolder}/{fileName}" : null,
                    hasPage ? schema.Properties : null,
                    hasPage && schema.Variants.Count > 0 ? schema.Variants : null);
            }

            var index = new OpenApiIndexDocument(
                OpenApiIndexDocument.CurrentSchemaVersion,
                tag,
                model.Title,
                model.Version,
                model.OpenApiVersion,
                new OpenApiIndexCounts(
                    model.Schemas.Count, entries.Values.Count(entry => entry.File is not null), model.Endpoints.Count),
                entries,
                model.Endpoints);

            written.Add(await KnowledgeFile.WriteAsync(
                Path.Combine(output.FullName, OpenApiRenderer.EndpointsFileName),
                this.renderer.RenderEndpoints(model),
                cancellationToken));
            written.Add(await KnowledgeFile.WriteAsync(
                Path.Combine(output.FullName, "index.md"), this.renderer.RenderIndex(tag, model), cancellationToken));
            written.Add(await KnowledgeFile.WriteAsync(
                this.layout.OpenApiIndex(tag).FullName, ApiJson.Render(index), cancellationToken));

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }

        /// <summary>Letters, digits, underscores, hyphens and inner dots; nothing a path could be built from.</summary>
        /// <param name="name">The schema name.</param>
        /// <returns><c>true</c> when <paramref name="name"/> can be used as a page's file name.</returns>
        private static bool IsPageName(string name) =>
            name.Length > 0
            && name[0] != '.'
            && name.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-' or '.');
    }
}
