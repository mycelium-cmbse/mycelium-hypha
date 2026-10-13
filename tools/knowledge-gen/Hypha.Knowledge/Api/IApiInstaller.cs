// ------------------------------------------------------------------------------------------------
// <copyright file="IApiInstaller.cs" company="Starion Group S.A.">
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
    /// Fetches one API version's inputs into <c>sources/api/&lt;tag&gt;/</c> and records it.
    /// </summary>
    public interface IApiInstaller
    {
        /// <summary>Installs the requested API version.</summary>
        /// <exception cref="System.ArgumentException">The tag is not release-shaped.</exception>
        Task<ApiInstallation> InstallAsync(ApiInstallRequest request, CancellationToken cancellationToken = default);
    }
}
