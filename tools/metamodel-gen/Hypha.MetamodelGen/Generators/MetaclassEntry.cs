// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// A single metaclass entry in the index.
    /// </summary>
    public sealed class MetaclassEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassEntry"/> class.
        /// </summary>
        public MetaclassEntry(string name, string summary)
        {
            this.Name = name;
            this.Summary = summary;
        }

        /// <summary>Gets the metaclass name (also the element file stem).</summary>
        public string Name { get; }

        /// <summary>Gets the one-line summary of the metaclass documentation (may be empty).</summary>
        public string Summary { get; }
    }
}
