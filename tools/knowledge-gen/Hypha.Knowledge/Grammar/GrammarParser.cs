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
        [GeneratedRegex(@"^(?<name>[A-Za-z][A-Za-z0-9_]*)[ \t]*(?::[ \t]*(?<produces>[A-Za-z][A-Za-z0-9_]*)[ \t]*)?=")]
        private static partial Regex ProductionHeader();

        // The dotted clause number is wrapped in an atomic group: it can never usefully be re-split,
        // and letting it backtrack is super-linear on a long line. There is deliberately no separator
        // between the number and the title - a `\s*` there would compete with `.*` for the same
        // spaces. The title is trimmed afterwards instead.
        [GeneratedRegex(@"^//[ \t]*Clause[ \t]+(?<clause>(?>[0-9]+(?:\.[0-9]+)*))(?<title>.*)$")]
        private static partial Regex ClauseComment();

        // `\s*` keeps its Python meaning here on purpose: an assignment may straddle a line break.
        [GeneratedRegex(@"(?<feature>[A-Za-z][A-Za-z0-9_]*)\s*(?<operator>\+=|\?=|=)(?!=)")]
        private static partial Regex Assignment();

        // The graphical grammar names its productions in kebab-case. "compartment =| general-compartment"
        // occurs upstream, hence the optional leading alternation bar.
        [GeneratedRegex(@"^(?<name>[a-z][a-z0-9-]*)[ \t]*=\|?")]
        private static partial Regex GraphicalProductionHeader();

        [GeneratedRegex(@"<img\s+src=""(?<path>[^""]+)""")]
        private static partial Regex ImageReference();

        /// <inheritdoc/>
        public IReadOnlyList<Production> Parse(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            return Scan(text, ProductionHeader(), skipComments: false, BuildTextual);
        }

        /// <inheritdoc/>
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
        /// Walks the grammar line by line, cutting it into productions at the headers and carrying the
        /// most recent <c>// Clause</c> comment forward.
        /// </summary>
        /// <param name="skipComments">
        /// The graphical grammar carries <c>// Note.</c> corrections between productions; they belong
        /// to no production and must not leak into the preceding body.
        /// </param>
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
        private sealed record Started(string Name, string? Produces, string? Clause, string ClauseTitle);
    }
}
