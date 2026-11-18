// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

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
