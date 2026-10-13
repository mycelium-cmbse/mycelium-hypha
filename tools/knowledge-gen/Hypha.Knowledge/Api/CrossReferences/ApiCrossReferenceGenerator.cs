// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.Generation;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/api/&lt;tag&gt;/cross-references.json</c>.
    /// </summary>
    /// <remarks>
    /// Runs last: it reads what the three other API generators wrote. Only the PIM is required. Without
    /// the REST/HTTP PSM knowledge the types carry no schema, and without the specification text they
    /// carry no clause and the operations no endpoint - the document records which were present.
    /// </remarks>
    public sealed class ApiCrossReferenceGenerator : IApiKnowledgeGenerator
    {
        private readonly IApiLayout layout;
        private readonly IApiCrossReferenceBuilder builder;
        private readonly ILogger<ApiCrossReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCrossReferenceGenerator"/> class.
        /// </summary>
        public ApiCrossReferenceGenerator(
            IApiLayout layout, IApiCrossReferenceBuilder builder, ILogger<ApiCrossReferenceGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.builder = builder;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "cross-references";

        /// <inheritdoc/>
        public int Order => 40;

        /// <inheritdoc/>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var pimFile = this.layout.PimGraph(tag);

            if (!pimFile.Exists)
            {
                return GenerationResult.Skipped(
                        $"the PIM knowledge for API version {tag} has not been generated; "
                        + "there is nothing to cross-reference without it")
                    .Report(this.logger, this.Artifact, tag);
            }

            var pim = ApiJson.Read<PimGraph>(await File.ReadAllTextAsync(pimFile.FullName, cancellationToken));

            var openApiFile = this.layout.OpenApiIndex(tag);
            var openApi = openApiFile.Exists
                ? ApiJson.Read<OpenApiIndexDocument>(await File.ReadAllTextAsync(openApiFile.FullName, cancellationToken))
                : null;

            var document = this.builder.Build(tag, pim, openApi, await this.ReadClausesAsync(tag, cancellationToken));

            var written = await KnowledgeFile.WriteAsync(
                this.layout.CrossReferences(tag).FullName, ApiJson.Render(document), cancellationToken);

            return GenerationResult.Generated([written]).Report(this.logger, this.Artifact, tag);
        }

        /// <summary>The clauses the catalog lists, with their text; none when there is no catalog.</summary>
        private async Task<IReadOnlyList<ApiClause>> ReadClausesAsync(string tag, CancellationToken cancellationToken)
        {
            var catalog = this.layout.SpecificationCatalog(tag);

            if (!catalog.Exists)
            {
                return [];
            }

            using var index = JsonDocument.Parse(await File.ReadAllTextAsync(catalog.FullName, cancellationToken));

            if (!index.RootElement.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            var clauses = new List<ApiClause>();

            foreach (var entry in entries.EnumerateObject())
            {
                var file = new FileInfo(Path.Combine(
                    catalog.Directory!.FullName, entry.Value.GetProperty("file").GetString() ?? string.Empty));

                if (!file.Exists)
                {
                    continue;
                }

                clauses.Add(new ApiClause(
                    entry.Name,
                    entry.Value.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty,
                    await File.ReadAllTextAsync(file.FullName, cancellationToken)));
            }

            return clauses;
        }
    }
}
