// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordUse.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Keywords;

    /// <summary>A keyword or operator as one grammar uses it.</summary>
    /// <param name="Grammar">The grammar that lists the literal: <c>kerml</c> or <c>sysml</c>.</param>
    /// <param name="Kind">
    /// <see cref="KeywordEntry.Reserved"/>, <see cref="KeywordEntry.Contextual"/> or
    /// <see cref="KeywordEntry.Operator"/>, as <paramref name="Grammar"/> classifies it.
    /// </param>
    /// <param name="SyntaxFrom">
    /// The grammar whose productions write the literal. It is <paramref name="Grammar"/> itself, except for a
    /// literal SysML text holds only through the KerML productions the SysML grammar calls without
    /// defining them, its expressions (<c>and</c>, <c>istype</c>, <c>true</c> or <c>+</c>): their syntax
    /// is KerML's, and this is <c>kerml</c>.
    /// </param>
    /// <param name="Productions">The productions of <paramref name="SyntaxFrom"/> that write the literal, ordered ordinally.</param>
    /// <param name="Metaclasses">
    /// The metaclasses whose syntax in <paramref name="SyntaxFrom"/> includes the literal, ordered ordinally
    /// by name, each saying whether it is written directly or through a helper production.
    /// </param>
    public sealed record KeywordUse(
        string Grammar,
        string Kind,
        string SyntaxFrom,
        IReadOnlyList<string> Productions,
        IReadOnlyList<KeywordMetaclass> Metaclasses);
}
