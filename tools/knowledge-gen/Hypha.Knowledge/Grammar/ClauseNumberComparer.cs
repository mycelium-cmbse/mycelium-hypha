// ------------------------------------------------------------------------------------------------
// <copyright file="ClauseNumberComparer.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Orders dotted clause numbers numerically, so <c>8.3.10</c> follows <c>8.3.2</c> instead of
    /// sorting between <c>8.3.1</c> and <c>8.3.2</c> as text would.
    /// </summary>
    public sealed class ClauseNumberComparer : IComparer<string>
    {
        /// <summary>The shared instance; the comparer holds no state.</summary>
        public static ClauseNumberComparer Instance { get; } = new();

        /// <inheritdoc/>
        public int Compare(string? x, string? y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }

            if (y is null)
            {
                return 1;
            }

            var left = x.Split('.');
            var right = y.Split('.');

            for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
            {
                var comparison = ComparePart(left[index], right[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            // A prefix sorts first: 8.3 before 8.3.1.
            return left.Length.CompareTo(right.Length);
        }

        /// <summary>
        /// Numeric parts sort numerically and before any non-numeric part, which sorts ordinally.
        /// Annexes and lettered sub-clauses are rare but do occur, and must not throw.
        /// </summary>
        private static int ComparePart(string left, string right)
        {
            var leftNumber = AsNumber(left);
            var rightNumber = AsNumber(right);

            if (leftNumber.HasValue && rightNumber.HasValue)
            {
                return leftNumber.Value.CompareTo(rightNumber.Value);
            }

            if (leftNumber.HasValue != rightNumber.HasValue)
            {
                return leftNumber.HasValue ? -1 : 1;
            }

            return string.CompareOrdinal(left, right);
        }

        private static int? AsNumber(string part)
        {
            if (part.Length == 0 || part.Any(character => character is < '0' or > '9'))
            {
                return null;
            }

            return int.Parse(part, CultureInfo.InvariantCulture);
        }
    }
}
