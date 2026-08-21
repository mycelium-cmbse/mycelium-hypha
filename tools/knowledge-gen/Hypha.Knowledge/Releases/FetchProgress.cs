// ------------------------------------------------------------------------------------------------
// <copyright file="FetchProgress.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    /// <summary>
    /// One file's fetch completed, out of a known total for that kind of input.
    /// </summary>
    /// <param name="Kind">Which input this is progress for, e.g. <c>metamodel</c> or <c>textual</c>.</param>
    /// <param name="Done">
    /// How many of <paramref name="Total"/> have completed, counting a skipped-because-already-present
    /// file the same as a freshly downloaded one - both are one target finished, not zero.
    /// </param>
    /// <param name="Total">The full count for <paramref name="Kind"/>, known before the first download.</param>
    public readonly record struct FetchProgress(string Kind, int Done, int Total);
}
