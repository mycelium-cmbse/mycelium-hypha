// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>One row of the specification's PIM-to-PSM model mapping table.</summary>
    /// <param name="Pim">The PIM concept, as written: a type, or <c>Type.member</c>.</param>
    /// <param name="Psm">The PSM model, as written.</param>
    /// <param name="Clause">The clause holding the table.</param>
    public sealed record ApiModelMappingRow(string Pim, string Psm, string Clause);
}
