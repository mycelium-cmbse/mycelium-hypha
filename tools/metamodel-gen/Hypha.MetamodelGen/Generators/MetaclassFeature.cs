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
        /// <summary>Gets the feature name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the visibility sigil (<c>+</c>, <c>-</c>, <c>#</c> or <c>~</c>).</summary>
        public required string Visibility { get; init; }

        /// <summary>Gets the feature type rendered as markdown: a link when the type resolves to a
        /// generated element file, otherwise the plain type name.</summary>
        public required string TypeMarkdown { get; init; }

        /// <summary>Gets the formatted multiplicity (e.g. <c>[0..*]</c>).</summary>
        public required string Multiplicity { get; init; }

        /// <summary>Gets the comma-separated modifier list (e.g. <c>derived, ordered</c>); may be empty.</summary>
        public required string Modifiers { get; init; }

        /// <summary>Gets the feature documentation (may be empty).</summary>
        public required string Documentation { get; init; }

        /// <summary>Gets the rendered (markdown link) references to the features this feature redefines.</summary>
        public required IReadOnlyList<string> Redefines { get; init; }

        /// <summary>Gets the rendered (markdown link) references to the features this feature subsets.</summary>
        public required IReadOnlyList<string> Subsets { get; init; }
    }
}
