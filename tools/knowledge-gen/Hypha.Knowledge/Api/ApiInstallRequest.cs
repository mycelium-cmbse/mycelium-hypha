// ------------------------------------------------------------------------------------------------
// <copyright file="ApiInstallRequest.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    /// <summary>
    /// What to install, and how.
    /// </summary>
    public sealed record ApiInstallRequest
    {
        /// <summary>Gets the API version to install.</summary>
        public required string Tag { get; init; }

        /// <summary>
        /// Gets a value indicating whether the specification PDF is fetched. It is OMG-copyrighted and
        /// lands in a git-ignored folder.
        /// </summary>
        public bool IncludeSpecification { get; init; }

        /// <summary>Gets a value indicating whether files already present are kept.</summary>
        public bool SkipExisting { get; init; } = true;

        /// <summary>Gets a value indicating whether this version becomes the default.</summary>
        public bool MakeDefault { get; init; } = true;
    }
}
