// ------------------------------------------------------------------------------------------------
// <copyright file="DeclaredMember.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>A member of a metaclass, a constraint or an operation, with the metaclass that declares it.</summary>
    /// <typeparam name="T">The member's type.</typeparam>
    /// <param name="DeclaredIn">The metaclass that declares the member: the one asked about, or one of its ancestors.</param>
    /// <param name="Member">The member, as the metamodel records it.</param>
    public sealed record DeclaredMember<T>(string DeclaredIn, T Member);
}
