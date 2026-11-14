// ------------------------------------------------------------------------------------------------
// <copyright file="UseAnswer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    /// <summary>
    /// What <c>hypha_use_release</c> did.
    /// </summary>
    /// <param name="Default">The release that now answers by default.</param>
    /// <param name="Previous">The release that answered by default before.</param>
    /// <param name="Changed">Whether the default changed, rather than already being <paramref name="Default"/>.</param>
    /// <param name="Axis"><c>release</c> or <c>api</c>: what <see cref="Default"/> and <see cref="Previous"/> name.</param>
    internal sealed record UseAnswer(string Default, string Previous, bool Changed, string Axis = Axes.Release);
}
