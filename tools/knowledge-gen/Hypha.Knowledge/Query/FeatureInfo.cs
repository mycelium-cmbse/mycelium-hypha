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

    using Hypha.Knowledge.Metamodel;

    /// <summary>One feature of a metaclass, owned or inherited.</summary>
    /// <param name="Name">The feature's name.</param>
    /// <param name="Type">The feature's type, or <c>null</c> when the metamodel gives none.</param>
    /// <param name="Lower">The lower bound of its multiplicity.</param>
    /// <param name="Upper">The upper bound of its multiplicity; <c>-1</c> for unbounded.</param>
    /// <param name="IsDerived">Whether the feature is derived.</param>
    /// <param name="IsDerivedUnion">Whether the feature is a derived union of the features that subset it.</param>
    /// <param name="IsComposite">Whether the feature is composite.</param>
    /// <param name="IsOrdered">Whether the feature is ordered.</param>
    /// <param name="IsUnique">Whether the feature's values are unique.</param>
    /// <param name="IsReadOnly">Whether the feature is read-only.</param>
    /// <param name="DefaultValue">The default value as the metamodel writes it, or <c>null</c> when it has none.</param>
    /// <param name="Redefines">The features it redefines, each with its owner.</param>
    /// <param name="Subsets">The features it subsets, each with its owner.</param>
    /// <param name="Opposite">The other end of its association, or <c>null</c> when it is not an association end.</param>
    /// <param name="InheritedFrom">
    /// The metaclass that owns the feature, or <c>null</c> when the queried metaclass owns it itself.
    /// </param>
    /// <param name="RedefinedBy">
    /// For an inherited feature, the feature that redefines it in the queried metaclass's context, or
    /// <c>null</c> when none does; always <c>null</c> for an owned feature.
    /// </param>
    public sealed record FeatureInfo(
        string Name,
        string? Type,
        int Lower,
        int Upper,
        bool IsDerived,
        bool IsDerivedUnion,
        bool IsComposite,
        bool IsOrdered,
        bool IsUnique,
        bool IsReadOnly,
        string? DefaultValue,
        IReadOnlyList<FeatureReference> Redefines,
        IReadOnlyList<FeatureReference> Subsets,
        FeatureReference? Opposite,
        string? InheritedFrom,
        FeatureReference? RedefinedBy);
}
