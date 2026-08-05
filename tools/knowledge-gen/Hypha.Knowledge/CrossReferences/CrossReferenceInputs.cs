// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceInputs.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System.Collections.Generic;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Everything the cross-references are assembled from, already parsed.
    /// </summary>
    /// <remarks>
    /// Gathered into one record so the builder stays a pure function of its inputs: nothing here
    /// touches disk, which is what makes the whole join testable on synthetic data.
    /// </remarks>
    public sealed record CrossReferenceInputs
    {
        /// <summary>Every element in the release's metamodel index.</summary>
        public required IReadOnlyList<ElementRecord> Elements { get; init; }

        /// <summary>The model URI the metamodel declares.</summary>
        public required string ModelVersionUri { get; init; }

        /// <summary>The specifications the clause identifiers refer to, keyed by document.</summary>
        public IReadOnlyDictionary<string, SpecificationDocument> Documents { get; init; } =
            new Dictionary<string, SpecificationDocument>();

        /// <summary>
        /// Clause titles per document, as <c>{clause: title}</c>. Empty when the specification PDFs
        /// have not been extracted - see <see cref="CarriedClauseEdges"/>.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ClauseTitles { get; init; } =
            new Dictionary<string, IReadOnlyDictionary<string, string>>();

        /// <summary>
        /// Title-matched clause edges recovered from a previously generated document, per element.
        /// </summary>
        /// <remarks>
        /// Used <b>instead of</b> <see cref="ClauseTitles"/>, never as well. Matching titles needs the
        /// OMG PDFs, which most users do not have; the resulting edges are clause identifiers only and
        /// are already committed, so carrying them forward lets everything else be rebuilt from
        /// committed sources without silently dropping them.
        /// </remarks>
        public IReadOnlyDictionary<string, IReadOnlyList<ClauseEdge>> CarriedClauseEdges { get; init; } =
            new Dictionary<string, IReadOnlyList<ClauseEdge>>();

        /// <summary>Which productions build each metaclass, per grammar.</summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> Productions
        {
            get;
            init;
        } = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>();

        /// <summary>Which clauses each metaclass's productions are stated to belong to, per grammar.</summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> GrammarClauses
        {
            get;
            init;
        } = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>();

        /// <summary>Which metamodel features each metaclass's syntax populates, per grammar.</summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>>> GrammarFeatures
        {
            get;
            init;
        } = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<FeatureLink>>>();

        /// <summary>Which specification document each grammar's clause numbers belong to.</summary>
        public IReadOnlyDictionary<string, string> GrammarDocuments { get; init; } =
            new Dictionary<string, string>();

        /// <summary>The elements each worked example declares, keyed by the example's path.</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Examples { get; init; } =
            new Dictionary<string, IReadOnlyList<string>>();
    }
}
