// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarReference.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Grammar
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Renders the per-release grammar references under <c>knowledge/&lt;tag&gt;/textual-notation/</c>.
    /// </summary>
    /// <remarks>
    /// The textual and graphical references are the same document with different facts under each
    /// production, so the page structure lives in one place and each renderer supplies only what
    /// differs: its front matter, the heading used for productions the grammar attributes to no
    /// clause, the code fence, and the facts.
    /// </remarks>
    public sealed class GrammarReference : IGrammarReference
    {
        /// <summary>The raw-content address a release's notation images are linked under, as a string.</summary>
        private readonly string rawContentBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarReference"/> class.
        /// </summary>
        /// <param name="rawContentBaseAddress">
        /// Where a file at a tag is served from; defaults to <see cref="Upstream.DefaultRawContentBaseAddress"/>.
        /// Injectable for the same reason as the API host: an enterprise instance serves its raw
        /// content from somewhere else.
        /// </param>
        public GrammarReference(Uri? rawContentBaseAddress = null) =>
            this.rawContentBase = (rawContentBaseAddress ?? Upstream.DefaultRawContentBaseAddress).ToString();

        /// <summary>
        /// Renders the textual grammar reference for one document: front matter, then one section per
        /// production grouped by the clause the grammar states, each with the metaclass it produces, its
        /// clause and the features it assigns.
        /// </summary>
        /// <param name="grammar">The grammar's name, <c>KerML</c> or <c>SysML</c>.</param>
        /// <param name="tag">The release the grammar was read at.</param>
        /// <param name="productions">The parsed productions, in document order.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="grammar"/> or <paramref name="tag"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="productions"/> is <c>null</c>.</exception>
        public string Render(string grammar, string tag, IReadOnlyList<Production> productions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(grammar);
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            ArgumentNullException.ThrowIfNull(productions);

            var preamble = Preamble(
                [
                    ("grammar", grammar),
                    ("tag", tag),
                    ("kind", "grammar-reference"),
                    ("productions", Count(productions.Count)),
                ],
                $"{grammar} textual grammar — {tag}",
                ".kebnf",
                [
                    "Each production shows the metaclass it builds (`produces`), the specification clause it is",
                    "defined in, and the metamodel features it populates (`=` sets, `+=` adds, `?=` is a boolean",
                    "flag). Both come from the grammar itself, not from name matching.",
                ]);

            return Compose(preamble, productions, "Lexical", "```kebnf", TextualFacts);
        }

        /// <summary>
        /// Renders the graphical-notation reference for one release, grouped by clause, with each
        /// production's notation images linked at <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The release the grammar was read at, which the image links point into.</param>
        /// <param name="repository">The upstream the images are published from.</param>
        /// <param name="productions">The parsed graphical productions, in document order.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> or <paramref name="repository"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="productions"/> is <c>null</c>.</exception>
        /// <remarks>
        /// The notation itself is a set of SVGs published alongside the grammar. They are linked at the
        /// release tag rather than copied in: 284 images per release is 1.5 MB of binaries nothing in
        /// the knowledge base would read, and the link is stable because the tag is.
        /// </remarks>
        public string RenderGraphical(string tag, string repository, IReadOnlyList<Production> productions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            ArgumentException.ThrowIfNullOrWhiteSpace(repository);
            ArgumentNullException.ThrowIfNull(productions);

            var withImages = productions.Count(production => production.Images.Count > 0);

            var preamble = Preamble(
                [
                    ("grammar", "SysML-graphical"),
                    ("tag", tag),
                    ("kind", "graphical-grammar-reference"),
                    ("productions", Count(productions.Count)),
                    ("withImages", Count(withImages)),
                ],
                $"SysML graphical notation — {tag}",
                ".kgbnf",
                [
                    "Most graphical productions *are* a picture. The images are linked at this release's tag",
                    "rather than copied into the repository, so they stay in step with the grammar.",
                ]);

            return Compose(
                preamble,
                productions,
                "General",
                "```kgbnf",
                production => this.GraphicalFacts(production, tag, repository));
        }

        /// <summary>The front matter, title and standing note every reference opens with.</summary>
        /// <param name="fields">The front-matter keys and values, in order.</param>
        /// <param name="title">The page's <c>#</c> heading.</param>
        /// <param name="source">The grammar file extension the page was generated from.</param>
        /// <param name="lead">The paragraph explaining how to read the page.</param>
        /// <returns>The page's opening lines, to be extended by <see cref="Compose"/>.</returns>
        private static List<string> Preamble(
            IReadOnlyList<(string Key, string Value)> fields,
            string title,
            string source,
            IReadOnlyList<string> lead)
        {
            var lines = new List<string> { "---" };

            lines.AddRange(fields.Select(field => $"{field.Key}: {field.Value}"));

            lines.Add("---");
            lines.Add(string.Empty);
            lines.Add($"# {title}");
            lines.Add(string.Empty);
            lines.Add(
                $"> Generated by `tools/knowledge-gen` from the committed `{source}` (EPL-2.0) — do not edit by");
            lines.Add("> hand; re-run the generator.");
            lines.Add(string.Empty);
            lines.AddRange(lead);
            lines.Add(string.Empty);

            return lines;
        }

        /// <summary>Appends one section per production, grouped by the clause the grammar states.</summary>
        /// <param name="lines">The page's lines so far, from <see cref="Preamble"/>; appended to in place.</param>
        /// <param name="productions">The productions, in document order.</param>
        /// <param name="unattributed">The heading for productions the grammar gives no clause.</param>
        /// <param name="fence">The opening code fence, which names the grammar's language.</param>
        /// <param name="facts">The lines between a production's heading and its body, blank line included.</param>
        /// <returns>The whole page, joined with LF.</returns>
        private static string Compose(
            List<string> lines,
            IReadOnlyList<Production> productions,
            string unattributed,
            string fence,
            Func<Production, IReadOnlyList<string>> facts)
        {
            (string? Clause, string Title)? current = null;

            foreach (var production in productions)
            {
                var key = (production.Clause, production.ClauseTitle);
                if (current != key)
                {
                    current = key;
                    lines.Add($"## {Heading(production, unattributed)}");
                    lines.Add(string.Empty);
                }

                lines.Add($"### {production.Name}");
                lines.Add(string.Empty);
                lines.AddRange(facts(production));

                lines.Add(fence);
                lines.Add(production.Body);
                lines.Add("```");
                lines.Add(string.Empty);
            }

            return string.Join("\n", lines);
        }

        /// <summary>What the grammar states about a textual production, on one line.</summary>
        /// <param name="production">The production.</param>
        /// <returns>That line and a blank one, or nothing when the grammar states nothing about it.</returns>
        private static IReadOnlyList<string> TextualFacts(Production production)
        {
            var facts = new List<string>();

            if (!string.IsNullOrEmpty(production.Produces))
            {
                facts.Add($"produces [{production.Produces}](../metamodel/elements/{production.Produces}.md)");
            }

            if (!string.IsNullOrEmpty(production.Clause))
            {
                facts.Add($"clause `{production.Clause}`");
            }

            if (production.Features.Count > 0)
            {
                facts.Add("features " + string.Join(
                    ", ", production.Features.Select(feature => $"`{feature.Feature} {feature.Operator}`")));
            }

            return facts.Count == 0 ? [] : [string.Join(" · ", facts), string.Empty];
        }

        /// <summary>A graphical production's clause, then the notation images it renders as.</summary>
        /// <param name="production">The production.</param>
        /// <param name="tag">The release the image links point into.</param>
        /// <param name="repository">The upstream the images are published from.</param>
        /// <returns>The lines, each followed by a blank one.</returns>
        private List<string> GraphicalFacts(Production production, string tag, string repository)
        {
            var lines = new List<string>();

            if (!string.IsNullOrEmpty(production.Clause))
            {
                lines.Add($"clause `{production.Clause}`");
                lines.Add(string.Empty);
            }

            foreach (var path in production.Images)
            {
                lines.Add($"![{production.Name}]({this.rawContentBase}{repository}/{tag}/bnf/{path})");
                lines.Add(string.Empty);
            }

            return lines;
        }

        /// <summary>
        /// The section heading for a production's clause, or <paramref name="unattributed"/> for the
        /// productions the grammar states no clause for.
        /// </summary>
        /// <param name="production">The first production of the section.</param>
        /// <param name="unattributed">The heading used when there is no clause.</param>
        /// <returns>The clause number and title, or <paramref name="unattributed"/>.</returns>
        private static string Heading(Production production, string unattributed) =>
            string.IsNullOrEmpty(production.Clause)
                ? unattributed
                : $"{production.Clause} {production.ClauseTitle}".Trim();

        /// <summary>Formats a count for the front matter, independently of the current culture.</summary>
        /// <param name="value">The count.</param>
        /// <returns>The invariant-culture digits.</returns>
        private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
