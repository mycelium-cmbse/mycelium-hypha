// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationIndex.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Specification
{
    using System.Collections.Generic;

    /// <summary>
    /// <c>knowledge/&lt;tag&gt;/spec/&lt;document&gt;/index.json</c>, and the Systems Modeling API's
    /// <c>knowledge/api/&lt;version&gt;/spec/index.json</c>: every clause of one specification, with the
    /// page that holds its verbatim text.
    /// </summary>
    /// <remarks>
    /// Written by <c>tools/spec-extract</c>, in Python, not by a C# generator; this is the shape it
    /// writes, so the query API can read it through a source-generated contract.
    /// </remarks>
    /// <param name="SchemaVersion">The version of this file's shape.</param>
    /// <param name="Document">The specification, as its cover names it: <c>KerML</c>, <c>SysML</c>.</param>
    /// <param name="Version">The specification's version, such as <c>1.0</c>.</param>
    /// <param name="Clauses">How many clauses the index lists.</param>
    /// <param name="Entries">The clauses, keyed by clause number.</param>
    public sealed record SpecificationIndex(
        string SchemaVersion,
        string Document,
        string Version,
        int Clauses,
        IReadOnlyDictionary<string, SpecificationClauseEntry> Entries)
    {
        /// <summary>The name of the index file in a specification's folder.</summary>
        public const string FileName = "index.json";
    }
}
