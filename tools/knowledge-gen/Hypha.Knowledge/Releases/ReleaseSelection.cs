// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSelection.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;

    /// <summary>
    /// The default release after a switch, and the one before it.
    /// </summary>
    /// <param name="Default">The release that now answers by default.</param>
    /// <param name="Previous">The release that answered by default before the switch.</param>
    public sealed record ReleaseSelection(string Default, string Previous)
    {
        /// <summary>Gets a value indicating whether the default changed, rather than already being <see cref="Default"/>.</summary>
        public bool Changed => !string.Equals(this.Default, this.Previous, StringComparison.Ordinal);
    }
}
