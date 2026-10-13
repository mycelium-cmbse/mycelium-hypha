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
}
