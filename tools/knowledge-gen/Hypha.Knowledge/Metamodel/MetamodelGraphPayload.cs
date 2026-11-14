// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelGraphPayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Metamodel
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
        /// The version of the JSON shape <c>metamodel.json</c> and the metamodel's <c>index.json</c> are
        /// written in, which <c>Hypha.Knowledge.Metamodel</c> writes and a reader expects.
        /// </summary>
        public const string CurrentSchemaVersion = "2.1.0";

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
    public sealed record PackageNode(string Name, string QualifiedName, string? Parent, IReadOnlyList<string> Classes);

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
    /// <param name="InheritedOperations">
    /// A reference to every operation the supertypes declare, ordered by name and then declaring
    /// metaclass, each saying which operation redefines it in this metaclass's context, if any.
    /// </param>
    /// <param name="Constraints">The owned constraints, ordered by name.</param>
    /// <param name="InheritedConstraints">
    /// A reference to every constraint the supertypes declare, ordered by name and then declaring metaclass.
    /// </param>
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
        IReadOnlyList<InheritedOperationNode> InheritedOperations,
        IReadOnlyList<ConstraintNode> Constraints,
        IReadOnlyList<InheritedConstraintNode> InheritedConstraints);

    /// <summary>An owned attribute (property) of a metaclass.</summary>
    /// <param name="Name">The attribute name.</param>
    /// <param name="XmiId">The attribute's XMI id (empty when it has none). It normally stays the same across releases, so a renamed attribute is still the same one.</param>
    /// <param name="Type">The name of the attribute's type, or <c>null</c> when it is untyped.</param>
    /// <param name="TypeIsElement">Whether the type has an element of its own in the document.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    /// <param name="IsDerived">Whether the attribute is derived.</param>
    /// <param name="IsDerivedUnion">Whether the attribute is a derived union of the attributes that subset it.</param>
    /// <param name="IsComposite">Whether the attribute is composite.</param>
    /// <param name="IsOrdered">Whether the attribute is ordered.</param>
    /// <param name="IsUnique">Whether the attribute's values are unique.</param>
    /// <param name="IsReadOnly">Whether the attribute is read-only.</param>
    /// <param name="DefaultValue">The default value as written in the model, or <c>null</c> when it has none.</param>
    /// <param name="Redefines">The attributes it redefines, each with its owner.</param>
    /// <param name="Subsets">The attributes it subsets, each with its owner.</param>
    /// <param name="Opposite">
    /// The other end of the binary association the attribute is an end of; <c>null</c> when it is not one,
    /// or when the other end cannot be named because it, or the association that owns it, has no name.
    /// </param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    public sealed record AttributeNode(
        string Name,
        string XmiId,
        string? Type,
        bool TypeIsElement,
        string Visibility,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsDerivedUnion,
        bool IsComposite,
        bool IsOrdered,
        bool IsUnique,
        bool IsReadOnly,
        string? DefaultValue,
        IReadOnlyList<FeatureReference> Redefines,
        IReadOnlyList<FeatureReference> Subsets,
        FeatureReference? Opposite,
        string Documentation);

    /// <summary>An attribute inherited from a supertype, tagged with the declaring metaclass.</summary>
    /// <param name="Name">The attribute name.</param>
    /// <param name="Type">The name of the attribute's type, or <c>null</c> when it is untyped.</param>
    /// <param name="TypeIsElement">Whether the type has an element of its own in the document.</param>
    /// <param name="Visibility">The visibility as its lowercase name (e.g. <c>public</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    /// <param name="IsDerived">Whether the attribute is derived.</param>
    /// <param name="IsDerivedUnion">Whether the attribute is a derived union of the attributes that subset it.</param>
    /// <param name="IsComposite">Whether the attribute is composite.</param>
    /// <param name="IsOrdered">Whether the attribute is ordered.</param>
    /// <param name="IsUnique">Whether the attribute's values are unique.</param>
    /// <param name="IsReadOnly">Whether the attribute is read-only.</param>
    /// <param name="DefaultValue">The default value as written in the model, or <c>null</c> when it has none.</param>
    /// <param name="Redefines">The attributes it redefines, each with its owner.</param>
    /// <param name="Subsets">The attributes it subsets, each with its owner.</param>
    /// <param name="Opposite">
    /// The other end of the binary association the attribute is an end of; <c>null</c> when it is not one,
    /// or when the other end cannot be named because it, or the association that owns it, has no name.
    /// </param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="InheritedFrom">The name of the supertype that declares the attribute.</param>
    /// <param name="RedefinedBy">
    /// The attribute that redefines this one in the inheriting metaclass's context, owned by it or by one
    /// of its supertypes, or <c>null</c> when none does, so the attribute applies as inherited.
    /// </param>
    public sealed record InheritedAttributeNode(
        string Name,
        string? Type,
        bool TypeIsElement,
        string Visibility,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsDerivedUnion,
        bool IsComposite,
        bool IsOrdered,
        bool IsUnique,
        bool IsReadOnly,
        string? DefaultValue,
        IReadOnlyList<FeatureReference> Redefines,
        IReadOnlyList<FeatureReference> Subsets,
        FeatureReference? Opposite,
        string Documentation,
        string InheritedFrom,
        FeatureReference? RedefinedBy);

    /// <summary>
    /// An owned operation. The return is captured separately from the (non-return) parameters;
    /// <see cref="Body"/> is the OCL of its <c>bodyCondition</c> (empty when it has none).
    /// </summary>
    /// <param name="Name">The operation name.</param>
    /// <param name="XmiId">The operation's XMI id (empty when it has none). It normally stays the same across releases, so a renamed operation is still the same one.</param>
    /// <param name="ReturnType">The name of the return type, or <c>null</c> when there is none or it is untyped.</param>
    /// <param name="ReturnLower">The lower bound of the return multiplicity.</param>
    /// <param name="ReturnUpper">The upper bound of the return multiplicity, <c>-1</c> when unbounded.</param>
    /// <param name="IsQuery">Whether the operation is a query.</param>
    /// <param name="Parameters">The non-return parameters, in declaration order.</param>
    /// <param name="Redefines">The operations it redefines, each with its owner.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Body">The OCL of the <c>bodyCondition</c> (may be empty).</param>
    public sealed record OperationNode(
        string Name,
        string XmiId,
        string? ReturnType,
        int ReturnLower,
        int ReturnUpper,
        bool IsQuery,
        IReadOnlyList<OperationParameterNode> Parameters,
        IReadOnlyList<OperationReference> Redefines,
        string Documentation,
        string Body);

    /// <summary>
    /// An operation a supertype declares, by reference: its signature and body are in the declaring
    /// metaclass's <see cref="ClassNode.OwnedOperations"/>.
    /// </summary>
    /// <param name="Class">The supertype that declares the operation.</param>
    /// <param name="Operation">The operation name, unique among the operations its metaclass owns.</param>
    /// <param name="RedefinedBy">
    /// The operation that redefines this one in the inheriting metaclass's context, owned by it or by one
    /// of its supertypes, or <c>null</c> when none does, so this one applies.
    /// </param>
    public sealed record InheritedOperationNode(string Class, string Operation, OperationReference? RedefinedBy);

    /// <summary>A single (non-return) operation parameter.</summary>
    /// <param name="Name">The parameter name.</param>
    /// <param name="XmiId">The parameter's XMI id (empty when it has none). It normally stays the same across releases, so a renamed parameter is still the same one.</param>
    /// <param name="Type">The name of the parameter's type, or <c>null</c> when it is untyped.</param>
    /// <param name="Direction">The direction as its lowercase name (e.g. <c>in</c>).</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound, <c>-1</c> when unbounded.</param>
    public sealed record OperationParameterNode(
        string Name,
        string XmiId,
        string? Type,
        string Direction,
        int Lower,
        int Upper);

    /// <summary>An owned constraint and its (OCL) body.</summary>
    /// <param name="Name">The constraint name.</param>
    /// <param name="XmiId">The constraint's XMI id (empty when it has none). It normally stays the same across releases, so a renamed constraint is still the same one.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    /// <param name="Ocl">The constraint body (may be empty).</param>
    /// <param name="ConstrainedElements">The elements the constraint constrains, in model order.</param>
    public sealed record ConstraintNode(
        string Name, string XmiId, string Documentation, string Ocl, IReadOnlyList<ElementReference> ConstrainedElements);

    /// <summary>
    /// A constraint a supertype declares, by reference: its documentation, body and constrained elements
    /// are in the declaring metaclass's <see cref="ClassNode.Constraints"/>.
    /// </summary>
    /// <param name="Class">The supertype that declares the constraint.</param>
    /// <param name="Constraint">The constraint name, unique among the constraints its metaclass owns.</param>
    public sealed record InheritedConstraintNode(string Class, string Constraint);

    /// <summary>
    /// A reference to an attribute by its owner: a classifier for an owned attribute, an association for
    /// an end the association owns. Exactly one of the two owners is set.
    /// </summary>
    /// <param name="Class">The classifier that owns the attribute, or <c>null</c> when an association does.</param>
    /// <param name="Association">The association that owns the end, or <c>null</c> when a classifier does.</param>
    /// <param name="Feature">The attribute name.</param>
    public sealed record FeatureReference(string? Class, string? Association, string Feature);

    /// <summary>A reference to an operation by the classifier that owns it.</summary>
    /// <param name="Class">The classifier that owns the operation.</param>
    /// <param name="Operation">The operation name.</param>
    public sealed record OperationReference(string Class, string Operation);

    /// <summary>A reference to a model element by kind, owner and name.</summary>
    /// <param name="Kind">
    /// The element's UML metaclass, camel-cased: <c>class</c>, <c>property</c>, <c>operation</c>, and so on.
    /// </param>
    /// <param name="Owner">
    /// The classifier or association that owns the element, or <c>null</c> for an element owned by a package.
    /// </param>
    /// <param name="Name">The element name.</param>
    public sealed record ElementReference(string Kind, string? Owner, string Name);

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
    /// <param name="XmiId">The literal's XMI id (empty when it has none). It normally stays the same across releases, so a renamed literal is still the same one.</param>
    /// <param name="Documentation">The documentation (may be empty).</param>
    public sealed record EnumerationLiteralNode(string Name, string XmiId, string Documentation);

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
