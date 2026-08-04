// ------------------------------------------------------------------------------------------------
// <copyright file="Production.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Grammar
{
    using System.Collections.Generic;

    /// <summary>
    /// One grammar production, with what the grammar itself says about it.
    /// </summary>
    /// <remarks>
    /// Nothing here is inferred. The metaclass comes from the production's own
    /// <c>Name : Metaclass =</c> header, the clause from the <c>// Clause 8.2.2.5.1</c> comment the
    /// grammar authors wrote above it, and the features from its assignment operators.
    /// </remarks>
    public sealed record Production
    {
        /// <summary>The production's name, e.g. <c>PackageDeclaration</c>.</summary>
        public required string Name { get; init; }

        /// <summary>
        /// The metaclass this production builds, when it declares one. Roughly three times as many
        /// productions declare a type as happen to be <i>named</i> after a metaclass, which makes this
        /// a far better grammar-to-metamodel join than name matching.
        /// </summary>
        public string? Produces { get; init; }

        /// <summary>The specification clause the production is defined in, per the grammar's own comment.</summary>
        public string? Clause { get; init; }

        /// <summary>The clause's title, as written in that comment. Empty when there is none.</summary>
        public string ClauseTitle { get; init; } = string.Empty;

        /// <summary>The production verbatim, header line included.</summary>
        public required string Body { get; init; }

        /// <summary>The metamodel features this production assigns, ordered.</summary>
        public IReadOnlyList<FeatureAssignment> Features { get; init; } = [];

        /// <summary>Image files this production renders as. Only the graphical grammar has these.</summary>
        public IReadOnlyList<string> Images { get; init; } = [];
    }
}
