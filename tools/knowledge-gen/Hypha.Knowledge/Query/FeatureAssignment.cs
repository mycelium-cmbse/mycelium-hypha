// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureAssignment.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using Hypha.Knowledge.CrossReferences;

    /// <summary>A metamodel feature that an element's textual syntax fills in.</summary>
    /// <param name="Element">The element whose syntax fills the feature in.</param>
    /// <param name="Edge">The feature, the assignment operator and the productions that assign it.</param>
    public sealed record FeatureAssignment(string Element, FeatureEdge Edge);
}
