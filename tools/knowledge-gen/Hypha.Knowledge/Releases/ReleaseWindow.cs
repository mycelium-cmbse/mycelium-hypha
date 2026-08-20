// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseWindow.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Which installed releases fall outside the rolling window, given how many to keep.
    /// </summary>
    /// <remarks>
    /// A pure function of a version list, like <see cref="ReleaseTag"/> - the filesystem side
    /// (deleting what this names) is <see cref="IReleaseWindowEvictor"/>'s job.
    /// </remarks>
    public static class ReleaseWindow
    {
        /// <summary>The tags that fall outside the newest <paramref name="keep"/>, oldest first.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="keep"/> is less than one - a window that keeps nothing is not a window.
        /// </exception>
        public static IReadOnlyList<string> Evicted(IReadOnlyList<InstalledVersion> versions, int keep)
        {
            ArgumentNullException.ThrowIfNull(versions);

            if (keep < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(keep), keep, "the window must keep at least one release");
            }

            return versions
                .OrderByDescending(version => ReleaseTag.SortKey(version.Tag))
                .Skip(keep)
                .Select(version => version.Tag)
                .Reverse()
                .ToList();
        }
    }
}
