// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeBuilder.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;

    /// <summary>
    /// Runs the registered generators of one collection in <see cref="IKnowledgeGenerator.Order"/>, one
    /// release or API version after the other.
    /// </summary>
    /// <typeparam name="TGenerator">The collection: <see cref="IKnowledgeGenerator"/> or <see cref="Api.IApiKnowledgeGenerator"/>.</typeparam>
    public sealed class KnowledgeBuilder<TGenerator> : IKnowledgeBuilder<TGenerator>
        where TGenerator : IKnowledgeGenerator
    {
        /// <summary>The generators, in the order they run.</summary>
        private readonly IReadOnlyList<TGenerator> generators;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeBuilder{TGenerator}"/> class.</summary>
        /// <param name="generators">The registered generators of the collection, in any order.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generators"/> is <c>null</c>.</exception>
        public KnowledgeBuilder(IEnumerable<TGenerator> generators)
        {
            ArgumentNullException.ThrowIfNull(generators);

            this.generators = [.. generators.OrderBy(generator => generator.Order)];
        }

        /// <summary>Gets every artifact the generators produce, in the order they run.</summary>
        public IReadOnlyList<string> Artifacts => [.. this.generators.Select(generator => generator.Artifact)];

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
        public async Task<KnowledgeBuild> BuildAsync(
            IReadOnlyList<string> tags,
            string? artifact = null,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tags);

            var selected = this.Select(artifact);
            var built = new List<ArtifactBuild>();

            foreach (var tag in tags)
            {
                var done = 0;

                foreach (var generator in selected)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    GenerationResult result;

                    try
                    {
                        result = await generator.GenerateAsync(tag, cancellationToken);
                    }
                    catch (InvalidOperationException exception)
                    {
                        // The deliberate refusals. They mean the output would be wrong, so the run stops
                        // rather than leaving a half-built knowledge base.
                        return new KnowledgeBuild(built, new ArtifactRefusal(tag, generator.Artifact, exception.Message));
                    }

                    built.Add(new ArtifactBuild(tag, generator.Artifact, result));
                    done++;

                    var message = result.Outcome == GenerationOutcome.Skipped
                        ? $"{generator.Artifact}: skipped, {result.Reason}"
                        : $"{generator.Artifact}: {result.Written.Count} files";

                    progress?.Report(new KnowledgeProgress(KnowledgeProgress.Generate, tag, done, selected.Count, message));
                }
            }

            return new KnowledgeBuild(built, null);
        }

        /// <summary>The generators to run.</summary>
        /// <param name="artifact">The one artifact asked for, or <c>null</c> or blank for every one.</param>
        /// <returns>Every generator, or the one producing <paramref name="artifact"/>.</returns>
        /// <exception cref="ArgumentException">No generator produces <paramref name="artifact"/>.</exception>
        private IReadOnlyList<TGenerator> Select(string? artifact)
        {
            if (string.IsNullOrWhiteSpace(artifact))
            {
                return this.generators;
            }

            var match = this.generators.Where(generator => string.Equals(generator.Artifact, artifact, StringComparison.Ordinal)).ToList();

            return match.Count > 0
                ? match
                : throw new ArgumentException(
                    $"No generator produces '{artifact}'. Known artifacts: {string.Join(", ", this.Artifacts)}.",
                    nameof(artifact));
        }
    }
}
