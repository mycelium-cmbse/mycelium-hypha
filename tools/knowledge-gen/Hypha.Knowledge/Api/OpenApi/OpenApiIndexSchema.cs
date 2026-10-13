// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiIndexSchema.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    using System.Collections.Generic;

    /// <summary>
    /// One schema in the index.
    /// </summary>
    /// <param name="Id">The schema's <c>$id</c>.</param>
    /// <param name="Namespace">Whether it belongs to the API or mirrors a metamodel element.</param>
    /// <param name="Kind"><c>object</c>, <c>union</c>, <c>enumeration</c>, or the JSON type.</param>
    /// <param name="Role">Whether the endpoints use it in a request body, a response, both or neither.</param>
    /// <param name="File">Its page, relative to the index; <c>null</c> for a schema that has none.</param>
    /// <param name="Properties">Its members; <c>null</c> for a schema that has no page.</param>
    /// <param name="Variants">The alternatives of a union; <c>null</c> for a schema that has no page.</param>
    public sealed record OpenApiIndexSchema(
        string Id,
        string Namespace,
        string Kind,
        string Role,
        string? File,
        IReadOnlyList<OpenApiProperty>? Properties,
        IReadOnlyList<string>? Variants);
}
