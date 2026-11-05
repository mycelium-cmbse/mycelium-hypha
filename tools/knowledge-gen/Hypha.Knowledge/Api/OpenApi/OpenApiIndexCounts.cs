// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiIndexCounts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>How much an OpenAPI document holds.</summary>
    /// <param name="Schemas">Every schema.</param>
    /// <param name="ApiSchemas">The schemas of the API itself, each of which has a page.</param>
    /// <param name="Endpoints">Every operation.</param>
    public sealed record OpenApiIndexCounts(int Schemas, int ApiSchemas, int Endpoints);
}
