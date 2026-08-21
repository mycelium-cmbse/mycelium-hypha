// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseRemover.cs" company="Starion Group S.A.">
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
    /// Removes one installed release the user no longer wants, on request.
    /// </summary>
    /// <remarks>
    /// The single-tag counterpart of <see cref="IReleaseWindowEvictor"/>, which shrinks the installed
    /// set back to a rolling window automatically. This is never automatic: it only ever runs because
    /// something - a user, through a skill - asked for exactly this tag to go.
    /// </remarks>
    public interface IReleaseRemover
    {
        /// <summary>
        /// Removes <paramref name="tag"/>: deletes its <c>sources/</c> and <c>knowledge/</c> folders
        /// and drops it from the version manifest.
        /// </summary>
        /// <param name="force">
        /// Removes the last installed release too. Without it, removing the only remaining release is
        /// refused - it would leave nothing for any skill to answer from.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="tag"/> is not installed.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// <paramref name="tag"/> is the manifest's current default (switch with <c>hypha use</c>
        /// first), or it is the only installed release and <paramref name="force"/> was not given.
        /// </exception>
        Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default);
    }
}
