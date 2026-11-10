// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInventory.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;

    /// <summary>
    /// The releases and Systems Modeling API versions installed, the default of each, and which
    /// artifacts each has generated.
    /// </summary>
    /// <param name="Default">The release answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="Releases">The installed releases, newest first, each with the artifacts it has generated.</param>
    /// <param name="ApiDefault">The API version answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="ApiVersions">The installed API versions, newest first, each with the artifacts it has generated.</param>
    public sealed record ReleaseInventory(
        string? Default,
        IReadOnlyList<ReleaseStatus> Releases,
        string? ApiDefault,
        IReadOnlyList<ApiVersionStatus> ApiVersions);
}
