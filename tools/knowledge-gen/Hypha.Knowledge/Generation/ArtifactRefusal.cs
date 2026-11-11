// ------------------------------------------------------------------------------------------------
// <copyright file="ArtifactRefusal.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    /// <summary>
    /// A generator that refused because its inputs were present but its output would have been wrong
    /// (a missing metamodel index, a clause title reaching the cross-references, a slug collision).
    /// </summary>
    /// <param name="Tag">The release or API version it refused for.</param>
    /// <param name="Artifact">The generator's <see cref="IKnowledgeGenerator.Artifact"/>.</param>
    /// <param name="Reason">Why, as the generator said it.</param>
    public sealed record ArtifactRefusal(string Tag, string Artifact, string Reason);
}
