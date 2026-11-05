// ------------------------------------------------------------------------------------------------
// <copyright file="PimAttribute.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>A PIM attribute; <see cref="InheritedFrom"/> is <c>null</c> when it is owned.</summary>
    public sealed record PimAttribute(string Name, int Lower, int Upper, bool IsDerived, string? InheritedFrom);
}
