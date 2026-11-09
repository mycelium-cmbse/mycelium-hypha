// ------------------------------------------------------------------------------------------------
// <copyright file="Grounding.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    /// <summary>Where the facts of a query result come from, so an answer can say how far to trust it.</summary>
    /// <param name="Tier">
    /// <see cref="CrossReferences.Provenance.Normative"/> (specification text),
    /// <see cref="CrossReferences.Provenance.Model"/> (read from the metamodel) or
    /// <see cref="CrossReferences.Provenance.Derived"/> (a closure, a name match or a curated link).
    /// </param>
    /// <param name="Release">The release tag or API version that answered.</param>
    /// <param name="Source">
    /// The generated file read, relative to the output root (the folder holding <c>knowledge/</c>), with
    /// <c>/</c> separators: <c>knowledge/2026-05/metamodel/metamodel.json</c>, for example.
    /// </param>
    public sealed record Grounding(string Tier, string Release, string Source);
}
