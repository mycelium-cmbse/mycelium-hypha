// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiResponse.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>One response of an endpoint.</summary>
    /// <param name="Status">The HTTP status code, or <c>default</c>.</param>
    /// <param name="Description">What the status means here.</param>
    /// <param name="Body">The body type, when there is one.</param>
    public sealed record OpenApiResponse(string Status, string Description, string? Body);
}
