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
    using System.Threading;
    using System.Threading.Tasks;

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
        /// <exception cref="System.ArgumentException">
        /// The tag is not an offerable release tag. Refusing early beats writing a
        /// <c>sources/2026-05-pre/</c> that nothing downstream will ever look at.
        /// </exception>
        Task<ReleaseInstallation> InstallAsync(
            ReleaseInstallRequest request, CancellationToken cancellationToken = default);
    }
}
