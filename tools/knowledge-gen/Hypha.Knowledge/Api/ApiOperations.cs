// ------------------------------------------------------------------------------------------------
// <copyright file="ApiOperations.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Installs, switches and removes Systems Modeling API versions on their own, each under the
    /// release-operation lock.
    /// </summary>
    public sealed class ApiOperations : IApiOperations
    {
        /// <summary>The release-operation lock, shared with the language releases.</summary>
        private readonly IReleaseOperationLock releaseLock;

        /// <summary>Fetches an API version's inputs and records it.</summary>
        private readonly IApiInstaller installer;

        /// <summary>Generates an API version's knowledge.</summary>
        private readonly IKnowledgeBuilder<IApiKnowledgeGenerator> builder;

        /// <summary>Switches the default API version.</summary>
        private readonly IApiSelector selector;

        /// <summary>Removes an API version.</summary>
        private readonly IApiRemover remover;

        /// <summary>Initializes a new instance of the <see cref="ApiOperations"/> class.</summary>
        /// <param name="releaseLock">The release-operation lock, shared with the language releases.</param>
        /// <param name="installer">Fetches an API version's inputs and records it.</param>
        /// <param name="builder">Generates an API version's knowledge.</param>
        /// <param name="selector">Switches the default API version.</param>
        /// <param name="remover">Removes an API version.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ApiOperations(
            IReleaseOperationLock releaseLock,
            IApiInstaller installer,
            IKnowledgeBuilder<IApiKnowledgeGenerator> builder,
            IApiSelector selector,
            IApiRemover remover)
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
        /// Fetches an API version's inputs, records it in the API version manifest and generates every
        /// artifact for it, holding the lock throughout; only then, and only when no generator refused,
        /// does it become the default.
        /// </summary>
        /// <param name="request">
        /// Which API version to install, and how. <see cref="ApiInstallRequest.MakeDefault"/> is applied
        /// once the knowledge is generated, not when the inputs are recorded.
        /// </param>
        /// <param name="progress">Reported per fetched document and per generated artifact; <c>null</c> for none.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock, the fetch and the generation.</param>
        /// <returns>What was fetched and what each artifact did.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The tag is not an API version.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task<ApiSetup> InstallAsync(
            ApiInstallRequest request,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            await using var lease = await this.releaseLock.AcquireAsync(cancellationToken);

            // Recorded without becoming the default, so no client answers from a version with no
            // knowledge yet while it generates, or after its generation failed.
            var installation = await this.installer.InstallAsync(request with { MakeDefault = false }, cancellationToken, progress);
            var build = await this.builder.BuildAsync([installation.Tag], progress: progress, cancellationToken: cancellationToken);

            if (request.MakeDefault && build.Refusal is null)
            {
                this.selector.Use(installation.Tag);
            }

            var manifest = ApiVersionManifestFile.ReadIfPresent(installation.Manifest.FullName);

            return new ApiSetup(installation, build, string.Equals(manifest?.Default, installation.Tag, StringComparison.Ordinal));
        }

        /// <summary>Makes an installed API version the default, holding the lock.</summary>
        /// <param name="tag">An installed API version.</param>
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

        /// <summary>Removes an installed API version, holding the lock.</summary>
        /// <param name="tag">The installed API version to remove.</param>
        /// <param name="force">Removes the last installed API version too.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock, and the removal before anything is changed.</param>
        /// <returns>A task that completes once the API version is removed.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is not an API version, or is not installed.</exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="tag"/> is the default, or the only installed API version and <paramref name="force"/>
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
