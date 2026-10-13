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
    /// Writes <c>knowledge/api/&lt;tag&gt;/openapi/</c>: the REST/HTTP PSM - one page per API schema with
    /// its required and nullable members, every endpoint, and the index.
    /// </summary>
    public sealed class OpenApiGenerator : IApiKnowledgeGenerator
    {
        private readonly IApiLayout layout;
        private readonly IOpenApiReader reader;
        private readonly IOpenApiRenderer renderer;
        private readonly ILogger<OpenApiGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator"/> class.
        /// </summary>
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

        /// <inheritdoc/>
        public string Artifact => "openapi";

        /// <inheritdoc/>
        public int Order => 20;

        /// <inheritdoc/>
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
    }
}
