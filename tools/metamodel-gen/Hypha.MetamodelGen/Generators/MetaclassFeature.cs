// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassFeature.cs" company="Starion Group S.A.">
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
    /// An owned feature (attribute / association end) of a metaclass, as rendered in its element file.
    /// </summary>
    public sealed class MetaclassFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassFeature"/> class.
        /// </summary>
        public MetaclassFeature(
            string name,
            string type,
            string multiplicity,
            string modifiers,
            string documentation,
            IReadOnlyList<string> redefines,
            IReadOnlyList<string> subsets)
        {
            this.Name = name;
            this.Type = type;
            this.Multiplicity = multiplicity;
            this.Modifiers = modifiers;
            this.Documentation = documentation;
            this.Redefines = redefines;
            this.Subsets = subsets;
        }

        /// <summary>Gets the feature name.</summary>
        public string Name { get; }

        /// <summary>Gets the feature type name.</summary>
        public string Type { get; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>0..*</c>).</summary>
        public string Multiplicity { get; }

        /// <summary>Gets the modifier annotation (e.g. <c>{derived, ordered}</c>); may be empty.</summary>
        public string Modifiers { get; }

        /// <summary>Gets the feature documentation (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the names of the features this feature redefines.</summary>
        public IReadOnlyList<string> Redefines { get; }

        /// <summary>Gets the names of the features this feature subsets.</summary>
        public IReadOnlyList<string> Subsets { get; }
    }
}
