// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceBuilder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Assembles <c>cross-references.json</c> from the parsed knowledge trees.
    /// </summary>
    /// <remarks>
    /// Every edge carries the provenance tier it belongs to and the method that produced it, so a
    /// reader can tell a fact read out of a source from one this repository inferred.
    /// </remarks>
    public sealed class CrossReferenceBuilder : ICrossReferenceBuilder
    {
        /// <summary>A clause found by matching an element name against a clause title. A guess.</summary>
        public const string ExactTitleMatch = "exact-title-match";

        /// <summary>A clause the grammar's own <c>// Clause</c> comment states. Read, not guessed.</summary>
        public const string GrammarClause = "grammar-clause";

        /// <inheritdoc/>
        public CrossReferenceDocument Build(CrossReferenceInputs inputs)
        {
            ArgumentNullException.ThrowIfNull(inputs);

            var names = inputs.Elements
                .Select(element => element.Name)
                .Order(StringComparer.Ordinal)
                .ToList();

            var byName = inputs.Elements.ToDictionary(
                element => element.Name, StringComparer.Ordinal);

            var clauses = MergeClauseEdges(TitleEdges(names, inputs), names, inputs);
            var grammar = GrammarEdges(names, inputs.Productions);
            var features = FeatureEdges(names, inputs.GrammarFeatures);
            var examples = ExampleEdges(names, inputs.Examples);

            var entries = new SortedDictionary<string, CrossReferenceEntry>(StringComparer.Ordinal);

            foreach (var name in names)
            {
                entries[name] = new CrossReferenceEntry(
                    byName[name].Kind,
                    "metamodel/" + byName[name].File,
                    clauses[name],
                    grammar[name],
                    features[name],
                    examples[name]);
            }

            return new CrossReferenceDocument(
                CrossReferenceDocument.CurrentSchemaVersion,
                inputs.ModelVersionUri,
                new SortedDictionary<string, SpecificationDocument>(
                    inputs.Documents.ToDictionary(entry => entry.Key, entry => entry.Value),
                    StringComparer.Ordinal),
                Provenance.Tiers,
                Count(names, clauses, grammar, features, examples),
                entries);
        }

        /// <summary>
        /// Matches element names against clause titles, exactly - or takes the matches a previous run
        /// recorded, when the specification catalog is not present.
        /// </summary>
        /// <remarks>
        /// An element may legitimately be treated in several clauses; every match is kept rather than
        /// guessing which one is the defining occurrence.
        /// </remarks>
        private static Dictionary<string, List<ClauseEdge>> TitleEdges(
            IReadOnlyList<string> names, CrossReferenceInputs inputs)
        {
            var edges = names.ToDictionary(
                name => name, _ => new List<ClauseEdge>(), StringComparer.Ordinal);

            if (inputs.ClauseTitles.Count == 0)
            {
                foreach (var (element, carried) in inputs.CarriedClauseEdges)
                {
                    if (edges.TryGetValue(element, out var bucket))
                    {
                        bucket.AddRange(carried);
                    }
                }

                return edges;
            }

            var wanted = names.ToHashSet(StringComparer.Ordinal);

            foreach (var document in inputs.ClauseTitles.Keys.Order(StringComparer.Ordinal))
            {
                foreach (var (clause, title) in inputs.ClauseTitles[document])
                {
                    if (wanted.Contains(title))
                    {
                        edges[title].Add(
                            new ClauseEdge(document, clause, Provenance.Derived, ExactTitleMatch));
                    }
                }
            }

            return edges;
        }

        /// <summary>
        /// Folds the grammar's own clause attribution into the title-matched edges.
        /// </summary>
        /// <remarks>
        /// A title match is a guess (<c>DERIVED</c>); the grammar's <c>// Clause</c> comment is a
        /// statement by its authors (<c>MODEL</c>). Where both point at the same clause the stronger
        /// one wins, so an edge is never reported as inferred when it was in fact read.
        /// </remarks>
        private static Dictionary<string, IReadOnlyList<ClauseEdge>> MergeClauseEdges(
            Dictionary<string, List<ClauseEdge>> titleEdges,
            IReadOnlyList<string> names,
            CrossReferenceInputs inputs)
        {
            foreach (var grammar in inputs.GrammarClauses.Keys.Order(StringComparer.Ordinal))
            {
                if (!inputs.GrammarDocuments.TryGetValue(grammar, out var document))
                {
                    continue;
                }

                foreach (var (element, clauses) in inputs.GrammarClauses[grammar])
                {
                    if (!titleEdges.TryGetValue(element, out var bucket))
                    {
                        continue;
                    }

                    foreach (var clause in clauses)
                    {
                        Upgrade(bucket, document, clause);
                    }
                }
            }

            return names.ToDictionary(
                name => name,
                name => (IReadOnlyList<ClauseEdge>)[.. titleEdges[name]
                    .OrderBy(edge => edge.Document, StringComparer.Ordinal)
                    .ThenBy(edge => edge.Clause, ClauseNumberComparer.Instance)],
                StringComparer.Ordinal);
        }

        /// <summary>Replaces a guessed edge with the stated one, or records it as new.</summary>
        private static void Upgrade(List<ClauseEdge> bucket, string document, string clause)
        {
            var stated = new ClauseEdge(document, clause, Provenance.Model, GrammarClause);

            var existing = bucket.FindIndex(edge =>
                string.Equals(edge.Document, document, StringComparison.Ordinal)
                && string.Equals(edge.Clause, clause, StringComparison.Ordinal));

            if (existing >= 0)
            {
                bucket[existing] = stated;
            }
            else
            {
                bucket.Add(stated);
            }
        }

        /// <summary>
        /// Links elements to the productions that build them. The grammar declares what a production
        /// produces, so these are read rather than inferred from a name.
        /// </summary>
        private static Dictionary<string, IReadOnlyList<GrammarEdge>> GrammarEdges(
            IReadOnlyList<string> names,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> links)
        {
            var edges = names.ToDictionary(
                name => name, _ => new List<GrammarEdge>(), StringComparer.Ordinal);

            foreach (var grammar in links.Keys.Order(StringComparer.Ordinal))
            {
                foreach (var (metaclass, productions) in links[grammar])
                {
                    if (!edges.TryGetValue(metaclass, out var bucket))
                    {
                        continue;
                    }

                    bucket.AddRange(productions.Select(production => new GrammarEdge(
                        grammar,
                        production,
                        Provenance.Model,
                        string.Equals(production, metaclass, StringComparison.Ordinal)
                            ? "production-name"
                            : "declared-production")));
                }
            }

            return Freeze(names, edges, bucket => bucket
                .OrderBy(edge => edge.Grammar, StringComparer.Ordinal)
                .ThenBy(edge => edge.Production, StringComparer.Ordinal));
        }

        /// <summary>Folds each grammar's feature assignments into one list per element.</summary>
        private static Dictionary<string, IReadOnlyList<FeatureEdge>> FeatureEdges(
            IReadOnlyList<string> names,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>>> grammarFeatures)
        {
            var edges = names.ToDictionary(
                name => name, _ => new List<FeatureEdge>(), StringComparer.Ordinal);

            foreach (var grammar in grammarFeatures.Keys.Order(StringComparer.Ordinal))
            {
                foreach (var (element, assignments) in grammarFeatures[grammar])
                {
                    if (!edges.TryGetValue(element, out var bucket))
                    {
                        continue;
                    }

                    bucket.AddRange(assignments.Select(assignment => new FeatureEdge(
                        grammar,
                        assignment.Feature,
                        assignment.Operator,
                        assignment.Productions,
                        Provenance.Model,
                        "grammar-assignment")));
                }
            }

            return Freeze(names, edges, bucket => bucket
                .OrderBy(edge => edge.Grammar, StringComparer.Ordinal)
                .ThenBy(edge => edge.Feature, StringComparer.Ordinal)
                .ThenBy(edge => edge.Operator, StringComparer.Ordinal));
        }

        /// <summary>Inverts the examples' front matter into element-to-example edges.</summary>
        private static Dictionary<string, IReadOnlyList<ExampleEdge>> ExampleEdges(
            IReadOnlyList<string> names, IReadOnlyDictionary<string, IReadOnlyList<string>> examples)
        {
            var edges = names.ToDictionary(
                name => name, _ => new List<ExampleEdge>(), StringComparer.Ordinal);

            foreach (var path in examples.Keys.Order(StringComparer.Ordinal))
            {
                foreach (var name in examples[path])
                {
                    if (edges.TryGetValue(name, out var bucket))
                    {
                        bucket.Add(new ExampleEdge(path, Provenance.Derived, "curated-front-matter"));
                    }
                }
            }

            return Freeze(names, edges, bucket => bucket.OrderBy(edge => edge.File, StringComparer.Ordinal));
        }

        /// <summary>
        /// Orders each element's edges and hands back an immutable view.
        /// </summary>
        /// <remarks>
        /// The caller supplies the ordering rather than a key selector on purpose: ordering a tuple
        /// key would fall back to the default string comparer, which is culture-sensitive, and the
        /// committed output has to be byte-stable on every machine.
        /// </remarks>
        private static Dictionary<string, IReadOnlyList<TEdge>> Freeze<TEdge>(
            IReadOnlyList<string> names,
            Dictionary<string, List<TEdge>> edges,
            Func<List<TEdge>, IEnumerable<TEdge>> order) =>
            names.ToDictionary(
                name => name,
                name => (IReadOnlyList<TEdge>)[.. order(edges[name])],
                StringComparer.Ordinal);

        /// <summary>
        /// Coverage, carried in the file so a drop shows up in a diff.
        /// </summary>
        /// <remarks>
        /// Takes the concrete collections its only caller already holds; the interfaces would add a
        /// lookup indirection per element for nothing.
        /// </remarks>
        private static CrossReferenceCounts Count(
            List<string> names,
            Dictionary<string, IReadOnlyList<ClauseEdge>> clauses,
            Dictionary<string, IReadOnlyList<GrammarEdge>> grammar,
            Dictionary<string, IReadOnlyList<FeatureEdge>> features,
            Dictionary<string, IReadOnlyList<ExampleEdge>> examples) =>
            new(
                names.Count,
                names.Count(name => clauses[name].Count > 0),
                names.Count(name => clauses[name].Count == 0),
                names.Count(name => grammar[name].Count > 0),
                names.Count(name => features[name].Count > 0),
                names.Count(name => examples[name].Count > 0),
                names.Sum(name => clauses[name].Count),
                names.Sum(name => clauses[name].Count(edge =>
                    string.Equals(edge.Method, GrammarClause, StringComparison.Ordinal))),
                names.Sum(name => grammar[name].Count),
                names.Sum(name => features[name].Count),
                names.Sum(name => examples[name].Count));
    }
}
