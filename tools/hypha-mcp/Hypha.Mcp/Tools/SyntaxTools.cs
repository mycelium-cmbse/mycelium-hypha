// ------------------------------------------------------------------------------------------------
// <copyright file="SyntaxTools.cs" company="Starion Group S.A.">
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
    /// The tool that answers questions about a release's textual notation, from its cross-references
    /// and its keyword index.
    /// </summary>
    [McpServerToolType]
    public sealed class SyntaxTools
    {
        /// <summary>Answers the questions.</summary>
        private readonly IKnowledgeQuery query;

        /// <summary>Initializes a new instance of the <see cref="SyntaxTools"/> class.</summary>
        /// <param name="query">Answers the questions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> is <c>null</c>.</exception>
        public SyntaxTools(IKnowledgeQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            this.query = query;
        }

        /// <summary>Answers a question about the textual notation.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="element">The element, for <c>cross-reference</c>.</param>
        /// <param name="feature">Only this feature, for <c>feature-assignments</c>.</param>
        /// <param name="operator">Only this assignment operator, for <c>feature-assignments</c>.</param>
        /// <param name="production">The grammar production, for <c>built-by</c>.</param>
        /// <param name="word">The keyword or operator, for <c>keyword</c>.</param>
        /// <param name="grammar">Only this grammar, for <c>built-by</c> and <c>keyword</c>.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_syntax", Title = "Textual notation", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Answers a question about the SysML v2 or KerML textual notation: 'cross-reference' (an element's "
            + "specification clauses, grammar productions, the features its syntax fills in and worked examples), "
            + "'feature-assignments' (which elements' syntax fills in a feature, through which operator and "
            + "productions), 'built-by' (the elements a grammar production builds) or 'keyword' (the productions "
            + "that write a keyword or operator, and the metaclasses whose syntax includes it).")]
        public string Syntax(
            [Description("What to answer.")] SyntaxQuestion question,
            [Description("For 'cross-reference': the element, such as PartUsage.")] string? element = null,
            [Description("For 'feature-assignments': only this feature, such as declaredName.")] string? feature = null,
            [Description("For 'feature-assignments': only this assignment operator: =, += or ?=.")] string? @operator = null,
            [Description("For 'built-by': the grammar production, such as PartUsage.")] string? production = null,
            [Description("For 'keyword': the keyword or operator, such as ref or :>>.")] string? word = null,
            [Description("For 'built-by' and 'keyword': only this grammar, kerml or sysml.")] string? grammar = null,
            [Description("The installed release to answer from, such as 2026-05. The default release when omitted.")] string? tag = null) =>
            question switch
            {
                SyntaxQuestion.CrossReference => Answer.Of(
                    () => this.query.CrossReference(Answer.Required(element, "element", "cross-reference"), tag)),
                SyntaxQuestion.FeatureAssignments => Answer.Of(() => this.query.FeatureAssignments(feature, @operator, tag)),
                SyntaxQuestion.BuiltBy => Answer.Of(
                    () => this.query.BuiltBy(Answer.Required(production, "production", "built-by"), grammar, tag)),
                SyntaxQuestion.Keyword => Answer.Of(
                    () => this.query.Keyword(Answer.Required(word, "word", "keyword"), grammar, tag)),
                _ => throw new McpException($"'{question}' is not a question hypha_syntax answers."),
            };
    }
}
