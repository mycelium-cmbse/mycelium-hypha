// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheck.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Compares what is installed with what is offered upstream, and says when something newer is.
    /// </summary>
    public sealed class ReleaseCheck : IReleaseCheck
    {
        /// <summary>Finds the offered language releases.</summary>
        private readonly IReleaseDiscovery releases;

        /// <summary>Finds the offered API versions.</summary>
        private readonly IApiDiscovery apiVersions;

        /// <summary>The installed language releases.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>The installed API versions.</summary>
        private readonly IApiLayout apiLayout;

        /// <summary>Initializes a new instance of the <see cref="ReleaseCheck"/> class.</summary>
        /// <param name="releases">Finds the offered language releases.</param>
        /// <param name="apiVersions">Finds the offered API versions.</param>
        /// <param name="layout">The installed language releases.</param>
        /// <param name="apiLayout">The installed API versions.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ReleaseCheck(IReleaseDiscovery releases, IApiDiscovery apiVersions, IKnowledgeLayout layout, IApiLayout apiLayout)
        {
            ArgumentNullException.ThrowIfNull(releases);
            ArgumentNullException.ThrowIfNull(apiVersions);
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(apiLayout);

            this.releases = releases;
            this.apiVersions = apiVersions;
            this.layout = layout;
            this.apiLayout = apiLayout;
        }

        /// <summary>Compares the installed language releases with the ones offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>The comparison.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        public async Task<UpstreamComparison> CheckReleasesAsync(CancellationToken cancellationToken = default) =>
            Compare(this.layout.InstalledTags, this.layout.DefaultTag, await this.releases.AvailableAsync(cancellationToken));

        /// <summary>Compares the installed Systems Modeling API versions with the ones offered upstream.</summary>
        /// <param name="cancellationToken">Cancels the requests to GitHub.</param>
        /// <returns>The comparison.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">GitHub could not be reached.</exception>
        public async Task<UpstreamComparison> CheckApiAsync(CancellationToken cancellationToken = default) =>
            Compare(this.apiLayout.InstalledTags, this.apiLayout.DefaultTag, await this.apiVersions.AvailableAsync(cancellationToken));

        /// <summary>Compares installed tags with offered ones.</summary>
        /// <param name="installed">The installed tags.</param>
        /// <param name="defaultTag">The default tag, or <c>null</c>.</param>
        /// <param name="available">The offered tags, newest first.</param>
        /// <returns>The comparison.</returns>
        private static UpstreamComparison Compare(IReadOnlyList<string> installed, string? defaultTag, IReadOnlyList<string> available)
        {
            // Only release-shaped tags are ranked: a hand-edited manifest may list anything.
            var ranked = installed.Where(ReleaseTag.IsRelease).ToList();
            var newestInstalled = ranked.Count == 0 ? (int?)null : ranked.Max(Rank);
            var offered = available.Where(ReleaseTag.IsRelease).OrderByDescending(Rank).ToList();

            return new UpstreamComparison(
                ranked.OrderByDescending(Rank).Concat(installed.Where(tag => !ReleaseTag.IsRelease(tag))).ToList(),
                defaultTag,
                [.. offered.Take(UpstreamComparison.MaxOffered)],
                offered.FirstOrDefault(tag => newestInstalled is null || Rank(tag) > newestInstalled));
        }

        /// <summary>A tag's place in release order, as one number: year, month and point.</summary>
        /// <param name="tag">A release-shaped tag.</param>
        /// <returns>Its rank; later releases rank higher.</returns>
        private static int Rank(string tag)
        {
            var (year, month, point) = ReleaseTag.SortKey(tag);

            return (year * 100 + month) * 1000 + point;
        }
    }
}
