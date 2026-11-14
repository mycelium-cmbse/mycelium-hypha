// ------------------------------------------------------------------------------------------------
// <copyright file="ISpecExtractProject.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Finds the <c>spec-extract</c> Python project the specification generators run through <c>uv</c>,
    /// putting it on disk first when the knowledge folder is not a source checkout.
    /// </summary>
    public interface ISpecExtractProject
    {
        /// <summary>Makes sure the project is on disk, and says where.</summary>
        /// <param name="cancellationToken">Cancels unpacking the project.</param>
        /// <returns>
        /// The project folder to hand to <c>uv run --project</c>: a source checkout's own
        /// <c>tools/spec-extract</c>, or the copy unpacked from this assembly under the toolchain folder.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">This build of hypha carries no copy, which is a build fault.</exception>
        /// <exception cref="System.OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        Task<DirectoryInfo> EnsureAsync(CancellationToken cancellationToken = default);
    }
}
