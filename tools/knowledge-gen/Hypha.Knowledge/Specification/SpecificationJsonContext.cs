// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationJsonContext.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Specification
{
    using System.Text.Json.Serialization;

    /// <summary>The source-generated contract for a specification's <c>index.json</c>.</summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(SpecificationIndex))]
    internal sealed partial class SpecificationJsonContext : JsonSerializerContext
    {
    }
}
