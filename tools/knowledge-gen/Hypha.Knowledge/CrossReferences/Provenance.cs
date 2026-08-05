// ------------------------------------------------------------------------------------------------
// <copyright file="Provenance.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System.Collections.Generic;

    /// <summary>
    /// Which tier a recorded fact belongs to, so a reader can tell what was read from a source from
    /// what this repository worked out.
    /// </summary>
    public static class Provenance
    {
        /// <summary>Verbatim specification text. Lives only in the git-ignored knowledge/spec tree.</summary>
        public const string Normative = "NORMATIVE";

        /// <summary>Read directly from a source: the metamodel XMI, or a statement in the grammar.</summary>
        public const string Model = "MODEL";

        /// <summary>Computed or asserted here: a closure, a name match, a curated cross-reference.</summary>
        public const string Derived = "DERIVED";

        /// <summary>
        /// Each tier and what it means, carried in the output so a consumer can explain them without
        /// hardcoding. Ordered as written - the order reaches the generated file.
        /// </summary>
        public static IReadOnlyDictionary<string, string> Tiers { get; } = new Dictionary<string, string>
        {
            [Normative] =
                "Verbatim specification text, clause-anchored. Lives only in the git-ignored knowledge/spec tree.",
            [Model] = "Read directly from the metamodel XMI by tools/metamodel-gen.",
            [Derived] =
                "Computed or asserted by this repository: a closure, a name match, or a curated cross-reference.",
        };
    }
}
