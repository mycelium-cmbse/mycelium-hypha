// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInstallRequest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// What to fetch for one release, and how to record it.
    /// </summary>
    /// <remarks>
    /// A request object rather than a run of optional boolean parameters: every one of these is a
    /// per-invocation choice the CLI exposes as a flag, so they do not belong on
    /// <see cref="HyphaKnowledgeOptions"/> either.
    /// </remarks>
    public sealed record ReleaseInstallRequest
    {
        /// <summary>Gets the release tag to install.</summary>
        public required string Tag { get; init; }

        /// <summary>
        /// Gets a value indicating whether the OMG specification PDFs are fetched too.
        /// </summary>
        /// <remarks>
        /// Off by default. The PDFs are OMG-copyrighted, land in a git-ignored folder and are only
        /// needed by whoever wants spec citation, so fetching them is an explicit choice.
        /// </remarks>
        public bool IncludeSpecifications { get; init; }

        /// <summary>
        /// Gets a value indicating whether files already present are left alone.
        /// </summary>
        /// <remarks>
        /// On by default, which is what makes an interrupted fetch resumable. A zero-byte file counts
        /// as interrupted rather than present, so it is fetched again.
        /// </remarks>
        public bool SkipExisting { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether this release becomes the one answered from by default.
        /// </summary>
        /// <remarks>
        /// On by default: installing a release and not being able to ask it anything would be a
        /// surprise. The first release installed is the default regardless.
        /// </remarks>
        public bool MakeDefault { get; init; } = true;
    }
}
