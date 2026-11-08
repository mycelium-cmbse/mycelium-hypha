// ------------------------------------------------------------------------------------------------
// <copyright file="IDocumentDownloader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Downloads one published document from an address that is not a repository at a tag.
    /// </summary>
    public interface IDocumentDownloader
    {
        /// <summary>
        /// Downloads <paramref name="address"/>, unless it is already there and
        /// <paramref name="skipExisting"/>.
        /// </summary>
        /// <param name="address">The document to download.</param>
        /// <param name="destination">The file to write it to.</param>
        /// <param name="skipExisting">Keeps a file already at <paramref name="destination"/>.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns><paramref name="destination"/>, once it holds the document.</returns>
        Task<FileInfo> DownloadAsync(
            Uri address, FileInfo destination, bool skipExisting = false, CancellationToken cancellationToken = default);
    }
}
