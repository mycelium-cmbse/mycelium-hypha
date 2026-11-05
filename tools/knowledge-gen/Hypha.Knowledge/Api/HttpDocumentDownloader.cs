// ------------------------------------------------------------------------------------------------
// <copyright file="HttpDocumentDownloader.cs" company="Starion Group S.A.">
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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Downloads a published document over HTTP.
    /// </summary>
    /// <remarks>
    /// Takes its own client rather than the upstream one, which carries the GitHub token on every
    /// request: a credential for one host must not be sent to another.
    /// </remarks>
    public sealed class HttpDocumentDownloader : IDocumentDownloader
    {
        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDocumentDownloader"/> class.
        /// </summary>
        /// <param name="client">A client without credentials.</param>
        public HttpDocumentDownloader(HttpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            this.client = client;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// A zero-byte file counts as an interrupted write rather than a completed download, so a
        /// resumed run replaces it.
        /// </remarks>
        public async Task<FileInfo> DownloadAsync(
            Uri address, FileInfo destination, bool skipExisting = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(address);
            ArgumentNullException.ThrowIfNull(destination);

            destination.Refresh();

            if (skipExisting && destination.Exists && destination.Length > 0)
            {
                return destination;
            }

            destination.Directory?.Create();

            var bytes = await this.client.GetByteArrayAsync(address, cancellationToken);
            await File.WriteAllBytesAsync(destination.FullName, bytes, cancellationToken);
            destination.Refresh();

            return destination;
        }
    }
}
