// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureInfo.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    /// <summary>One feature of a metaclass, owned or inherited.</summary>
    /// <param name="Name">The feature's name.</param>
    /// <param name="Type">The feature's type, or <c>null</c> when the metamodel gives none.</param>
    /// <param name="Lower">The lower bound of its multiplicity.</param>
    /// <param name="Upper">The upper bound of its multiplicity; <c>-1</c> for unbounded.</param>
    /// <param name="IsDerived">Whether the feature is derived.</param>
    /// <param name="IsComposite">Whether the feature is composite.</param>
    /// <param name="Redefines">The features it redefines.</param>
    /// <param name="Subsets">The features it subsets.</param>
    /// <param name="InheritedFrom">
    /// The metaclass that owns the feature, or <c>null</c> when the queried metaclass owns it itself.
    /// </param>
    public sealed record FeatureInfo(
        string Name,
        string? Type,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsComposite,
        IReadOnlyList<string> Redefines,
        IReadOnlyList<string> Subsets,
        string? InheritedFrom);
}
