// ------------------------------------------------------------------------------------------------
// <copyright file="ApiSchemaEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>The PSM schemas of a PIM type.</summary>
    /// <param name="Response">The schema used in responses.</param>
    /// <param name="ResponseMethod">What links it.</param>
    /// <param name="Request">The schema used in request bodies, when it differs.</param>
    /// <param name="RequestMethod">What links it.</param>
    public sealed record ApiSchemaEdge(string? Response, string? ResponseMethod, string? Request, string? RequestMethod);
}
