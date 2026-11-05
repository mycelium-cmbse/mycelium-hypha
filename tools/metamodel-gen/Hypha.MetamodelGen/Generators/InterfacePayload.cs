// ------------------------------------------------------------------------------------------------
// <copyright file="InterfacePayload.cs" company="Starion Group S.A.">
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
    /// The Handlebars context for a single interface element file.
    /// </summary>
    public sealed class InterfacePayload
    {
        /// <summary>Gets the interface name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the owning package name.</summary>
        public required string Package { get; init; }

        /// <summary>Gets the fully qualified name.</summary>
        public required string FullyQualifiedName { get; init; }

        /// <summary>Gets the interface visibility (e.g. <c>public</c>).</summary>
        public required string Visibility { get; init; }

        /// <summary>Gets the interface documentation (may be empty).</summary>
        public required string Documentation { get; init; }

        /// <summary>Gets the names of the classifiers that realize the interface, ordered.</summary>
        public required IReadOnlyList<string> RealizedBy { get; init; }

        /// <summary>
        /// Gets the realizing classifiers as markdown, linked where the model has a page for them.
        /// </summary>
        public required IReadOnlyList<string> RealizedByMarkdown { get; init; }

        /// <summary>Gets the owned operations, ordered by name.</summary>
        public required IReadOnlyList<MetaclassOperation> Operations { get; init; }
    }
}
