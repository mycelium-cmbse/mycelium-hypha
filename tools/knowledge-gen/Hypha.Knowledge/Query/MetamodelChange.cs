// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelChange.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>
    /// One change to a metamodel element, or to an attribute, operation, parameter, constraint or literal
    /// it owns. It states what the two documents say; whether the change breaks a model is for the
    /// caller to judge.
    /// </summary>
    /// <param name="Element">
    /// The element's name: in the newer release, or in the older one for an element it removes.
    /// </param>
    /// <param name="ElementKind">The element's kind: <c>class</c>, <c>enumeration</c> or <c>primitiveType</c>.</param>
    /// <param name="Member">
    /// The member's name, named the same way as <paramref name="Element"/>; a parameter is written as
    /// <c>operation(parameter)</c>. <c>null</c> for a change to the element itself.
    /// </param>
    /// <param name="MemberKind">
    /// The member's kind: <c>attribute</c>, <c>operation</c>, <c>parameter</c>, <c>constraint</c> or
    /// <c>literal</c>; <c>null</c> for a change to the element itself.
    /// </param>
    /// <param name="Kind">
    /// <c>added</c>, <c>removed</c>, <c>renamed</c> (the same XMI id under another name), <c>changed</c>
    /// (one field has another value) or <c>documentation</c> (the documentation text was rewritten; the
    /// texts themselves are left out).
    /// </param>
    /// <param name="Field">
    /// The field that changed, named as in <c>metamodel.json</c> (<c>lower</c>, <c>redefines</c>), plus
    /// <c>name</c> for a rename, <c>package</c> for a move and <c>position</c> for a reordered parameter;
    /// <c>null</c> for an addition, a removal or a documentation change.
    /// </param>
    /// <param name="From">
    /// The value in the older release, as text: a number or <c>true</c>/<c>false</c> as written, a list of
    /// references as <c>Owner::name</c> joined by <c>, </c>; <c>null</c> when the field had no value or
    /// no field changed.
    /// </param>
    /// <param name="To">The value in the newer release, written the same way.</param>
    public sealed record MetamodelChange(
        string Element,
        string ElementKind,
        string? Member,
        string? MemberKind,
        string Kind,
        string? Field,
        string? From,
        string? To)
    {
        /// <summary>The <see cref="Kind"/> of a change that adds an element or member.</summary>
        public const string Added = "added";

        /// <summary>The <see cref="Kind"/> of a change that removes an element or member.</summary>
        public const string Removed = "removed";

        /// <summary>The <see cref="Kind"/> of a change that renames an element or member.</summary>
        public const string Renamed = "renamed";

        /// <summary>The <see cref="Kind"/> of a change to one field.</summary>
        public const string Changed = "changed";

        /// <summary>The <see cref="Kind"/> of a change to the documentation text.</summary>
        public const string Documentation = "documentation";
    }
}
