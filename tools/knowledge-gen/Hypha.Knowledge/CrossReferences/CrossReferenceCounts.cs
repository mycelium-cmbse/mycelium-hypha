// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceCounts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    /// <summary>
    /// Coverage, carried in the file so a drop is visible in a diff rather than having to be measured.
    /// </summary>
    /// <param name="Elements">Every element in the metamodel index.</param>
    /// <param name="WithClauses">Elements resolved to at least one specification clause.</param>
    /// <param name="WithoutClauses">Elements resolved to none.</param>
    /// <param name="WithGrammar">Elements built by at least one grammar production.</param>
    /// <param name="WithFeatures">Elements whose syntax populates at least one feature.</param>
    /// <param name="WithExamples">Elements declared by at least one worked example.</param>
    /// <param name="ClauseEdges">Total clause edges.</param>
    /// <param name="StatedClauseEdges">
    /// Clause edges the grammar states outright, rather than matched on a clause title. These need no
    /// specification PDF, which is why the file can be rebuilt without one.
    /// </param>
    /// <param name="GrammarEdges">Total grammar edges.</param>
    /// <param name="FeatureEdges">Total feature edges.</param>
    /// <param name="ExampleEdges">Total example edges.</param>
    public sealed record CrossReferenceCounts(
        int Elements,
        int WithClauses,
        int WithoutClauses,
        int WithGrammar,
        int WithFeatures,
        int WithExamples,
        int ClauseEdges,
        int StatedClauseEdges,
        int GrammarEdges,
        int FeatureEdges,
        int ExampleEdges);
}
