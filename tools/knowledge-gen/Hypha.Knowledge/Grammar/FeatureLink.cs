// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureLink.cs" company="Starion Group S.A.">
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
    /// One metamodel feature a metaclass's syntax populates, and the productions that populate it.
    /// </summary>
    /// <param name="Feature">The metamodel feature name.</param>
    /// <param name="Operator"><c>=</c>, <c>+=</c> or <c>?=</c>.</param>
    /// <param name="Productions">The productions carrying the assignment, ordered.</param>
    public sealed record FeatureLink(string Feature, string Operator, IReadOnlyList<string> Productions);
}
