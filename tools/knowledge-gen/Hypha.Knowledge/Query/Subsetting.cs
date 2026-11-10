// ------------------------------------------------------------------------------------------------
// <copyright file="Subsetting.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.Metamodel;

    /// <summary>A feature that subsets another.</summary>
    /// <param name="Class">The metaclass that owns the subsetting feature.</param>
    /// <param name="Feature">The subsetting feature.</param>
    /// <param name="Subsets">The feature it subsets, with its owner.</param>
    public sealed record Subsetting(string Class, string Feature, FeatureReference Subsets);
}
