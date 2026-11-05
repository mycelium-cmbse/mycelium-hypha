// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiEndpoint.cs" company="Starion Group S.A.">
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
}
