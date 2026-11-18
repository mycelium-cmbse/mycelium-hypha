// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.ModelLibrary
{
    /// <summary>Coverage counts for one release's standard-library index.</summary>
    /// <param name="Files">The standard-library source files fetched for this release.</param>
    /// <param name="Declarations">Distinct qualified names indexed.</param>
    public sealed record ModelLibraryCounts(int Files, int Declarations);
}
