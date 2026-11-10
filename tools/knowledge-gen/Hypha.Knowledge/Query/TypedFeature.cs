// ------------------------------------------------------------------------------------------------
// <copyright file="TypedFeature.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>A feature with the metaclass that declares it, its type and its multiplicity.</summary>
    /// <param name="Class">The metaclass that declares the feature.</param>
    /// <param name="Feature">The feature's name.</param>
    /// <param name="Type">The feature's type, or <c>null</c> when the metamodel gives none.</param>
    /// <param name="Lower">The lower bound of its multiplicity.</param>
    /// <param name="Upper">The upper bound of its multiplicity; <c>-1</c> for unbounded.</param>
    /// <param name="IsComposite">Whether the feature is composite, so its values are owned through it.</param>
    /// <param name="IsDerived">Whether the feature is derived.</param>
    public sealed record TypedFeature(
        string Class,
        string Feature,
        string? Type,
        int Lower,
        int Upper,
        bool IsComposite,
        bool IsDerived);
}
