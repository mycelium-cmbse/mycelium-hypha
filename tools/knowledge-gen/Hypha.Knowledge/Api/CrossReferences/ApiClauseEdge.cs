// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>A clause and how it was linked.</summary>
    /// <param name="Clause">The clause number.</param>
    /// <param name="Method">What links it; see the method constants on the document.</param>
    public sealed record ApiClauseEdge(string Clause, string Method);
}
