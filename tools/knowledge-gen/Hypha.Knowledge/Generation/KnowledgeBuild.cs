// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeBuild.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>What one run of <see cref="IKnowledgeBuilder{TGenerator}.BuildAsync"/> did.</summary>
    /// <param name="Artifacts">What each artifact did, in the order run.</param>
    /// <param name="Refusal">The generator that refused and stopped the run, or <c>null</c> when none did.</param>
    public sealed record KnowledgeBuild(IReadOnlyList<ArtifactBuild> Artifacts, ArtifactRefusal? Refusal)
    {
        /// <summary>Gets how many files were written, across every artifact.</summary>
        public int Written => this.Artifacts.Sum(artifact => artifact.Result.Written.Count);

        /// <summary>Gets how many artifacts were skipped for want of their inputs.</summary>
        public int Skipped => this.Artifacts.Count(artifact => artifact.Result.Outcome == GenerationOutcome.Skipped);
    }
}
