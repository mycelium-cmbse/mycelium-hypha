// ------------------------------------------------------------------------------------------------
// <copyright file="IApiRemover.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Removes one installed API version's sources and knowledge.
    /// </summary>
    public interface IApiRemover
    {
        /// <summary>Removes <paramref name="tag"/>.</summary>
        /// <param name="tag">The API version to remove.</param>
        /// <param name="force">
        /// Removes the last installed API version too. Without it, removing the only remaining version is
        /// refused.
        /// </param>
        /// <param name="cancellationToken">Cancels the removal.</param>
        /// <returns>A task that completes once the version is removed.</returns>
        /// <exception cref="System.ArgumentException">The version is not installed.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// It is the default while another is installed, or the only one and
        /// <paramref name="force"/> was not given.
        /// </exception>
        Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default);
    }
}
