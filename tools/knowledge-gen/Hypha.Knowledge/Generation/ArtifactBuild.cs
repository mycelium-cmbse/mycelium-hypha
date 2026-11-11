// ------------------------------------------------------------------------------------------------
// <copyright file="ArtifactBuild.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    /// <summary>What one generator did for one release or API version, within a build.</summary>
    /// <param name="Tag">The release or API version.</param>
    /// <param name="Artifact">The generator's <see cref="IKnowledgeGenerator.Artifact"/>.</param>
    /// <param name="Result">What it wrote, or why it was skipped.</param>
    public sealed record ArtifactBuild(string Tag, string Artifact, GenerationResult Result);
}
