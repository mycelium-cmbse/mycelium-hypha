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
    /// The answer, or <c>null</c> when the element the query is about does not exist in the release, or
    /// when the artifact it would be read from is <see cref="Unavailable"/>. A query that found the
    /// element but nothing else returns an empty list, not <c>null</c>.
    /// </param>
    /// <param name="Grounding">Where the answer comes from.</param>
    /// <param name="Unavailable">
    /// Why the answer cannot be given, and how to make it available, when it would come from an optional
    /// artifact the release has not generated (the specification text, which needs the OMG PDFs);
    /// otherwise <c>null</c>. A missing core artifact is an error, not an unavailable answer.
    /// </param>
    public sealed record QueryResult<T>(T? Value, Grounding Grounding, string? Unavailable = null)
        where T : class
    {
        /// <summary>Gets a value indicating whether the element the query is about exists in the release.</summary>
        public bool Found => this.Value is not null;

        /// <summary>Gets a value indicating whether the artifact the answer comes from has been generated.</summary>
        public bool Available => this.Unavailable is null;
    }
}
