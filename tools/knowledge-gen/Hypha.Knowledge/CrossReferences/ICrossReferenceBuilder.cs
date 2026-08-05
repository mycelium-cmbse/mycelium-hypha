// ------------------------------------------------------------------------------------------------
// <copyright file="ICrossReferenceBuilder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// Joins the knowledge trees: metamodel element to specification clause, grammar production, the
    /// metamodel features its syntax populates, and worked examples.
    /// </summary>
    public interface ICrossReferenceBuilder
    {
        /// <summary>Assembles the whole document. Pure: no disk access, no ordering surprises.</summary>
        CrossReferenceDocument Build(CrossReferenceInputs inputs);
    }
}
