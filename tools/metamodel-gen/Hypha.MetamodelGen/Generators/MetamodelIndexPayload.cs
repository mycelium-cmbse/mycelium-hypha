// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelIndexPayload.cs" company="Starion Group S.A.">
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
    /// The Handlebars context for the metamodel index template.
    /// </summary>
    public sealed class MetamodelIndexPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetamodelIndexPayload"/> class.
        /// </summary>
        public MetamodelIndexPayload(string title, int metaclassCount, int packageCount, IReadOnlyList<PackageGroup> packages)
        {
            this.Title = title;
            this.MetaclassCount = metaclassCount;
            this.PackageCount = packageCount;
            this.Packages = packages;
        }

        /// <summary>Gets the human-readable metamodel title (e.g. "SysML v2").</summary>
        public string Title { get; }

        /// <summary>Gets the total number of metaclasses listed.</summary>
        public int MetaclassCount { get; }

        /// <summary>Gets the number of packages that contain at least one metaclass.</summary>
        public int PackageCount { get; }

        /// <summary>Gets the package groups, ordered for deterministic output.</summary>
        public IReadOnlyList<PackageGroup> Packages { get; }
    }
}
