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
    public sealed record PimClass(
        string Name,
        IReadOnlyList<PimAttribute>? OwnedAttributes,
        IReadOnlyList<PimAttribute>? InheritedAttributes,
        IReadOnlyList<PimNamed>? OwnedOperations);
}
