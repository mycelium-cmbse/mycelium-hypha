// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseDiff.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System.Collections.Generic;

    /// <summary>What changed between two installed releases.</summary>
    /// <param name="From">The older release, compared from.</param>
    /// <param name="To">The newer release, compared to.</param>
    /// <param name="Metamodel">
    /// The changes to the metamodel's metaclasses, enumerations and primitive types and to what they own,
    /// sorted by element, member, kind and then field.
    /// </param>
    public sealed record ReleaseDiff(DiffSide From, DiffSide To, IReadOnlyList<MetamodelChange> Metamodel);
}
