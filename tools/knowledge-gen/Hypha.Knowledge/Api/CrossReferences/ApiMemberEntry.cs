// ------------------------------------------------------------------------------------------------
// <copyright file="ApiMemberEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>One effective PIM attribute and whether the PSM models carry it.</summary>
    /// <param name="Name">The attribute name.</param>
    /// <param name="Multiplicity">Its PIM multiplicity, e.g. <c>[0..*]</c>.</param>
    /// <param name="Derived">Whether it is derived in the PIM.</param>
    /// <param name="InheritedFrom">The class it is inherited from; <c>null</c> when owned.</param>
    /// <param name="Response">Whether the response model has a member of the same name.</param>
    /// <param name="Request">Whether the request model has a member of the same name.</param>
    public sealed record ApiMemberEntry(
        string Name, string Multiplicity, bool Derived, string? InheritedFrom, bool Response, bool Request);
}
