// ------------------------------------------------------------------------------------------------
// <copyright file="Redefinition.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>A feature that redefines another.</summary>
    /// <param name="Class">The metaclass that owns the redefining feature.</param>
    /// <param name="Feature">The redefining feature.</param>
    /// <param name="Redefines">The feature it redefines.</param>
    public sealed record Redefinition(string Class, string Feature, string Redefines);
}
