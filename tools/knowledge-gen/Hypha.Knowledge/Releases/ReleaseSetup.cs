// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSetup.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using Hypha.Knowledge.Generation;

    /// <summary>
    /// What installing a release did: the inputs it fetched, and the knowledge generated from them.
    /// </summary>
    /// <param name="Installation">The fetched inputs and the version recorded in the manifest.</param>
    /// <param name="Build">
    /// What each artifact did. A generator that refused stops the build, and <see cref="KnowledgeBuild.Refusal"/>
    /// names it; the inputs stay fetched and recorded either way.
    /// </param>
    public sealed record ReleaseSetup(ReleaseInstallation Installation, KnowledgeBuild Build);
}
