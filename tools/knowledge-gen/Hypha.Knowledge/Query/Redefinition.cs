// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.Metamodel;

    /// <summary>A feature that redefines another.</summary>
    /// <param name="Class">The metaclass that owns the redefining feature.</param>
    /// <param name="Feature">The redefining feature.</param>
    /// <param name="Redefines">The feature it redefines, with its owner.</param>
    public sealed record Redefinition(string Class, string Feature, FeatureReference Redefines);
}
