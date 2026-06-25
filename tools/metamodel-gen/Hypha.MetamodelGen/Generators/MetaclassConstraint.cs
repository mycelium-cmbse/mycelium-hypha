// ------------------------------------------------------------------------------------------------
// <copyright file="MetaclassConstraint.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// An owned constraint of a metaclass, as rendered in its element file.
    /// </summary>
    public sealed class MetaclassConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaclassConstraint"/> class.
        /// </summary>
        public MetaclassConstraint(string name, string documentation, string body)
        {
            this.Name = name;
            this.Documentation = documentation;
            this.Body = body;
        }

        /// <summary>Gets the constraint name.</summary>
        public string Name { get; }

        /// <summary>Gets the constraint documentation/intent text (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the constraint body/specification text (may be empty).</summary>
        public string Body { get; }
    }
}
