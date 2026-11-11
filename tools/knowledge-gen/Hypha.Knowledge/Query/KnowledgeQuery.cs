// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeQuery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Metamodel;
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// Answers structural questions about a release from its generated JSON, read through the same
    /// contracts the generators write and held in memory until the files change.
    /// </summary>
    /// <remarks>
    /// Read-only: it never fetches or generates. Names match exactly (ordinal), as the documents key
    /// them. Lists are sorted ordinally, so the same knowledge always gives the same answer. Safe to
    /// call from several threads at once.
    /// </remarks>
    public sealed class KnowledgeQuery : IKnowledgeQuery
    {
        /// <summary>The most hits a search returns.</summary>
        public const int MaxSearchHits = 200;

        /// <summary>Where each release's generated knowledge lives, and which releases are installed.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Which API versions are installed.</summary>
        private readonly IApiLayout apiLayout;

        /// <summary>The documents read so far.</summary>
        private readonly IDocumentCache documents;

        /// <summary>Reads the specification text.</summary>
        private readonly ISpecificationReader specifications;

        /// <summary>Compares two releases' metamodels, for the release diff.</summary>
        private readonly IMetamodelDiffer differ;

        /// <summary>Compares two releases' cross-references, for the release diff.</summary>
        private readonly ICrossReferenceDiffer crossReferenceDiffer;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeQuery"/> class.</summary>
        /// <param name="layout">Where each release's generated knowledge lives, and which releases are installed.</param>
        /// <param name="apiLayout">Which API versions are installed.</param>
        /// <param name="documents">The documents read so far, shared with the other queries.</param>
        /// <param name="specifications">Reads the specification text.</param>
        /// <param name="differ">Compares two releases' metamodels, for the release diff.</param>
        /// <param name="crossReferenceDiffer">Compares two releases' cross-references, for the release diff.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="layout"/>, <paramref name="apiLayout"/>, <paramref name="documents"/>,
        /// <paramref name="specifications"/>, <paramref name="differ"/> or
        /// <paramref name="crossReferenceDiffer"/> is <c>null</c>.
        /// </exception>
        public KnowledgeQuery(
            IKnowledgeLayout layout,
            IApiLayout apiLayout,
            IDocumentCache documents,
            ISpecificationReader specifications,
            IMetamodelDiffer differ,
            ICrossReferenceDiffer crossReferenceDiffer)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(apiLayout);
            ArgumentNullException.ThrowIfNull(documents);
            ArgumentNullException.ThrowIfNull(specifications);
            ArgumentNullException.ThrowIfNull(differ);
            ArgumentNullException.ThrowIfNull(crossReferenceDiffer);

            this.layout = layout;
            this.apiLayout = apiLayout;
            this.documents = documents;
            this.specifications = specifications;
            this.differ = differ;
            this.crossReferenceDiffer = crossReferenceDiffer;
        }

        /// <summary>The installed releases and API versions, the default of each, and which artifacts each has generated.</summary>
        /// <returns>
        /// The inventory, read from the manifests and the knowledge folders on each call; empty lists when
        /// nothing is installed.
        /// </returns>
        public ReleaseInventory Releases() => new(this.layout.DefaultTag, ReleaseStatus.ReadAll(this.layout), this.apiLayout.DefaultTag,
            ApiVersionStatus.ReadAll(this.apiLayout));

        /// <summary>The metaclasses that specialize a metaclass, directly or not.</summary>
        /// <param name="of">The general metaclass.</param>
        /// <param name="concreteOnly">Leave abstract metaclasses out.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The names, sorted, grounded as <see cref="Provenance.Derived"/> since they come from the
        /// ancestor closure; <c>null</c> when <paramref name="of"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="of"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<string>> Subclasses(string of, bool concreteOnly = false, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(of);

            var (graph, grounding) = this.Graph(tag, Provenance.Derived);

            if (!graph.Classes.Any(node => string.Equals(node.Name, of, StringComparison.Ordinal)))
            {
                return new(null, grounding);
            }

            IReadOnlyList<string> names = graph.Classes
                .Where(node => node.AllAncestors.Contains(of, StringComparer.Ordinal))
                .Where(node => !concreteOnly || !node.IsAbstract)
                .Select(node => node.Name)
                .Order(StringComparer.Ordinal)
                .ToList();

            return new(names, grounding);
        }

        /// <summary>The metaclasses that own a feature typed by a given type.</summary>
        /// <param name="type">The feature type.</param>
        /// <param name="compositeOnly">Count only composite features.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The names, sorted and each named once, grounded as <see cref="Provenance.Model"/>; empty when
        /// no metaclass owns such a feature.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<string>> ClassesWithFeatureTyped(string type, bool compositeOnly = false, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            IReadOnlyList<string> names = graph.Classes
                .Where(node => node.OwnedAttributes.Any(attribute =>
                    string.Equals(attribute.Type, type, StringComparison.Ordinal)
                    && (!compositeOnly || attribute.IsComposite)))
                .Select(node => node.Name)
                .Order(StringComparer.Ordinal)
                .ToList();

            return new(names, grounding);
        }

        /// <summary>
        /// A metaclass's full effective feature set, owned features first, each inherited one naming the
        /// metaclass it comes from.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="name">Return only the features with this name, or <c>null</c> for all.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features in the order the metamodel lists them, grounded as <see cref="Provenance.Derived"/>
        /// since the inherited set is a closure; <c>null</c> when <paramref name="className"/> is not a
        /// metaclass of the release. An empty list with a name means the metaclass has no such feature,
        /// owned or inherited.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<FeatureInfo>> Features(string className, string? name = null, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var (graph, grounding) = this.Graph(tag, Provenance.Derived);
            var node = graph.Classes.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, className, StringComparison.Ordinal));

            if (node is null)
            {
                return new(null, grounding);
            }

            var owned = node.OwnedAttributes.Select(attribute => new FeatureInfo(
                attribute.Name,
                attribute.Type,
                attribute.Lower,
                attribute.Upper,
                attribute.IsDerived,
                attribute.IsDerivedUnion,
                attribute.IsComposite,
                attribute.IsOrdered,
                attribute.IsUnique,
                attribute.IsReadOnly,
                attribute.DefaultValue,
                attribute.Redefines,
                attribute.Subsets,
                attribute.Opposite,
                InheritedFrom: null,
                RedefinedBy: null));
            var inherited = node.InheritedAttributes.Select(attribute => new FeatureInfo(
                attribute.Name,
                attribute.Type,
                attribute.Lower,
                attribute.Upper,
                attribute.IsDerived,
                attribute.IsDerivedUnion,
                attribute.IsComposite,
                attribute.IsOrdered,
                attribute.IsUnique,
                attribute.IsReadOnly,
                attribute.DefaultValue,
                attribute.Redefines,
                attribute.Subsets,
                attribute.Opposite,
                attribute.InheritedFrom,
                attribute.RedefinedBy));

            IReadOnlyList<FeatureInfo> features = owned.Concat(inherited)
                .Where(feature => name is null || string.Equals(feature.Name, name, StringComparison.Ordinal))
                .ToList();

            return new(features, grounding);
        }

        /// <summary>The owned features that redefine a feature of the given name.</summary>
        /// <param name="feature">The redefined feature's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <param name="owner">
        /// Only the redefinitions of the feature this metaclass or association owns, or <c>null</c> for
        /// every feature of that name; several metaclasses declare a feature of the same name.
        /// </param>
        /// <returns>
        /// The redefinitions, each naming the owner of the redefined feature, sorted by metaclass and then
        /// feature, grounded as <see cref="Provenance.Model"/>; empty when nothing redefines such a feature.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<Redefinition>> Redefinitions(string feature, string? tag = null, string? owner = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            IReadOnlyList<Redefinition> redefinitions = graph.Classes
                .SelectMany(node => node.OwnedAttributes
                    .SelectMany(attribute => attribute.Redefines
                        .Where(reference => Names(reference, feature, owner))
                        .Select(reference => new Redefinition(node.Name, attribute.Name, reference))))
                .OrderBy(redefinition => redefinition.Class, StringComparer.Ordinal)
                .ThenBy(redefinition => redefinition.Feature, StringComparer.Ordinal)
                .ToList();

            return new(redefinitions, grounding);
        }

        /// <summary>The metaclasses a metaclass specializes, directly or not.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The names, sorted, grounded as <see cref="Provenance.Derived"/> since they come from the ancestor
        /// closure; <c>null</c> when <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<string>> Ancestors(string className, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var (graph, grounding) = this.Graph(tag, Provenance.Derived);
            var node = ClassNamed(graph, className);

            return new(node?.AllAncestors.Order(StringComparer.Ordinal).ToList(), grounding);
        }

        /// <summary>Whether one metaclass specializes another, and through which supertypes.</summary>
        /// <param name="specific">The metaclass asked about.</param>
        /// <param name="general">The metaclass it may specialize.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The answer, grounded as <see cref="Provenance.Derived"/>: whether <paramref name="general"/> is
        /// one of the ancestors the generator computed, or the metaclass itself (a metaclass conforms to
        /// itself), and then a shortest chain of direct generalizations as an explanation. <c>null</c> when
        /// either name is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="specific"/> or <paramref name="general"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<Specialization> Specializes(string specific, string general, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(specific);
            ArgumentException.ThrowIfNullOrWhiteSpace(general);

            var (graph, grounding) = this.Graph(tag, Provenance.Derived);
            var classes = graph.Classes.ToDictionary(node => node.Name, StringComparer.Ordinal);

            if (!classes.TryGetValue(specific, out var node) || !classes.ContainsKey(general))
            {
                return new(null, grounding);
            }

            // The answer is the generator's closure; the chain only explains it, following the generated
            // direct generalizations, so no UML rule is applied here.
            var specializes = string.Equals(specific, general, StringComparison.Ordinal)
                              || node.AllAncestors.Contains(general, StringComparer.Ordinal);

            return new(
                new Specialization(specific, general, specializes, specializes ? ShortestPath(classes, specific, general) : []),
                grounding);
        }

        /// <summary>The OCL constraints that apply to a metaclass.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the constraints its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The constraints, the metaclass's own first and then the inherited ones by declaring metaclass
        /// and name, each naming the metaclass that declares it. Grounded as <see cref="Provenance.Model"/>
        /// for the metaclass's own only, and as <see cref="Provenance.Derived"/> with the inherited ones,
        /// which the generator computed from the ancestor closure. <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> Constraints(string className, bool includeInherited = true, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var (graph, grounding) = this.Graph(tag, includeInherited ? Provenance.Derived : Provenance.Model);
            var node = ClassNamed(graph, className);

            if (node is null)
            {
                return new(null, grounding);
            }

            IEnumerable<DeclaredMember<ConstraintNode>> inherited = includeInherited
                ? node.InheritedConstraints
                    .Select(reference => new DeclaredMember<ConstraintNode>(
                        reference.Class,
                        Member(graph, reference.Class, reference.Constraint, declaring => declaring.Constraints, constraint => constraint.Name)))
                    .OrderBy(member => member.DeclaredIn, StringComparer.Ordinal)
                    .ThenBy(member => member.Member.Name, StringComparer.Ordinal)
                : [];

            IReadOnlyList<DeclaredMember<ConstraintNode>> constraints = node.Constraints
                .Select(constraint => new DeclaredMember<ConstraintNode>(node.Name, constraint))
                .Concat(inherited)
                .ToList();

            return new(constraints, grounding);
        }

        /// <summary>The operations of a metaclass, with their signatures and OCL bodies.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">
        /// Include the operations its ancestors declare that apply to it: an inherited operation that an
        /// operation of its own context redefines is left out, since the redefinition's body applies.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The operations, the metaclass's own first and then the inherited ones by declaring metaclass
        /// and name, each naming the metaclass that declares it. Grounded as <see cref="Provenance.Model"/>
        /// for the metaclass's own only, and as <see cref="Provenance.Derived"/> with the inherited ones.
        /// <c>null</c> when <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<DeclaredMember<OperationNode>>> Operations(string className, bool includeInherited = true, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var (graph, grounding) = this.Graph(tag, includeInherited ? Provenance.Derived : Provenance.Model);
            var node = ClassNamed(graph, className);

            if (node is null)
            {
                return new(null, grounding);
            }

            IEnumerable<DeclaredMember<OperationNode>> inherited = includeInherited
                ? node.InheritedOperations
                    .Where(reference => reference.RedefinedBy is null)
                    .Select(reference => new DeclaredMember<OperationNode>(
                        reference.Class,
                        Member(graph, reference.Class, reference.Operation, declaring => declaring.OwnedOperations, operation => operation.Name)))
                    .OrderBy(member => member.DeclaredIn, StringComparer.Ordinal)
                    .ThenBy(member => member.Member.Name, StringComparer.Ordinal)
                : [];

            IReadOnlyList<DeclaredMember<OperationNode>> operations = node.OwnedOperations
                .Select(operation => new DeclaredMember<OperationNode>(node.Name, operation))
                .Concat(inherited)
                .ToList();

            return new(operations, grounding);
        }

        /// <summary>The literals of an enumeration, in the order the metamodel declares them.</summary>
        /// <param name="enumeration">The enumeration, such as <c>VisibilityKind</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The literals, grounded as <see cref="Provenance.Model"/>; <c>null</c> when the release has no
        /// such enumeration.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="enumeration"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<EnumerationLiteralNode>> Literals(string enumeration, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(enumeration);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);
            var node = graph.Enumerations.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, enumeration, StringComparison.Ordinal));

            return new(node?.Literals, grounding);
        }

        /// <summary>The owned features that subset a feature of the given name.</summary>
        /// <param name="feature">The subsetted feature's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <param name="owner">
        /// Only the subsettings of the feature this metaclass or association owns, or <c>null</c> for
        /// every feature of that name; several metaclasses declare a feature of the same name.
        /// </param>
        /// <returns>
        /// The subsettings, each naming the owner of the subsetted feature, sorted by metaclass and then
        /// feature, grounded as <see cref="Provenance.Model"/>; empty when nothing subsets such a feature.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<Subsetting>> Subsettings(string feature, string? tag = null, string? owner = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            IReadOnlyList<Subsetting> subsettings = graph.Classes
                .SelectMany(node => node.OwnedAttributes
                    .SelectMany(attribute => attribute.Subsets
                        .Where(reference => Names(reference, feature, owner))
                        .Select(reference => new Subsetting(node.Name, attribute.Name, reference))))
                .OrderBy(subsetting => subsetting.Class, StringComparer.Ordinal)
                .ThenBy(subsetting => subsetting.Feature, StringComparer.Ordinal)
                .ToList();

            return new(subsettings, grounding);
        }

        /// <summary>The composite features through which an element can be owned.</summary>
        /// <param name="element">The element owned, such as <c>PartUsage</c>.</param>
        /// <param name="className">
        /// Only the features this metaclass has, owned or inherited and not redefined in its context, or
        /// <c>null</c> for every feature where it is declared.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The composite features typed by the element or by one of its ancestors, each naming the
        /// metaclass that declares it, sorted by metaclass and then feature; grounded as
        /// <see cref="Provenance.Derived"/>, since conformance comes from the ancestor closure. A feature
        /// declared once applies to every metaclass that inherits it, unless that metaclass redefines it.
        /// <c>null</c> when the element, or <paramref name="className"/>, is not an element of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="element"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<TypedFeature>> Containers(string element, string? className = null, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(element);

            return this.Accepting(element, className, tag, compositeOnly: true);
        }

        /// <summary>The features that can hold a value of a type.</summary>
        /// <param name="type">The type of the value, such as <c>PartUsage</c> or <c>Boolean</c>.</param>
        /// <param name="className">
        /// Only the features this metaclass has, owned or inherited and not redefined in its context, or
        /// <c>null</c> for every feature where it is declared.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features typed by the type or, for a metaclass, by one of its ancestors, each naming the
        /// metaclass that declares it, sorted by metaclass and then feature; grounded as
        /// <see cref="Provenance.Derived"/>. A feature declared once applies to every metaclass that
        /// inherits it, unless that metaclass redefines it. <c>null</c> when the type, or
        /// <paramref name="className"/>, is not an element of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<TypedFeature>> FeaturesAccepting(string type, string? className = null, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);

            return this.Accepting(type, className, tag, compositeOnly: false);
        }

        /// <summary>
        /// The metaclasses around a metaclass: those its own features are typed by, and those whose own
        /// features are typed by it.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features both ways, owned features only, with their multiplicity and whether they are
        /// composite, grounded as <see cref="Provenance.Model"/>; <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<Neighbourhood> Neighbours(string className, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);
            var node = ClassNamed(graph, className);

            if (node is null)
            {
                return new(null, grounding);
            }

            var metaclasses = graph.Classes.Select(candidate => candidate.Name).ToHashSet(StringComparer.Ordinal);

            IReadOnlyList<TypedFeature> outgoing = Declared(node)
                .Where(feature => feature.Type is not null && metaclasses.Contains(feature.Type))
                .OrderBy(feature => feature.Feature, StringComparer.Ordinal)
                .ToList();

            IReadOnlyList<TypedFeature> incoming = graph.Classes
                .Where(candidate => !string.Equals(candidate.Name, className, StringComparison.Ordinal))
                .SelectMany(Declared)
                .Where(feature => string.Equals(feature.Type, className, StringComparison.Ordinal))
                .OrderBy(feature => feature.Class, StringComparer.Ordinal)
                .ThenBy(feature => feature.Feature, StringComparer.Ordinal)
                .ToList();

            return new(new Neighbourhood(className, outgoing, incoming), grounding);
        }

        /// <summary>The packages of a name, each with the metaclasses in it and in its nested packages.</summary>
        /// <param name="name">
        /// The package's qualified name, such as <c>KerML::Kernel::Metadata</c>, or its name, such as
        /// <c>Metadata</c>, which more than one package can have.
        /// </param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The package with that qualified name, or else every package with that name, sorted by qualified
        /// name, grounded as <see cref="Provenance.Model"/>; <c>null</c> when no package has it.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<PackageContents>> Package(string name, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            var matches = graph.Packages
                .Where(package => string.Equals(package.QualifiedName, name, StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                matches = graph.Packages.Where(package => string.Equals(package.Name, name, StringComparison.Ordinal)).ToList();
            }

            if (matches.Count == 0)
            {
                return new(null, grounding);
            }

            IReadOnlyList<PackageContents> packages = matches
                .OrderBy(package => package.QualifiedName, StringComparer.Ordinal)
                .Select(package =>
                {
                    var prefix = package.QualifiedName + "::";
                    var nested = graph.Packages
                        .Where(candidate => candidate.QualifiedName.StartsWith(prefix, StringComparison.Ordinal))
                        .ToList();

                    return new PackageContents(
                        package.Name,
                        package.QualifiedName,
                        package.Classes.Order(StringComparer.Ordinal).ToList(),
                        nested
                            .Where(candidate => !candidate.QualifiedName[prefix.Length..].Contains("::", StringComparison.Ordinal))
                            .Select(candidate => candidate.QualifiedName)
                            .Order(StringComparer.Ordinal)
                            .ToList(),
                        package.Classes.Concat(nested.SelectMany(candidate => candidate.Classes))
                            .Distinct(StringComparer.Ordinal)
                            .Order(StringComparer.Ordinal)
                            .ToList());
                })
                .ToList();

            return new(packages, grounding);
        }

        /// <summary>
        /// The features a metaclass really has: its own, and the inherited ones no feature of its context
        /// redefines.
        /// </summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The features, owned first and then inherited, in the order the metamodel lists them, grounded as
        /// <see cref="Provenance.Derived"/>; <c>null</c> when <paramref name="className"/> is not a
        /// metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<FeatureInfo>> EffectiveFeatures(string className, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            var features = this.Features(className, tag: tag);

            return new(features.Value?.Where(feature => feature.RedefinedBy is null).ToList(), features.Grounding);
        }

        /// <summary>The other end of the association a feature is an end of.</summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature, such as <c>ownedFeature</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The feature with its declaring metaclass, its other end and, when the metaclass's context
        /// redefines the inherited feature, the feature that does, grounded as
        /// <see cref="Provenance.Derived"/>; the other end is <c>null</c> when the feature is not an
        /// association end, or that end cannot be named. <c>null</c> when the metaclass has no such
        /// feature, owned or inherited.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<OppositeEnd> Opposite(string className, string feature, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var features = this.Features(className, feature, tag);
            var found = features.Value?.FirstOrDefault();

            return new(
                found is null
                    ? null
                    : new OppositeEnd(className, feature, found.InheritedFrom ?? className, found.Opposite, found.RedefinedBy),
                features.Grounding);
        }

        /// <summary>The constraints that name a feature among the elements they constrain.</summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The constraints, each naming the metaclass that declares it, sorted by metaclass and then name,
        /// grounded as <see cref="Provenance.Model"/> for a feature the metaclass owns and as
        /// <see cref="Provenance.Derived"/> for one it inherits; empty when none constrains the feature, which in
        /// the KerML and SysML releases so far is always the case, since their constraints name their
        /// metaclass. <c>null</c> when the metaclass has no such feature, owned or inherited.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> FeatureConstraints(
            string className, string feature, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var owner = this.DeclaringClass(className, feature, tag);

            // An inherited feature is found through the generated inheritance closure.
            var (graph, grounding) = this.Graph(
                tag,
                owner is null || string.Equals(owner, className, StringComparison.Ordinal) ? Provenance.Model : Provenance.Derived);

            if (owner is null)
            {
                return new(null, grounding);
            }

            IReadOnlyList<DeclaredMember<ConstraintNode>> constraints = graph.Classes
                .SelectMany(node => node.Constraints
                    .Where(constraint => constraint.ConstrainedElements.Any(element =>
                        string.Equals(element.Kind, "property", StringComparison.Ordinal)
                        && string.Equals(element.Owner, owner, StringComparison.Ordinal)
                        && string.Equals(element.Name, feature, StringComparison.Ordinal)))
                    .Select(constraint => new DeclaredMember<ConstraintNode>(node.Name, constraint)))
                .OrderBy(member => member.DeclaredIn, StringComparer.Ordinal)
                .ThenBy(member => member.Member.Name, StringComparer.Ordinal)
                .ToList();

            return new(constraints, grounding);
        }

        /// <summary>
        /// The features that contribute to a feature by subsetting or redefining it, directly or through
        /// another contributor, such as everything that contributes to the derived union
        /// <c>Namespace::ownedMember</c>. A redefining feature contributes too: its values are values of the
        /// feature it redefines, as <c>EnumerationDefinition::enumeratedValue</c>'s are of
        /// <c>Definition::variant</c>.
        /// </summary>
        /// <param name="className">The metaclass, which owns or inherits the feature.</param>
        /// <param name="feature">The feature contributed to, such as <c>ownedMember</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// One contribution for each subsets or redefines reference to the feature or to one of its
        /// contributors, sorted by metaclass, feature, kind and then the owner referred to, grounded as
        /// <see cref="Provenance.Derived"/>, since the indirect ones follow the generated references;
        /// empty when nothing contributes. <c>null</c> when the metaclass has no such feature, owned or
        /// inherited.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="className"/> or <paramref name="feature"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<Contribution>> Contributors(string className, string feature, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var owner = this.DeclaringClass(className, feature, tag);
            var (graph, grounding) = this.Graph(tag, Provenance.Derived);

            if (owner is null)
            {
                return new(null, grounding);
            }

            // Breadth first over the generated subsets and redefines references: no UML rule is applied,
            // the edges are the generator's.
            var edges = graph.Classes
                .SelectMany(node => node.OwnedAttributes.SelectMany(attribute =>
                    attribute.Subsets.Select(reference => new Contribution(node.Name, attribute.Name, "subsets", reference))
                        .Concat(attribute.Redefines.Select(reference =>
                            new Contribution(node.Name, attribute.Name, "redefines", reference)))))
                .ToList();

            var seen = new HashSet<(string, string)> { (owner, feature) };
            var queue = new Queue<(string Owner, string Feature)>([(owner, feature)]);
            var contributors = new List<Contribution>();

            while (queue.Count > 0)
            {
                var (targetOwner, target) = queue.Dequeue();

                foreach (var edge in edges.Where(candidate => Names(candidate.Contributes, target, targetOwner)))
                {
                    contributors.Add(edge);

                    if (seen.Add((edge.Class, edge.Feature)))
                    {
                        queue.Enqueue((edge.Class, edge.Feature));
                    }
                }
            }

            IReadOnlyList<Contribution> sorted = contributors
                .OrderBy(contribution => contribution.Class, StringComparer.Ordinal)
                .ThenBy(contribution => contribution.Feature, StringComparer.Ordinal)
                .ThenBy(contribution => contribution.Through, StringComparer.Ordinal)
                .ThenBy(contribution => contribution.Contributes.Class ?? contribution.Contributes.Association, StringComparer.Ordinal)
                .ToList();

            return new(sorted, grounding);
        }

        /// <summary>
        /// What changed between two installed releases: metaclasses, enumerations and primitive types
        /// added, removed, renamed or moved, and what changed in the attributes, operations, parameters,
        /// constraints and literals they own. An element or member is matched on its XMI id, so a rename
        /// is reported as one, and on its name when the ids differ. The ids normally stay the same, but
        /// upstream regenerated every one between 2025-04 and 2025-06, so across that gap a rename reads
        /// as a removal plus an addition. Inherited members are not compared, since they follow from what
        /// their declaring metaclass owns.
        /// For the elements found in both releases it also compares the clauses, grammar productions,
        /// features and examples <c>cross-references.json</c> links them to, when both releases have
        /// generated it.
        /// </summary>
        /// <param name="from">The older release.</param>
        /// <param name="to">The newer release.</param>
        /// <param name="element">
        /// Only the changes to this element and its members, by its name in either release, or <c>null</c>
        /// for every change.
        /// </param>
        /// <returns>
        /// Both releases and the changes, sorted by element, member, kind and then field, grounded as
        /// <see cref="Provenance.Derived"/> on <paramref name="to"/>; empty when nothing changed.
        /// <c>null</c> when <paramref name="element"/> is in neither release.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="from"/> or <paramref name="to"/> is <c>null</c>, empty or white space, or
        /// <paramref name="element"/> is empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// Either release is not installed or has not been generated, its documents are invalid, or the two
        /// metamodel documents have different schema versions. Two cross-reference documents of different
        /// schema versions are not an error: the cross-reference part is <c>null</c> instead.
        /// </exception>
        public QueryResult<ReleaseDiff> Diff(string from, string to, string? element = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(from);
            ArgumentException.ThrowIfNullOrWhiteSpace(to);

            if (element is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(element);
            }

            var (older, olderGrounding) = this.Graph(from, Provenance.Derived);
            var (newer, grounding) = this.Graph(to, Provenance.Derived);

            if (!string.Equals(older.SchemaVersion, newer.SchemaVersion, StringComparison.Ordinal))
            {
                throw new KnowledgeLookupException(
                    $"Release '{from}' has metamodel schema {older.SchemaVersion} and '{to}' has {newer.SchemaVersion}, "
                    + "so they cannot be compared. Regenerate both with the same hypha "
                    + $"('hypha generate --tag {from}' and 'hypha generate --tag {to}').");
            }

            var changes = this.differ.Compare(older, newer);

            // The elements renamed between the releases, older name to newer name.
            var renames = changes
                .Where(change => change is { Member: null, Kind: MetamodelChange.Renamed })
                .GroupBy(change => change.From!, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First().Element, StringComparer.Ordinal);

            if (element is not null && !Declares(older, element) && !Declares(newer, element))
            {
                return new(null, grounding);
            }

            var crossReferences = this.CompareCrossReferences(from, to, renames);

            if (element is not null)
            {
                // An element renamed between the releases is written under its newer name.
                var renamed = renames.GetValueOrDefault(element);

                bool Concerns(string name) =>
                    string.Equals(name, element, StringComparison.Ordinal) || string.Equals(name, renamed, StringComparison.Ordinal);

                changes = changes.Where(change => Concerns(change.Element)).ToList();
                crossReferences = crossReferences?.Where(change => Concerns(change.Element)).ToList();
            }

            return new(
                new ReleaseDiff(
                    new DiffSide(olderGrounding.Release, olderGrounding.Source, older.SourceXmiSha256),
                    new DiffSide(grounding.Release, grounding.Source, newer.SourceXmiSha256),
                    changes,
                    crossReferences),
                grounding);
        }

        /// <summary>Compares the cross-references of two releases, when both have generated them.</summary>
        /// <param name="from">The older release.</param>
        /// <param name="to">The newer release.</param>
        /// <param name="renames">The elements renamed between the releases, older name to newer name.</param>
        /// <returns>
        /// The changes, or <c>null</c> when either release has no <c>cross-references.json</c> or the two
        /// have different schema versions, so the metamodel diff is still returned.
        /// </returns>
        /// <exception cref="KnowledgeLookupException">A document exists but is invalid.</exception>
        private IReadOnlyList<CrossReferenceChange>? CompareCrossReferences(
            string from, string to, IReadOnlyDictionary<string, string> renames)
        {
            if (!this.layout.CrossReferences(this.Release(from)).Exists || !this.layout.CrossReferences(this.Release(to)).Exists)
            {
                return null;
            }

            var (older, _) = this.CrossReferences(from);
            var (newer, _) = this.CrossReferences(to);

            return string.Equals(older.SchemaVersion, newer.SchemaVersion, StringComparison.Ordinal)
                ? this.crossReferenceDiffer.Compare(older, newer, renames)
                : null;
        }

        /// <summary>One element's entry in the metamodel index: its kind, package, supertypes and page.</summary>
        /// <param name="name">The element's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The entry, grounded as <see cref="Provenance.Model"/>; <c>null</c> when the release has no
        /// such element.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<LookupEntry> Element(string name, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var release = this.Release(tag);
            var index = this.MetamodelIndex(release);

            return new(
                index.Entries.GetValueOrDefault(name),
                this.Ground(Provenance.Model, release, this.layout.MetamodelIndex(release)));
        }

        /// <summary>
        /// An element's cross-references: the clauses that treat it, the productions that build it, the
        /// features its syntax fills in and the examples that declare it. Each edge names its own provenance.
        /// </summary>
        /// <param name="element">The element's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The entry, grounded as <see cref="Provenance.Derived"/>; <c>null</c> when the release has no
        /// cross-references for the element.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="element"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<CrossReferenceEntry> CrossReference(string element, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(element);

            var (document, grounding) = this.CrossReferences(tag);

            return new(document.Entries.GetValueOrDefault(element), grounding);
        }

        /// <summary>Which elements' syntax fills in a feature, through which operator and productions.</summary>
        /// <param name="feature">Only this feature, or <c>null</c> for any.</param>
        /// <param name="assignmentOperator">Only this operator (<c>=</c>, <c>+=</c>, <c>?=</c>), or <c>null</c> for any.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The assignments, sorted by element and in the order the cross-references list them within one,
        /// grounded as <see cref="Provenance.Derived"/>; empty when none matches both filters.
        /// </returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<FeatureAssignment>> FeatureAssignments(string? feature = null, string? assignmentOperator = null, string? tag = null)
        {
            var (document, grounding) = this.CrossReferences(tag);

            IReadOnlyList<FeatureAssignment> assignments = document.Entries
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .SelectMany(entry => entry.Value.Features
                    .Where(edge => feature is null || string.Equals(edge.Feature, feature, StringComparison.Ordinal))
                    .Where(edge => assignmentOperator is null
                                   || string.Equals(edge.Operator, assignmentOperator, StringComparison.Ordinal))
                    .Select(edge => new FeatureAssignment(entry.Key, edge)))
                .ToList();

            return new(assignments, grounding);
        }

        /// <summary>The elements a grammar production builds.</summary>
        /// <param name="production">The production, such as <c>PartUsage</c>.</param>
        /// <param name="grammar">Only this grammar (<c>kerml</c>, <c>sysml</c>), or <c>null</c> for any.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The elements and the links, sorted by element and then grammar, grounded as
        /// <see cref="Provenance.Derived"/>; empty when no element is built by that production.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="production"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<ProductionUse>> BuiltBy(string production, string? grammar = null, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(production);

            var (document, grounding) = this.CrossReferences(tag);

            IReadOnlyList<ProductionUse> uses = document.Entries
                .SelectMany(entry => entry.Value.Grammar
                    .Where(edge => string.Equals(edge.Production, production, StringComparison.Ordinal))
                    .Where(edge => grammar is null || string.Equals(edge.Grammar, grammar, StringComparison.Ordinal))
                    .Select(edge => new ProductionUse(entry.Key, edge)))
                .OrderBy(use => use.Element, StringComparer.Ordinal)
                .ThenBy(use => use.Edge.Grammar, StringComparer.Ordinal)
                .ToList();

            return new(uses, grounding);
        }

        /// <summary>
        /// What a keyword or operator, such as <c>ref</c>, <c>individual</c> or <c>:&gt;&gt;</c>, does in each
        /// grammar: the productions that write it and the metaclasses whose syntax includes it.
        /// </summary>
        /// <param name="word">The keyword or operator, matched exactly.</param>
        /// <param name="grammar">Only this grammar (<c>kerml</c>, <c>sysml</c>), or <c>null</c> for any.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// One use per grammar that lists the word, ordered by grammar, grounded as
        /// <see cref="Provenance.Derived"/> since the links come from the grammar text; <c>null</c> when no
        /// grammar lists the word. In SysML text, a word only the KerML expressions SysML calls write (such
        /// as <c>and</c>, <c>true</c> or <c>+</c>) names KerML's productions and metaclasses, and
        /// <see cref="KeywordUse.SyntaxFrom"/> says so.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="word"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<KeywordUse>> Keyword(string word, string? grammar = null, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(word);

            var (document, grounding) = this.Keywords(tag);
            var uses = new List<KeywordUse>();

            foreach (var (name, table) in document.Grammars.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if ((grammar is not null && !string.Equals(name, grammar, StringComparison.Ordinal))
                    || !table.Entries.TryGetValue(word, out var entry))
                {
                    continue;
                }

                uses.Add(new KeywordUse(name, entry.Kind, entry.SyntaxFrom, entry.Productions, entry.Metaclasses));
            }

            return new(uses.Count == 0 ? null : uses, grounding);
        }

        /// <summary>The elements a specification clause treats.</summary>
        /// <param name="document">The specification, as the cross-references key it (<c>kerml</c>, <c>sysml2</c>).</param>
        /// <param name="clause">The clause number, such as <c>8.3.6</c>.</param>
        /// <param name="includeSubclauses">Include the clauses below it, such as <c>8.3.6.3</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The elements and the links, sorted by element and then clause, grounded as
        /// <see cref="Provenance.Derived"/>; each link keeps its own provenance and method. Empty when no
        /// element is linked to the clause.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="document"/> or <paramref name="clause"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<ClauseUse>> ElementsInClause(string document, string clause, bool includeSubclauses = false, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(document);
            ArgumentException.ThrowIfNullOrWhiteSpace(clause);

            var (crossReferences, grounding) = this.CrossReferences(tag);

            IReadOnlyList<ClauseUse> uses = crossReferences.Entries
                .SelectMany(entry => entry.Value.Clauses
                    .Where(edge => string.Equals(edge.Document, document, StringComparison.Ordinal))
                    .Where(edge => string.Equals(edge.Clause, clause, StringComparison.Ordinal)
                                   || (includeSubclauses && edge.Clause.StartsWith(clause + ".", StringComparison.Ordinal)))
                    .Select(edge => new ClauseUse(entry.Key, edge)))
                .OrderBy(use => use.Element, StringComparer.Ordinal)
                .ThenBy(use => use.Edge.Clause, StringComparer.Ordinal)
                .ToList();

            return new(uses, grounding);
        }

        /// <summary>A standard-library declaration by qualified name: its kind, page and source file.</summary>
        /// <param name="qualifiedName">The qualified name, such as <c>ISQBase::mass</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The declaration, grounded as <see cref="Provenance.Derived"/> since the index is a scan of the
        /// library sources; <c>null</c> when the library declares no such name.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="qualifiedName"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<ModelLibraryDeclarationEntry> Declaration(string qualifiedName, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedName);

            var release = this.Release(tag);
            var index = this.ModelLibraryIndex(release);

            return new(
                index.Declarations.GetValueOrDefault(qualifiedName),
                this.Ground(Provenance.Derived, release, this.layout.ModelLibraryIndex(release)));
        }

        /// <summary>One clause of the KerML or SysML specification, with its verbatim text.</summary>
        /// <param name="document">The specification: <c>kerml</c> or <c>sysml2</c>.</param>
        /// <param name="clause">The clause number, such as <c>7.4.2</c>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The clause, grounded as <see cref="Provenance.Normative"/> and naming its page; <c>null</c> when
        /// the specification has no such clause. When the release has not generated the specification
        /// text, which needs the OMG PDFs, the result is <see cref="QueryResult{T}.Unavailable"/>, saying
        /// how to generate it.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="document"/> is not <c>kerml</c> or <c>sysml2</c>, or <paramref name="clause"/> is
        /// <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, or its specification index or page is invalid.
        /// </exception>
        public QueryResult<SpecificationClause> Clause(string document, string clause, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clause);
            CheckDocument(document);

            var release = this.Release(tag);

            return this.specifications.Read(
                this.layout.SpecificationCatalog(release, document),
                document,
                clause.Trim(),
                release,
                $"release {release}",
                SpecificationCommand(release),
                SpecificationUnavailable(release),
                file => this.Ground(Provenance.Normative, release, file));
        }

        /// <summary>
        /// Searches the release's metaclasses, standard-library declarations and specification clauses by
        /// name, qualified name, clause number or title.
        /// </summary>
        /// <param name="text">The text to look for.</param>
        /// <param name="kind">Only hits of this kind, one of <see cref="HyphaAddress.Kinds"/>, or <c>null</c> for any.</param>
        /// <param name="limit">The most hits returned, from 1 to <see cref="MaxSearchHits"/>.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The hits, closest match first, then by kind and name; empty when nothing matches. A clause whose
        /// title is the text matches as closely as one whose number is. Grounded as
        /// <see cref="Provenance.Derived"/>, since the ranking is this library's, with the release's
        /// knowledge folder as the source. The model library and the specification text are searched
        /// when they have been generated.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="text"/> is <c>null</c>, empty or white space, <paramref name="kind"/> is not a
        /// known kind, or <paramref name="limit"/> is out of range.
        /// </exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not generated its metamodel, or its documents
        /// are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<SearchHit>> Search(string text, string? kind = null, int limit = 20, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            if (kind is not null && !HyphaAddress.Kinds.Contains(kind, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    $"'{kind}' is not a kind to search; use one of {string.Join(", ", HyphaAddress.Kinds)}.", nameof(kind));
            }

            if (limit is < 1 or > MaxSearchHits)
            {
                throw new ArgumentException($"The limit must be between 1 and {MaxSearchHits}.", nameof(limit));
            }

            var release = this.Release(tag);
            var wanted = text.Trim();
            // A clause's title names it as much as its number does; a metaclass's summary is a sentence and
            // a declaration's title is its kind, so those only count when they contain the text.
            var candidates = new List<(HyphaAddress Address, string QualifiedName, string Title, bool TitleNames)>();

            if (kind is null or HyphaAddress.Metamodel)
            {
                var index = this.MetamodelIndex(release);
                candidates.AddRange(index.Entries.Select(entry => (
                    new HyphaAddress(release, HyphaAddress.Metamodel, entry.Key), entry.Value.QualifiedName, entry.Value.Summary, false)));
            }

            if (kind is null or HyphaAddress.ModelLibrary && this.layout.ModelLibraryIndex(release).Exists)
            {
                var library = this.ModelLibraryIndex(release);
                candidates.AddRange(library.Declarations.Select(entry => (
                    new HyphaAddress(release, HyphaAddress.ModelLibrary, entry.Key), entry.Key, entry.Value.Kind, false)));
            }

            foreach (var document in new[] { HyphaAddress.Kerml, HyphaAddress.Sysml }.Where(document => kind is null || kind == document))
            {
                var specification = this.specifications.ReadIndex(
                    this.layout.SpecificationCatalog(release, document), SpecificationCommand(release));

                if (specification is not null)
                {
                    candidates.AddRange(specification.Entries.Select(entry => (
                        new HyphaAddress(release, document, entry.Key), entry.Key, entry.Value.Title, true)));
                }
            }

            var kindOrder = HyphaAddress.Kinds.ToList();

            IReadOnlyList<SearchHit> hits = candidates
                .Select(candidate => (
                    candidate,
                    Match: Match(wanted, candidate.Address.Name, candidate.QualifiedName, candidate.Title, candidate.TitleNames)))
                .Where(scored => scored.Match is not null)
                .Select(scored => new SearchHit(scored.candidate.Address, scored.candidate.Title, scored.Match!.Value))
                .OrderBy(hit => hit.Match)
                .ThenBy(hit => kindOrder.IndexOf(hit.Address.Kind))
                .ThenBy(hit => hit.Address.Name, StringComparer.Ordinal)
                .Take(limit)
                .ToList();

            return new(hits, new Grounding(Provenance.Derived, release, this.Relative(this.layout.Knowledge(release).FullName)));
        }

        /// <summary>Resolves an address to the generated page it names, through the release's indexes.</summary>
        /// <param name="address">The address.</param>
        /// <returns>
        /// The page, grounded as <see cref="Provenance.Model"/> for a metaclass, <see cref="Provenance.Derived"/>
        /// for a library declaration and <see cref="Provenance.Normative"/> for a clause; <c>null</c> when the
        /// release has no such element, declaration or clause. <see cref="QueryResult{T}.Unavailable"/> for a
        /// clause of a specification the release has not generated.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The address's kind is not one of <see cref="HyphaAddress.Kinds"/>.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The address's release is not installed, has not generated the artifact, or its index points at a
        /// page that is missing or outside the release's folder.
        /// </exception>
        public QueryResult<ResolvedAddress> Resolve(HyphaAddress address)
        {
            ArgumentNullException.ThrowIfNull(address);

            var release = this.Release(address.Tag);

            switch (address.Kind)
            {
                case HyphaAddress.Metamodel:
                {
                    var index = this.layout.MetamodelIndex(release);
                    var entry = this.MetamodelIndex(release).Entries.GetValueOrDefault(address.Name);

                    return entry is null
                        ? new(null, this.Ground(Provenance.Model, release, index))
                        : this.Resolved(address, entry.Summary, this.layout.Metamodel(release), entry.File, Provenance.Model);
                }

                case HyphaAddress.ModelLibrary:
                {
                    var index = this.layout.ModelLibraryIndex(release);
                    var entry = this.ModelLibraryIndex(release).Declarations.GetValueOrDefault(address.Name);

                    return entry is null
                        ? new(null, this.Ground(Provenance.Derived, release, index))
                        : this.Resolved(address, entry.Kind, this.layout.ModelLibrary(release), entry.File, Provenance.Derived);
                }

                case HyphaAddress.Kerml or HyphaAddress.Sysml:
                {
                    var index = this.layout.SpecificationCatalog(release, address.Kind);
                    var specification = this.specifications.ReadIndex(index, SpecificationCommand(release));

                    if (specification is null)
                    {
                        return new(null, this.Ground(Provenance.Normative, release, index), SpecificationUnavailable(release));
                    }

                    var entry = specification.Entries.GetValueOrDefault(address.Name);

                    return entry is null
                        ? new(null, this.Ground(Provenance.Normative, release, index))
                        : this.Resolved(
                            address, entry.Title, index.Directory!, entry.File, Provenance.Normative, SpecificationCommand(release));
                }

                default:
                    throw new ArgumentException(
                        $"'{address.Kind}' is not a kind of address; use one of {string.Join(", ", HyphaAddress.Kinds)}.",
                        nameof(address));
            }
        }

        /// <summary>How closely a search candidate matches the text, or <c>null</c> when it does not.</summary>
        /// <param name="text">The text searched for.</param>
        /// <param name="name">The candidate's name: an element name, a qualified name or a clause number.</param>
        /// <param name="qualifiedName">The candidate's qualified name, or its name when it has none.</param>
        /// <param name="title">The candidate's summary, kind or clause title.</param>
        /// <param name="titleNames">
        /// Whether the title names the candidate, as a clause title does, so that a title equal to the text
        /// matches as closely as an equal name.
        /// </param>
        /// <returns>The closest tier the candidate matches in, or <c>null</c>.</returns>
        private static SearchMatch? Match(string text, string name, string qualifiedName, string title, bool titleNames)
        {
            bool Equal(StringComparison comparison) =>
                string.Equals(name, text, comparison)
                || string.Equals(qualifiedName, text, comparison)
                || (titleNames && string.Equals(title, text, comparison));

            if (Equal(StringComparison.Ordinal))
            {
                return SearchMatch.Exact;
            }

            if (Equal(StringComparison.OrdinalIgnoreCase))
            {
                return SearchMatch.IgnoringCase;
            }

            if (qualifiedName.EndsWith("::" + text, StringComparison.OrdinalIgnoreCase))
            {
                return SearchMatch.TrailingPart;
            }

            return name.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || qualifiedName.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || title.Contains(text, StringComparison.OrdinalIgnoreCase)
                ? SearchMatch.Contains
                : null;
        }

        /// <summary>Whether a document has a metaclass, enumeration or primitive type of a name.</summary>
        /// <param name="graph">The document.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> when one of the three has that name.</returns>
        private static bool Declares(MetamodelDocument graph, string name) =>
            graph.Classes.Any(node => string.Equals(node.Name, name, StringComparison.Ordinal))
            || graph.Enumerations.Any(node => string.Equals(node.Name, name, StringComparison.Ordinal))
            || graph.PrimitiveTypes.Any(node => string.Equals(node.Name, name, StringComparison.Ordinal));

        /// <summary>The features a metaclass declares itself, with their types.</summary>
        /// <param name="node">The metaclass.</param>
        /// <returns>Its owned features, in the order the metamodel lists them.</returns>
        private static IEnumerable<TypedFeature> Declared(ClassNode node) =>
            node.OwnedAttributes.Select(attribute => new TypedFeature(
                node.Name, attribute.Name, attribute.Type, attribute.Lower, attribute.Upper, attribute.IsComposite, attribute.IsDerived));

        /// <summary>
        /// The features a metaclass really has: its own, and the inherited ones no feature of its context
        /// redefines, each naming the metaclass that declares it.
        /// </summary>
        /// <param name="node">The metaclass.</param>
        /// <returns>The features, owned first.</returns>
        private static IEnumerable<TypedFeature> Effective(ClassNode node) =>
            Declared(node).Concat(node.InheritedAttributes
                .Where(attribute => attribute.RedefinedBy is null)
                .Select(attribute => new TypedFeature(
                    attribute.InheritedFrom,
                    attribute.Name,
                    attribute.Type,
                    attribute.Lower,
                    attribute.Upper,
                    attribute.IsComposite,
                    attribute.IsDerived)));

        /// <summary>Which feature types accept a value of a type.</summary>
        /// <param name="graph">The metamodel graph.</param>
        /// <param name="type">The type of the value.</param>
        /// <returns>
        /// A test of a feature type: for a metaclass, the metaclass and its ancestors from the generated
        /// closure; for any other element, the element itself. <c>null</c> when the release has no
        /// element of that name.
        /// </returns>
        private static Func<string?, bool>? Acceptor(MetamodelDocument graph, string type)
        {
            if (ClassNamed(graph, type) is { } node)
            {
                var accepted = new HashSet<string>(node.AllAncestors, StringComparer.Ordinal) { type };

                return candidate => candidate is not null && accepted.Contains(candidate);
            }

            var names = graph.Enumerations.Select(enumeration => enumeration.Name)
                .Concat(graph.PrimitiveTypes.Select(primitive => primitive.Name))
                .Concat((graph.DataTypes ?? []).Select(dataType => dataType.Name))
                .Concat((graph.Interfaces ?? []).Select(@interface => @interface.Name));

            return names.Contains(type, StringComparer.Ordinal)
                ? candidate => string.Equals(candidate, type, StringComparison.Ordinal)
                : null;
        }

        /// <summary>The features that accept a value of a type, for <see cref="Containers"/> and <see cref="FeaturesAccepting"/>.</summary>
        /// <param name="type">The type of the value.</param>
        /// <param name="className">Only the effective features of this metaclass, or <c>null</c> for every declared feature.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <param name="compositeOnly">Only composite features.</param>
        /// <returns>The features, sorted by declaring metaclass and then feature; <c>null</c> when a name is unknown.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        private QueryResult<IReadOnlyList<TypedFeature>> Accepting(string type, string? className, string? tag, bool compositeOnly)
        {
            var (graph, grounding) = this.Graph(tag, Provenance.Derived);
            var accepts = Acceptor(graph, type);
            var scope = className is null ? null : ClassNamed(graph, className);

            if (accepts is null || (className is not null && scope is null))
            {
                return new(null, grounding);
            }

            IReadOnlyList<TypedFeature> features = (scope is null ? graph.Classes.SelectMany(Declared) : Effective(scope))
                .Where(feature => accepts(feature.Type) && (!compositeOnly || feature.IsComposite))
                .OrderBy(feature => feature.Class, StringComparer.Ordinal)
                .ThenBy(feature => feature.Feature, StringComparer.Ordinal)
                .ToList();

            return new(features, grounding);
        }

        /// <summary>The metaclass that declares a feature a metaclass owns or inherits.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="feature">The feature.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The declaring metaclass, or <c>null</c> when the metaclass has no such feature.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        private string? DeclaringClass(string className, string feature, string? tag) =>
            this.Features(className, feature, tag).Value?.FirstOrDefault() is { } found ? found.InheritedFrom ?? className : null;

        /// <summary>The member an inherited reference names, read from the metaclass that declares it.</summary>
        /// <typeparam name="T">The member's type.</typeparam>
        /// <param name="graph">The metamodel graph.</param>
        /// <param name="className">The declaring metaclass.</param>
        /// <param name="name">The member's name.</param>
        /// <param name="members">Selects the members of that kind from a metaclass.</param>
        /// <param name="nameOf">The name of a member.</param>
        /// <returns>The member.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The graph names a member its declaring metaclass does not own, so it is inconsistent.
        /// </exception>
        private static T Member<T>(MetamodelDocument graph, string className, string name, Func<ClassNode, IReadOnlyList<T>> members, Func<T, string> nameOf)
        {
            var declaring = ClassNamed(graph, className);
            var member = declaring is null
                ? default
                : members(declaring).FirstOrDefault(candidate => string.Equals(nameOf(candidate), name, StringComparison.Ordinal));

            return member ?? throw new KnowledgeLookupException(
                $"The metamodel names '{className}::{name}' as inherited, but '{className}' does not own it. "
                + "The metamodel is inconsistent; regenerate it.");
        }

        /// <summary>Whether a reference names a feature, and the owner asked for if one is.</summary>
        /// <param name="reference">The reference.</param>
        /// <param name="feature">The feature name.</param>
        /// <param name="owner">The metaclass or association that must own it, or <c>null</c> for any.</param>
        /// <returns>Whether the reference names that feature.</returns>
        private static bool Names(FeatureReference reference, string feature, string? owner) =>
            string.Equals(reference.Feature, feature, StringComparison.Ordinal)
            && (owner is null
                || string.Equals(reference.Class, owner, StringComparison.Ordinal)
                || string.Equals(reference.Association, owner, StringComparison.Ordinal));

        /// <summary>Checks a specification name.</summary>
        /// <param name="document">The specification, which must be <c>kerml</c> or <c>sysml2</c>.</param>
        /// <exception cref="ArgumentException"><paramref name="document"/> is neither.</exception>
        private static void CheckDocument(string document)
        {
            if (!ReleaseStatus.SpecificationDocuments.Contains(document, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    $"'{document}' is not a specification; use one of {string.Join(", ", ReleaseStatus.SpecificationDocuments)}.",
                    nameof(document));
            }
        }

        /// <summary>The command that generates a release's specification text.</summary>
        /// <param name="release">The release.</param>
        /// <returns>The command line.</returns>
        private static string SpecificationCommand(string release) => $"hypha generate spec --tag {release}";

        /// <summary>Why a release has no specification text, and how to generate it.</summary>
        /// <param name="release">The release.</param>
        /// <returns>The reason, naming the commands to run.</returns>
        private static string SpecificationUnavailable(string release) =>
            $"The specification text of release {release} has not been generated. It needs the OMG specification "
            + $"PDFs: run 'hypha fetch --tag {release}', then '{SpecificationCommand(release)}'.";

        /// <summary>The metaclass of a given name, or <c>null</c> when the graph has none.</summary>
        /// <param name="graph">The metamodel graph.</param>
        /// <param name="name">The metaclass name.</param>
        /// <returns>The metaclass, or <c>null</c>.</returns>
        private static ClassNode? ClassNamed(MetamodelDocument graph, string name) =>
            graph.Classes.FirstOrDefault(node => string.Equals(node.Name, name, StringComparison.Ordinal));

        /// <summary>A shortest chain of direct generalizations from one metaclass to another.</summary>
        /// <param name="classes">The metaclasses, by name.</param>
        /// <param name="specific">Where the chain starts.</param>
        /// <param name="general">Where the chain ends.</param>
        /// <returns>The chain, both ends included; empty when there is none.</returns>
        private static IReadOnlyList<string> ShortestPath(IReadOnlyDictionary<string, ClassNode> classes, string specific, string general)
        {
            // Breadth first over the direct supertypes, each visited in name order, so the chain found
            // is a shortest one and the same one on every run.
            var previous = new Dictionary<string, string?>(StringComparer.Ordinal) { [specific] = null };
            var queue = new Queue<string>([specific]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (string.Equals(current, general, StringComparison.Ordinal))
                {
                    var path = new List<string>();

                    for (var step = (string?)current; step is not null; step = previous[step])
                    {
                        path.Add(step);
                    }

                    path.Reverse();

                    return path;
                }

                var supertypes = classes.TryGetValue(current, out var node) ? node.Extends : [];

                foreach (var supertype in supertypes.Order(StringComparer.Ordinal))
                {
                    if (previous.TryAdd(supertype, current))
                    {
                        queue.Enqueue(supertype);
                    }
                }
            }

            return [];
        }

        /// <summary>Reads a release's metamodel <c>index.json</c>.</summary>
        /// <param name="release">The release.</param>
        /// <returns>The index.</returns>
        /// <exception cref="KnowledgeLookupException">The release has not generated its metamodel, or the index is invalid.</exception>
        private MetamodelLookup MetamodelIndex(string release) => ReleaseIndexes.Metamodel(this.documents, this.layout, release);

        /// <summary>Reads a release's model-library <c>index.json</c>.</summary>
        /// <param name="release">The release.</param>
        /// <returns>The index.</returns>
        /// <exception cref="KnowledgeLookupException">The release has not generated its model library, or the index is invalid.</exception>
        private ModelLibraryDocument ModelLibraryIndex(string release) => ReleaseIndexes.ModelLibrary(this.documents, this.layout, release);

        /// <summary>The page an index entry names, checked to stay inside its artifact's folder.</summary>
        /// <param name="address">The address being resolved.</param>
        /// <param name="title">What the page is about.</param>
        /// <param name="root">The artifact's folder.</param>
        /// <param name="file">The page, as the index records it, relative to <paramref name="root"/>.</param>
        /// <param name="tier">The grounding tier.</param>
        /// <param name="regenerate">
        /// The command that regenerates a missing page, or <c>null</c> for regenerating the whole release.
        /// </param>
        /// <returns>The resolved address.</returns>
        /// <exception cref="KnowledgeLookupException">The page is missing or outside the folder.</exception>
        private QueryResult<ResolvedAddress> Resolved(HyphaAddress address, string title, DirectoryInfo root, string file, string tier, string? regenerate = null)
        {
            var page = KnowledgeLookup.ContainedFile(root, file, address.Tag, regenerate: regenerate);

            return new(
                new ResolvedAddress(address, title, this.Relative(page.FullName)),
                this.Ground(tier, address.Tag, page));
        }

        /// <summary>A path relative to the output root, with <c>/</c> separators.</summary>
        /// <param name="path">The full path.</param>
        /// <returns>The relative path, starting with <c>knowledge/</c>.</returns>
        private string Relative(string path) =>
            Path.GetRelativePath(this.layout.OutputRoot.FullName, path).Replace('\\', '/');

        /// <summary>Reads a release's <c>metamodel.json</c>.</summary>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <param name="tier">The grounding tier of the query that reads it.</param>
        /// <returns>The graph and the grounding of answers taken from it.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or the graph is invalid.
        /// </exception>
        private (MetamodelDocument Graph, Grounding Grounding) Graph(string? tag, string tier)
        {
            var release = this.Release(tag);
            var file = this.layout.MetamodelGraph(release);
            var graph = this.documents.Get(
                file,
                ReadContracts.MetamodelGraph,
                document => document.Classes is not null,
                ReleaseAxis.Language.NotGenerated("metamodel", release),
                ReleaseAxis.Language.GenerateCommand(release));

            return (graph, this.Ground(tier, release, file));
        }

        /// <summary>Reads a release's <c>cross-references.json</c>.</summary>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The document and the grounding of answers taken from it.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or the document is invalid.
        /// </exception>
        private (CrossReferenceDocument Document, Grounding Grounding) CrossReferences(string? tag)
        {
            var release = this.Release(tag);
            var file = this.layout.CrossReferences(release);
            var document = this.documents.Get(
                file,
                ReadContracts.CrossReferences,
                document => document.Entries is not null,
                ReleaseAxis.Language.NotGenerated("cross-references", release),
                ReleaseAxis.Language.GenerateCommand(release));

            return (document, this.Ground(Provenance.Derived, release, file));
        }

        /// <summary>Reads a release's <c>textual-notation/keywords.json</c>.</summary>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>The document and the grounding of answers taken from it.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or the document is invalid.
        /// </exception>
        private (KeywordDocument Document, Grounding Grounding) Keywords(string? tag)
        {
            var release = this.Release(tag);
            var file = this.layout.Keywords(release);
            var document = this.documents.Get(
                file,
                ReadContracts.Keywords,
                document => document.Grammars is not null,
                ReleaseAxis.Language.NotGenerated("keywords", release),
                ReleaseAxis.Language.GenerateCommand(release));

            return (document, this.Ground(Provenance.Derived, release, file));
        }

        /// <summary>The grounding of an answer read from a file.</summary>
        /// <param name="tier">The grounding tier.</param>
        /// <param name="release">The release that answered.</param>
        /// <param name="file">The file read.</param>
        /// <returns>
        /// The grounding, with the file relative to the output root (the folder holding <c>knowledge/</c>),
        /// so it starts with <c>knowledge/</c>.
        /// </returns>
        private Grounding Ground(string tier, string release, FileInfo file) => new(
            tier,
            release,
            Path.GetRelativePath(this.layout.OutputRoot.FullName, file.FullName).Replace('\\', '/'));

        /// <summary>The requested release, or the default one, checked to be installed.</summary>
        /// <param name="requested">The requested tag, or <c>null</c> for the default.</param>
        /// <returns>The tag to answer from.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// No release is installed, none is the default, or the requested one is not installed.
        /// </exception>
        private string Release(string? requested) =>
            ReleaseAxis.Language.Resolve(this.layout.InstalledTags, this.layout.DefaultTag, requested);
    }
}
