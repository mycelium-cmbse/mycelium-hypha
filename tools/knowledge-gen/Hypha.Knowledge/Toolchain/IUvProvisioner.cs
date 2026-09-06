// ------------------------------------------------------------------------------------------------
// <copyright file="IUvProvisioner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Toolchain
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Ensures a cached, checksum-verified <c>uv</c> executable is available.</summary>
    /// <remarks>
    /// <c>uv</c> is the Python toolchain <c>SpecGenerator</c> runs <c>tools/spec-extract</c> through: it
    /// resolves or fetches a matching Python itself and installs that project's dependencies into a
    /// managed venv on demand, so an installed plugin never needs a pre-existing Python or a provisioned
    /// <c>.venv</c>.
    /// </remarks>
    public interface IUvProvisioner
    {
        /// <summary>Ensures <c>uv</c> is cached, downloading and verifying it if necessary.</summary>
        /// <returns>The cached executable, or <c>null</c> when it could not be provisioned.</returns>
        Task<FileInfo?> EnsureAsync(CancellationToken cancellationToken = default);
    }
}
