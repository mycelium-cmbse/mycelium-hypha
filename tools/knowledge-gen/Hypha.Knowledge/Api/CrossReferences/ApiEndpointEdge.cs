// ------------------------------------------------------------------------------------------------
// <copyright file="ApiEndpointEdge.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>An endpoint a PIM operation maps to.</summary>
    /// <param name="Method">The HTTP method.</param>
    /// <param name="Path">The path template, as the specification writes it.</param>
    /// <param name="OperationId">
    /// The OpenAPI operation with that method and path; <c>null</c> when the OpenAPI document has none.
    /// </param>
    public sealed record ApiEndpointEdge(string Method, string Path, string? OperationId);
}
