// ------------------------------------------------------------------------------------------------
// <copyright file="ArtifactAnswer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    /// <summary>
    /// What generating one artifact did during an install.
    /// </summary>
    /// <param name="Artifact">The artifact, such as <c>metamodel</c> or <c>spec</c>.</param>
    /// <param name="Outcome"><c>generated</c>, <c>skipped</c> (its inputs were absent) or <c>refused</c>.</param>
    /// <param name="Files">How many files it wrote.</param>
    /// <param name="Reason">Why it was skipped or refused; <c>null</c> when it was generated.</param>
    internal sealed record ArtifactAnswer(string Artifact, string Outcome, int Files, string? Reason);
}
