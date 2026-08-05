// ------------------------------------------------------------------------------------------------
// <copyright file="ClauseNumberComparerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Tests for ordering dotted clause numbers.
    /// </summary>
    /// <remarks>
    /// The Python this replaced covered the non-numeric case directly; the port kept the behaviour but
    /// lost the test, which left the annex path as the least-covered code in the ported library. This
    /// restores it and exercises the comparer through its own surface rather than incidentally.
    /// </remarks>
    [TestFixture]
    public class ClauseNumberComparerTests
    {
        private static IReadOnlyList<string> Sorted(params string[] clauses) =>
            [.. clauses.Order(ClauseNumberComparer.Instance)];

        [Test]
        public void Orders_numerically_not_lexically() =>
            Assert.That(
                Sorted("8.3.6.10", "8.3.6.3", "8.3.6.2"),
                Is.EqualTo(new[] { "8.3.6.2", "8.3.6.3", "8.3.6.10" }));

        [Test]
        public void Tolerates_non_numeric_parts_and_sorts_them_after_numeric_ones()
        {
            // Annex-style identifiers occur in both specifications. They must not throw, and they must
            // not sort among the numbered clauses.
            Assert.That(Sorted("A.1", "8.1"), Is.EqualTo(new[] { "8.1", "A.1" }));
        }

        [Test]
        public void Orders_numerically_within_an_annex() =>
            Assert.That(Sorted("A.10", "A.2", "A.1"), Is.EqualTo(new[] { "A.1", "A.2", "A.10" }));

        [Test]
        public void Two_non_numeric_parts_fall_back_to_ordinal() =>
            Assert.That(Sorted("B.1", "A.1"), Is.EqualTo(new[] { "A.1", "B.1" }));

        [Test]
        public void A_prefix_sorts_before_the_clause_that_extends_it() =>
            Assert.That(Sorted("8.3.1", "8.3"), Is.EqualTo(new[] { "8.3", "8.3.1" }));

        [Test]
        public void Leading_zeroes_do_not_change_the_order() =>
            Assert.That(Sorted("8.10", "8.02"), Is.EqualTo(new[] { "8.02", "8.10" }));

        [Test]
        public void An_empty_part_is_not_a_number_and_does_not_throw() =>
            Assert.That(() => Sorted("8..1", "8.1"), Throws.Nothing);

        [Test]
        public void Equal_clauses_compare_equal() =>
            Assert.That(ClauseNumberComparer.Instance.Compare("8.3.2", "8.3.2"), Is.Zero);

        [Test]
        public void Nulls_sort_first_and_never_throw()
        {
            // Reachable through IComparer<string>, so it is behaviour rather than dead defence.
            Assert.Multiple(() =>
            {
                Assert.That(ClauseNumberComparer.Instance.Compare(null, null), Is.Zero);
                Assert.That(ClauseNumberComparer.Instance.Compare(null, "8.1"), Is.Negative);
                Assert.That(ClauseNumberComparer.Instance.Compare("8.1", null), Is.Positive);
            });
        }
    }
}
