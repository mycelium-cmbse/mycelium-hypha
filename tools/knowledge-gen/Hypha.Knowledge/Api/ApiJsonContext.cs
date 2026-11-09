// ------------------------------------------------------------------------------------------------
// <copyright file="ApiJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Text.Json.Serialization;

    using Hypha.Knowledge.Api.CrossReferences;
    using Hypha.Knowledge.Api.OpenApi;

    /// <summary>
    /// The source-generated contract for every generated Systems Modeling API document, so writing and
    /// reading them needs no reflection-based serialization.
    /// </summary>
    /// <remarks>
    /// A document type absent here cannot be read or written through <see cref="ApiJson"/>: that is
    /// the point, since an undeclared type would otherwise be serialized by reflection unnoticed.
    /// </remarks>
    [JsonSerializable(typeof(ApiCrossReferenceDocument))]
    [JsonSerializable(typeof(OpenApiIndexDocument))]
    [JsonSerializable(typeof(PimGraph))]
    internal sealed partial class ApiJsonContext : JsonSerializerContext
    {
    }
}
