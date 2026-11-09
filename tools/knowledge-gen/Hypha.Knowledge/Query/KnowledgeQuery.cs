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
        /// <summary>Where each release's generated knowledge lives, and which releases are installed.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Which API versions are installed.</summary>
        private readonly IApiLayout apiLayout;

        /// <summary>The documents read so far.</summary>
        private readonly DocumentCache documents = new();

        /// <summary>Initializes a new instance of the <see cref="KnowledgeQuery"/> class.</summary>
        /// <param name="layout">Where each release's generated knowledge lives, and which releases are installed.</param>
        /// <param name="apiLayout">Which API versions are installed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> or <paramref name="apiLayout"/> is <c>null</c>.</exception>
        public KnowledgeQuery(IKnowledgeLayout layout, IApiLayout apiLayout)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(apiLayout);

            this.layout = layout;
            this.apiLayout = apiLayout;
        }

        /// <summary>The installed releases and API versions, and the default of each.</summary>
        /// <returns>The inventory, read from the manifests on each call; empty lists when nothing is installed.</returns>
        public ReleaseInventory Releases() => new(
            this.layout.DefaultTag,
            this.layout.InstalledTags,
            this.apiLayout.DefaultTag,
            this.apiLayout.InstalledTags);

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
        public QueryResult<IReadOnlyList<string>> ClassesWithFeatureTyped(
            string type, bool compositeOnly = false, string? tag = null)
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
                attribute.Name, attribute.Type, attribute.Lower, attribute.Upper, attribute.IsDerived,
                attribute.IsComposite, attribute.Redefines, attribute.Subsets, InheritedFrom: null));
            var inherited = node.InheritedAttributes.Select(attribute => new FeatureInfo(
                attribute.Name, attribute.Type, attribute.Lower, attribute.Upper, attribute.IsDerived,
                attribute.IsComposite, attribute.Redefines, attribute.Subsets, attribute.InheritedFrom));

            IReadOnlyList<FeatureInfo> features = owned.Concat(inherited)
                .Where(feature => name is null || string.Equals(feature.Name, name, StringComparison.Ordinal))
                .ToList();

            return new(features, grounding);
        }

        /// <summary>The owned features that redefine a feature of the given name.</summary>
        /// <param name="feature">The redefined feature's name.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The redefinitions, sorted by metaclass and then feature, grounded as <see cref="Provenance.Model"/>;
        /// empty when nothing redefines a feature of that name.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<Redefinition>> Redefinitions(string feature, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            IReadOnlyList<Redefinition> redefinitions = graph.Classes
                .SelectMany(node => node.OwnedAttributes
                    .Where(attribute => attribute.Redefines.Contains(feature, StringComparer.Ordinal))
                    .Select(attribute => new Redefinition(node.Name, attribute.Name, feature)))
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
        /// The answer with a shortest chain of direct generalizations, grounded as
        /// <see cref="Provenance.Derived"/>; <c>null</c> when either name is not a metaclass of the
        /// release. A metaclass specializes itself, with a chain of one.
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

            if (!classes.ContainsKey(specific) || !classes.ContainsKey(general))
            {
                return new(null, grounding);
            }

            return new(new Specialization(specific, general, ShortestPath(classes, specific, general)), grounding);
        }

        /// <summary>The OCL constraints that apply to a metaclass.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the constraints its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The constraints, the metaclass's own first and then its ancestors' in name order, each naming
        /// the metaclass that declares it. Grounded as <see cref="Provenance.Model"/> for the metaclass's
        /// own only, and as <see cref="Provenance.Derived"/> with the inherited ones, since those come
        /// from the ancestor closure. <c>null</c> when <paramref name="className"/> is not a metaclass of
        /// the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<DeclaredMember<ConstraintNode>>> Constraints(
            string className, bool includeInherited = true, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            return this.Declared(className, includeInherited, tag, node => node.Constraints);
        }

        /// <summary>The operations of a metaclass, with their signatures and OCL bodies.</summary>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the operations its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <returns>
        /// The operations, the metaclass's own first and then its ancestors' in name order, each naming
        /// the metaclass that declares it. Grounded as <see cref="Provenance.Model"/> for the metaclass's
        /// own only, and as <see cref="Provenance.Derived"/> with the inherited ones. <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="className"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<DeclaredMember<OperationNode>>> Operations(
            string className, bool includeInherited = true, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(className);

            return this.Declared(className, includeInherited, tag, node => node.OwnedOperations);
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
        /// <returns>
        /// The subsettings, sorted by metaclass and then feature, grounded as <see cref="Provenance.Model"/>;
        /// empty when nothing subsets a feature of that name.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="feature"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        public QueryResult<IReadOnlyList<Subsetting>> Subsettings(string feature, string? tag = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature);

            var (graph, grounding) = this.Graph(tag, Provenance.Model);

            IReadOnlyList<Subsetting> subsettings = graph.Classes
                .SelectMany(node => node.OwnedAttributes
                    .Where(attribute => attribute.Subsets.Contains(feature, StringComparer.Ordinal))
                    .Select(attribute => new Subsetting(node.Name, attribute.Name, feature)))
                .OrderBy(subsetting => subsetting.Class, StringComparer.Ordinal)
                .ThenBy(subsetting => subsetting.Feature, StringComparer.Ordinal)
                .ToList();

            return new(subsettings, grounding);
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
            var file = this.layout.MetamodelIndex(release);
            var index = this.documents.Get(
                file,
                ReadContracts.MetamodelIndex,
                document => document.Entries is not null,
                ReleaseAxis.Language.NotGenerated("metamodel", release),
                ReleaseAxis.Language.GenerateCommand(release));

            return new(index.Entries.GetValueOrDefault(name), this.Ground(Provenance.Model, release, file));
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
        public QueryResult<IReadOnlyList<FeatureAssignment>> FeatureAssignments(
            string? feature = null, string? assignmentOperator = null, string? tag = null)
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
        public QueryResult<IReadOnlyList<ClauseUse>> ElementsInClause(
            string document, string clause, bool includeSubclauses = false, string? tag = null)
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
            var file = this.layout.ModelLibraryIndex(release);
            var index = this.documents.Get(
                file,
                ReadContracts.ModelLibrary,
                document => document.Declarations is not null,
                ReleaseAxis.Language.NotGenerated("model library", release),
                ReleaseAxis.Language.GenerateCommand(release));

            return new(
                index.Declarations.GetValueOrDefault(qualifiedName), this.Ground(Provenance.Derived, release, file));
        }

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
        private static IReadOnlyList<string> ShortestPath(
            IReadOnlyDictionary<string, ClassNode> classes, string specific, string general)
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

        /// <summary>The members of one kind a metaclass declares, and optionally those its ancestors declare.</summary>
        /// <typeparam name="T">The member's type.</typeparam>
        /// <param name="className">The metaclass.</param>
        /// <param name="includeInherited">Include the members its ancestors declare.</param>
        /// <param name="tag">The release, or <c>null</c> for the default.</param>
        /// <param name="members">Selects the members from a metaclass.</param>
        /// <returns>
        /// The members, the metaclass's own first, then its ancestors' in name order; <c>null</c> when
        /// <paramref name="className"/> is not a metaclass of the release.
        /// </returns>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its documents are invalid.
        /// </exception>
        private QueryResult<IReadOnlyList<DeclaredMember<T>>> Declared<T>(
            string className, bool includeInherited, string? tag, Func<ClassNode, IReadOnlyList<T>> members)
        {
            var (graph, grounding) = this.Graph(tag, includeInherited ? Provenance.Derived : Provenance.Model);
            var node = ClassNamed(graph, className);

            if (node is null)
            {
                return new(null, grounding);
            }

            var classes = graph.Classes.ToDictionary(candidate => candidate.Name, StringComparer.Ordinal);
            IEnumerable<string> declaring = includeInherited
                ? new[] { node.Name }.Concat(node.AllAncestors.Order(StringComparer.Ordinal))
                : [node.Name];

            IReadOnlyList<DeclaredMember<T>> declared = declaring
                .Where(classes.ContainsKey)
                .SelectMany(name => members(classes[name]).Select(member => new DeclaredMember<T>(name, member)))
                .ToList();

            return new(declared, grounding);
        }

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
