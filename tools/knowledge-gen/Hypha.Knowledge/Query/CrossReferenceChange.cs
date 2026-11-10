// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceChange.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>
    /// One change to where an element is treated: a specification clause, a grammar production, a
    /// feature its syntax populates or a worked example, as <c>cross-references.json</c> records them.
    /// It states what the two documents say; whether the change matters is for the caller to judge.
    /// </summary>
    /// <param name="Element">The element's name in the newer release.</param>
    /// <param name="ElementKind">The element's kind: <c>class</c>, <c>enumeration</c> or <c>primitiveType</c>.</param>
    /// <param name="Facet">
    /// What the edge links the element to: <see cref="Clause"/>, <see cref="Grammar"/>,
    /// <see cref="Feature"/> or <see cref="Example"/>.
    /// </param>
    /// <param name="Edge">
    /// The edge, written as what identifies it: <c>document clause</c> for a clause (<c>kerml 8.3.6.3</c>),
    /// <c>grammar production</c> for a production (<c>sysml PartUsageDeclaration</c>),
    /// <c>grammar feature operator</c> for a feature (<c>kerml declaredName =</c>) and the page path for
    /// an example.
    /// </param>
    /// <param name="Kind">
    /// <see cref="MetamodelChange.Added"/>, <see cref="MetamodelChange.Removed"/> or
    /// <see cref="MetamodelChange.Changed"/> (the same edge with another value in one field).
    /// </param>
    /// <param name="Field">
    /// The field that changed: <c>provenance</c>, <c>method</c> or, for a feature, <c>productions</c>;
    /// <c>null</c> for an addition or a removal.
    /// </param>
    /// <param name="From">
    /// The value in the older release, with a list of productions sorted and joined by <c>, </c>;
    /// <c>null</c> for an addition or a removal.
    /// </param>
    /// <param name="To">The value in the newer release, written the same way.</param>
    public sealed record CrossReferenceChange(
        string Element,
        string ElementKind,
        string Facet,
        string Edge,
        string Kind,
        string? Field,
        string? From,
        string? To)
    {
        /// <summary>The <see cref="Facet"/> of a specification clause that treats the element.</summary>
        public const string Clause = "clause";

        /// <summary>The <see cref="Facet"/> of a grammar production that builds the element.</summary>
        public const string Grammar = "grammar";

        /// <summary>The <see cref="Facet"/> of a metamodel feature the element's syntax populates.</summary>
        public const string Feature = "feature";

        /// <summary>The <see cref="Facet"/> of a worked example that declares the element.</summary>
        public const string Example = "example";
    }
}
