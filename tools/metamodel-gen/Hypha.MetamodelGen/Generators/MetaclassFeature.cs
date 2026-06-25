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
            string visibility,
            string typeMarkdown,
            string multiplicity,
            string modifiers,
            string documentation,
            IReadOnlyList<string> redefines,
            IReadOnlyList<string> subsets)
        {
            this.Name = name;
            this.Visibility = visibility;
            this.TypeMarkdown = typeMarkdown;
            this.Multiplicity = multiplicity;
            this.Modifiers = modifiers;
            this.Documentation = documentation;
            this.Redefines = redefines;
            this.Subsets = subsets;
        }

        /// <summary>Gets the feature name.</summary>
        public string Name { get; }

        /// <summary>Gets the visibility sigil (<c>+</c>, <c>-</c>, <c>#</c> or <c>~</c>).</summary>
        public string Visibility { get; }

        /// <summary>Gets the feature type rendered as markdown: a link when the type resolves to a
        /// generated element file, otherwise the plain type name.</summary>
        public string TypeMarkdown { get; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>[0..*]</c>).</summary>
        public string Multiplicity { get; }

        /// <summary>Gets the comma-separated modifier list (e.g. <c>derived, ordered</c>); may be empty.</summary>
        public string Modifiers { get; }

        /// <summary>Gets the feature documentation (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the rendered (markdown link) references to the features this feature redefines.</summary>
        public IReadOnlyList<string> Redefines { get; }

        /// <summary>Gets the rendered (markdown link) references to the features this feature subsets.</summary>
        public IReadOnlyList<string> Subsets { get; }
    }
}
