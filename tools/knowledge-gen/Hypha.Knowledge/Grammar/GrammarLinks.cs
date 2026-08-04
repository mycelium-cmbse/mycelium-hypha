// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarLinks.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Grammar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Joins the grammar to the metamodel: which productions build a metaclass, which clauses define
    /// it, and which of its features the syntax populates.
    /// </summary>
    /// <remarks>
    /// Every join here is something the grammar <i>states</i> - a declared type, a <c>// Clause</c>
    /// comment, an assignment operator - rather than something matched on a name, so a consumer can
    /// cite it as read.
    /// </remarks>
    public sealed partial class GrammarLinks : IGrammarLinks
    {
        /// <summary>How far helper delegation is followed. Deep enough for the real grammars.</summary>
        private const int MaxDepth = 4;

        // Capitalised identifiers are production references; lowercase ones are features and keywords.
        [GeneratedRegex(@"\b([A-Z][A-Za-z0-9_]*)\b")]
        private static partial Regex Reference();

        [GeneratedRegex(@"'[^']*'")]
        private static partial Regex Literal();

        /// <summary>
        /// Maps each metaclass to the productions that build it, using the declared type then the name.
        /// </summary>
        /// <remarks>
        /// Only names present in <paramref name="metaclasses"/> are kept, so the grammar can never
        /// introduce an element the metamodel does not have.
        /// </remarks>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> MetaclassLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses)
        {
            ArgumentNullException.ThrowIfNull(productions);
            ArgumentNullException.ThrowIfNull(metaclasses);

            var links = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);

            foreach (var production in productions)
            {
                var element = BuiltElement(production, metaclasses);
                if (element is not null)
                {
                    Bucket(links, element).Add(production.Name);
                }
            }

            return Freeze(links, StringComparer.Ordinal);
        }

        /// <summary>
        /// Maps each metaclass to the clauses its productions are defined in, per the grammar's own
        /// <c>// Clause</c> attribution.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> ClauseLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses)
        {
            ArgumentNullException.ThrowIfNull(productions);
            ArgumentNullException.ThrowIfNull(metaclasses);

            var links = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);

            foreach (var production in productions)
            {
                var element = BuiltElement(production, metaclasses);
                if (!string.IsNullOrEmpty(production.Clause) && element is not null)
                {
                    Bucket(links, element).Add(production.Clause);
                }
            }

            return Freeze(links, ClauseNumberComparer.Instance);
        }

        /// <summary>
        /// Maps each metaclass to the metamodel features its productions populate, and how.
        /// </summary>
        /// <remarks>
        /// Assignments frequently sit in an <b>untyped helper</b> production rather than the typed one
        /// - <c>PartUsage</c> delegates to <c>PartUsageDeclaration</c> - so a typed production also
        /// contributes the assignments of the helpers it reaches, transitively.
        /// <para>
        /// Two guards keep that honest. A helper must declare no metaclass of its own, so an assignment
        /// is never taken from an element the grammar assigns elsewhere; and it must be referenced by
        /// exactly one production, so a helper shared between two elements cannot spread its
        /// assignments across both. <b>96 of the 190 referenced helpers are shared</b>, so dropping the
        /// second guard would report more assignments and mean less. Under-reporting is the better
        /// failure here than misattribution.
        /// </para>
        /// </remarks>
        public IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>> FeatureLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses)
        {
            ArgumentNullException.ThrowIfNull(productions);
            ArgumentNullException.ThrowIfNull(metaclasses);

            var byName = ByName(productions);
            var exclusive = ExclusiveHelpers(productions, byName);

            var links = new Dictionary<string, Dictionary<FeatureAssignment, SortedSet<string>>>(
                StringComparer.Ordinal);

            foreach (var production in productions)
            {
                var element = BuiltElement(production, metaclasses);
                if (element is null)
                {
                    continue;
                }

                if (!links.TryGetValue(element, out var bucket))
                {
                    bucket = [];
                    links[element] = bucket;
                }

                foreach (var contributor in Contributors(production, byName, exclusive))
                {
                    foreach (var assignment in contributor.Features)
                    {
                        if (!bucket.TryGetValue(assignment, out var carriers))
                        {
                            carriers = new SortedSet<string>(StringComparer.Ordinal);
                            bucket[assignment] = carriers;
                        }

                        carriers.Add(contributor.Name);
                    }
                }
            }

            var result = new SortedDictionary<string, IReadOnlyList<FeatureLink>>(StringComparer.Ordinal);

            foreach (var (element, assignments) in links.Where(entry => entry.Value.Count > 0))
            {
                result[element] = assignments.Keys
                    .Order(FeatureAssignment.Order)
                    .Select(assignment => new FeatureLink(
                        assignment.Feature, assignment.Operator, [.. assignments[assignment]]))
                    .ToList();
            }

            return result;
        }

        /// <summary>The metaclass a production builds: its declared type, else its own name, else nothing.</summary>
        private static string? BuiltElement(Production production, ISet<string> metaclasses) =>
            new[] { production.Produces, production.Name }
                .FirstOrDefault(candidate => !string.IsNullOrEmpty(candidate) && metaclasses.Contains(candidate));

        /// <summary>The production names a body refers to: capitalised identifiers outside quoted literals.</summary>
        private static IEnumerable<string> References(string body)
        {
            var withoutLiterals = Literal().Replace(body, " ");

            return Reference().Matches(withoutLiterals).Select(match => match.Groups[1].Value);
        }

        private static Dictionary<string, Production> ByName(IReadOnlyList<Production> productions)
        {
            var byName = new Dictionary<string, Production>(StringComparer.Ordinal);

            foreach (var production in productions)
            {
                byName[production.Name] = production;
            }

            return byName;
        }

        /// <summary>
        /// A production plus the untyped helper productions it reaches, transitively.
        /// </summary>
        /// <remarks>
        /// Depth is bounded and visits are tracked, so a grammar that references itself cannot loop.
        /// </remarks>
        private static List<Production> Contributors(
            Production production,
            Dictionary<string, Production> byName,
            HashSet<string> exclusive)
        {
            var contributors = new List<Production> { production };
            var seen = new HashSet<string>(StringComparer.Ordinal) { production.Name };
            var frontier = new List<Production> { production };

            for (var depth = 0; depth < MaxDepth; depth++)
            {
                var following = new List<Production>();

                foreach (var current in frontier)
                {
                    foreach (var referenced in References(current.Body).Where(exclusive.Contains))
                    {
                        // Add() reports whether this is the first visit, which is also the loop guard.
                        if (seen.Add(referenced))
                        {
                            var helper = byName[referenced];
                            contributors.Add(helper);
                            following.Add(helper);
                        }
                    }
                }

                if (following.Count == 0)
                {
                    break;
                }

                frontier = following;
            }

            return contributors;
        }

        /// <summary>
        /// Untyped productions referenced by exactly one other production. Being referenced once is
        /// what makes a helper safe to attribute: its assignments can only belong to the one element
        /// that reaches it.
        /// </summary>
        private static HashSet<string> ExclusiveHelpers(
            IReadOnlyList<Production> productions, Dictionary<string, Production> byName)
        {
            var untyped = byName
                .Where(entry => entry.Value.Produces is null)
                .Select(entry => entry.Key)
                .ToHashSet(StringComparer.Ordinal);

            var referrers = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            foreach (var production in productions)
            {
                var helpers = References(production.Body)
                    .Where(referenced => untyped.Contains(referenced)
                        && !string.Equals(referenced, production.Name, StringComparison.Ordinal));

                foreach (var referenced in helpers)
                {
                    if (!referrers.TryGetValue(referenced, out var callers))
                    {
                        callers = new HashSet<string>(StringComparer.Ordinal);
                        referrers[referenced] = callers;
                    }

                    callers.Add(production.Name);
                }
            }

            return referrers
                .Where(entry => entry.Value.Count == 1)
                .Select(entry => entry.Key)
                .ToHashSet(StringComparer.Ordinal);
        }

        private static SortedSet<string> Bucket(
            Dictionary<string, SortedSet<string>> links, string element)
        {
            if (!links.TryGetValue(element, out var bucket))
            {
                bucket = new SortedSet<string>(StringComparer.Ordinal);
                links[element] = bucket;
            }

            return bucket;
        }

        private static SortedDictionary<string, IReadOnlyList<string>> Freeze(
            Dictionary<string, SortedSet<string>> links, IComparer<string> order)
        {
            var frozen = new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            foreach (var (element, values) in links)
            {
                frozen[element] = [.. values.Order(order)];
            }

            return frozen;
        }
    }
}
