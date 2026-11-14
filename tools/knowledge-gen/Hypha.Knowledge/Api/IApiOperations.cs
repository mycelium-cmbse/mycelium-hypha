// ------------------------------------------------------------------------------------------------
// <copyright file="IApiOperations.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Installs, switches and removes Systems Modeling API versions on their own, each under the same
    /// release-operation lock the language releases take.
    /// </summary>
    /// <remarks>
    /// The API versions are an axis of their own, with their own manifest, but they share
    /// <c>HYPHA_HOME</c> and the lock with the releases, so one client's install never runs into another's.
    /// </remarks>
    public interface IApiOperations
    {
        /// <summary>
        /// Fetches an API version's inputs, records it and generates its knowledge, holding the lock
        /// throughout; only then, and only when no generator refused, does it become the default.
        /// </summary>
        /// <param name="request">Which API version to install, and how.</param>
        /// <param name="progress">Reported per fetched document and per generated artifact; <c>null</c> for none.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock, the fetch and the generation.</param>
        /// <returns>What was fetched and what each artifact did.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The tag is not an API version.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<ApiSetup> InstallAsync(
            ApiInstallRequest request,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>Makes an installed API version the default, holding the lock.</summary>
        /// <param name="tag">An installed API version.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock.</param>
        /// <returns>The new default and the one before it.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is not installed.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<ReleaseSelection> UseAsync(string tag, CancellationToken cancellationToken = default);

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
        Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default);
    }
}
