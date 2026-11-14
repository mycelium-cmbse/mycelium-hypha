// ------------------------------------------------------------------------------------------------
// <copyright file="ApiQuery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Metamodel;

    /// <summary>
    /// Answers questions about a Systems Modeling API version from its generated JSON, held in memory
    /// until the files change.
    /// </summary>
    /// <remarks>
    /// Read-only: it never fetches or generates. Names match exactly (ordinal). Safe to call from
    /// several threads at once.
    /// </remarks>
    public sealed class ApiQuery : IApiQuery
    {
        /// <summary>The name a clause of the API specification gives its document.</summary>
        public const string Document = "api";

        /// <summary>Where each API version's generated knowledge lives, and which versions are installed.</summary>
        private readonly IApiLayout layout;

        /// <summary>The knowledge layout, whose output root the grounding names files relative to.</summary>
        private readonly IKnowledgeLayout knowledgeLayout;

        /// <summary>The documents read so far.</summary>
        private readonly IDocumentCache documents;

        /// <summary>Reads the specification text.</summary>
        private readonly ISpecificationReader specifications;

        /// <summary>Initializes a new instance of the <see cref="ApiQuery"/> class.</summary>
        /// <param name="layout">Where each API version's generated knowledge lives, and which versions are installed.</param>
        /// <param name="knowledgeLayout">The knowledge layout, whose output root the grounding names files relative to.</param>
        /// <param name="documents">The documents read so far, shared with the other queries.</param>
        /// <param name="specifications">Reads the specification text.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="layout"/>, <paramref name="knowledgeLayout"/>, <paramref name="documents"/> or
        /// <paramref name="specifications"/> is <c>null</c>.
        /// </exception>
        public ApiQuery(
            IApiLayout layout, IKnowledgeLayout knowledgeLayout, IDocumentCache documents, ISpecificationReader specifications)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(knowledgeLayout);
            ArgumentNullException.ThrowIfNull(documents);
            ArgumentNullException.ThrowIfNull(specifications);

            this.layout = layout;
            this.knowledgeLayout = knowledgeLayout;
            this.documents = documents;
            this.specifications = specifications;
        }

        /// <summary>A PIM class with its owned and inherited attributes and its operations.</summary>
        /// <param name="name">The class name, such as <c>Project</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>
        /// The class, grounded as <see cref="Provenance.Model"/>; <c>null</c> when the PIM has no such class.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<ClassNode> PimClass(string name, string? version = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var resolved = this.Version(version);
            var file = this.layout.PimGraph(resolved);
            var graph = this.documents.Get(
                file,
                ReadContracts.MetamodelGraph,
                document => document.Classes is not null,
                ReleaseAxis.Api.NotGenerated("PIM", resolved),
                ReleaseAxis.Api.GenerateCommand(resolved));

            var node = graph.Classes.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, name, StringComparison.Ordinal));

            return new(node, this.Ground(Provenance.Model, resolved, file));
        }

        /// <summary>
        /// A PIM type's cross-references: the specification clauses that cover it, its REST/HTTP schema
        /// and its members.
        /// </summary>
        /// <param name="name">The type name, such as <c>Commit</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>
        /// The entry, grounded as <see cref="Provenance.Derived"/>; <c>null</c> when the cross-references
        /// have no such type.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<ApiTypeEntry> Type(string name, string? version = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var (document, grounding) = this.CrossReferences(version);

            return new(document.Types.GetValueOrDefault(name), grounding);
        }

        /// <summary>A PIM service operation's cross-references: its clauses and REST/HTTP endpoints.</summary>
        /// <param name="name">The operation, qualified by its service, such as <c>ProjectService.createProject</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>
        /// The entry, grounded as <see cref="Provenance.Derived"/>; <c>null</c> when the cross-references
        /// have no such operation.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<ApiOperationEntry> Operation(string name, string? version = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var (document, grounding) = this.CrossReferences(version);

            return new(document.Operations.GetValueOrDefault(name), grounding);
        }

        /// <summary>
        /// A schema of the REST/HTTP PSM, as the OpenAPI document states it: its kind, how the endpoints
        /// use it, its properties and, for a union, its variants.
        /// </summary>
        /// <param name="name">
        /// The schema name, such as <c>Commit</c> or <c>CommitRequest</c>; <see cref="Type"/> names the
        /// schemas of a PIM type.
        /// </param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>
        /// The schema, grounded as <see cref="Provenance.Model"/>, since the index holds what the OpenAPI
        /// document states; <c>null</c> when the document has no such schema. A schema that mirrors a
        /// metamodel element rather than belonging to the API has no properties here.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<OpenApiIndexSchema> Schema(string name, string? version = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var resolved = this.Version(version);
            var file = this.layout.OpenApiIndex(resolved);
            var index = this.documents.Get(
                file,
                ReadContracts.OpenApiIndex,
                document => document.Schemas is not null,
                ReleaseAxis.Api.NotGenerated("REST schemas", resolved),
                ReleaseAxis.Api.GenerateCommand(resolved));

            return new(index.Schemas.GetValueOrDefault(name), this.Ground(Provenance.Model, resolved, file));
        }

        /// <summary>One clause of the Systems Modeling API and Services specification, with its verbatim text.</summary>
        /// <param name="clause">The clause number, such as <c>7.1.4</c>.</param>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>
        /// The clause, its document named <c>api</c>, grounded as <see cref="Provenance.Normative"/> and naming
        /// its page; <c>null</c> when the specification has no such clause. When the version has not
        /// generated the specification text, the result is <see cref="QueryResult{T}.Unavailable"/>, saying
        /// how to generate it.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="clause"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, or its specification index or page is invalid.
        /// </exception>
        public QueryResult<SpecificationClause> Clause(string clause, string? version = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clause);

            var resolved = this.Version(version);
            var command = $"hypha api generate spec --tag {resolved}";

            return this.specifications.Read(
                this.layout.SpecificationCatalog(resolved),
                Document,
                clause.Trim(),
                resolved,
                $"API version {resolved}",
                command,
                $"The specification text of API version {resolved} has not been generated. It needs the OMG "
                + $"specification PDF: run 'hypha api fetch --tag {resolved}', then '{command}'.",
                file => this.Ground(Provenance.Normative, resolved, file));
        }

        /// <summary>Reads an API version's <c>cross-references.json</c>.</summary>
        /// <param name="version">The API version, or <c>null</c> for the default.</param>
        /// <returns>The document and the grounding of answers taken from it.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The version is not installed, has no default, has not been generated, or the document is invalid.
        /// </exception>
        private (ApiCrossReferenceDocument Document, Grounding Grounding) CrossReferences(string? version)
        {
            var resolved = this.Version(version);
            var file = this.layout.CrossReferences(resolved);
            var document = this.documents.Get(
                file,
                ReadContracts.ApiCrossReferences,
                document => document.Types is not null && document.Operations is not null,
                ReleaseAxis.Api.NotGenerated("cross-references", resolved),
                ReleaseAxis.Api.GenerateCommand(resolved));

            return (document, this.Ground(Provenance.Derived, resolved, file));
        }

        /// <summary>The grounding of an answer read from a file.</summary>
        /// <param name="tier">The grounding tier.</param>
        /// <param name="version">The API version that answered.</param>
        /// <param name="file">The file read.</param>
        /// <returns>
        /// The grounding, with the file relative to the output root (the folder holding <c>knowledge/</c>),
        /// so it starts with <c>knowledge/api/</c>.
        /// </returns>
        private Grounding Ground(string tier, string version, FileInfo file) => new(
            tier,
            version,
            Path.GetRelativePath(this.knowledgeLayout.OutputRoot.FullName, file.FullName).Replace('\\', '/'));

        /// <summary>The requested API version, or the default one, checked to be installed.</summary>
        /// <param name="requested">The requested version, or <c>null</c> for the default.</param>
        /// <returns>The version to answer from.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// No API version is installed, none is the default, or the requested one is not installed.
        /// </exception>
        private string Version(string? requested) =>
            ReleaseAxis.Api.Resolve(this.layout.InstalledTags, this.layout.DefaultTag, requested);
    }
}
