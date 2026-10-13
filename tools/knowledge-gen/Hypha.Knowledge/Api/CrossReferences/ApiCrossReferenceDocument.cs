// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// <c>knowledge/api/&lt;tag&gt;/cross-references.json</c>: how the three views of the Systems
    /// Modeling API line up - PIM type, specification clause, REST/HTTP PSM schema - and which
    /// endpoint each PIM service operation maps to.
    /// </summary>
    /// <remarks>
    /// Carries identifiers only: type names, clause numbers, schema names, paths. It never contains
    /// clause text, which is OMG-copyrighted and lives in the git-ignored <c>spec/</c> folder alone.
    /// Every edge names the <c>method</c> that produced it, so a reader can tell a link the
    /// specification states from one inferred from a shared name.
    /// </remarks>
    /// <param name="SchemaVersion">The version of this document's own shape.</param>
    /// <param name="ApiVersion">The API version (the tag) it was generated for.</param>
    /// <param name="Sources">Which of the three inputs were present when it was generated.</param>
    /// <param name="Counts">How much it holds.</param>
    /// <param name="Types">The PIM types, by name.</param>
    /// <param name="Operations">The PIM service operations, keyed <c>Service.operation</c>.</param>
    /// <param name="ModelMapping">
    /// The rows of the specification's PIM-to-PSM model mapping table, as written. Empty without the
    /// specification.
    /// </param>
    public sealed record ApiCrossReferenceDocument(
        string SchemaVersion,
        string ApiVersion,
        ApiCrossReferenceSources Sources,
        ApiCrossReferenceCounts Counts,
        IReadOnlyDictionary<string, ApiTypeEntry> Types,
        IReadOnlyDictionary<string, ApiOperationEntry> Operations,
        IReadOnlyList<ApiModelMappingRow> ModelMapping)
    {
        /// <summary>The version of the document shape.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>A clause whose title is the type's or service's name.</summary>
        public const string TitleMethod = "clause-title";

        /// <summary>A clause with a paragraph that opens by defining the type in bold.</summary>
        public const string DefinitionMethod = "definition-paragraph";

        /// <summary>A clause that sets the name in bold - typically a figure it appears in.</summary>
        public const string BoldMentionMethod = "bold-mention";

        /// <summary>A clause that lists the operation's signature.</summary>
        public const string SignatureMethod = "operation-signature";

        /// <summary>A row of one of the specification's PIM-to-PSM mapping tables.</summary>
        public const string MappingTableMethod = "mapping-table";

        /// <summary>A PSM schema that carries the same name as the PIM type.</summary>
        public const string SameNameMethod = "same-name";

        /// <summary>A request schema whose <c>@type</c> is fixed to the PIM type's name.</summary>
        public const string TypeConstantMethod = "type-constant";
    }

    /// <summary>Which inputs were present.</summary>
    /// <param name="Pim">Always true: there are no cross-references without the PIM.</param>
    /// <param name="OpenApi">Whether the REST/HTTP PSM knowledge was generated.</param>
    /// <param name="Specification">Whether the specification text was generated.</param>
    public sealed record ApiCrossReferenceSources(bool Pim, bool OpenApi, bool Specification);

    /// <summary>How much a cross-reference document holds.</summary>
    /// <param name="Types">The PIM types.</param>
    /// <param name="TypesWithClause">Those with at least one clause.</param>
    /// <param name="TypesWithSchema">Those with a PSM schema.</param>
    /// <param name="Operations">The PIM service operations.</param>
    /// <param name="OperationsWithEndpoint">Those mapped to at least one endpoint.</param>
    public sealed record ApiCrossReferenceCounts(
        int Types, int TypesWithClause, int TypesWithSchema, int Operations, int OperationsWithEndpoint);

    /// <summary>
    /// One PIM type and what it lines up with.
    /// </summary>
    /// <param name="Kind"><c>class</c>, <c>interface</c>, <c>enumeration</c> or <c>dataType</c>.</param>
    /// <param name="Pim">Its PIM page, relative to the API version's knowledge folder.</param>
    /// <param name="Clauses">The clauses that cover it, in clause order.</param>
    /// <param name="Schema">Its PSM schemas; <c>null</c> when it has none.</param>
    /// <param name="Members">
    /// Its effective PIM attributes and whether each appears in the PSM models; <c>null</c> when it has
    /// no PSM schema to compare against.
    /// </param>
    /// <param name="PsmOnly">
    /// Members of the PSM response model that are not PIM attributes of the type; <c>null</c> when
    /// there is nothing to compare.
    /// </param>
    public sealed record ApiTypeEntry(
        string Kind,
        string Pim,
        IReadOnlyList<ApiClauseEdge> Clauses,
        ApiSchemaEdge? Schema,
        IReadOnlyList<ApiMemberEntry>? Members,
        IReadOnlyList<string>? PsmOnly);

    /// <summary>A clause and how it was linked.</summary>
    /// <param name="Clause">The clause number.</param>
    /// <param name="Method">What links it - see the method constants on the document.</param>
    public sealed record ApiClauseEdge(string Clause, string Method);

    /// <summary>The PSM schemas of a PIM type.</summary>
    /// <param name="Response">The schema used in responses.</param>
    /// <param name="ResponseMethod">What links it.</param>
    /// <param name="Request">The schema used in request bodies, when it differs.</param>
    /// <param name="RequestMethod">What links it.</param>
    public sealed record ApiSchemaEdge(string? Response, string? ResponseMethod, string? Request, string? RequestMethod);

    /// <summary>One effective PIM attribute and whether the PSM models carry it.</summary>
    /// <param name="Name">The attribute name.</param>
    /// <param name="Multiplicity">Its PIM multiplicity, e.g. <c>[0..*]</c>.</param>
    /// <param name="Derived">Whether it is derived in the PIM.</param>
    /// <param name="InheritedFrom">The class it is inherited from; <c>null</c> when owned.</param>
    /// <param name="Response">Whether the response model has a member of the same name.</param>
    /// <param name="Request">Whether the request model has a member of the same name.</param>
    public sealed record ApiMemberEntry(
        string Name, string Multiplicity, bool Derived, string? InheritedFrom, bool Response, bool Request);

    /// <summary>
    /// One PIM service operation and what it maps to.
    /// </summary>
    /// <param name="Service">The service that owns it.</param>
    /// <param name="Clauses">The clauses that cover it.</param>
    /// <param name="Endpoints">The endpoints the specification maps it to on a single table row.</param>
    /// <param name="MappingClause">
    /// The clause holding the PIM-to-PSM endpoint mapping, when the operation is named there. An
    /// operation with this set and no endpoints is mapped in prose that spans several rows: read the
    /// clause.
    /// </param>
    public sealed record ApiOperationEntry(
        string Service,
        IReadOnlyList<ApiClauseEdge> Clauses,
        IReadOnlyList<ApiEndpointEdge> Endpoints,
        string? MappingClause);

    /// <summary>An endpoint a PIM operation maps to.</summary>
    /// <param name="Method">The HTTP method.</param>
    /// <param name="Path">The path template, as the specification writes it.</param>
    /// <param name="OperationId">
    /// The OpenAPI operation with that method and path; <c>null</c> when the OpenAPI document has none.
    /// </param>
    public sealed record ApiEndpointEdge(string Method, string Path, string? OperationId);

    /// <summary>One row of the specification's PIM-to-PSM model mapping table.</summary>
    /// <param name="Pim">The PIM concept, as written - a type, or <c>Type.member</c>.</param>
    /// <param name="Psm">The PSM model, as written.</param>
    /// <param name="Clause">The clause holding the table.</param>
    public sealed record ApiModelMappingRow(string Pim, string Psm, string Clause);
}
