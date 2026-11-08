// ------------------------------------------------------------------------------------------------
// <copyright file="PimClass.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>A PIM class.</summary>
    /// <param name="Name">The class name.</param>
    /// <param name="OwnedAttributes">The attributes it owns; <c>null</c> when the document lists none.</param>
    /// <param name="InheritedAttributes">The attributes it inherits; <c>null</c> when the document lists none.</param>
    /// <param name="OwnedOperations">
    /// The operations it owns (a class with any is a PIM service); <c>null</c> when the document lists none.
    /// </param>
    public sealed record PimClass(
        string Name,
        IReadOnlyList<PimAttribute>? OwnedAttributes,
        IReadOnlyList<PimAttribute>? InheritedAttributes,
        IReadOnlyList<PimNamed>? OwnedOperations);
}
