// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionManifestJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The source-generated contract for <c>knowledge/api/versions.json</c>, so writing and reading
    /// it needs no reflection-based serialization.
    /// </summary>
    [JsonSerializable(typeof(ApiVersionManifest))]
    internal sealed partial class ApiVersionManifestJsonContext : JsonSerializerContext
    {
    }
}
