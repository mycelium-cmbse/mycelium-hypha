// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseFetcher.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;

    /// <summary>
    /// Downloads one release's inputs into <c>sources/&lt;tag&gt;/</c>.
    /// </summary>
    public interface IReleaseFetcher
    {
        /// <summary>Every blob path in a repository at a tag.</summary>
        /// <param name="repository">The upstream repository, as <c>owner/name</c>.</param>
        /// <param name="tag">The tag to list.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The repository-relative paths of every file at the tag.</returns>
        Task<IReadOnlyList<string>> ListTreeAsync(
            string repository, string tag, CancellationToken cancellationToken = default);

        /// <summary>Downloads one file, unless it is already there and <paramref name="skipExisting"/>.</summary>
        /// <param name="repository">The upstream repository, as <c>owner/name</c>.</param>
        /// <param name="tag">The tag to download the file at.</param>
        /// <param name="path">The file's path in the repository.</param>
        /// <param name="destination">Where the file is written; its folder is created if needed.</param>
        /// <param name="skipExisting">Keeps a file that is already there instead of downloading it again.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns><paramref name="destination"/>, refreshed.</returns>
        Task<FileInfo> DownloadAsync(
            string repository,
            string tag,
            string path,
            FileInfo destination,
            bool skipExisting = false,
            CancellationToken cancellationToken = default);

        /// <summary>Fetches the metamodel XMI for a release.</summary>
        /// <param name="tag">The release to fetch.</param>
        /// <param name="sourcesRoot">The <c>sources/</c> folder; the files land under <c>&lt;tag&gt;/xmi/</c>.</param>
        /// <param name="skipExisting">Keeps files that are already there instead of downloading them again.</param>
        /// <param name="cancellationToken">Cancels the downloads.</param>
        /// <param name="progress">
        /// Reported once per file, after it completes, whether it was downloaded or skipped because
        /// it was already there. <c>null</c> when nobody needs to show progress.
        /// </param>
        /// <returns>The fetched files.</returns>
        Task<IReadOnlyList<FileInfo>> FetchMetamodelAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default,
            IProgress<KnowledgeProgress>? progress = null);

        /// <summary>Fetches the grammar and textual models, preserving the upstream layout.</summary>
        /// <param name="tag">The release to fetch.</param>
        /// <param name="sourcesRoot">The <c>sources/</c> folder; the files land under <c>&lt;tag&gt;/textual/</c>.</param>
        /// <param name="skipExisting">Keeps files that are already there instead of downloading them again.</param>
        /// <param name="cancellationToken">Cancels the listing and the downloads.</param>
        /// <param name="progress">
        /// Reported once per file, after it completes, whether it was downloaded or skipped because
        /// it was already there. <c>null</c> when nobody needs to show progress.
        /// </param>
        /// <returns>The fetched files.</returns>
        Task<IReadOnlyList<FileInfo>> FetchTextualAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default,
            IProgress<KnowledgeProgress>? progress = null);

        /// <summary>
        /// Fetches the specification PDFs into the <b>git-ignored</b> folder. They are OMG-copyrighted
        /// and must never be committed.
        /// </summary>
        /// <param name="tag">The release to fetch.</param>
        /// <param name="sourcesRoot">The <c>sources/</c> folder; the files land under <c>&lt;tag&gt;/specs/</c>.</param>
        /// <param name="skipExisting">Keeps files that are already there instead of downloading them again.</param>
        /// <param name="cancellationToken">Cancels the downloads.</param>
        /// <param name="progress">
        /// Reported once per file, after it completes, whether it was downloaded or skipped because
        /// it was already there. <c>null</c> when nobody needs to show progress.
        /// </param>
        /// <returns>The fetched files.</returns>
        Task<IReadOnlyList<FileInfo>> FetchSpecificationsAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default,
            IProgress<KnowledgeProgress>? progress = null);
    }
}
