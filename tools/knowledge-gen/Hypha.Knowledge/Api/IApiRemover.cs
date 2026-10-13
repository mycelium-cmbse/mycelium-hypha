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
        /// <exception cref="System.ArgumentException">The version is not installed.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// It is the default while another is installed, or the only one and
        /// <paramref name="force"/> was not given.
        /// </exception>
        Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default);
    }
}
