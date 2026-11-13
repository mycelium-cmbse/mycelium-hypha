// ------------------------------------------------------------------------------------------------
// <copyright file="InstallAnswer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// What <c>hypha_install_release</c> did, as the model reads it.
    /// </summary>
    /// <param name="Tag">The release installed.</param>
    /// <param name="IsDefault">
    /// Whether it is now the default release: as asked when nothing refused, and always for the first
    /// release installed.
    /// </param>
    /// <param name="Fetched">How many input files were fetched, by kind.</param>
    /// <param name="Artifacts">What each artifact did, in the order generated.</param>
    /// <param name="Refusal">
    /// The artifact that refused because its output would have been wrong, which stopped the generation;
    /// <c>null</c> when none did.
    /// </param>
    internal sealed record InstallAnswer(
        string Tag,
        bool IsDefault,
        IReadOnlyDictionary<string, int> Fetched,
        IReadOnlyList<ArtifactAnswer> Artifacts,
        ArtifactAnswer? Refusal)
    {
        /// <summary>The answer for a finished install.</summary>
        /// <param name="setup">What the install did.</param>
        /// <returns>The answer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="setup"/> is <c>null</c>.</exception>
        public static InstallAnswer From(ReleaseSetup setup)
        {
            ArgumentNullException.ThrowIfNull(setup);

            var installation = setup.Installation;

            return new InstallAnswer(
                installation.Tag,
                setup.IsDefault,
                new SortedDictionary<string, int>(StringComparer.Ordinal)
                {
                    ["metamodel"] = installation.Metamodel.Count,
                    ["textual"] = installation.Textual.Count,
                    ["specifications"] = installation.Specifications.Count,
                },
                setup.Build.Artifacts
                    .Select(artifact => new ArtifactAnswer(
                        artifact.Artifact,
                        artifact.Result.Outcome == GenerationOutcome.Generated ? "generated" : "skipped",
                        artifact.Result.Written.Count,
                        artifact.Result.Reason))
                    .ToList(),
                setup.Build.Refusal is { } refusal
                    ? new ArtifactAnswer(refusal.Artifact, "refused", 0, refusal.Reason)
                    : null);
        }
    }
}
