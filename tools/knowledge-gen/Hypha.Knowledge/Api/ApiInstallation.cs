// ------------------------------------------------------------------------------------------------
// <copyright file="ApiInstallation.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// What installing an API version produced.
    /// </summary>
    /// <param name="Tag">The API version.</param>
    /// <param name="Files">The inputs now on disk, downloaded or already there.</param>
    /// <param name="Version">What the tag resolved to in each upstream.</param>
    /// <param name="Manifest">The manifest it was recorded in.</param>
    public sealed record ApiInstallation(
        string Tag, IReadOnlyList<FileInfo> Files, InstalledApiVersion Version, FileInfo Manifest);
}
