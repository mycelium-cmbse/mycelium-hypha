// ------------------------------------------------------------------------------------------------
// <copyright file="ApiSetup.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using Hypha.Knowledge.Generation;

    /// <summary>
    /// What installing an API version did: the inputs it fetched, the knowledge generated from them, and
    /// whether the version now answers by default.
    /// </summary>
    /// <param name="Installation">The fetched inputs and the version recorded in the manifest.</param>
    /// <param name="Build">
    /// What each artifact did. A generator that refused stops the build, and <see cref="KnowledgeBuild.Refusal"/>
    /// names it; the inputs stay fetched and recorded either way.
    /// </param>
    /// <param name="IsDefault">
    /// Whether the version is the default API version once the install is done: as asked, when nothing
    /// refused, and always for the first API version installed.
    /// </param>
    public sealed record ApiSetup(ApiInstallation Installation, KnowledgeBuild Build, bool IsDefault);
}
