// ------------------------------------------------------------------------------------------------
// <copyright file="PackageDiagramPayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    using System.Collections.Generic;

    /// <summary>
    /// The rendering model for one package's class diagram: the package's own metaclasses, the
    /// one-hop boundary nodes pulled in from other packages, and the edges between them.
    /// </summary>
    public sealed class PackageDiagramPayload
    {
        /// <summary>Gets the package name.</summary>
        public required string Package { get; init; }

        /// <summary>Gets the package's own metaclasses, ordered by name.</summary>
        public required IReadOnlyList<DiagramClass> Classes { get; init; }

        /// <summary>
        /// Gets the names of elements outside the package that an edge reaches, ordered by name.
        /// These are drawn as bare nodes so the hierarchy is not misrepresented, and are never
        /// expanded further.
        /// </summary>
        public required IReadOnlyList<string> BoundaryClasses { get; init; }

        /// <summary>Gets the diagram edges in a deterministic order.</summary>
        public required IReadOnlyList<DiagramRelationship> Relationships { get; init; }

        /// <summary>Gets the number of metaclasses owned by the package.</summary>
        public int ClassCount => this.Classes.Count;

        /// <summary>Gets the number of boundary nodes drawn from other packages.</summary>
        public int BoundaryCount => this.BoundaryClasses.Count;
    }
}
