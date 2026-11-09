// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeQuery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// Structural questions about a release's generated knowledge, answered from its
    /// <c>metamodel.json</c>, <c>index.json</c>, <c>cross-references.json</c> and model-library index.
    /// </summary>
    /// <remarks>
    /// Read-only: it never fetches or generates. Names match exactly (ordinal), as the documents key
    /// them. Every query takes an optional release tag and answers from the default release without
    /// one. Lists are sorted ordinally, so the same knowledge always gives the same answer.
    /// </remarks>
    public interface IKnowledgeQuery
    {
        /// <summary>The installed releases and API versions, and the default of each.</summary>
        /// <returns>The inventory; empty lists when nothing is installed.</returns>
        ReleaseInventory Releases();

        /// <summary>The metaclasses that specialize a metaclass, directly or not.</summary>
        /// <param name="of">The general metaclass.</param>
        /// <param name="concreteOnly">Leave abstract metaclasses out.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The names, or <c>null</c> when <paramref name="of"/> is not a metaclass of the release.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<string>> Subclasses(string of, bool concreteOnly = false, string? tag = null);

        /// <summary>The metaclasses that own a feature typed by a given type.</summary>
        /// <param name="type">The feature type.</param>
        /// <param name="compositeOnly">Count only composite features.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The names; empty when no metaclass has such a feature.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<string>> ClassesWithFeatureTyped(string type, bool compositeOnly = false, string? tag = null);

        /// <summary>
        /// A metaclass's full effective feature set, owned features first, each inherited one naming the
        /// metaclass it comes from.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="name">Return only the features with this name, or <c>null</c> for all.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features, or <c>null</c> when <paramref name="className"/> is not a metaclass of the release.
        /// An empty list with a name means the metaclass has no such feature, owned or inherited.
        /// </returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<FeatureInfo>> Features(string className, string? name = null, string? tag = null);

        /// <summary>The owned features that redefine a feature of the given name.</summary>
        /// <param name="feature">The redefined feature's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The redefinitions, sorted by metaclass and then feature.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<Redefinition>> Redefinitions(string feature, string? tag = null);

        /// <summary>The metaclasses a metaclass specializes, directly or not.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The names, sorted; <c>null</c> when <paramref name="className"/> is not a metaclass of the release.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<string>> Ancestors(string className, string? tag = null);

        /// <summary>Whether one metaclass specializes another, and through which supertypes.</summary>
        /// <param name="specific">The metaclass asked about.</param>
        /// <param name="general">The metaclass it may specialize.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The answer with a shortest chain of generalizations; <c>null</c> when either name is not a
        /// metaclass of the release. A metaclass specializes itself, with a chain of one.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="specific"/> or <paramref name="general"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<Specialization> Specializes(string specific, string general, string? tag = null);

        /// <summary>The OCL constraints that apply to a metaclass.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the constraints its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The constraints, the metaclass's own first and then its ancestors' in name order, each naming
        /// the metaclass that declares it; <c>null</c> when <paramref name="className"/> is not a
        /// metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> Constraints(
            string className, bool includeInherited = true, string? tag = null);

        /// <summary>The operations of a metaclass, with their signatures and OCL bodies.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the operations its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The operations, the metaclass's own first and then its ancestors' in name order, each naming
        /// the metaclass that declares it; <c>null</c> when <paramref name="className"/> is not a
        /// metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<DeclaredMember<OperationNode>>> Operations(
            string className, bool includeInherited = true, string? tag = null);

        /// <summary>The literals of an enumeration, in the order the metamodel declares them.</summary>
        /// <param name="enumeration">The enumeration, such as <c>VisibilityKind</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The literals; <c>null</c> when the release has no such enumeration.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="enumeration"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<EnumerationLiteralNode>> Literals(string enumeration, string? tag = null);

        /// <summary>The owned features that subset a feature of the given name.</summary>
        /// <param name="feature">The subsetted feature's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The subsettings, sorted by metaclass and then feature.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<Subsetting>> Subsettings(string feature, string? tag = null);

        /// <summary>One element's entry in the metamodel index: its kind, package, supertypes and page.</summary>
        /// <param name="name">The element's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The entry, or <c>null</c> when the release has no such element.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<LookupEntry> Element(string name, string? tag = null);

        /// <summary>
        /// An element's cross-references: the clauses that treat it, the productions that build it, the
        /// features its syntax fills in and the examples that declare it. Each edge names its own provenance.
        /// </summary>
        /// <param name="element">The element's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The entry, or <c>null</c> when the release has no cross-references for the element.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<CrossReferenceEntry> CrossReference(string element, string? tag = null);

        /// <summary>Which elements' syntax fills in a feature, through which operator and productions.</summary>
        /// <param name="feature">Only this feature, or <c>null</c> for any.</param>
        /// <param name="assignmentOperator">Only this operator (<c>=</c>, <c>+=</c>, <c>?=</c>), or <c>null</c> for any.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The assignments, sorted by element, in the order the cross-references list them.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<FeatureAssignment>> FeatureAssignments(string? feature = null, string? assignmentOperator = null, string? tag = null);

        /// <summary>The elements a grammar production builds.</summary>
        /// <param name="production">The production, such as <c>PartUsage</c>.</param>
        /// <param name="grammar">Only this grammar (<c>kerml</c>, <c>sysml</c>), or <c>null</c> for any.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The elements and the links, sorted by element and then grammar.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="production"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<ProductionUse>> BuiltBy(string production, string? grammar = null, string? tag = null);

        /// <summary>The elements a specification clause treats.</summary>
        /// <param name="document">The specification, as the cross-references key it (<c>kerml</c>, <c>sysml2</c>).</param>
        /// <param name="clause">The clause number, such as <c>8.3.6</c>.</param>
        /// <param name="includeSubclauses">Include the clauses below it, such as <c>8.3.6.3</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The elements and the links, sorted by element and then clause.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="document"/> or <paramref name="clause"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<ClauseUse>> ElementsInClause(
            string document, string clause, bool includeSubclauses = false, string? tag = null);

        /// <summary>A standard-library declaration by qualified name: its kind, page and source file.</summary>
        /// <param name="qualifiedName">The qualified name, such as <c>ISQBase::mass</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The declaration, or <c>null</c> when the library declares no such name.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<ModelLibraryDeclarationEntry> Declaration(string qualifiedName, string? tag = null);
    }
}
