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
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassPayload"/> class.
        /// </summary>
        public MetaclassPayload(
            string name,
            string package,
            string fullyQualifiedName,
            bool isAbstract,
            string visibility,
            string documentation,
            IReadOnlyList<string> generalizations,
            IReadOnlyList<string> specializations,
            IReadOnlyList<MetaclassFeature> features,
            IReadOnlyList<InheritedFeature> inheritedFeatures,
            IReadOnlyList<MetaclassConstraint> constraints)
        {
            this.Name = name;
            this.Package = package;
            this.FullyQualifiedName = fullyQualifiedName;
            this.IsAbstract = isAbstract;
            this.Visibility = visibility;
            this.Documentation = documentation;
            this.Generalizations = generalizations;
            this.Specializations = specializations;
            this.Features = features;
            this.InheritedFeatures = inheritedFeatures;
            this.Constraints = constraints;
        }

        /// <summary>Gets the metaclass name.</summary>
        public string Name { get; }

        /// <summary>Gets the owning package name.</summary>
        public string Package { get; }

        /// <summary>Gets the fully qualified name (e.g. <c>SysML::Systems::Actions::AcceptActionUsage</c>).</summary>
        public string FullyQualifiedName { get; }

        /// <summary>Gets a value indicating whether the metaclass is abstract.</summary>
        public bool IsAbstract { get; }

        /// <summary>Gets the metaclass visibility (e.g. <c>public</c>).</summary>
        public string Visibility { get; }

        /// <summary>Gets the metaclass documentation (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the direct supertype (generalization) names, ordered.</summary>
        public IReadOnlyList<string> Generalizations { get; }

        /// <summary>Gets the direct subtype (specialization) names, ordered.</summary>
        public IReadOnlyList<string> Specializations { get; }

        /// <summary>Gets the owned features, ordered by name.</summary>
        public IReadOnlyList<MetaclassFeature> Features { get; }

        /// <summary>Gets the features inherited from supertypes, ordered by name.</summary>
        public IReadOnlyList<InheritedFeature> InheritedFeatures { get; }

        /// <summary>Gets the owned constraints, ordered by name.</summary>
        public IReadOnlyList<MetaclassConstraint> Constraints { get; }
    }
}
