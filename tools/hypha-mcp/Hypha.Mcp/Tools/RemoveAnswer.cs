// ------------------------------------------------------------------------------------------------
// <copyright file="RemoveAnswer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    /// <summary>
    /// What <c>hypha_remove_release</c> did.
    /// </summary>
    /// <param name="Removed">The release whose sources and knowledge were deleted.</param>
    internal sealed record RemoveAnswer(string Removed);
}
