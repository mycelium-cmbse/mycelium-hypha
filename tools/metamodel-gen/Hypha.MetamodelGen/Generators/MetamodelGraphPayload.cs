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
    using System.Text.Json.Serialization;

    /// <summary>The serialization context for <c>metamodel.json</c>: the whole metamodel as a graph.</summary>
    /// <remarks>
    /// Record members are declared in serialization order; System.Text.Json honours that order.
    /// Every collection is always emitted (an empty array, never <c>null</c>) for a stable shape.
    /// </remarks>
    /// <param name="SchemaVersion">The version of the JSON shape.</param>
    /// <param name="Metamodel">The name of the model's root package.</param>
    /// <param name="ModelVersionUri">The URI of the model's root package (empty when it has none).</param>
    /// <param name="SourceXmiSha256">The provenance digest of the input XMI.</param>
    /// <param name="Counts">The element tallies.</param>
    /// <param name="Packages">Every named package, ordered by qualified name.</param>
    /// <param name="Classes">Every metaclass, ordered by name.</param>
    /// <param name="Enumerations">Every enumeration, ordered by name.</param>
    /// <param name="PrimitiveTypes">Every primitive type, ordered by name.</param>
    public sealed record MetamodelDocument(
        string SchemaVersion,
        string Metamodel,
        string ModelVersionUri,
        string SourceXmiSha256,
        MetamodelCounts Counts,
        IReadOnlyList<PackageNode> Packages,
        IReadOnlyList<ClassNode> Classes,
        IReadOnlyList<EnumerationNode> Enumerations,
        IReadOnlyList<PrimitiveTypeNode> PrimitiveTypes)
    {
        /// <summary>
        /// Gets the interfaces, or <c>null</c> for a model that declares none, which leaves the
        /// property out of the document, so a KerML or SysML document keeps its exact shape.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<InterfaceNode>? Interfaces { get; init; }

        /// <summary>
        /// Gets the data types that are neither primitive nor an enumeration, or <c>null</c> for a
        /// model that declares none.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<PrimitiveTypeNode>? DataTypes { get; init; }
    }

    /// <summary>One interface, and the classifiers that realize it.</summary>
    /// <param name="Name">The element name.</param>
    /// <param name="XmiId">The element's XMI id (empty when it has none).</param>
    /// <param name="Package">The name of the owning package (empty when it has none).</param>
    /// <param name="QualifiedName">The fully qualified name.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Summary">The one-line summary of the documentation (may be empty).</param>
    /// <param name="RealizedBy">The names of the classifiers that realize the interface, ordered.</param>
    /// <param name="OwnedOperations">The owned operations, ordered by name.</param>
    public sealed record InterfaceNode(
        string Name,
        string XmiId,
        string Package,
        string QualifiedName,
        string Visibility,
        string Documentation,
        string Summary,
        IReadOnlyList<string> RealizedBy,
        IReadOnlyList<OperationNode> OwnedOperations);

    /// <summary>Element tallies, for quick sanity checks against the array lengths.</summary>
    /// <param name="Classes">The number of metaclasses.</param>
    /// <param name="Enumerations">The number of enumerations.</param>
    /// <param name="PrimitiveTypes">The number of primitive types.</param>
    /// <param name="Packages">The number of named packages.</param>
    public sealed record MetamodelCounts(int Classes, int Enumerations, int PrimitiveTypes, int Packages);

    /// <summary>A package and the names of the metaclasses it directly contains.</summary>
    /// <param name="Name">The package name.</param>
    /// <param name="QualifiedName">The fully qualified name.</param>
    /// <param name="Parent">The name of the owning namespace, or <c>null</c> for a root package.</param>
    /// <param name="Classes">The names of the metaclasses the package directly contains, ordered.</param>
    public sealed record PackageNode(
        string Name,
        string QualifiedName,
        string? Parent,
        IReadOnlyList<string> Classes);

    /// <summary>A metaclass with its precomputed inheritance closures, features and constraints.</summary>
    /// <param name="Name">The element name.</param>
    /// <param name="XmiId">The element's XMI id (empty when it has none).</param>
    /// <param name="Package">The name of the owning package (empty when it has none).</param>
    /// <param name="QualifiedName">The fully qualified name.</param>
    /// <param name="IsAbstract">Whether the metaclass is abstract.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Summary">The one-line summary of the documentation (may be empty).</param>
    /// <param name="Extends">The names of the direct supertypes, ordered.</param>
    /// <param name="AllAncestors">The names of every transitive supertype, ordered.</param>
    /// <param name="DirectSubclasses">The names of the direct subtypes, ordered.</param>
    /// <param name="AllDescendants">The names of every transitive subtype, ordered.</param>
    /// <param name="OwnedAttributes">The owned attributes, ordered by name.</param>
    /// <param name="InheritedAttributes">The attributes inherited from supertypes, ordered by name.</param>
    /// <param name="OwnedOperations">The owned operations, ordered by name.</param>
    /// <param name="Constraints">The owned constraints, ordered by name.</param>
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
    /// <param name="Name">The attribute name.</param>
    /// <param name="Type">The name of the attribute's type, or <c>null</c> when it is untyped.</param>
    /// <param name="TypeIsElement">Whether the type has an element of its own in the document.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    /// <param name="IsDerived">Whether the attribute is derived.</param>
    /// <param name="IsComposite">Whether the attribute is composite.</param>
    /// <param name="IsOrdered">Whether the attribute is ordered.</param>
    /// <param name="Redefines">The names of the attributes it redefines.</param>
    /// <param name="Subsets">The names of the attributes it subsets.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
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
    /// <param name="Name">The attribute name.</param>
    /// <param name="Type">The name of the attribute's type, or <c>null</c> when it is untyped.</param>
    /// <param name="TypeIsElement">Whether the type has an element of its own in the document.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    /// <param name="IsDerived">Whether the attribute is derived.</param>
    /// <param name="IsComposite">Whether the attribute is composite.</param>
    /// <param name="IsOrdered">Whether the attribute is ordered.</param>
    /// <param name="Redefines">The names of the attributes it redefines.</param>
    /// <param name="Subsets">The names of the attributes it subsets.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="InheritedFrom">The name of the supertype that declares the attribute.</param>
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

    /// <summary>
    /// An owned operation. The return is captured separately from the (non-return) parameters;
    /// <see cref="Body"/> is the OCL of its <c>bodyCondition</c> (empty when it has none).
    /// </summary>
    /// <param name="Name">The operation name.</param>
    /// <param name="ReturnType">The name of the return type, or <c>null</c> when there is none or it is untyped.</param>
    /// <param name="ReturnLower">The lower bound of the return multiplicity.</param>
    /// <param name="ReturnUpper">The upper bound of the return multiplicity, <c>-1</c> when unbounded.</param>
    /// <param name="IsQuery">Whether the operation is a query.</param>
    /// <param name="Parameters">The non-return parameters, in declaration order.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Body">The OCL of the <c>bodyCondition</c> (may be empty).</param>
    public sealed record OperationNode(
        string Name,
        string? ReturnType,
        int ReturnLower,
        int ReturnUpper,
        bool IsQuery,
        IReadOnlyList<OperationParameterNode> Parameters,
        string Documentation,
        string Body);

    /// <summary>A single (non-return) operation parameter.</summary>
    /// <param name="Name">The parameter name.</param>
    /// <param name="Type">The name of the parameter's type, or <c>null</c> when it is untyped.</param>
    /// <param name="Direction">The direction as its lowercase name (e.g. <c>in</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    public sealed record OperationParameterNode(
        string Name,
        string? Type,
        string Direction,
        int Lower,
        int Upper);

    /// <summary>An owned constraint and its (OCL) body.</summary>
    /// <param name="Name">The constraint name.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Ocl">The constraint body (may be empty).</param>
    public sealed record ConstraintNode(string Name, string Documentation, string Ocl);

    /// <summary>An enumeration and its literals (declaration order).</summary>
    /// <param name="Name">The element name.</param>
    /// <param name="XmiId">The element's XMI id (empty when it has none).</param>
    /// <param name="Package">The name of the owning package (empty when it has none).</param>
    /// <param name="QualifiedName">The fully qualified name.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Summary">The one-line summary of the documentation (may be empty).</param>
    /// <param name="Literals">The literals, in declaration order.</param>
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
    /// <param name="Name">The literal name.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    public sealed record EnumerationLiteralNode(string Name, string Documentation);

    /// <summary>A primitive type.</summary>
    /// <param name="Name">The element name.</param>
    /// <param name="XmiId">The element's XMI id (empty when it has none).</param>
    /// <param name="Package">The name of the owning package (empty when it has none).</param>
    /// <param name="QualifiedName">The fully qualified name.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Summary">The one-line summary of the documentation (may be empty).</param>
    public sealed record PrimitiveTypeNode(
        string Name,
        string XmiId,
        string Package,
        string QualifiedName,
        string Visibility,
        string Documentation,
        string Summary);
}
