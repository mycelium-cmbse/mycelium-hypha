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
        /// <param name="name">The feature name.</param>
        /// <param name="typeMarkdown">The feature type rendered as markdown.</param>
        /// <param name="multiplicity">The formatted multiplicity.</param>
        /// <param name="owner">The name of the supertype that declares the feature.</param>
        /// <param name="modifiers">The comma-separated modifier list (may be empty).</param>
        public InheritedFeature(string name, string typeMarkdown, string multiplicity, string owner, string modifiers)
        {
            this.Name = name;
            this.TypeMarkdown = typeMarkdown;
            this.Multiplicity = multiplicity;
            this.Owner = owner;
            this.Modifiers = modifiers;
        }

        /// <summary>Gets the feature name.</summary>
        public string Name { get; }

        /// <summary>Gets the feature type rendered as markdown: a link when the type resolves to a
        /// generated element file, otherwise the plain type name.</summary>
        public string TypeMarkdown { get; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>[0..*]</c>).</summary>
        public string Multiplicity { get; }

        /// <summary>Gets the name of the supertype that declares this feature.</summary>
        public string Owner { get; }

        /// <summary>Gets the comma-separated modifier list (e.g. <c>derived, ordered</c>); may be empty.</summary>
        public string Modifiers { get; }
    }
}
