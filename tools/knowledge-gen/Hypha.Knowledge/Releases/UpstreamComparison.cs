// ------------------------------------------------------------------------------------------------
// <copyright file="UpstreamComparison.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System.Collections.Generic;

    /// <summary>
    /// What is installed of one version axis, the language releases or the Systems Modeling API versions,
    /// against what is offered upstream.
    /// </summary>
    /// <param name="Installed">The installed tags, newest first; empty when none is.</param>
    /// <param name="Default">The tag every question is answered from when none is named, or <c>null</c>.</param>
    /// <param name="Offered">
    /// The newest tags offered upstream, newest first, at most <see cref="MaxOffered"/>: what can be installed.
    /// </param>
    /// <param name="Newer">
    /// The newest offered tag that is newer than every installed one, or the newest offered one when nothing is
    /// installed; <c>null</c> when nothing newer is offered.
    /// </param>
    public sealed record UpstreamComparison(
        IReadOnlyList<string> Installed, string? Default, IReadOnlyList<string> Offered, string? Newer)
    {
        /// <summary>How many offered tags <see cref="Offered"/> ever carries.</summary>
        public const int MaxOffered = 5;
    }
}
