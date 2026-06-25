// ------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTypePayload.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Generators
{
    /// <summary>
    /// The Handlebars context for a single primitive-type element file.
    /// </summary>
    public sealed class PrimitiveTypePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveTypePayload"/> class.
        /// </summary>
        public PrimitiveTypePayload(
            string name,
            string package,
            string fullyQualifiedName,
            string visibility,
            string documentation)
        {
            this.Name = name;
            this.Package = package;
            this.FullyQualifiedName = fullyQualifiedName;
            this.Visibility = visibility;
            this.Documentation = documentation;
        }

        /// <summary>Gets the primitive-type name.</summary>
        public string Name { get; }

        /// <summary>Gets the owning package name.</summary>
        public string Package { get; }

        /// <summary>Gets the fully qualified name (e.g. <c>PrimitiveTypes::String</c>).</summary>
        public string FullyQualifiedName { get; }

        /// <summary>Gets the primitive-type visibility (e.g. <c>public</c>).</summary>
        public string Visibility { get; }

        /// <summary>Gets the primitive-type documentation (may be empty).</summary>
        public string Documentation { get; }
    }
}
