// ------------------------------------------------------------------------------------------------
// <copyright file="OpenApiParameter.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.OpenApi
{
    /// <summary>One parameter of an endpoint.</summary>
    /// <param name="Name">The parameter name.</param>
    /// <param name="In">Where it is passed: <c>path</c> or <c>query</c>.</param>
    /// <param name="Required">Whether it must be given.</param>
    /// <param name="Type">Its type, in the same notation as a schema member.</param>
    /// <param name="Description">What it is for.</param>
    public sealed record OpenApiParameter(string Name, string In, bool Required, string Type, string Description);
}
