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
    /// carry no clause and the operations no endpoint, and the document records which were present.
    /// </remarks>
    public sealed class ApiCrossReferenceGenerator : IApiKnowledgeGenerator
    {
        /// <summary>Where the generated PIM, OpenAPI and specification knowledge are read and the document is written.</summary>
        private readonly IApiLayout layout;

        /// <summary>Lines up the PIM, the specification clauses and the REST/HTTP PSM.</summary>
        private readonly IApiCrossReferenceBuilder builder;

        /// <summary>The logger progress and outcomes are reported to.</summary>
        private readonly ILogger<ApiCrossReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCrossReferenceGenerator"/> class.
        /// </summary>
        /// <param name="layout">
        /// Where the generated PIM, OpenAPI and specification knowledge are read and the document is written.
        /// </param>
        /// <param name="builder">Lines up the PIM, the specification clauses and the REST/HTTP PSM.</param>
        /// <param name="logger">The logger progress and outcomes are reported to.</param>
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

        /// <summary>
        /// Gets what this generator produces: <c>cross-references</c>. Used when reporting progress.
        /// </summary>
        public string Artifact => "cross-references";

        /// <summary>
        /// Gets where this generator sits in a full API run: <c>40</c>, last, because it reads what the
        /// other API generators wrote.
        /// </summary>
        public int Order => 40;

        /// <summary>
        /// Generates <c>knowledge/api/&lt;tag&gt;/cross-references.json</c> from the generated PIM graph,
        /// and from the OpenAPI index and the specification clauses when they are present.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <param name="cancellationToken">Cancels reading the inputs and writing the document.</param>
        /// <returns>
        /// The document written, or <see cref="GenerationOutcome.Skipped"/> when the PIM knowledge has not
        /// been generated for <paramref name="tag"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="JsonException">The PIM graph or the OpenAPI index is not valid JSON.</exception>
        /// <exception cref="ArgumentException">The PIM graph or the OpenAPI index is empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// The PIM graph or the OpenAPI index is the JSON literal <c>null</c>.
        /// </exception>
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
        /// <param name="tag">The API version.</param>
        /// <param name="cancellationToken">Cancels reading the catalog and the clause files.</param>
        /// <returns>
        /// The clauses in catalog order. An entry that names no file, or a file that is not there, is left out.
        /// </returns>
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
                // An entry that names no file, or a file that is not there, is a clause whose text
                // cannot be read: it is left out rather than failing the whole document.
                if (entry.Value.ValueKind != JsonValueKind.Object
                    || !entry.Value.TryGetProperty("file", out var name)
                    || name.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(name.GetString()))
                {
                    continue;
                }

                var file = new FileInfo(Path.Combine(catalog.Directory!.FullName, name.GetString()!));

                if (!file.Exists)
                {
                    continue;
                }

                clauses.Add(new ApiClause(
                    entry.Name,
                    entry.Value.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String
                        ? title.GetString() ?? string.Empty
                        : string.Empty,
                    await File.ReadAllTextAsync(file.FullName, cancellationToken)));
            }

            return clauses;
        }
    }
}
