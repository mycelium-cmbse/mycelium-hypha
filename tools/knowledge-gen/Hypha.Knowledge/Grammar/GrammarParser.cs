// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarParser.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses the KerML / SysML BNF into productions.
    /// </summary>
    /// <remarks>
    /// The <c>.kebnf</c> files carry far more than syntax. Three things are stated explicitly and are
    /// worth extracting rather than inferring: the metaclass a production produces, the specification
    /// clause it belongs to, and the metamodel feature a piece of syntax populates.
    /// <para>
    /// Everything here is a pure function of the grammar text, so it is unit-tested without the
    /// sources. Character classes are written out rather than using <c>\w</c> and <c>\d</c>: the
    /// Python original compiled these with <c>re.ASCII</c>, and .NET's shorthands are Unicode-aware,
    /// so porting them literally would silently widen what counts as an identifier or a digit.
    /// </para>
    /// </remarks>
    public sealed partial class GrammarParser : IGrammarParser
    {
        // A production starts at column 0: "Name =" or "Name : Metaclass =". Bodies are indented.
        /// <summary>
        /// Matches a textual production header at column 0, <c>Name =</c> or <c>Name : Metaclass =</c>,
        /// capturing <c>name</c> and the optional <c>produces</c>.
        /// </summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^(?<name>[A-Za-z][A-Za-z0-9_]*)[ \t]*(?::[ \t]*(?<produces>[A-Za-z][A-Za-z0-9_]*)[ \t]*)?=")]
        private static partial Regex ProductionHeader();

        // The dotted clause number is wrapped in an atomic group: it can never usefully be re-split,
        // and letting it backtrack is super-linear on a long line. There is deliberately no separator
        // between the number and the title: a `\s*` there would compete with `.*` for the same
        // spaces. The title is trimmed afterwards instead.
        /// <summary>
        /// Matches a <c>// Clause 8.2.2.5.1 Title</c> comment, capturing <c>clause</c> and the untrimmed
        /// <c>title</c>.
        /// </summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^//[ \t]*Clause[ \t]+(?<clause>(?>[0-9]+(?:\.[0-9]+)*))(?<title>.*)$")]
        private static partial Regex ClauseComment();

        // `\s*` keeps its Python meaning here on purpose: an assignment may straddle a line break.
        /// <summary>
        /// Matches a feature assignment, capturing the <c>feature</c> and its <c>operator</c>
        /// (<c>=</c>, <c>+=</c> or <c>?=</c>) but not an <c>==</c> comparison.
        /// </summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"(?<feature>[A-Za-z][A-Za-z0-9_]*)\s*(?<operator>\+=|\?=|=)(?!=)")]
        private static partial Regex Assignment();

        // The graphical grammar names its productions in kebab-case. "compartment =| general-compartment"
        // occurs upstream, hence the optional leading alternation bar.
        /// <summary>Matches a kebab-case graphical production header, capturing <c>name</c>.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^(?<name>[a-z][a-z0-9-]*)[ \t]*=\|?")]
        private static partial Regex GraphicalProductionHeader();

        /// <summary>Matches an <c>&lt;img src="…"&gt;</c> reference, capturing its <c>path</c>.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"<img\s+src=""(?<path>[^""]+)""")]
        private static partial Regex ImageReference();

        /// <summary>Matches the header of the <c>RESERVED_KEYWORD</c> production.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^RESERVED_KEYWORD[ \t]*=")]
        private static partial Regex ReservedKeywordProduction();

        // Deliberately more permissive than ProductionHeader(): this only has to recognise that the
        // keyword block has ended, so it accepts a leading underscore too.
        /// <summary>Matches any production header, which ends the reserved keyword block.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*[ \t]*(?::[ \t]*[A-Za-z0-9_]+[ \t]*)?=")]
        private static partial Regex AnyProductionHeader();

        /// <summary>Matches a single-quoted literal, capturing its content in group 1.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"'([^']+)'")]
        private static partial Regex QuotedLiteral();

        /// <summary>
        /// Parses a <c>.kebnf</c> grammar into productions, in document order, each attributed to the most
        /// recent <c>// Clause</c> comment above it and carrying the feature assignments of its body.
        /// </summary>
        /// <param name="text">The grammar's content.</param>
        /// <returns>The productions.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public IReadOnlyList<Production> Parse(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            return Scan(text, ProductionHeader(), skipComments: false, BuildTextual);
        }

        /// <summary>Parses a <c>.kgbnf</c> graphical grammar into productions, in document order.</summary>
        /// <param name="text">The graphical grammar's content.</param>
        /// <returns>The productions, with no metaclass and with their image references.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Same clause attribution as the textual grammar, but the productions are kebab-case and most
        /// of them render as an image rather than a token sequence, so the image references are
        /// captured instead of the feature assignments.
        /// </remarks>
        public IReadOnlyList<Production> ParseGraphical(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            return Scan(text, GraphicalProductionHeader(), skipComments: true, BuildGraphical);
        }

        /// <summary>
        /// The reserved keywords a grammar declares, ordered ordinally. Empty when it declares none.
        /// </summary>
        /// <param name="text">The grammar's content.</param>
        /// <returns>The quoted literals of its <c>RESERVED_KEYWORD</c> production.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Scanned line by line rather than with one multi-line regex. The block runs from the
        /// <c>RESERVED_KEYWORD</c> production until the next production or a blank line, which is far
        /// clearer to reason about than a reluctant match against a lookahead, and the regex version
        /// was flagged by SonarCloud for being able to match zero repetitions.
        /// </remarks>
        public IReadOnlyList<string> ReservedKeywords(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            var collected = new SortedSet<string>(StringComparer.Ordinal);
            var inside = false;

            foreach (var line in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            {
                if (!inside)
                {
                    if (ReservedKeywordProduction().IsMatch(line))
                    {
                        inside = true;
                        Collect(collected, line);
                    }

                    continue;
                }

                if (line.Trim().Length == 0 || AnyProductionHeader().IsMatch(line))
                {
                    break;
                }

                Collect(collected, line);
            }

            return [.. collected];
        }

        /// <summary>Adds every quoted literal on <paramref name="line"/> to <paramref name="collected"/>.</summary>
        /// <param name="collected">The keywords gathered so far.</param>
        /// <param name="line">One line of the reserved keyword block.</param>
        private static void Collect(SortedSet<string> collected, string line)
        {
            foreach (Match match in QuotedLiteral().Matches(line))
            {
                collected.Add(match.Groups[1].Value);
            }
        }

        /// <summary>
        /// Walks the grammar line by line, cutting it into productions at the headers and carrying the
        /// most recent <c>// Clause</c> comment forward.
        /// </summary>
        /// <param name="text">The grammar's content.</param>
        /// <param name="header">Recognises a production header and captures its <c>name</c> (and, optionally, <c>produces</c>).</param>
        /// <param name="skipComments">
        /// The graphical grammar carries <c>// Note.</c> corrections between productions; they belong
        /// to no production and must not leak into the preceding body.
        /// </param>
        /// <param name="build">Turns a header and its collected body lines into a production.</param>
        /// <returns>The productions, in document order.</returns>
        private static List<Production> Scan(
            string text,
            Regex header,
            bool skipComments,
            Func<Started, List<string>, Production> build)
        {
            var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var productions = new List<Production>();

            string? clause = null;
            var clauseTitle = string.Empty;
            Started? current = null;
            var body = new List<string>();

            void Flush()
            {
                if (current is not null)
                {
                    productions.Add(build(current, body));
                }
            }

            foreach (var line in lines)
            {
                var heading = ClauseComment().Match(line);
                if (heading.Success)
                {
                    Flush();
                    current = null;
                    body = [];
                    clause = heading.Groups["clause"].Value;
                    clauseTitle = heading.Groups["title"].Value.Trim();
                    continue;
                }

                if (skipComments && line.StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                var start = header.Match(line);
                if (start.Success)
                {
                    Flush();
                    current = new Started(
                        start.Groups["name"].Value,
                        start.Groups["produces"].Success ? start.Groups["produces"].Value : null,
                        clause,
                        clauseTitle);
                    body = [line];
                    continue;
                }

                if (current is not null)
                {
                    body.Add(line);
                }
            }

            Flush();

            return productions;
        }

        /// <summary>Builds a textual production, with the feature assignments that follow its header.</summary>
        /// <param name="started">What the header line said.</param>
        /// <param name="body">The production's lines, header included.</param>
        /// <returns>The production.</returns>
        private static Production BuildTextual(Started started, List<string> body)
        {
            var text = string.Join("\n", body).TrimEnd();

            // Scan for feature assignments *after* the production's own "Name : Type =" header,
            // otherwise the production name itself reads as an assignment.
            var header = ProductionHeader().Match(text);
            var scanned = header.Success ? text[(header.Index + header.Length)..] : text;

            var features = Assignment().Matches(scanned)
                .Select(match => new FeatureAssignment(
                    match.Groups["feature"].Value, match.Groups["operator"].Value))
                .Distinct()
                .Order(FeatureAssignment.Order)
                .ToList();

            return new Production
            {
                Name = started.Name,
                Produces = started.Produces,
                Clause = started.Clause,
                ClauseTitle = started.ClauseTitle,
                Body = text,
                Features = features,
            };
        }

        /// <summary>Builds a graphical production, with the images its body references.</summary>
        /// <param name="started">What the header line said.</param>
        /// <param name="body">The production's lines, header included.</param>
        /// <returns>The production, with no metaclass.</returns>
        private static Production BuildGraphical(Started started, List<string> body)
        {
            var text = string.Join("\n", body).TrimEnd();

            var images = ImageReference().Matches(text)
                .Select(match => match.Groups["path"].Value)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();

            return new Production
            {
                Name = started.Name,
                Produces = null,
                Clause = started.Clause,
                ClauseTitle = started.ClauseTitle,
                Body = text,
                Images = images,
            };
        }

        /// <summary>What a header line told us, before the body has been collected.</summary>
        /// <param name="Name">The production's name.</param>
        /// <param name="Produces">The metaclass the header declares, or <c>null</c>.</param>
        /// <param name="Clause">The clause of the most recent <c>// Clause</c> comment, or <c>null</c> before the first.</param>
        /// <param name="ClauseTitle">That comment's title, trimmed; empty when there is none.</param>
        private sealed record Started(string Name, string? Produces, string? Clause, string ClauseTitle);
    }
}
