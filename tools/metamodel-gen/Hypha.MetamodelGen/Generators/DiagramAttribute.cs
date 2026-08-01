// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramAttribute.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// A non-derived owned feature rendered inside a class box of a package diagram: the primitive-
    /// and enumeration-typed features, which would otherwise drag <c>String</c> and <c>Boolean</c>
    /// into every diagram as hub nodes.
    /// </summary>
    public sealed class DiagramAttribute
    {
        /// <summary>Gets the feature name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the feature type name (a primitive type or an enumeration).</summary>
        public required string Type { get; init; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>[0..1]</c>).</summary>
        public required string Multiplicity { get; init; }
    }
}
