// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

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
    /// The index is read through the shared <see cref="IDocumentCache"/>, so it is kept until its file
    /// changes; a page is read on each call. Safe to call from several threads at once.
    /// </remarks>
    public sealed class SpecificationReader : ISpecificationReader
    {
        /// <summary>The line that opens and closes a page's front matter.</summary>
        private const string FrontMatterFence = "---";

        /// <summary>The documents read so far.</summary>
        private readonly IDocumentCache documents;

        /// <summary>Initializes a new instance of the <see cref="SpecificationReader"/> class.</summary>
        /// <param name="documents">The documents read so far, shared with the queries.</param>
        /// <exception cref="ArgumentNullException"><paramref name="documents"/> is <c>null</c>.</exception>
        public SpecificationReader(IDocumentCache documents)
        {
            ArgumentNullException.ThrowIfNull(documents);

            this.documents = documents;
        }

        /// <summary>Reads a specification's index, or returns <c>null</c> when it has not been generated.</summary>
        /// <param name="index">The specification's <c>index.json</c>.</param>
        /// <param name="regenerate">The command that regenerates the specification text, for the messages.</param>
        /// <returns>The index, or <c>null</c> when it does not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="index"/> is <c>null</c>.</exception>
        /// <exception cref="KnowledgeLookupException">The index exists but cannot be read or is invalid.</exception>
        public SpecificationIndex? ReadIndex(FileInfo index, string regenerate)
        {
            ArgumentNullException.ThrowIfNull(index);

            index.Refresh();

            return index.Exists
                ? this.documents.Get(
                    index,
                    ReadContracts.Specification,
                    document => document.Entries is not null,
                    $"{index.FullName} does not exist.",
                    regenerate)
                : null;
        }

        /// <summary>Reads one clause, with its verbatim text and without the page's front matter.</summary>
        /// <param name="index">The specification's <c>index.json</c>.</param>
        /// <param name="document">The specification, as the knowledge keys it, such as <c>kerml</c>.</param>
        /// <param name="clause">The clause number.</param>
        /// <param name="version">The release or API version.</param>
        /// <param name="owner">What the text belongs to, such as <c>release 2026-05</c>, for the messages.</param>
        /// <param name="regenerate">The command that regenerates the specification text, for the messages.</param>
        /// <param name="unavailable">Why the text is missing and how to generate it, for a version that has not.</param>
        /// <param name="ground">The grounding of an answer read from a file.</param>
        /// <returns>
        /// The clause, grounded as <see cref="Provenance.Normative"/> on its page; <c>null</c> when the
        /// index has no such clause; <see cref="QueryResult{T}.Unavailable"/> when the index does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="index"/> or <paramref name="ground"/> is <c>null</c>.</exception>
        /// <exception cref="KnowledgeLookupException">The index or the page is invalid, missing, or outside its folder.</exception>
        public QueryResult<SpecificationClause> Read(
            FileInfo index,
            string document,
            string clause,
            string version,
            string owner,
            string regenerate,
            string unavailable,
            Func<FileInfo, Grounding> ground)
        {
            ArgumentNullException.ThrowIfNull(ground);

            var specification = this.ReadIndex(index, regenerate);

            if (specification is null)
            {
                return new(null, ground(index), unavailable);
            }

            if (!specification.Entries.TryGetValue(clause, out var entry))
            {
                return new(null, ground(index));
            }

            var page = KnowledgeLookup.ContainedFile(index.Directory!, entry.File, version, owner, regenerate);
            var text = StripFrontMatter(this.documents.ReadText(page));

            return new(
                new SpecificationClause(document, clause, entry.Title, entry.Pages, entry.Normative, text),
                ground(page) with { Tier = Provenance.Normative });
        }

        /// <summary>The page's text without its front matter.</summary>
        /// <param name="page">The page's text, LF or CRLF.</param>
        /// <returns>The text after the closing fence, or the whole page when it has no front matter.</returns>
        private static string StripFrontMatter(string page)
        {
            var text = page.Replace("\r\n", "\n", StringComparison.Ordinal);

            if (!text.StartsWith(FrontMatterFence + "\n", StringComparison.Ordinal))
            {
                return text;
            }

            var close = text.IndexOf("\n" + FrontMatterFence + "\n", FrontMatterFence.Length, StringComparison.Ordinal);

            return close < 0 ? text : text[(close + FrontMatterFence.Length + 2)..].TrimStart('\n');
        }
    }
}
