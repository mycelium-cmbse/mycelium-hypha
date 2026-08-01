// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramRelationship.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// An edge of a package diagram, already oriented for Mermaid: <c>{Left} {Arrow} "{Multiplicity}"
    /// {Right} : {Label}</c>. Generalization reads parent-first (<c>Parent &lt;|-- Child</c>), while
    /// composition and association read owner-first.
    /// </summary>
    public sealed class DiagramRelationship
    {
        /// <summary>The Mermaid arrow for a generalization, written parent-first.</summary>
        public const string Generalization = "<|--";

        /// <summary>The Mermaid arrow for a composite (owned) feature.</summary>
        public const string Composition = "*--";

        /// <summary>The Mermaid arrow for a non-composite (referencing) feature.</summary>
        public const string Association = "-->";

        /// <summary>Gets the element on the left of the arrow.</summary>
        public required string Left { get; init; }

        /// <summary>Gets the Mermaid arrow.</summary>
        public required string Arrow { get; init; }

        /// <summary>Gets the element on the right of the arrow.</summary>
        public required string Right { get; init; }

        /// <summary>Gets the multiplicity shown on the target end; empty for generalizations.</summary>
        public string Multiplicity { get; init; } = string.Empty;

        /// <summary>Gets the edge label (the feature name); empty for generalizations.</summary>
        public string Label { get; init; } = string.Empty;
    }
}
