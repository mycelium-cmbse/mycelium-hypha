// ------------------------------------------------------------------------------------------------
// <copyright file="LeftOutRecord.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System.Collections.Generic;

    /// <summary>
    /// The contents of <see cref="LeftOutFile.FileName"/>: every version a migration into this
    /// <c>HYPHA_HOME</c> left out.
    /// </summary>
    /// <param name="Versions">The versions left out, by kind and then tag.</param>
    public sealed record LeftOutRecord(IReadOnlyList<LeftOutVersion> Versions);
}
