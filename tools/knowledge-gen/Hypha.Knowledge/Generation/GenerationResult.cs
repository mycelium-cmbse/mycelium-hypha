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
        /// <param name="written">The files written.</param>
        /// <returns>A <see cref="GenerationOutcome.Generated"/> result with no reason.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="written"/> is <c>null</c>.</exception>
        public static GenerationResult Generated(IReadOnlyList<FileInfo> written)
        {
            ArgumentNullException.ThrowIfNull(written);

            return new GenerationResult(GenerationOutcome.Generated, written);
        }

        /// <summary>
        /// The inputs are not present. <paramref name="reason"/> is shown to whoever asked for the
        /// generation, so it should name what is missing rather than restating that something is.
        /// </summary>
        /// <param name="reason">Why nothing was written, in words a user can act on.</param>
        /// <returns>A <see cref="GenerationOutcome.Skipped"/> result with no written files.</returns>
        /// <exception cref="ArgumentException"><paramref name="reason"/> is <c>null</c>, empty or white space.</exception>
        public static GenerationResult Skipped(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            return new GenerationResult(GenerationOutcome.Skipped, [], reason);
        }
    }
}
