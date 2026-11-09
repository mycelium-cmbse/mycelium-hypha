// ------------------------------------------------------------------------------------------------
// <copyright file="QueryResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>The answer to one query, with where it comes from.</summary>
    /// <typeparam name="T">What the query returns.</typeparam>
    /// <param name="Value">
    /// The answer, or <c>null</c> when the element the query is about does not exist in the release.
    /// A query that found the element but nothing else returns an empty list, not <c>null</c>.
    /// </param>
    /// <param name="Grounding">Where the answer comes from.</param>
    public sealed record QueryResult<T>(T? Value, Grounding Grounding)
        where T : class
    {
        /// <summary>Gets a value indicating whether the element the query is about exists in the release.</summary>
        public bool Found => this.Value is not null;
    }
}
