// ------------------------------------------------------------------------------------------------
// <copyright file="IGrammarLinks.cs" company="Starion Group S.A.">
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
    /// Joins the grammar to the metamodel. Every join is something the grammar <i>states</i> rather
    /// than something matched on a name, so a consumer can cite it as read.
    /// </summary>
    public interface IGrammarLinks
    {
        /// <summary>The productions that build each metaclass, by declared type then by name.</summary>
        /// <param name="productions">One grammar's parsed productions.</param>
        /// <param name="metaclasses">The metamodel's element names; nothing outside them is linked.</param>
        /// <returns>The production names, ordered, by metaclass name.</returns>
        IReadOnlyDictionary<string, IReadOnlyList<string>> MetaclassLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses);

        /// <summary>The clauses each metaclass's productions are defined in.</summary>
        /// <param name="productions">One grammar's parsed productions.</param>
        /// <param name="metaclasses">The metamodel's element names; nothing outside them is linked.</param>
        /// <returns>The clause numbers, in clause order, by metaclass name.</returns>
        IReadOnlyDictionary<string, IReadOnlyList<string>> ClauseLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses);

        /// <summary>The metamodel features each metaclass's productions populate, and how.</summary>
        /// <param name="productions">One grammar's parsed productions.</param>
        /// <param name="metaclasses">The metamodel's element names; nothing outside them is linked.</param>
        /// <returns>The feature links, ordered, by metaclass name.</returns>
        IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>> FeatureLinks(
            IReadOnlyList<Production> productions, ISet<string> metaclasses);
    }
}
