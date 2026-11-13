// ------------------------------------------------------------------------------------------------
// <copyright file="IReleaseSelector.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// Switches which installed release answers by default.
    /// </summary>
    /// <remarks>
    /// The CLI's <c>use</c> verb and the MCP server's <c>hypha_use_release</c> both switch through here,
    /// so the rule for what may become the default is written once. It takes no lock itself; see
    /// <see cref="IReleaseOperations"/> for the switch that does.
    /// </remarks>
    public interface IReleaseSelector
    {
        /// <summary>Makes <paramref name="tag"/> the default release.</summary>
        /// <param name="tag">An installed release.</param>
        /// <returns>
        /// The new default and the one before it. When <paramref name="tag"/> already was the default,
        /// nothing is written and <see cref="ReleaseSelection.Changed"/> is <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, or is not installed; the message
        /// names the installed releases, or says that nothing is installed.
        /// </exception>
        ReleaseSelection Use(string tag);
    }
}
