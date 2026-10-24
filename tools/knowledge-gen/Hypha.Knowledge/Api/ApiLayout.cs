// ------------------------------------------------------------------------------------------------
// <copyright file="ApiLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// The API layout, resolved from the same roots as the language releases.
    /// </summary>
    public sealed class ApiLayout : IApiLayout
    {
        /// <summary>The folder, under both <c>sources/</c> and <c>knowledge/</c>, the API lives in.</summary>
        public const string ApiFolder = "api";

        private readonly IKnowledgeLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLayout"/> class.
        /// </summary>
        public ApiLayout(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <inheritdoc/>
        /// <remarks>Read on each access: installing rewrites the manifest.</remarks>
        public IReadOnlyList<string> InstalledTags => this.Manifest()?.GetTags() ?? [];

        /// <inheritdoc/>
        public string? DefaultTag => this.Manifest()?.Default;

        /// <inheritdoc/>
        public FileInfo VersionManifest => new(Path.Combine(
            this.layout.Root.FullName, KnowledgeLayout.KnowledgeFolder, ApiFolder, ApiVersionManifest.FileName));

        /// <inheritdoc/>
        public FileInfo SharedPrimitiveTypes => this.layout.SharedPrimitiveTypes;

        /// <inheritdoc/>
        public DirectoryInfo Sources(string tag) => new(Path.Combine(
            this.layout.Root.FullName, KnowledgeLayout.SourcesFolder, ApiFolder, Tag(tag)));

        /// <inheritdoc/>
        public FileInfo OpenApi(string tag) =>
            new(Path.Combine(this.Sources(tag).FullName, "openapi", ApiInputs.OpenApi));

        /// <inheritdoc/>
        public DirectoryInfo Xmi(string tag) => new(Path.Combine(this.Sources(tag).FullName, "xmi"));

        /// <inheritdoc/>
        public FileInfo PimModel(string tag) => new(Path.Combine(this.Xmi(tag).FullName, ApiInputs.PimModel));

        /// <inheritdoc/>
        public DirectoryInfo Specifications(string tag) => new(Path.Combine(this.Sources(tag).FullName, "specs"));

        /// <inheritdoc/>
        public DirectoryInfo Knowledge(string tag) => new(Path.Combine(
            this.layout.OutputRoot.FullName, KnowledgeLayout.KnowledgeFolder, ApiFolder, Tag(tag)));

        /// <inheritdoc/>
        public DirectoryInfo Pim(string tag) => new(Path.Combine(this.Knowledge(tag).FullName, "pim"));

        /// <inheritdoc/>
        public FileInfo PimGraph(string tag) => new(Path.Combine(this.Pim(tag).FullName, "metamodel.json"));

        /// <inheritdoc/>
        public DirectoryInfo OpenApiKnowledge(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "openapi"));

        /// <inheritdoc/>
        public FileInfo OpenApiIndex(string tag) =>
            new(Path.Combine(this.OpenApiKnowledge(tag).FullName, "index.json"));

        /// <inheritdoc/>
        public DirectoryInfo SpecificationText(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "spec"));

        /// <inheritdoc/>
        public FileInfo SpecificationCatalog(string tag) =>
            new(Path.Combine(this.SpecificationText(tag).FullName, "index.json"));

        /// <inheritdoc/>
        public FileInfo CrossReferences(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "cross-references.json"));

        private static string Tag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return tag;
        }

        private ApiVersionManifest? Manifest() =>
            ApiVersionManifestFile.ReadIfPresent(this.VersionManifest.FullName);
    }
}
