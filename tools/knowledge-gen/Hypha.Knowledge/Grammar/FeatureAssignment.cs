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
    using System;

    /// <summary>
    /// A metamodel feature a piece of syntax populates, and how.
    /// </summary>
    /// <param name="Feature">The metamodel feature name, e.g. <c>ownedRelationship</c>.</param>
    /// <param name="Operator">
    /// <c>=</c> sets a value, <c>+=</c> adds to a collection, and <c>?=</c> sets a boolean flag from
    /// the presence of a keyword.
    /// </param>
    public sealed record FeatureAssignment(string Feature, string Operator)
        : IComparable<FeatureAssignment>
    {
        /// <summary>Ordinal ordering, so generated output is stable across machines and cultures.</summary>
        public int CompareTo(FeatureAssignment? other)
        {
            if (other is null)
            {
                return 1;
            }

            var byFeature = string.CompareOrdinal(this.Feature, other.Feature);

            return byFeature != 0 ? byFeature : string.CompareOrdinal(this.Operator, other.Operator);
        }
    }
}
