// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelGraphPayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System.Collections.Generic;

    /// <summary>The serialization context for <c>metamodel.json</c>: the whole metamodel as a graph.</summary>
    /// <remarks>
    /// Record members are declared in serialization order; System.Text.Json honours that order.
    /// Every collection is always emitted (an empty array, never <c>null</c>) for a stable shape.
    /// </remarks>
    public sealed record MetamodelDocument(
        string SchemaVersion,
        string Metamodel,
        string ModelVersionUri,
        string SourceXmiSha256,
        MetamodelCounts Counts,
        IReadOnlyList<PackageNode> Packages,
        IReadOnlyList<ClassNode> Classes,
        IReadOnlyList<EnumerationNode> Enumerations,
        IReadOnlyList<PrimitiveTypeNode> PrimitiveTypes);

    /// <summary>Element tallies, for quick sanity checks against the array lengths.</summary>
    public sealed record MetamodelCounts(int Classes, int Enumerations, int PrimitiveTypes, int Packages);

    /// <summary>A package and the names of the metaclasses it directly contains.</summary>
    public sealed record PackageNode(
        string Name,
        string QualifiedName,
        string? Parent,
        IReadOnlyList<string> Classes);

    /// <summary>A metaclass with its precomputed inheritance closures, features and constraints.</summary>
    public sealed record ClassNode(
        string Name,
        string XmiId,
        string Package,
        string QualifiedName,
        bool IsAbstract,
        string Visibility,
        string Documentation,
        string Summary,
        IReadOnlyList<string> Extends,
        IReadOnlyList<string> AllAncestors,
        IReadOnlyList<string> DirectSubclasses,
        IReadOnlyList<string> AllDescendants,
        IReadOnlyList<AttributeNode> OwnedAttributes,
        IReadOnlyList<InheritedAttributeNode> InheritedAttributes,
        IReadOnlyList<OperationNode> OwnedOperations,
        IReadOnlyList<ConstraintNode> Constraints);

    /// <summary>An owned attribute (property) of a metaclass.</summary>
    public sealed record AttributeNode(
        string Name,
        string? Type,
        bool TypeIsElement,
        string Visibility,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsComposite,
        bool IsOrdered,
        IReadOnlyList<string> Redefines,
        IReadOnlyList<string> Subsets,
        string Documentation);

    /// <summary>An attribute inherited from a supertype, tagged with the declaring metaclass.</summary>
    public sealed record InheritedAttributeNode(
        string Name,
        string? Type,
        bool TypeIsElement,
        string Visibility,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsComposite,
        bool IsOrdered,
        IReadOnlyList<string> Redefines,
        IReadOnlyList<string> Subsets,
        string Documentation,
        string InheritedFrom);

    /// <summary>An owned operation. The return is captured separately from the (non-return) parameters.</summary>
    public sealed record OperationNode(
        string Name,
        string? ReturnType,
        int ReturnLower,
        int ReturnUpper,
        bool IsQuery,
        IReadOnlyList<OperationParameterNode> Parameters);

    /// <summary>A single (non-return) operation parameter.</summary>
    public sealed record OperationParameterNode(
        string Name,
        string? Type,
        string Direction,
        int Lower,
        int Upper);

    /// <summary>An owned constraint and its (OCL) body.</summary>
    public sealed record ConstraintNode(string Name, string Documentation, string Ocl);

    /// <summary>An enumeration and its literals (declaration order).</summary>
    public sealed record EnumerationNode(
        string Name,
        string XmiId,
        string Package,
        string QualifiedName,
        string Visibility,
        string Documentation,
        string Summary,
        IReadOnlyList<EnumerationLiteralNode> Literals);

    /// <summary>A single enumeration literal.</summary>
    public sealed record EnumerationLiteralNode(string Name, string Documentation);

    /// <summary>A primitive type.</summary>
    public sealed record PrimitiveTypeNode(
        string Name,
        string XmiId,
        string Package,
        string QualifiedName,
        string Visibility,
        string Documentation,
        string Summary);
}
