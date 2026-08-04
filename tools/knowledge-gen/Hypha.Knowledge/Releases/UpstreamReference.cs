// ------------------------------------------------------------------------------------------------
// <copyright file="UpstreamReference.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// Which upstream commit one repository's tag resolved to, recorded for traceability only.
    /// </summary>
    /// <remarks>
    /// The <b>tag</b> is the version identifier. The commit says what that tag pointed at when the
    /// release was generated, so a regeneration can be traced, but nothing selects on it.
    /// </remarks>
    /// <param name="Repo">The upstream repository, as <c>owner/name</c>.</param>
    /// <param name="Commit">The commit SHA the tag resolved to.</param>
    public sealed record UpstreamReference(string Repo, string Commit);
}
