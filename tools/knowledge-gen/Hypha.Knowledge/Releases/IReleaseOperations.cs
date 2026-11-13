// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseOperations.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Progress;

    /// <summary>
    /// Installs, switches and removes releases on their own, each under the release-operation lock, so
    /// that several hosts sharing one knowledge folder change it one at a time.
    /// </summary>
    /// <remarks>
    /// The MCP server's release tools run through here. The CLI's <c>fetch</c>, <c>generate</c>,
    /// <c>use</c> and <c>remove</c> verbs deliberately do not, since the VS Code extension runs them
    /// while it holds the lock itself (see <see cref="IReleaseOperationLock"/>).
    /// </remarks>
    public interface IReleaseOperations
    {
        /// <summary>
        /// Fetches a release's inputs, records it in the version manifest and generates every artifact
        /// for it, holding the lock throughout.
        /// </summary>
        /// <param name="request">Which release to install, and how.</param>
        /// <param name="progress">
        /// Reported per fetched file and per generated artifact (<see cref="KnowledgeProgress.Fetch"/>,
        /// then <see cref="KnowledgeProgress.Generate"/>); <c>null</c> when nobody needs to show progress.
        /// </param>
        /// <param name="cancellationToken">Cancels the wait for the lock, the fetch and the generation.</param>
        /// <returns>What was fetched and what each artifact did.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The tag is not an offerable release tag.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<ReleaseSetup> InstallAsync(
            ReleaseInstallRequest request,
            IProgress<KnowledgeProgress>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>Makes an installed release the default, holding the lock.</summary>
        /// <param name="tag">An installed release.</param>
        /// <param name="cancellationToken">Cancels the wait for the lock.</param>
        /// <returns>The new default and the one before it.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is not installed.</exception>
        /// <exception cref="ReleaseOperationLockException">The lock could not be taken; the message says why.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<ReleaseSelection> UseAsync(string tag, CancellationToken cancellationToken = default);

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
        Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default);
    }
}
