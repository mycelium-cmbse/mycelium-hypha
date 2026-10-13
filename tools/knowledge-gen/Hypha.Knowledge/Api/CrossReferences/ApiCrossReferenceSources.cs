// ------------------------------------------------------------------------------------------------
// <copyright file="ApiCrossReferenceSources.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>Which inputs were present.</summary>
    /// <param name="Pim">Always true: there are no cross-references without the PIM.</param>
    /// <param name="OpenApi">Whether the REST/HTTP PSM knowledge was generated.</param>
    /// <param name="Specification">Whether the specification text was generated.</param>
    public sealed record ApiCrossReferenceSources(bool Pim, bool OpenApi, bool Specification);
}
