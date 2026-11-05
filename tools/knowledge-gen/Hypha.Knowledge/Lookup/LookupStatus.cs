// ------------------------------------------------------------------------------------------------
// <copyright file="LookupStatus.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    using System.Text.Json.Serialization;

    /// <summary>How a lookup name resolved.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LookupStatus>))]
    public enum LookupStatus
    {
        /// <summary>Exactly one entry matched; its source is returned.</summary>
        [JsonStringEnumMemberName("found")]
        Found,

        /// <summary>
        /// Several entries match equally well (for example the same short name in two packages). The
        /// caller has to choose one of the matches and ask again.
        /// </summary>
        [JsonStringEnumMemberName("ambiguous")]
        Ambiguous,

        /// <summary>
        /// Nothing matched the name itself; the matches only contain it. They are suggestions, never
        /// an answer, so none is picked even when there is only one.
        /// </summary>
        [JsonStringEnumMemberName("candidates")]
        Candidates,

        /// <summary>Nothing matched or contained the name.</summary>
        [JsonStringEnumMemberName("not-found")]
        NotFound,
    }
}
