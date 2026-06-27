// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelLookupPayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System.Collections.Generic;

    /// <summary>The serialization context for <c>index.json</c>: a flat name → record lookup.</summary>
    public sealed record MetamodelLookup(
        string SchemaVersion,
        string Metamodel,
        string ModelVersionUri,
        IReadOnlyDictionary<string, LookupEntry> Entries);

    /// <summary>A single index entry, keyed by element simple name in <see cref="MetamodelLookup.Entries"/>.</summary>
    public sealed record LookupEntry(
        string Kind,
        string Package,
        string QualifiedName,
        bool IsAbstract,
        string Summary,
        IReadOnlyList<string> Extends,
        string File);
}
