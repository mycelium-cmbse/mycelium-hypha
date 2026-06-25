// ------------------------------------------------------------------------------------------------
// <copyright file="PackageGroup.cs" company="Starion Group S.A.">
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
    /// A package and its contained metaclasses, as rendered in the index.
    /// </summary>
    public sealed class PackageGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageGroup"/> class.
        /// </summary>
        public PackageGroup(string name, IReadOnlyList<IndexEntry> metaclasses)
        {
            this.Name = name;
            this.Metaclasses = metaclasses;
        }

        /// <summary>Gets the package name.</summary>
        public string Name { get; }

        /// <summary>Gets the metaclasses contained in the package, ordered by name.</summary>
        public IReadOnlyList<IndexEntry> Metaclasses { get; }
    }
}
