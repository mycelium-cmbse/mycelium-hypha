// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseInstaller.cs" company="Starion Group S.A.">
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
    /// Fetches a release's inputs into <c>sources/</c> and records it in the version manifest.
    /// </summary>
    /// <remarks>
    /// The counterpart of <see cref="Generation.IKnowledgeGenerator"/>: one gets the inputs, the other
    /// turns them into the knowledge base. Both are resolved rather than constructed, so the CLI and
    /// the tests drive the same code.
    /// </remarks>
    public interface IReleaseInstaller
    {
        /// <summary>
        /// Installs one release.
        /// </summary>
        /// <param name="request">Which release to install, and how.</param>
        /// <param name="cancellationToken">Cancels the fetch and the commit resolution.</param>
        /// <param name="progress">
        /// Reported once per fetched file, as a <see cref="KnowledgeProgress.Fetch"/> step per kind of
        /// input: <c>metamodel</c>, <c>textual</c> and, when requested, <c>specifications</c>. <c>null</c>
        /// when nobody needs to show progress.
        /// </param>
        /// <returns>What was fetched, the version recorded, and the manifest it was recorded in.</returns>
        /// <exception cref="System.ArgumentException">
        /// The tag is not an offerable release tag. Refusing early beats writing a
        /// <c>sources/2026-05-pre/</c> that nothing downstream will ever look at.
        /// </exception>
        Task<ReleaseInstallation> InstallAsync(
            ReleaseInstallRequest request,
            CancellationToken cancellationToken = default,
            IProgress<KnowledgeProgress>? progress = null);
    }
}
