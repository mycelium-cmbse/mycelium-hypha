// ------------------------------------------------------------------------------------------------
// <copyright file="LegacyInstall.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System.IO;

    /// <summary>
    /// A knowledge folder a client kept for itself before <c>HYPHA_HOME</c>: the Claude plugin's own
    /// folder, or the VS Code extension's global storage.
    /// </summary>
    /// <param name="Root">The folder holding its <c>knowledge/</c> and <c>sources/</c>.</param>
    /// <param name="Origin">What kept it, for a person, such as <c>Claude plugin 2.1.2</c> or <c>VS Code</c>.</param>
    public sealed record LegacyInstall(DirectoryInfo Root, string Origin);
}
