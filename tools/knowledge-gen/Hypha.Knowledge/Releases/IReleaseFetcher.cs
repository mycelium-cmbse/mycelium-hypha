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
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Downloads one release's inputs into <c>sources/&lt;tag&gt;/</c>.
    /// </summary>
    public interface IReleaseFetcher
    {
        /// <summary>Every blob path in a repository at a tag.</summary>
        Task<IReadOnlyList<string>> ListTreeAsync(
            string repository, string tag, CancellationToken cancellationToken = default);

        /// <summary>Downloads one file, unless it is already there and <paramref name="skipExisting"/>.</summary>
        Task<FileInfo> DownloadAsync(
            string repository,
            string tag,
            string path,
            FileInfo destination,
            bool skipExisting = false,
            CancellationToken cancellationToken = default);

        /// <summary>Fetches the metamodel XMI for a release.</summary>
        Task<IReadOnlyList<FileInfo>> FetchMetamodelAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default);

        /// <summary>Fetches the grammar and textual models, preserving the upstream layout.</summary>
        Task<IReadOnlyList<FileInfo>> FetchTextualAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the specification PDFs into the <b>git-ignored</b> folder. They are OMG-copyrighted
        /// and must never be committed.
        /// </summary>
        Task<IReadOnlyList<FileInfo>> FetchSpecificationsAsync(
            string tag,
            DirectoryInfo sourcesRoot,
            bool skipExisting = false,
            CancellationToken cancellationToken = default);
    }
}
