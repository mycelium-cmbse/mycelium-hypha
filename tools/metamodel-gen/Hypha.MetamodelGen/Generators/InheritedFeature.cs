// ------------------------------------------------------------------------------------------------
// <copyright file="InheritedFeature.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// A feature a metaclass inherits from one of its supertypes. Rendered as a compact table row
    /// (no documentation) so the element file shows the metaclass' effective feature set without
    /// the reader having to walk the generalization chain.
    /// </summary>
    public sealed class InheritedFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedFeature"/> class.
        /// </summary>
        public InheritedFeature(string name, string type, string multiplicity, string owner, string modifiers)
        {
            this.Name = name;
            this.Type = type;
            this.Multiplicity = multiplicity;
            this.Owner = owner;
            this.Modifiers = modifiers;
        }

        /// <summary>Gets the feature name.</summary>
        public string Name { get; }

        /// <summary>Gets the feature type name.</summary>
        public string Type { get; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>[0..*]</c>).</summary>
        public string Multiplicity { get; }

        /// <summary>Gets the name of the supertype that declares this feature.</summary>
        public string Owner { get; }

        /// <summary>Gets the comma-separated modifier list (e.g. <c>derived, ordered</c>); may be empty.</summary>
        public string Modifiers { get; }
    }
}
