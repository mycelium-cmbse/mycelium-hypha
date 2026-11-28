// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Query
{
    using System;
    using System.IO;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Specification;

    /// <summary>
    /// Reads the specification text spec-extract generates, an <c>index.json</c> and one page per
    /// clause, for a language release and an API version alike.
    /// </summary>
    /// <remarks>
    /// The text is optional, since it needs the OMG PDFs: an index that does not exist is an answer, not
    /// an error, while one that exists but is invalid is an error. Implementations must be safe to call
    /// from several threads at once.
    /// </remarks>
    public interface ISpecificationReader
    {
        /// <summary>Reads a specification's index, or returns <c>null</c> when it has not been generated.</summary>
        /// <param name="index">The specification's <c>index.json</c>.</param>
        /// <param name="regenerate">The command that regenerates the specification text, for the messages.</param>
        /// <returns>The index, or <c>null</c> when it does not exist.</returns>
        /// <exception cref="KnowledgeLookupException">The index exists but cannot be read or is invalid.</exception>
        SpecificationIndex? ReadIndex(FileInfo index, string regenerate);

        /// <summary>Reads one clause, with its verbatim text and without the page's front matter.</summary>
        /// <param name="index">The specification's <c>index.json</c>.</param>
        /// <param name="document">The specification, as the knowledge keys it, such as <c>kerml</c>.</param>
        /// <param name="clause">The clause number; case is ignored.</param>
        /// <param name="version">The release or API version.</param>
        /// <param name="owner">What the text belongs to, such as <c>release 2026-05</c>, for the messages.</param>
        /// <param name="regenerate">The command that regenerates the specification text, for the messages.</param>
        /// <param name="unavailable">Why the text is missing and how to generate it, for a version that has not.</param>
        /// <param name="ground">The grounding of an answer read from a file.</param>
        /// <returns>
        /// The clause, grounded as <see cref="Provenance.Normative"/> on its page; <c>null</c> when the
        /// index has no such clause; <see cref="QueryResult{T}.Unavailable"/> when the index does not exist.
        /// </returns>
        /// <exception cref="KnowledgeLookupException">The index or the page is invalid, missing, or outside its folder.</exception>
        QueryResult<SpecificationClause> Read(
            FileInfo index,
            string document,
            string clause,
            string version,
            string owner,
            string regenerate,
            string unavailable,
            Func<FileInfo, Grounding> ground);

        /// <summary>
        /// Reads one clause of a specification whose index the caller has already read, with its verbatim
        /// text and without the page's front matter.
        /// </summary>
        /// <remarks>
        /// The index file is neither refreshed nor read again, so callers that read many clauses, from
        /// several threads at once, can share one <see cref="FileInfo"/> of it.
        /// </remarks>
        /// <param name="index">The specification's <c>index.json</c>, which locates the pages.</param>
        /// <param name="specification">The index, as <see cref="ReadIndex"/> returned it.</param>
        /// <param name="document">The specification, as the knowledge keys it, such as <c>kerml</c>.</param>
        /// <param name="clause">The clause number; case is ignored.</param>
        /// <param name="version">The release or API version.</param>
        /// <param name="owner">What the text belongs to, such as <c>release 2026-05</c>, for the messages.</param>
        /// <param name="regenerate">The command that regenerates the specification text, for the messages.</param>
        /// <param name="ground">The grounding of an answer read from a file.</param>
        /// <returns>
        /// The clause, grounded as <see cref="Provenance.Normative"/> on its page; <c>null</c> when the
        /// index has no such clause.
        /// </returns>
        /// <exception cref="KnowledgeLookupException">The page is missing or outside its folder.</exception>
        QueryResult<SpecificationClause> Read(
            FileInfo index,
            SpecificationIndex specification,
            string document,
            string clause,
            string version,
            string owner,
            string regenerate,
            Func<FileInfo, Grounding> ground);
    }
}
