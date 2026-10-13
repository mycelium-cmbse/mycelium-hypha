// ------------------------------------------------------------------------------------------------
// <copyright file="PimGraph.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// What the cross-references read of <c>pim/metamodel.json</c>: the PIM types, their effective
    /// attributes and their operations.
    /// </summary>
    /// <remarks>
    /// A reading model, not the document's full shape - that belongs to the metamodel generator, which
    /// this library does not depend on. Members the cross-references do not use are left out and
    /// ignored on reading.
    /// </remarks>
    public sealed record PimGraph(
        IReadOnlyList<PimClass>? Classes,
        IReadOnlyList<PimNamed>? Interfaces,
        IReadOnlyList<PimNamed>? Enumerations,
        IReadOnlyList<PimNamed>? DataTypes);
}
