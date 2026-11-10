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
        /// <summary>The installed releases and API versions, the default of each, and which artifacts each has generated.</summary>
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
        /// <param name="owner">
        /// Only the redefinitions of the feature this metaclass or association owns, or <c>null</c> for
        /// every feature of that name; several metaclasses declare a feature of the same name.
        /// </param>
        /// <returns>
        /// The redefinitions, each naming the owner of the redefined feature, sorted by metaclass and then feature.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<Redefinition>> Redefinitions(string feature, string? tag = null, string? owner = null);

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
        /// The answer: whether <paramref name="general"/> is one of the ancestors the generator computed,
        /// or the metaclass itself (a metaclass conforms to itself), and then a shortest chain of direct
        /// generalizations as an explanation; <c>null</c> when either name is not a metaclass of the release.
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
        /// The constraints, the metaclass's own first and then the inherited ones by declaring metaclass
        /// and name, each naming the metaclass that declares it; <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> Constraints(string className, bool includeInherited = true, string? tag = null);

        /// <summary>The operations of a metaclass, with their signatures and OCL bodies.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">
        /// Include the operations its ancestors declare that apply to it: an inherited operation that an
        /// operation of its own context redefines is left out, since the redefinition's body applies.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The operations, the metaclass's own first and then the inherited ones by declaring metaclass
        /// and name, each naming the metaclass that declares it; <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<DeclaredMember<OperationNode>>> Operations(string className, bool includeInherited = true, string? tag = null);

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
        /// <param name="owner">
        /// Only the subsettings of the feature this metaclass or association owns, or <c>null</c> for
        /// every feature of that name; several metaclasses declare a feature of the same name.
        /// </param>
        /// <returns>
        /// The subsettings, each naming the owner of the subsetted feature, sorted by metaclass and then feature.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<Subsetting>> Subsettings(string feature, string? tag = null, string? owner = null);

        /// <summary>The composite features through which an element can be owned.</summary>
        /// <param name="element">The element owned, such as <c>PartUsage</c>.</param>
        /// <param name="className">
        /// Only the features this metaclass has, owned or inherited and not redefined in its context, or
        /// <c>null</c> for every feature where it is declared.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The composite features typed by the element or by one of its ancestors, each naming the
        /// metaclass that declares it, sorted by metaclass and then feature. A feature declared once
        /// applies to every metaclass that inherits it, unless that metaclass redefines it. <c>null</c>
        /// when the element, or <paramref name="className"/>, is not an element of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="element"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<TypedFeature>> Containers(string element, string? className = null, string? tag = null);

        /// <summary>The features that can hold a value of a type.</summary>
        /// <param name="type">The type of the value, such as <c>PartUsage</c> or <c>Boolean</c>.</param>
        /// <param name="className">
        /// Only the features this metaclass has, owned or inherited and not redefined in its context, or
        /// <c>null</c> for every feature where it is declared.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features typed by the type or, for a metaclass, by one of its ancestors, each naming the
        /// metaclass that declares it, sorted by metaclass and then feature. A feature declared once
        /// applies to every metaclass that inherits it, unless that metaclass redefines it. <c>null</c>
        /// when the type, or <paramref name="className"/>, is not an element of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="type"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<TypedFeature>> FeaturesAccepting(string type, string? className = null, string? tag = null);

        /// <summary>
        /// The metaclasses around a metaclass: those its own features are typed by, and those whose own
        /// features are typed by it.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features both ways, owned features only; <c>null</c> when <paramref name="className"/> is
        /// not a metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<Neighbourhood> Neighbours(string className, string? tag = null);

        /// <summary>The packages of a name, each with the metaclasses in it and in its nested packages.</summary>
        /// <param name="name">
        /// The package's qualified name, such as <c>KerML::Kernel::Metadata</c>, or its name, such as
        /// <c>Metadata</c>, which more than one package can have.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The package with that qualified name, or else every package with that name, sorted by qualified
        /// name; <c>null</c> when no package has it.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<PackageContents>> Package(string name, string? tag = null);

        /// <summary>
        /// The features a metaclass really has: its own, and the inherited ones no feature of its context
        /// redefines.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features, owned first and then inherited; <c>null</c> when <paramref name="className"/> is
        /// not a metaclass of the release.
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<FeatureInfo>> EffectiveFeatures(string className, string? tag = null);

        /// <summary>The other end of the association a feature is an end of.</summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature, such as <c>ownedFeature</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The feature with its declaring metaclass, its other end, which is <c>null</c> when the feature
        /// is not an association end or that end cannot be named, and the feature that redefines it in the
        /// metaclass's context, if any; <c>null</c> when the metaclass has no such feature, owned or inherited.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<OppositeEnd> Opposite(string className, string feature, string? tag = null);

        /// <summary>The constraints that name a feature among the elements they constrain.</summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The constraints, each naming the metaclass that declares it, sorted by metaclass and then name,
        /// grounded as model for a feature the metaclass owns and as derived for one it inherits; empty when
        /// none constrains the feature. <c>null</c> when the metaclass has no such feature.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> FeatureConstraints(
            string className, string feature, string? tag = null);

        /// <summary>
        /// The features that contribute to a feature by subsetting or redefining it, directly or through
        /// another contributor, such as everything that contributes to the derived union
        /// <c>Namespace::ownedMember</c>.
        /// </summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature contributed to, such as <c>ownedMember</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// One contribution for each subsets or redefines reference to the feature or to one of its
        /// contributors, sorted by metaclass, feature, kind and then the owner referred to; empty when
        /// nothing contributes; <c>null</c> when the metaclass has no such feature.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<Contribution>> Contributors(string className, string feature, string? tag = null);

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
        QueryResult<IReadOnlyList<ClauseUse>> ElementsInClause(string document, string clause, bool includeSubclauses = false, string? tag = null);

        /// <summary>A standard-library declaration by qualified name: its kind, page and source file.</summary>
        /// <param name="qualifiedName">The qualified name, such as <c>ISQBase::mass</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The declaration, or <c>null</c> when the library declares no such name.</returns>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        QueryResult<ModelLibraryDeclarationEntry> Declaration(string qualifiedName, string? tag = null);

        /// <summary>One clause of the KerML or SysML specification, with its verbatim text.</summary>
        /// <param name="document">The specification: <c>kerml</c> or <c>sysml2</c>.</param>
        /// <param name="clause">The clause number, such as <c>7.4.2</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The clause; <c>null</c> when the specification has no such clause. When the release has not
        /// generated the specification text, which needs the OMG PDFs, the result is not an error but
        /// <see cref="QueryResult{T}.Unavailable"/>, saying how to generate it.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="document"/> is not <c>kerml</c> or <c>sysml2</c>, or <paramref name="clause"/> is
        /// <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, or its specification index or page is invalid.
        /// </exception>
        QueryResult<SpecificationClause> Clause(string document, string clause, string? tag = null);

        /// <summary>
        /// Searches the release's metaclasses, standard-library declarations and specification clauses by
        /// name, qualified name, clause number or title.
        /// </summary>
        /// <param name="text">The text to look for.</param>
        /// <param name="kind">Only hits of this kind, one of <see cref="HyphaAddress.Kinds"/>, or <c>null</c> for any.</param>
        /// <param name="limit">The most hits returned, from 1 to <c>200</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The hits, closest match first, then by kind and name; empty when nothing matches. A clause whose
        /// title is the text matches as closely as one whose number is. The model library and the
        /// specification text are searched when they have been generated.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="text"/> is <c>null</c>, empty or white space, <paramref name="kind"/> is not a
        /// known kind, or <paramref name="limit"/> is out of range.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The release is not installed, has no default, has not generated its metamodel, or its documents
        /// are invalid.
        /// </exception>
        QueryResult<IReadOnlyList<SearchHit>> Search(string text, string? kind = null, int limit = 20, string? tag = null);

        /// <summary>Resolves an address to the generated page it names, through the release's indexes.</summary>
        /// <param name="address">The address.</param>
        /// <returns>
        /// The page; <c>null</c> when the release has no such element, declaration or clause.
        /// <see cref="QueryResult{T}.Unavailable"/> for a clause of a specification the release has not generated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="address"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">
        /// The address's kind is not one of <see cref="HyphaAddress.Kinds"/>, which only an address built
        /// directly rather than parsed can have.
        /// </exception>
        /// <exception cref="Lookup.KnowledgeLookupException">
        /// The address's release is not installed, has not generated the artifact, or its index points at a
        /// page that is missing or outside the release's folder.
        /// </exception>
        QueryResult<ResolvedAddress> Resolve(HyphaAddress address);
    }
}
