// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Specification
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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

        /// <summary>
        /// Finds a clause by its number, ignoring case, so that <c>a.3</c> finds the annex clause
        /// <c>A.3</c>. A number that matches exactly is preferred.
        /// </summary>
        /// <param name="clause">The clause number, as asked for.</param>
        /// <param name="number">The clause number as the index writes it, or <c>null</c> when there is no such clause.</param>
        /// <param name="entry">The clause, or <c>null</c> when there is no such clause.</param>
        /// <returns>Whether the index has the clause.</returns>
        public bool TryFind(string clause, [NotNullWhen(true)] out string? number, [NotNullWhen(true)] out SpecificationClauseEntry? entry)
        {
            if (this.Entries.TryGetValue(clause, out entry))
            {
                number = clause;
                return true;
            }

            foreach (var (key, value) in this.Entries)
            {
                if (string.Equals(key, clause, StringComparison.OrdinalIgnoreCase))
                {
                    (number, entry) = (key, value);
                    return true;
                }
            }

            (number, entry) = (null, null);
            return false;
        }
    }
}
