// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgePrompts.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Prompts
{
    using System;
    using System.ComponentModel;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// Guided flows a user can pick from a client's prompt menu: the capabilities <c>commands/overview.md</c>
    /// describes, each telling the model which tools answer it and how to ground the answer.
    /// </summary>
    /// <remarks>
    /// A prompt is the user's request, filled in; it names tools but runs none. Each says what to do when a
    /// release is not installed, so it works on a machine that has nothing installed yet.
    /// </remarks>
    [McpServerPromptType]
    public sealed class KnowledgePrompts
    {
        /// <summary>What every flow does first, and what it does when nothing is installed.</summary>
        private const string Start =
            "Call hypha_release_status first. If no release is installed, tell me, and offer to install the newest "
            + "one with hypha_install_release; do not answer from memory instead.";

        /// <summary>How every flow presents its answer.</summary>
        private const string Grounding =
            "Give the release that answered, and say for each fact whether it is NORMATIVE (quote it, with its clause "
            + "number), MODEL or DERIVED, as its grounding says.";

        /// <summary>What hypha can do here, with example questions.</summary>
        /// <returns>The request.</returns>
        [McpServerPrompt(Name = "overview", Title = "What hypha can do")]
        [Description("Explains what the hypha server answers about SysML v2 and KerML, with example questions to ask.")]
        public static string Overview() =>
            """
            Explain briefly what the hypha MCP server can answer, from what is installed on this machine. Call
            hypha_release_status and say which releases are installed, which one is the default, and whether its
            specification text was generated (citations need it). Then list what I can ask, with one example each:
            - the metamodel: a metaclass's features, supertypes, subclasses, redefinitions and constraints, such as
              "What features does PartUsage own and inherit?";
            - the specification: the exact normative wording with its clause, such as "What does the KerML
              specification say about redefinition?";
            - the textual notation: which syntax builds an element and where a keyword is used, and checking a
              snippet, such as "Is part def Vehicle { attribute mass : Real[2..1]; } valid SysML v2?";
            - the Systems Modeling API: PIM classes, API types and operations, such as "Which operation creates a
              project?";
            - the releases themselves: what changed between two, and installing, switching or removing one.
            If nothing is installed, say so and offer to install the newest release with hypha_install_release,
            rather than answering from memory.
            """;

        /// <summary>Explains one metaclass.</summary>
        /// <param name="name">The metaclass.</param>
        /// <returns>The request.</returns>
        /// <exception cref="McpException"><paramref name="name"/> is blank.</exception>
        [McpServerPrompt(Name = "explain_metaclass", Title = "Explain a metaclass")]
        [Description("Explains a SysML v2 or KerML metaclass from the metamodel: its place in the hierarchy, its features and its constraints.")]
        public static string ExplainMetaclass(
            [Description("The metaclass, such as PartUsage.")] string name) =>
            $"""
            Explain the SysML v2 / KerML metaclass {Required(name, nameof(name))}. {Start}
            Use hypha_metaclass with the questions 'element' (what it is and where it is defined), 'ancestors',
            'subclasses', 'features' (owned and inherited, with where each comes from) and 'constraints'. If it is
            not found, look for the intended name with hypha_search. Where its textual notation matters, add
            hypha_syntax 'cross-reference'. {Grounding}
            """;

        /// <summary>Finds and quotes what the specification says about a topic.</summary>
        /// <param name="topic">What to find.</param>
        /// <returns>The request.</returns>
        /// <exception cref="McpException"><paramref name="topic"/> is blank.</exception>
        [McpServerPrompt(Name = "cite_specification", Title = "Cite the specification")]
        [Description("Finds what the KerML and SysML v2 specifications say about a topic and quotes it, with clause numbers.")]
        public static string CiteSpecification(
            [Description("What to find, such as redefinition or conformance.")] string topic) =>
            $"""
            What do the KerML and SysML v2 specifications say about {Required(topic, nameof(topic))}? {Start}
            Find the clauses with hypha_search_spec (and hypha_search for a clause title), then read the most relevant
            ones with hypha_spec 'clause'. Quote their text verbatim, each quotation with its specification and
            clause number; never quote an excerpt from the search, and never paraphrase as a quotation. If the
            specification text is unavailable, say so and say how to generate it, as the result does, rather than
            answering from memory. {Grounding}
            """;

        /// <summary>Checks a snippet of textual notation.</summary>
        /// <param name="notation">The SysML v2 or KerML textual notation.</param>
        /// <returns>The request.</returns>
        /// <exception cref="McpException"><paramref name="notation"/> is blank.</exception>
        [McpServerPrompt(Name = "validate_notation", Title = "Check textual notation")]
        [Description("Checks SysML v2 or KerML textual notation against the grammar and the metamodel, and explains each problem with a fix.")]
        public static string ValidateNotation(
            [Description("The textual notation to check.")] string notation) =>
            $"""
            Check this SysML v2 / KerML textual notation for syntactic and structural problems:

            ```
            {Required(notation, nameof(notation))}
            ```

            {Start} For each keyword and construct, check the grammar with hypha_syntax ('keyword', 'built-by',
            'feature-assignments') and the metamodel with hypha_metaclass ('features', 'constraints'): that each
            keyword is used where the grammar allows it, that each feature exists on the metaclass, and that every
            multiplicity, type and redefinition is valid. List each problem with where it is, why it is one, and a
            fix; if there is none, say so. {Grounding}
            """;

        /// <summary>Answers a question about the Systems Modeling API.</summary>
        /// <param name="question">The question.</param>
        /// <returns>The request.</returns>
        /// <exception cref="McpException"><paramref name="question"/> is blank.</exception>
        [McpServerPrompt(Name = "api_lookup", Title = "Look up the Systems Modeling API")]
        [Description("Answers a question about the Systems Modeling API and Services: PIM classes, API types and operations, and the specification.")]
        public static string ApiLookup(
            [Description("The question, such as which operation creates a project.")] string question) =>
            $"""
            Answer this question about the OMG Systems Modeling API and Services: {Required(question, nameof(question))}
            Call hypha_release_status first and check that an API version is installed; if none is, say so rather
            than answering from memory. Use hypha_api with 'pim-class', 'type', 'operation' and 'clause', and say
            which API version answered and how each link between the PIM and the REST API was established, as the
            result states it. {Grounding}
            """;

        /// <summary>The value of a prompt argument, which must not be blank.</summary>
        /// <param name="value">The argument's value.</param>
        /// <param name="argument">The argument's name.</param>
        /// <returns><paramref name="value"/>, trimmed.</returns>
        /// <exception cref="McpException"><paramref name="value"/> is blank.</exception>
        private static string Required(string? value, string argument) =>
            string.IsNullOrWhiteSpace(value)
                ? throw new McpException($"The prompt needs the argument '{argument}'.")
                : value.Trim();
    }
}
