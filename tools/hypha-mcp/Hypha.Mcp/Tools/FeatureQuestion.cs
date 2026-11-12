// ------------------------------------------------------------------------------------------------
// <copyright file="FeatureQuestion.cs" company="Starion Group S.A.">
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
    /// What <c>hypha_feature</c> answers about a feature. Each value is the name of the
    /// <c>hypha query</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<FeatureQuestion>))]
    public enum FeatureQuestion
    {
        /// <summary>The features that redefine it.</summary>
        [JsonStringEnumMemberName("redefinitions")]
        Redefinitions,

        /// <summary>The features that subset it.</summary>
        [JsonStringEnumMemberName("subsettings")]
        Subsettings,

        /// <summary>The other end of its association.</summary>
        [JsonStringEnumMemberName("opposite")]
        Opposite,

        /// <summary>The constraints that name it among what they constrain.</summary>
        [JsonStringEnumMemberName("feature-constraints")]
        FeatureConstraints,

        /// <summary>The features that subset or redefine it, directly or through another contributor.</summary>
        [JsonStringEnumMemberName("contributors")]
        Contributors,
    }
}
