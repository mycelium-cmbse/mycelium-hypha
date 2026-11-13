// ------------------------------------------------------------------------------------------------
// <copyright file="SyntaxQuestion.cs" company="Starion Group S.A.">
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
    /// What <c>hypha_syntax</c> answers about the textual notation. Each value is the name of the
    /// <c>hypha query</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SyntaxQuestion>))]
    public enum SyntaxQuestion
    {
        /// <summary>An element's clauses, grammar productions, the features its syntax fills in and its examples.</summary>
        [JsonStringEnumMemberName("cross-reference")]
        CrossReference,

        /// <summary>Which elements' syntax fills in a feature, through which operator and productions.</summary>
        [JsonStringEnumMemberName("feature-assignments")]
        FeatureAssignments,

        /// <summary>The elements a grammar production builds.</summary>
        [JsonStringEnumMemberName("built-by")]
        BuiltBy,

        /// <summary>The productions that write a keyword or operator, and the metaclasses whose syntax includes it.</summary>
        [JsonStringEnumMemberName("keyword")]
        Keyword,
    }
}
