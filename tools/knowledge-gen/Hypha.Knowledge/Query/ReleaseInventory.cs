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

    /// <summary>The releases and Systems Modeling API versions installed, and the default of each.</summary>
    /// <param name="Default">The release answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="Releases">The installed release tags, newest first.</param>
    /// <param name="ApiDefault">The API version answered from by default, or <c>null</c> when none is installed.</param>
    /// <param name="ApiVersions">The installed API versions, newest first.</param>
    public sealed record ReleaseInventory(string? Default, IReadOnlyList<string> Releases, string? ApiDefault,
        IReadOnlyList<string> ApiVersions);
}
