// ------------------------------------------------------------------------------------------------
// <copyright file="SearchMatch.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Text.Json.Serialization;

    /// <summary>How closely a search hit matched, closest first; the same tiers the lookup resolves names in.</summary>
    /// <remarks>Written to JSON as its kebab-case name, such as <c>ignoring-case</c>, not its number.</remarks>
    [JsonConverter(typeof(JsonStringEnumConverter<SearchMatch>))]
    public enum SearchMatch
    {
        /// <summary>The name, or the clause number, is the text exactly.</summary>
        [JsonStringEnumMemberName("exact")]
        Exact = 0,

        /// <summary>The name is the text, ignoring case.</summary>
        [JsonStringEnumMemberName("ignoring-case")]
        IgnoringCase = 1,

        /// <summary>A trailing part of the qualified name is the text (<c>Real</c> for <c>ScalarValues::Real</c>), ignoring case.</summary>
        [JsonStringEnumMemberName("trailing-part")]
        TrailingPart = 2,

        /// <summary>The name or the title contains the text, ignoring case.</summary>
        [JsonStringEnumMemberName("contains")]
        Contains = 3,
    }
}
