// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationQuestion.cs" company="Starion Group S.A.">
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
    /// What <c>hypha_spec</c> answers about a specification clause. Each value is the name of the
    /// <c>hypha query</c> verb that answers the same question.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SpecificationQuestion>))]
    public enum SpecificationQuestion
    {
        /// <summary>The clause, with its verbatim text.</summary>
        [JsonStringEnumMemberName("clause")]
        Clause,

        /// <summary>The elements the clause treats.</summary>
        [JsonStringEnumMemberName("elements-in-clause")]
        ElementsInClause,
    }
}
