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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// What <c>hypha_install_release</c> did, as the model reads it.
    /// </summary>
    /// <param name="Tag">The release or API version installed.</param>
    /// <param name="IsDefault">
    /// Whether it is now the default on its axis: as asked when nothing refused, and always for the first
    /// one installed.
    /// </param>
    /// <param name="Fetched">How many input files were fetched, by kind.</param>
    /// <param name="Artifacts">What each artifact did, in the order generated.</param>
    /// <param name="Refusal">
    /// The artifact that refused because its output would have been wrong, which stopped the generation;
    /// <c>null</c> when none did.
    /// </param>
    /// <param name="Axis"><c>release</c> or <c>api</c>: what <see cref="Tag"/> names.</param>
    internal sealed record InstallAnswer(
        string Tag,
        bool IsDefault,
        IReadOnlyDictionary<string, int> Fetched,
        IReadOnlyList<ArtifactAnswer> Artifacts,
        ArtifactAnswer? Refusal,
        string Axis = Axes.Release)
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
                Generated(setup.Build),
                Refused(setup.Build));
        }

        /// <summary>The answer for a finished install of an API version.</summary>
        /// <param name="setup">What the install did.</param>
        /// <returns>The answer, with the documents fetched counted as one kind.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="setup"/> is <c>null</c>.</exception>
        public static InstallAnswer From(ApiSetup setup)
        {
            ArgumentNullException.ThrowIfNull(setup);

            return new InstallAnswer(
                setup.Installation.Tag,
                setup.IsDefault,
                new SortedDictionary<string, int>(StringComparer.Ordinal) { ["documents"] = setup.Installation.Files.Count },
                Generated(setup.Build),
                Refused(setup.Build),
                Axes.Api);
        }

        /// <summary>What each artifact did, in the order generated.</summary>
        /// <param name="build">The build.</param>
        /// <returns>One answer per artifact.</returns>
        private static List<ArtifactAnswer> Generated(KnowledgeBuild build) =>
            [.. build.Artifacts.Select(artifact => new ArtifactAnswer(
                artifact.Artifact,
                artifact.Result.Outcome == GenerationOutcome.Generated ? "generated" : "skipped",
                artifact.Result.Written.Count,
                artifact.Result.Reason))];

        /// <summary>The artifact that refused, which stopped the build.</summary>
        /// <param name="build">The build.</param>
        /// <returns>The refusal, or <c>null</c> when none did.</returns>
        private static ArtifactAnswer? Refused(KnowledgeBuild build) =>
            build.Refusal is { } refusal ? new ArtifactAnswer(refusal.Artifact, "refused", 0, refusal.Reason) : null;
    }
}
