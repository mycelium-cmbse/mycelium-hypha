// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramClass.cs" company="Starion Group S.A.">
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
    /// A metaclass node of a package diagram: one of the package's own metaclasses, with the
    /// attributes shown inside its box.
    /// </summary>
    public sealed class DiagramClass
    {
        /// <summary>Gets the metaclass name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets a value indicating whether the metaclass is abstract.</summary>
        public required bool IsAbstract { get; init; }

        /// <summary>Gets the attributes rendered inside the class box, ordered by name.</summary>
        public required IReadOnlyList<DiagramAttribute> Attributes { get; init; }

        /// <summary>
        /// Gets a value indicating whether the class needs a braced body — either to carry the
        /// <c>&lt;&lt;abstract&gt;&gt;</c> annotation or because it has attributes to show.
        /// </summary>
        public bool HasBody => this.IsAbstract || this.Attributes.Count > 0;
    }
}
