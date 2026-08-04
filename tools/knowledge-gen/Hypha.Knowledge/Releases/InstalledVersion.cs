// ------------------------------------------------------------------------------------------------
// <copyright file="InstalledVersion.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// One release this checkout carries: the tag, and what that tag resolved to in each upstream.
    /// </summary>
    /// <param name="Tag">The release tag — the version identifier.</param>
    /// <param name="Release">The Release repository reference (specs, grammar, models).</param>
    /// <param name="Pilot">The Pilot-Implementation reference (the metamodel XMI).</param>
    public sealed record InstalledVersion(string Tag, UpstreamReference Release, UpstreamReference Pilot);
}
