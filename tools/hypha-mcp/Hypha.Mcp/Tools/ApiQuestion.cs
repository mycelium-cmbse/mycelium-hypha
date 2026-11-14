// ------------------------------------------------------------------------------------------------
// <copyright file="ApiQuestion.cs" company="Starion Group S.A.">
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
    /// What <c>hypha_api</c> answers about a Systems Modeling API version. Each value is the name of
    /// the <c>hypha query api</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ApiQuestion>))]
    public enum ApiQuestion
    {
        /// <summary>A class of the API's platform-independent model.</summary>
        [JsonStringEnumMemberName("pim-class")]
        PimClass,

        /// <summary>An API type's cross-references: its PIM class, schema and operations.</summary>
        [JsonStringEnumMemberName("type")]
        Type,

        /// <summary>An API operation's cross-references.</summary>
        [JsonStringEnumMemberName("operation")]
        Operation,

        /// <summary>A schema of the REST/HTTP PSM: its properties and, for a union, its variants.</summary>
        [JsonStringEnumMemberName("schema")]
        Schema,

        /// <summary>One clause of the API specification, with its verbatim text.</summary>
        [JsonStringEnumMemberName("clause")]
        Clause,
    }
}
