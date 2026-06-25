// ------------------------------------------------------------------------------------------------
// <copyright file="EnumerationPayload.cs" company="Starion Group S.A.">
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
    /// The Handlebars context for a single enumeration element file.
    /// </summary>
    public sealed class EnumerationPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationPayload"/> class.
        /// </summary>
        public EnumerationPayload(
            string name,
            string package,
            string fullyQualifiedName,
            string visibility,
            string documentation,
            IReadOnlyList<EnumerationLiteralEntry> literals)
        {
            this.Name = name;
            this.Package = package;
            this.FullyQualifiedName = fullyQualifiedName;
            this.Visibility = visibility;
            this.Documentation = documentation;
            this.Literals = literals;
        }

        /// <summary>Gets the enumeration name.</summary>
        public string Name { get; }

        /// <summary>Gets the owning package name.</summary>
        public string Package { get; }

        /// <summary>Gets the fully qualified name (e.g. <c>KerML::Core::Features::FeatureDirectionKind</c>).</summary>
        public string FullyQualifiedName { get; }

        /// <summary>Gets the enumeration visibility (e.g. <c>public</c>).</summary>
        public string Visibility { get; }

        /// <summary>Gets the enumeration documentation (may be empty).</summary>
        public string Documentation { get; }

        /// <summary>Gets the enumeration literals, in declaration order.</summary>
        public IReadOnlyList<EnumerationLiteralEntry> Literals { get; }
    }

    /// <summary>
    /// A single literal of an enumeration, as rendered in its element file.
    /// </summary>
    public sealed class EnumerationLiteralEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationLiteralEntry"/> class.
        /// </summary>
        public EnumerationLiteralEntry(string name, string documentation)
        {
            this.Name = name;
            this.Documentation = documentation;
        }

        /// <summary>Gets the literal name.</summary>
        public string Name { get; }

        /// <summary>Gets the literal documentation (may be empty).</summary>
        public string Documentation { get; }
    }
}
