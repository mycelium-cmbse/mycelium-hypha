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

        /// <summary>
        /// The language-release layout whose repository root and output root this layout resolves
        /// against.
        /// </summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLayout"/> class.
        /// </summary>
        /// <param name="layout">
        /// The language-release layout: inputs resolve against its <see cref="IKnowledgeLayout.Root"/>,
        /// generated knowledge against its <see cref="IKnowledgeLayout.OutputRoot"/>.
        /// </param>
        public ApiLayout(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <summary>
        /// Gets the installed API versions, newest first, as recorded in
        /// <c>knowledge/api/versions.json</c>. Empty when there is no manifest.
        /// </summary>
        /// <remarks>Read on each access: installing rewrites the manifest.</remarks>
        public IReadOnlyList<string> InstalledTags => this.Manifest()?.GetTags() ?? [];

        /// <summary>
        /// Gets the API version answered from by default, or <c>null</c> when there is no manifest.
        /// </summary>
        /// <remarks>Read from the manifest on each access, like <see cref="InstalledTags"/>.</remarks>
        public string? DefaultTag => this.Manifest()?.Default;

        /// <summary>
        /// Gets <c>knowledge/api/versions.json</c>, resolved against the repository root: the manifest
        /// is what installing reads and writes, not generation output.
        /// </summary>
        public FileInfo VersionManifest => new(Path.Combine(
            this.layout.Root.FullName, KnowledgeLayout.KnowledgeFolder, ApiFolder, ApiVersionManifest.FileName));

        /// <summary>
        /// Gets <c>sources/PrimitiveTypes.xmi</c>, the same file the language-release layout resolves.
        /// </summary>
        public FileInfo SharedPrimitiveTypes => this.layout.SharedPrimitiveTypes;

        /// <summary>
        /// Gets <c>sources/api/&lt;tag&gt;</c>, resolved against the repository root: everything
        /// fetched for one API version.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Sources(string tag) => new(Path.Combine(
            this.layout.Root.FullName, KnowledgeLayout.SourcesFolder, ApiFolder, Tag(tag)));

        /// <summary>Gets <c>sources/api/&lt;tag&gt;/openapi/openapi.json</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The OpenAPI document; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo OpenApi(string tag) =>
            new(Path.Combine(this.Sources(tag).FullName, "openapi", ApiInputs.OpenApi));

        /// <summary>
        /// Gets <c>sources/api/&lt;tag&gt;/xmi</c>: the PIM model and the KerML model it refers to.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Xmi(string tag) => new(Path.Combine(this.Sources(tag).FullName, "xmi"));

        /// <summary>Gets <c>sources/api/&lt;tag&gt;/xmi/Systems-Modeling-API.xmi</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The PIM model; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo PimModel(string tag) => new(Path.Combine(this.Xmi(tag).FullName, ApiInputs.PimModel));

        /// <summary>Gets <c>sources/api/&lt;tag&gt;/specs/</c>, which is OMG-copyrighted.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Specifications(string tag) => new(Path.Combine(this.Sources(tag).FullName, "specs"));

        /// <summary>
        /// Gets <c>knowledge/api/&lt;tag&gt;</c>, resolved against the output root.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Knowledge(string tag) => new(Path.Combine(
            this.layout.OutputRoot.FullName, KnowledgeLayout.KnowledgeFolder, ApiFolder, Tag(tag)));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/pim</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Pim(string tag) => new(Path.Combine(this.Knowledge(tag).FullName, "pim"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/pim/metamodel.json</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The PIM graph; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo PimGraph(string tag) => new(Path.Combine(this.Pim(tag).FullName, "metamodel.json"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/openapi</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo OpenApiKnowledge(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "openapi"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/openapi/index.json</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The OpenAPI index; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo OpenApiIndex(string tag) =>
            new(Path.Combine(this.OpenApiKnowledge(tag).FullName, "index.json"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/spec</c>, the verbatim OMG text.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The folder; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo SpecificationText(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "spec"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/spec/index.json</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The specification catalog; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo SpecificationCatalog(string tag) =>
            new(Path.Combine(this.SpecificationText(tag).FullName, "index.json"));

        /// <summary>Gets <c>knowledge/api/&lt;tag&gt;/cross-references.json</c>.</summary>
        /// <param name="tag">The API version.</param>
        /// <returns>The cross-reference document; it need not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo CrossReferences(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, "cross-references.json"));

        /// <summary>
        /// Checks that <paramref name="tag"/> is not <c>null</c>, empty or white space before it is combined
        /// into a path. It does not check for characters a folder name cannot hold.
        /// </summary>
        /// <param name="tag">The API version.</param>
        /// <returns><paramref name="tag"/>, unchanged.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        private static string Tag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return tag;
        }

        /// <summary>
        /// Reads the API version manifest.
        /// </summary>
        /// <returns>The manifest, or <c>null</c> when <see cref="VersionManifest"/> does not exist.</returns>
        private ApiVersionManifest? Manifest() =>
            ApiVersionManifestFile.ReadIfPresent(this.VersionManifest.FullName);
    }
}
