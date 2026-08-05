// ------------------------------------------------------------------------------------------------
// <copyright file="GenerationOutcome.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    /// <summary>
    /// What became of one artifact for one release.
    /// </summary>
    /// <remarks>
    /// There is deliberately no <c>Failed</c>. A generator that cannot produce correct output throws:
    /// a caller has to opt into ignoring an exception, where it could easily overlook a status. See
    /// the cold-start guard in <see cref="CrossReferences.CrossReferenceBuilder"/> for the case that
    /// forced the distinction.
    /// </remarks>
    public enum GenerationOutcome
    {
        /// <summary>The artifact was written.</summary>
        Generated,

        /// <summary>
        /// Its inputs are not present, which is normal - a release whose sources were never fetched,
        /// or specifications the user does not have.
        /// </summary>
        Skipped,
    }
}
