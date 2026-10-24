// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiProperty.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>
    /// One member of an object schema.
    /// </summary>
    /// <param name="Name">The member name.</param>
    /// <param name="Type">The member's type, in a compact notation (<c>string (uuid)</c>, <c>Commit[]</c>).</param>
    /// <param name="Required">Whether the schema lists it as required.</param>
    /// <param name="Nullable">Whether <c>null</c> is one of its allowed values.</param>
    /// <param name="RefersTo">
    /// The type a reference by identifier points at, when the document states it: a member typed
    /// <c>Identified</c> carries only an <c>@id</c>, and names its target in a <c>$comment</c>.
    /// </param>
    public sealed record OpenApiProperty(string Name, string Type, bool Required, bool Nullable, string? RefersTo);
}
