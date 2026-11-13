// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperations.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Progress;

    /// <summary>
    /// Installs, switches and removes releases on their own, each under the release-operation lock.
    /// </summary>
    public sealed class ReleaseOperations : IReleaseOperations
    {
        /// <summary>The release-operation lock.</summary>
        private readonly IReleaseOperationLock releaseLock;

        /// <summary>Fetches a release's inputs and records it.</summary>
        private readonly IReleaseInstaller installer;

        /// <summary>Generates a release's knowledge.</summary>
        private readonly IKnowledgeBuilder<IKnowledgeGenerator> builder;

        /// <summary>Switches the default release.</summary>
        private readonly IReleaseSelector selector;

        /// <summary>Removes a release.</summary>
        private readonly IReleaseRemover remover;

        /// <summary>Initializes a new instance of the <see cref="ReleaseOperations"/> class.</summary>
        /// <param name="releaseLock">The release-operation lock.</param>
        /// <param name="installer">Fetches a release's inputs and records it.</param>
        /// <param name="builder">Generates a release's knowledge.</param>
        /// <param name="selector">Switches the default release.</param>
        /// <param name="remover">Removes a release.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ReleaseOperations(
            IReleaseOperationLock releaseLock,
            IReleaseInstaller installer,
            IKnowledgeBuilder<IKnowledgeGenerator> builder,
            IReleaseSelector selector,
            IReleaseRemover remover)
        {
            ArgumentNullException.ThrowIfNull(releaseLock);
            ArgumentNullException.ThrowIfNull(installer);
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentNullException.ThrowIfNull(remover);

            this.releaseLock = releaseLock;
            this.installer = installer;
            this.builder = builder;
            this.selector = selector;
            this.remover = remover;
        }

        /// <summary>
        /// Fetches a release's inputs, records it in the version manifest and generates every artifact
        /// for it, holding the lock throughout.
        /// </summary>
        /// <param name="request">Which release to install, and how.</param>
        /// <param name="progress">Reported per fetched file and per generated artifact; <c>null</c> for none.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock, the fetch and the generation.</param>
        /// <returns>What was fetched and what each artifact did.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The tag is not an offerable release tag.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task<ReleaseSetup> InstallAsync(
            ReleaseInstallRequest request,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            await using var lease = await this.releaseLock.AcquireAsync(cancellationToken);

            var installation = await this.installer.InstallAsync(request, cancellationToken, progress);
            var build = await this.builder.BuildAsync([installation.Tag], progress: progress, cancellationToken: cancellationToken);

            return new ReleaseSetup(installation, build);
        }

        /// <summary>Makes an installed release the default, holding the lock.</summary>
        /// <param name="tag">An installed release.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock.</param>
        /// <returns>The new default and the one before it.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is not installed.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task<ReleaseSelection> UseAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            await using var lease = await this.releaseLock.AcquireAsync(cancellationToken);

            return this.selector.Use(tag);
        }

        /// <summary>Removes an installed release, holding the lock.</summary>
        /// <param name="tag">The installed release to remove.</param>
        /// <param name="force">Removes the last installed release too.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock, and the removal before anything is changed.</param>
        /// <returns>A task that completes once the release is removed.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is not a release tag, or is not installed.</exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="tag"/> is the default, or the only installed release and <paramref name="force"/>
        /// was not given; or, as a <see cref="ReleaseOperationLockException"/>, the lock could not be taken.
        /// </exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            await using var lease = await this.releaseLock.AcquireAsync(cancellationToken);

            await this.remover.RemoveAsync(tag, force, cancellationToken);
        }
    }
}
