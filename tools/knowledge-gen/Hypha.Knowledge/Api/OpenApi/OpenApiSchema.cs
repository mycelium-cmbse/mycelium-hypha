// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiSchema.cs" company="Starion Group S.A.">
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
    /// One schema of the document.
    /// </summary>
    /// <param name="Name">The schema name, as keyed under <c>components/schemas</c>.</param>
    /// <param name="Id">The schema's <c>$id</c>, which is what says which specification it belongs to.</param>
    /// <param name="Namespace">
    /// <see cref="OpenApiNames.ApiNamespace"/> for a schema of the API itself,
    /// <see cref="OpenApiNames.MetamodelNamespace"/> for one that mirrors a KerML / SysML element.
    /// </param>
    /// <param name="Kind"><c>object</c>, <c>union</c>, <c>enumeration</c>, or the JSON type.</param>
    /// <param name="Role">
    /// Whether the endpoints use it in a request body, a response, both or neither - reached directly
    /// or through another schema.
    /// </param>
    /// <param name="AdditionalProperties">Whether members beyond the declared ones are allowed, when stated.</param>
    /// <param name="Properties">The members, ordered by name. Empty for a union or an enumeration.</param>
    /// <param name="Variants">The alternatives of a union, in document order.</param>
    /// <param name="Values">The literals of an enumeration, in document order.</param>
    public sealed record OpenApiSchema(
        string Name,
        string Id,
        string Namespace,
        string Kind,
        string Role,
        bool? AdditionalProperties,
        IReadOnlyList<OpenApiProperty> Properties,
        IReadOnlyList<string> Variants,
        IReadOnlyList<string> Values);
}
