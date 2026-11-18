// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Mcp.Tools
{
    using Hypha.Knowledge.Releases;

    /// <summary>What <c>hypha_check_releases</c> found.</summary>
    /// <param name="Releases">The language releases, installed and offered.</param>
    /// <param name="Api">The Systems Modeling API versions, installed and offered; <c>null</c> when not asked for.</param>
    internal sealed record CheckAnswer(UpstreamComparison Releases, UpstreamComparison? Api)
    {
        /// <summary>
        /// Gets a warning that few of GitHub's API requests without a token are left, and how to raise the
        /// limit; <c>null</c> when there are enough, or a token is set.
        /// </summary>
        public string? Notice { get; init; }
    }
}
