// ------------------------------------------------------------------------------------------------
// <copyright file="GenerationResult.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// What one generator did for one release.
    /// </summary>
    /// <param name="Outcome">Whether the artifact was written or skipped.</param>
    /// <param name="Written">The files written; empty when skipped.</param>
    /// <param name="Reason">Why it was skipped, in words a user can act on; <c>null</c> otherwise.</param>
    public sealed record GenerationResult(
        GenerationOutcome Outcome,
        IReadOnlyList<FileInfo> Written,
        string? Reason = null)
    {
        /// <summary>The artifact was written.</summary>
        public static GenerationResult Generated(IReadOnlyList<FileInfo> written)
        {
            ArgumentNullException.ThrowIfNull(written);

            return new GenerationResult(GenerationOutcome.Generated, written);
        }

        /// <summary>
        /// The inputs are not present. <paramref name="reason"/> is shown to whoever asked for the
        /// generation, so it should name what is missing rather than restating that something is.
        /// </summary>
        public static GenerationResult Skipped(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            return new GenerationResult(GenerationOutcome.Skipped, [], reason);
        }
    }
}
