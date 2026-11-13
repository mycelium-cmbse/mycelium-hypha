// ------------------------------------------------------------------------------------------------
// <copyright file="ILegacyInstallLocator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System.Collections.Generic;

    /// <summary>
    /// Finds the knowledge folders the clients kept for themselves before <c>HYPHA_HOME</c>.
    /// </summary>
    public interface ILegacyInstallLocator
    {
        /// <summary>Finds every such folder on this machine that has installed something.</summary>
        /// <returns>
        /// The folders holding a <c>knowledge/versions.json</c>: each version of the Claude plugin in the
        /// plugin cache, and the VS Code extension's global storage of each VS Code build; in no particular order.
        /// </returns>
        IReadOnlyList<LegacyInstall> Locate();
    }
}
