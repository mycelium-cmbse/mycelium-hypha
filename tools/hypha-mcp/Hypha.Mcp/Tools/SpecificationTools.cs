// ------------------------------------------------------------------------------------------------
// <copyright file="SpecificationTools.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.ComponentModel;

    using Hypha.Knowledge.Query;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tools that read the specification text and find things by name: a clause, a search, and the
    /// page a <c>hypha://</c> address names.
    /// </summary>
    [McpServerToolType]
    public sealed class SpecificationTools
    {
        /// <summary>The description of every tool's <c>tag</c> argument.</summary>
        private const string TagDescription = "The installed release to answer from, such as 2026-05. The default release when omitted.";

        /// <summary>Answers the questions.</summary>
        private readonly IKnowledgeQuery query;

        /// <summary>Initializes a new instance of the <see cref="SpecificationTools"/> class.</summary>
        /// <param name="query">Answers the questions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> is <c>null</c>.</exception>
        public SpecificationTools(IKnowledgeQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            this.query = query;
        }

        /// <summary>Answers a question about one specification clause.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="document">The specification: <c>kerml</c> or <c>sysml2</c>.</param>
        /// <param name="clause">The clause number.</param>
        /// <param name="includeSubclauses">Also the clauses below it, for <c>elements-in-clause</c>.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_spec", Title = "Specification clause", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Reads a clause of the KerML or SysML v2 specification: 'clause' (its verbatim, normative text, to quote "
            + "with its number) or 'elements-in-clause' (the elements the clause treats). The text is only there when "
            + "the specification PDFs were processed on this machine; otherwise the result says so.")]
        public string Spec(
            [Description("What to answer.")] SpecificationQuestion question,
            [Description("The specification: kerml or sysml2.")] string document,
            [Description("The clause number, such as 7.4.2.")] string clause,
            [Description("For 'elements-in-clause': also the clauses below it.")] bool includeSubclauses = false,
            [Description(TagDescription)] string? tag = null) =>
            question switch
            {
                SpecificationQuestion.Clause => Answer.Of(() => this.query.Clause(document, clause, tag)),
                SpecificationQuestion.ElementsInClause => Answer.Of(
                    () => this.query.ElementsInClause(document, clause, includeSubclauses, tag)),
                _ => throw new McpException($"'{question}' is not a question hypha_spec answers."),
            };

        /// <summary>Searches a release by name.</summary>
        /// <param name="text">The text to look for.</param>
        /// <param name="kind">Only hits of this kind, or <c>null</c> for any.</param>
        /// <param name="limit">The most hits returned.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The hits, with their grounding, as JSON.</returns>
        /// <exception cref="McpException">The search could not be run; the message says why.</exception>
        [McpServerTool(Name = "hypha_search", Title = "Search", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Searches a release's metaclasses, standard-library declarations and specification clauses by name, "
            + "qualified name, clause number or title, closest match first. Each hit has a hypha:// address that "
            + "hypha_resolve reads.")]
        public string Search(
            [Description("The text to look for: a name, qualified name, clause number or title.")] string text,
            [Description($"Only hits of this kind: {HyphaAddress.Metamodel}, {HyphaAddress.ModelLibrary}, {HyphaAddress.Kerml} or {HyphaAddress.Sysml}.")] string? kind = null,
            [Description("The most hits returned, from 1 to 200.")] int limit = 20,
            [Description(TagDescription)] string? tag = null) =>
            Answer.Of(() => this.query.Search(text, kind, limit, tag));

        /// <summary>Reads the page an address names.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The page, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The address could not be resolved; the message says why.</exception>
        [McpServerTool(Name = "hypha_resolve", Title = "Resolve address", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Reads the generated page a hypha:// address names, such as hypha://2026-05/metamodel/PartUsage: a "
            + "metaclass page, a standard-library declaration or a specification clause.")]
        public string Resolve(
            [Description("The address, such as hypha://2026-05/metamodel/PartUsage.")] string address) =>
            Answer.Of(() => this.query.Resolve(HyphaAddress.Parse(address)));
    }
}
