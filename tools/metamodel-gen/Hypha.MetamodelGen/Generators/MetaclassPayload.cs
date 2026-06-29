// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassPayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System.Collections.Generic;

    /// <summary>
    /// The Handlebars context for a single metaclass element file.
    /// </summary>
    public sealed class MetaclassPayload
    {
        /// <summary>Gets the metaclass name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the owning package name.</summary>
        public required string Package { get; init; }

        /// <summary>Gets the fully qualified name (e.g. <c>SysML::Systems::Actions::AcceptActionUsage</c>).</summary>
        public required string FullyQualifiedName { get; init; }

        /// <summary>Gets a value indicating whether the metaclass is abstract.</summary>
        public required bool IsAbstract { get; init; }

        /// <summary>Gets the metaclass visibility (e.g. <c>public</c>).</summary>
        public required string Visibility { get; init; }

        /// <summary>Gets the metaclass documentation (may be empty).</summary>
        public required string Documentation { get; init; }

        /// <summary>Gets the direct supertype (generalization) names, ordered.</summary>
        public required IReadOnlyList<string> Generalizations { get; init; }

        /// <summary>Gets the direct subtype (specialization) names, ordered.</summary>
        public required IReadOnlyList<string> Specializations { get; init; }

        /// <summary>Gets the owned features, ordered by name.</summary>
        public required IReadOnlyList<MetaclassFeature> Features { get; init; }

        /// <summary>Gets the features inherited from supertypes, ordered by name.</summary>
        public required IReadOnlyList<InheritedFeature> InheritedFeatures { get; init; }

        /// <summary>Gets the owned constraints, ordered by name.</summary>
        public required IReadOnlyList<MetaclassConstraint> Constraints { get; init; }
    }
}
