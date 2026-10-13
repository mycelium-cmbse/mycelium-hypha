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

    /// <summary>A PIM type that is known by name only.</summary>
    public sealed record PimNamed(string Name);

    /// <summary>A PIM class.</summary>
    public sealed record PimClass(
        string Name,
        IReadOnlyList<PimAttribute>? OwnedAttributes,
        IReadOnlyList<PimAttribute>? InheritedAttributes,
        IReadOnlyList<PimNamed>? OwnedOperations);

    /// <summary>A PIM attribute; <see cref="InheritedFrom"/> is <c>null</c> when it is owned.</summary>
    public sealed record PimAttribute(string Name, int Lower, int Upper, bool IsDerived, string? InheritedFrom);
}
