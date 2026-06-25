// ------------------------------------------------------------------------------------------------
// <copyright file="IndexEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// A single element entry in the index (metaclass, enumeration or primitive type).
    /// </summary>
    public sealed class IndexEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexEntry"/> class.
        /// </summary>
        public IndexEntry(string name, string summary)
        {
            this.Name = name;
            this.Summary = summary;
        }

        /// <summary>Gets the element name (also the element file stem).</summary>
        public string Name { get; }

        /// <summary>Gets the one-line summary of the element documentation (may be empty).</summary>
        public string Summary { get; }
    }
}
