// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceDiffer.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Hypha.Knowledge.CrossReferences;

    /// <summary>
    /// Compares two <c>cross-references.json</c> documents edge by edge. An edge is matched on what
    /// identifies it (a clause on its document and number, a production on its grammar and name, a
    /// feature on its grammar, name and operator, an example on its page), and the rest of it is
    /// compared field by field.
    /// </summary>
    public sealed class CrossReferenceDiffer : ICrossReferenceDiffer
    {
        /// <summary>Every change to the edges of the elements found in both documents.</summary>
        /// <param name="from">The older document.</param>
        /// <param name="to">The newer document.</param>
        /// <param name="renames">
        /// The elements renamed between the releases, older name to newer name, as the metamodel diff
        /// found them.
        /// </param>
        /// <returns>The changes, sorted by element, element kind, facet, edge, kind and then field.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="from"/>, <paramref name="to"/> or <paramref name="renames"/> is <c>null</c>.
        /// </exception>
        public IReadOnlyList<CrossReferenceChange> Compare(
            CrossReferenceDocument from, CrossReferenceDocument to, IReadOnlyDictionary<string, string> renames)
        {
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);
            ArgumentNullException.ThrowIfNull(renames);

            var changes = new List<CrossReferenceChange>();
            var targets = renames.Values.ToHashSet(StringComparer.Ordinal);

            foreach (var (name, older) in from.Entries)
            {
                // A name that another element was renamed to, while this one was not, is the other element.
                var renamed = renames.TryGetValue(name, out var newer) ? newer : null;

                if (renamed is null && targets.Contains(name))
                {
                    continue;
                }

                if (!to.Entries.TryGetValue(renamed ?? name, out var entry))
                {
                    continue;
                }

                var element = renamed ?? name;

                Edges(
                    changes,
                    element,
                    entry.Kind,
                    CrossReferenceChange.Clause,
                    older.Clauses,
                    entry.Clauses,
                    edge => $"{edge.Document} {edge.Clause}",
                    edge => [("provenance", edge.Provenance), ("method", edge.Method)]);
                Edges(
                    changes,
                    element,
                    entry.Kind,
                    CrossReferenceChange.Grammar,
                    older.Grammar,
                    entry.Grammar,
                    edge => $"{edge.Grammar} {edge.Production}",
                    edge => [("provenance", edge.Provenance), ("method", edge.Method)]);
                Edges(
                    changes,
                    element,
                    entry.Kind,
                    CrossReferenceChange.Feature,
                    older.Features,
                    entry.Features,
                    edge => $"{edge.Grammar} {edge.Feature} {edge.Operator}",
                    edge =>
                    [
                        ("productions", string.Join(", ", edge.Productions.Order(StringComparer.Ordinal))),
                        ("provenance", edge.Provenance),
                        ("method", edge.Method),
                    ]);
                Edges(
                    changes,
                    element,
                    entry.Kind,
                    CrossReferenceChange.Example,
                    older.Examples,
                    entry.Examples,
                    edge => edge.File,
                    edge => [("provenance", edge.Provenance), ("method", edge.Method)]);
            }

            return changes
                .OrderBy(change => change.Element, StringComparer.Ordinal)
                .ThenBy(change => change.ElementKind, StringComparer.Ordinal)
                .ThenBy(change => change.Facet, StringComparer.Ordinal)
                .ThenBy(change => change.Edge, StringComparer.Ordinal)
                .ThenBy(change => change.Kind, StringComparer.Ordinal)
                .ThenBy(change => change.Field ?? string.Empty, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Compares the edges of one facet of an element.</summary>
        /// <typeparam name="T">The kind of edge.</typeparam>
        /// <param name="changes">The changes found so far, added to.</param>
        /// <param name="element">The element's name in the newer release.</param>
        /// <param name="kind">The element's kind.</param>
        /// <param name="facet">The facet, for the changes.</param>
        /// <param name="from">The older edges.</param>
        /// <param name="to">The newer edges.</param>
        /// <param name="key">What identifies an edge, as it is written in a change.</param>
        /// <param name="fields">The other fields of an edge, each with its value as text.</param>
        private static void Edges<T>(
            List<CrossReferenceChange> changes,
            string element,
            string kind,
            string facet,
            IReadOnlyList<T> from,
            IReadOnlyList<T> to,
            Func<T, string> key,
            Func<T, (string Field, string Value)[]> fields)
        {
            // An edge is not expected twice under one key; a queue per key pairs them in order if it is.
            var newer = to
                .GroupBy(key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => new Queue<T>(group), StringComparer.Ordinal);

            foreach (var older in from)
            {
                var edge = key(older);

                if (!newer.TryGetValue(edge, out var candidates) || candidates.Count == 0)
                {
                    changes.Add(new CrossReferenceChange(element, kind, facet, edge, MetamodelChange.Removed, null, null, null));
                    continue;
                }

                var match = candidates.Dequeue();

                foreach (var ((field, before), (_, after)) in fields(older).Zip(fields(match)))
                {
                    if (!string.Equals(before, after, StringComparison.Ordinal))
                    {
                        changes.Add(new CrossReferenceChange(element, kind, facet, edge, MetamodelChange.Changed, field, before, after));
                    }
                }
            }

            foreach (var (edge, remaining) in newer)
            {
                changes.AddRange(remaining.Select(_ =>
                    new CrossReferenceChange(element, kind, facet, edge, MetamodelChange.Added, null, null, null)));
            }
        }
    }
}
