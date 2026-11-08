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
    /// <param name="SchemaVersion">The version of the <c>index.json</c> shape.</param>
    /// <param name="Metamodel">The name of the model the lookup covers.</param>
    /// <param name="ModelVersionUri">The URI identifying the model's version.</param>
    /// <param name="Entries">Every element, keyed by its simple name.</param>
    public sealed record MetamodelLookup(
        string SchemaVersion,
        string Metamodel,
        string ModelVersionUri,
        IReadOnlyDictionary<string, LookupEntry> Entries);

    /// <summary>A single index entry, keyed by element simple name in <see cref="MetamodelLookup.Entries"/>.</summary>
    /// <param name="Kind">
    /// What the element is: <c>class</c>, <c>enumeration</c>, <c>primitiveType</c>, <c>interface</c> or
    /// <c>dataType</c>.
    /// </param>
    /// <param name="Package">The name of the package that owns the element.</param>
    /// <param name="QualifiedName">The element's fully qualified name.</param>
    /// <param name="IsAbstract">Whether the element is an abstract class; <c>false</c> for every other kind.</param>
    /// <param name="Summary">The one-line summary of the element's documentation (may be empty).</param>
    /// <param name="Extends">The names of the element's direct supertypes; empty for every kind but a class.</param>
    /// <param name="File">The element's page, relative to the metamodel directory.</param>
    public sealed record LookupEntry(
        string Kind,
        string Package,
        string QualifiedName,
        bool IsAbstract,
        string Summary,
        IReadOnlyList<string> Extends,
        string File);
}
