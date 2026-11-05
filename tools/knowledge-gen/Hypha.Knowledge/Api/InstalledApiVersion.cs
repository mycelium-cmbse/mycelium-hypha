// ------------------------------------------------------------------------------------------------
// <copyright file="InstalledApiVersion.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// One API version this checkout carries: the tag, and what it resolved to in each upstream.
    /// </summary>
    /// <param name="Tag">The API-Services tag - the version identifier.</param>
    /// <param name="Services">The API-Services reference (the OpenAPI document).</param>
    /// <param name="Release">The Release repository reference (the specification PDF).</param>
    /// <param name="Pilot">The Pilot-Implementation reference (the KerML model the PIM refers to).</param>
    /// <param name="Pim">The OMG document identifier of the PIM model.</param>
    public sealed record InstalledApiVersion(
        string Tag, UpstreamReference Services, UpstreamReference Release, UpstreamReference Pilot, string Pim);
}
