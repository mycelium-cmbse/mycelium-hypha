// ------------------------------------------------------------------------------------------------
// <copyright file="Contribution.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.Metamodel;

    /// <summary>A feature whose values are values of another, because it subsets or redefines it.</summary>
    /// <param name="Class">The metaclass that owns the contributing feature.</param>
    /// <param name="Feature">The contributing feature.</param>
    /// <param name="Through">
    /// How it contributes, named as the metamodel's attribute field: <c>subsets</c> or <c>redefines</c>.
    /// </param>
    /// <param name="Contributes">The feature it subsets or redefines, with its owner.</param>
    public sealed record Contribution(string Class, string Feature, string Through, FeatureReference Contributes);
}
