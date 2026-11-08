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

        /// <summary>
        /// Compares two dotted clause numbers part by part: numerically where both parts are numbers, with a
        /// numeric part before a non-numeric one, and ordinally otherwise. A prefix sorts first, so
        /// <c>8.3</c> precedes <c>8.3.1</c>; <c>null</c> sorts before any number.
        /// </summary>
        /// <param name="x">The first clause number.</param>
        /// <param name="y">The second clause number.</param>
        /// <returns>
        /// Less than zero when <paramref name="x"/> sorts first, zero when they are equal, and greater than
        /// zero when <paramref name="y"/> sorts first.
        /// </returns>
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
        /// <param name="left">A part of the first clause number.</param>
        /// <param name="right">The part of the second clause number at the same position.</param>
        /// <returns>The parts' relative order, as <see cref="IComparer{T}.Compare"/> reports it.</returns>
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

        /// <summary>Reads a clause number part as an integer when it consists of ASCII digits only.</summary>
        /// <param name="part">One dot-separated part.</param>
        /// <returns>Its value, or <c>null</c> when it is empty or not all digits.</returns>
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
