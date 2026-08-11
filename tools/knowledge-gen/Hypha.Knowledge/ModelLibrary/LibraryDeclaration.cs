// ------------------------------------------------------------------------------------------------
// <copyright file="LibraryDeclaration.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    /// <summary>
    /// One named declaration a standard-library model introduces, e.g. <c>ISQ::mass</c>.
    /// </summary>
    /// <param name="QualifiedName">The dotted-scope name, e.g. <c>ISQBase::LengthValue</c>.</param>
    /// <param name="Kind">
    /// The literal keyword(s) that introduced it, e.g. <c>attribute def</c> or <c>datatype</c> - the
    /// source's own words, not a resolved metaclass. Nothing here reads <c>:&gt;</c>/<c>:&gt;&gt;</c>
    /// chains, so this is the declaration surface, not its resolved type.
    /// </param>
    public sealed record LibraryDeclaration(string QualifiedName, string Kind);
}
