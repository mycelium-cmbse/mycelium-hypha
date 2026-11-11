// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeBuilder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;

    /// <summary>
    /// Runs one collection of generators over releases or API versions: every artifact, or one, in
    /// <see cref="IKnowledgeGenerator.Order"/>, reporting each one as it finishes.
    /// </summary>
    /// <typeparam name="TGenerator">
    /// The collection the generators are registered as: <see cref="IKnowledgeGenerator"/> for the language
    /// releases, <see cref="Api.IApiKnowledgeGenerator"/> for the API versions. A builder of either is a
    /// builder of <see cref="IKnowledgeGenerator"/>, so a caller that only builds needs not know which.
    /// </typeparam>
    /// <remarks>
    /// The CLI's <c>generate</c> and <c>move-window</c>, and the MCP server's install, run generation
    /// through here, so a full run is the same loop wherever it starts.
    /// </remarks>
    public interface IKnowledgeBuilder<out TGenerator>
        where TGenerator : IKnowledgeGenerator
    {
        /// <summary>Gets every artifact the generators produce, in the order they run.</summary>
        IReadOnlyList<string> Artifacts { get; }

        /// <summary>Generates the artifacts of each release or API version, one release after the other.</summary>
        /// <param name="tags">The releases or API versions, in the order to generate them.</param>
        /// <param name="artifact">Only this artifact, one of <see cref="Artifacts"/>; <c>null</c> or blank for every one.</param>
        /// <param name="progress">
        /// Reported once per artifact as it finishes, written or skipped, as a
        /// <see cref="KnowledgeProgress.Generate"/> step per tag counting its artifacts. <c>null</c> when nobody
        /// needs to show progress.
        /// </param>
        /// <param name="cancellationToken">Cancels the run between and within artifacts.</param>
        /// <returns>
        /// What each artifact did, in the order run. When a generator refused, because its output would
        /// have been wrong, the run stops there and the result names the refusal.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tags"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="artifact"/> is not one of <see cref="Artifacts"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<KnowledgeBuild> BuildAsync(
            IReadOnlyList<string> tags,
            string? artifact = null,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
