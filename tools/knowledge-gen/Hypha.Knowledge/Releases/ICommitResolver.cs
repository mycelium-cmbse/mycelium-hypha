// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Releases
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Resolves a tag to the commit it points at, for the record kept in the manifest.
    /// </summary>
    /// <remarks>
    /// Traceability only. The <b>tag</b> is the version identifier throughout.
    /// </remarks>
    public interface ICommitResolver
    {
        /// <summary>The commit a tag resolves to in one repository.</summary>
        Task<string> ResolveAsync(
            string repository, string tag, CancellationToken cancellationToken = default);

        /// <summary>The tag resolved in both upstreams, ready to record.</summary>
        Task<InstalledVersion> ResolveVersionAsync(
            string tag, CancellationToken cancellationToken = default);
    }
}
