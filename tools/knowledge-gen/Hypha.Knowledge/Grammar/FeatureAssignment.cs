// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureAssignment.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Grammar
{
    using System.Collections.Generic;

    /// <summary>
    /// A metamodel feature a piece of syntax populates, and how.
    /// </summary>
    /// <param name="Feature">The metamodel feature name, e.g. <c>ownedRelationship</c>.</param>
    /// <param name="Operator">
    /// <c>=</c> sets a value, <c>+=</c> adds to a collection, and <c>?=</c> sets a boolean flag from
    /// the presence of a keyword.
    /// </param>
    public sealed record FeatureAssignment(string Feature, string Operator)
    {
        /// <summary>
        /// Ordinal ordering by feature then operator, so generated output is stable across machines
        /// and cultures.
        /// </summary>
        /// <remarks>
        /// Deliberately a comparer rather than <see cref="System.IComparable{T}"/>: ordering these is
        /// a presentation choice made at the two sort sites, not an intrinsic property of an
        /// assignment, and a record that is comparable is expected to carry the relational operators
        /// too.
        /// </remarks>
        public static IComparer<FeatureAssignment> Order { get; } = new OrdinalOrder();

        private sealed class OrdinalOrder : IComparer<FeatureAssignment>
        {
            public int Compare(FeatureAssignment? x, FeatureAssignment? y)
            {
                if (x is null)
                {
                    return y is null ? 0 : -1;
                }

                if (y is null)
                {
                    return 1;
                }

                var byFeature = string.CompareOrdinal(x.Feature, y.Feature);

                return byFeature != 0 ? byFeature : string.CompareOrdinal(x.Operator, y.Operator);
            }
        }
    }
}
