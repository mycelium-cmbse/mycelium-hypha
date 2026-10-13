// ------------------------------------------------------------------------------------------------
// <copyright file="PimNamed.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    /// <summary>A PIM type that is known by name only.</summary>
    public sealed record PimNamed(string Name);
}
