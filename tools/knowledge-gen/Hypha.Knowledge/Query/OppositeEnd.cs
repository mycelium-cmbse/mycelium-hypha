// ------------------------------------------------------------------------------------------------
// <copyright file="OppositeEnd.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.Metamodel;

    /// <summary>A feature and the other end of the association it is an end of.</summary>
    /// <param name="Class">The metaclass asked about.</param>
    /// <param name="Feature">The feature asked about.</param>
    /// <param name="DeclaredIn">The metaclass that declares the feature: <paramref name="Class"/> or one of its ancestors.</param>
    /// <param name="Other">
    /// The other end, with its owner; <c>null</c> when the feature is not an end of a binary
    /// association, or the other end cannot be named.
    /// </param>
    /// <param name="RedefinedBy">
    /// For an inherited feature, the feature that redefines it in <paramref name="Class"/>'s context, whose
    /// own opposite is the one that applies there; <c>null</c> when none does, and always for an owned feature.
    /// </param>
    public sealed record OppositeEnd(
        string Class,
        string Feature,
        string DeclaredIn,
        FeatureReference? Other,
        FeatureReference? RedefinedBy);
}
