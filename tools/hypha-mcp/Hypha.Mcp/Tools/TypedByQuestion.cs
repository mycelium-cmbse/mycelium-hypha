// ------------------------------------------------------------------------------------------------
// <copyright file="TypedByQuestion.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// What <c>hypha_typed_by</c> answers about a type. Each value is the name of the
    /// <c>hypha query</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<TypedByQuestion>))]
    public enum TypedByQuestion
    {
        /// <summary>The metaclasses with a feature typed by the type.</summary>
        [JsonStringEnumMemberName("classes-with-feature-typed")]
        ClassesWithFeatureTyped,

        /// <summary>Every feature that can hold a value of the type.</summary>
        [JsonStringEnumMemberName("features-accepting")]
        FeaturesAccepting,

        /// <summary>The composite features through which an element of the type can be owned.</summary>
        [JsonStringEnumMemberName("containers")]
        Containers,
    }
}
