// ------------------------------------------------------------------------------------------------
// <copyright file="ISurfaceForms.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System.Collections.Generic;

    /// <summary>
    /// Derives how a metaclass is written in the textual notation, from the metamodel's own naming
    /// convention rather than from a curated table.
    /// </summary>
    public interface ISurfaceForms
    {
        /// <summary>The surface form of a metaclass name, or <c>null</c> when it has none.</summary>
        /// <param name="metaclass">The metaclass name, e.g. <c>PartDefinition</c>.</param>
        /// <returns>How it is written in the notation, e.g. <c>part def</c>, or <c>null</c>.</returns>
        string? Of(string metaclass);

        /// <summary>Indexes every metaclass that has a surface form, ready to match against models.</summary>
        /// <param name="metaclasses">The metaclass names to consider; no other name is ever indexed.</param>
        /// <returns>The index of those that have a surface form.</returns>
        SurfaceFormIndex Index(IEnumerable<string> metaclasses);
    }
}
