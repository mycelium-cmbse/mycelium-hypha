// ------------------------------------------------------------------------------------------------
// <copyright file="ApiDiscovery.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Finds the API versions the upstreams offer, over the same tag listing the language releases use.
    /// </summary>
    public sealed class ApiDiscovery : IApiDiscovery
    {
        private readonly IReleaseDiscovery releases;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDiscovery"/> class.
        /// </summary>
        public ApiDiscovery(IReleaseDiscovery releases)
        {
            ArgumentNullException.ThrowIfNull(releases);

            this.releases = releases;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> AvailableAsync(CancellationToken cancellationToken = default)
        {
            var services = await this.releases.FetchTagsAsync(ApiUpstream.ServicesRepository, cancellationToken);
            var release = await this.releases.FetchTagsAsync(Upstream.ReleaseRepository, cancellationToken);
            var pilot = await this.releases.FetchTagsAsync(Upstream.PilotRepository, cancellationToken);

            return ApiCatalog.Available(services, release, pilot);
        }
    }
}
