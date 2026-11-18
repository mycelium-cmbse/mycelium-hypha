// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Migration;

    /// <summary>
    /// The releases and Systems Modeling API versions installed, the default of each, and which
    /// artifacts each has generated.
    /// </summary>
    /// <param name="Default">The release answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="Releases">The installed releases, newest first, each with the artifacts it has generated.</param>
    /// <param name="ApiDefault">The API version answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="ApiVersions">The installed API versions, newest first, each with the artifacts it has generated.</param>
    /// <param name="LeftOut">
    /// The releases and API versions a client had installed that were not copied into this folder, because
    /// they are in an older format and would answer wrongly, and that are still not installed here: each is
    /// to be installed again. Empty when there are none.
    /// </param>
    public sealed record ReleaseInventory(
        string? Default,
        IReadOnlyList<ReleaseStatus> Releases,
        string? ApiDefault,
        IReadOnlyList<ApiVersionStatus> ApiVersions,
        IReadOnlyList<LeftOutVersion> LeftOut);
}
