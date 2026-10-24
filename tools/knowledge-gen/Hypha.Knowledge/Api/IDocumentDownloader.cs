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
        Task<FileInfo> DownloadAsync(
            Uri address, FileInfo destination, bool skipExisting = false, CancellationToken cancellationToken = default);
    }
}
