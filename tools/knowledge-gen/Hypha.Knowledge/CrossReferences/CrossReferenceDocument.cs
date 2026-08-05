// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// The whole of <c>knowledge/&lt;tag&gt;/cross-references.json</c>.
    /// </summary>
    /// <remarks>
    /// Property order is the file's key order, and every collection is ordered, so two runs over the
    /// same inputs produce the same bytes.
    /// </remarks>
    /// <param name="SchemaVersion">The version of <c>cross-references.schema.json</c> this satisfies.</param>
    /// <param name="ModelVersionUri">The model URI the metamodel declares - recorded, never used as a version.</param>
    /// <param name="Documents">The specifications the clause identifiers refer to.</param>
    /// <param name="ProvenanceTiers">Each tier and what it means.</param>
    /// <param name="Counts">Coverage.</param>
    /// <param name="Entries">One entry per element, keyed by simple name.</param>
    public sealed record CrossReferenceDocument(
        string SchemaVersion,
        string ModelVersionUri,
        IReadOnlyDictionary<string, SpecificationDocument> Documents,
        IReadOnlyDictionary<string, string> ProvenanceTiers,
        CrossReferenceCounts Counts,
        IReadOnlyDictionary<string, CrossReferenceEntry> Entries)
    {
        /// <summary>The schema version this builder emits.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to a release's knowledge directory.</summary>
        public const string FileName = "cross-references.json";
    }
}
