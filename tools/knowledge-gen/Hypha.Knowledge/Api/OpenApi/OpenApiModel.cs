// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiModel.cs" company="Starion Group S.A.">
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
    /// What hypha keeps of the REST/HTTP PSM: the schemas and the endpoints of the OpenAPI document.
    /// </summary>
    /// <param name="Title">The document title.</param>
    /// <param name="Version">The API version the document declares.</param>
    /// <param name="OpenApiVersion">The OpenAPI specification version the document is written in.</param>
    /// <param name="Tags">The endpoint groups, in the order the document declares them.</param>
    /// <param name="Schemas">Every schema, ordered by name.</param>
    /// <param name="Endpoints">Every endpoint, in document order.</param>
    public sealed record OpenApiModel(
        string Title,
        string Version,
        string OpenApiVersion,
        IReadOnlyList<string> Tags,
        IReadOnlyList<OpenApiSchema> Schemas,
        IReadOnlyList<OpenApiEndpoint> Endpoints);

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

    /// <summary>
    /// One operation on one path.
    /// </summary>
    /// <param name="Method">The HTTP method, upper case.</param>
    /// <param name="Path">The path template.</param>
    /// <param name="OperationId">The operation identifier.</param>
    /// <param name="Summary">The one-line summary.</param>
    /// <param name="Tags">The groups the operation belongs to.</param>
    /// <param name="Parameters">The path and query parameters, in document order.</param>
    /// <param name="Request">The request body type, when there is one.</param>
    /// <param name="Responses">The responses, in document order.</param>
    public sealed record OpenApiEndpoint(
        string Method,
        string Path,
        string OperationId,
        string Summary,
        IReadOnlyList<string> Tags,
        IReadOnlyList<OpenApiParameter> Parameters,
        string? Request,
        IReadOnlyList<OpenApiResponse> Responses);

    /// <summary>One parameter of an endpoint.</summary>
    /// <param name="Name">The parameter name.</param>
    /// <param name="In">Where it is passed: <c>path</c> or <c>query</c>.</param>
    /// <param name="Required">Whether it must be given.</param>
    /// <param name="Type">Its type, in the same notation as a schema member.</param>
    /// <param name="Description">What it is for.</param>
    public sealed record OpenApiParameter(string Name, string In, bool Required, string Type, string Description);

    /// <summary>One response of an endpoint.</summary>
    /// <param name="Status">The HTTP status code, or <c>default</c>.</param>
    /// <param name="Description">What the status means here.</param>
    /// <param name="Body">The body type, when there is one.</param>
    public sealed record OpenApiResponse(string Status, string Description, string? Body);
}
