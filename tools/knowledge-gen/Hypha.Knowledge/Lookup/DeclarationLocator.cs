// ------------------------------------------------------------------------------------------------
// <copyright file="DeclarationLocator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Finds the line a model-library declaration starts on inside its package page.
    /// </summary>
    /// <remarks>
    /// A package page holds a whole upstream file, so a lookup that started at line 1 would return
    /// the top of <c>ISQSpaceTime</c> when asked for <c>ISQSpaceTime::length</c>. The page's fenced
    /// source block is searched for each segment of the qualified name in turn, each one inside the
    /// body the previous one opened (tracked by brace depth), and the last one together with the
    /// keyword the index recorded for it. That tells a member apart from a same-named member of a
    /// nested or sibling scope. When the nesting cannot be followed, a plain search in order is the
    /// fallback, and when even that fails the caller starts at the top of the page.
    /// </remarks>
    internal static class DeclarationLocator
    {
        private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

        /// <summary>Finds the declaration.</summary>
        /// <returns>The 1-based line number, or <c>null</c> when the declaration cannot be located.</returns>
        public static int? Find(IReadOnlyList<string> lines, string qualifiedName, string keyword)
        {
            ArgumentNullException.ThrowIfNull(lines);
            ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedName);
            ArgumentNullException.ThrowIfNull(keyword);

            var (first, end) = SourceBlock(lines);
            var depths = DepthsBefore(lines, first, end);
            var segments = qualifiedName.Split("::", StringSplitOptions.RemoveEmptyEntries);
            var patterns = segments
                .Select((segment, index) => index == segments.Length - 1
                    ? Declared(segment, keyword)
                    : new Regex(NamePattern(segment), RegexOptions.None, MatchTimeout))
                .ToList();

            var nested = FindNested(lines, depths, first, end, patterns);
            if (nested is not null)
            {
                return nested + 1;
            }

            // The nesting could not be followed (an unusual layout of braces, say): settle for the
            // segments in order, ignoring depth, then for the bare name anywhere after the scopes.
            var position = first;
            for (var index = 0; index < patterns.Count - 1; index++)
            {
                position = Search(lines, position, end, patterns[index]) ?? position;
            }

            var line = Search(lines, position, end, patterns[^1])
                       ?? Search(lines, position, end, new Regex(NamePattern(segments[^1]), RegexOptions.None, MatchTimeout));

            return line + 1;
        }

        /// <summary>Follows the segments down the scopes, each one directly inside the previous one.</summary>
        private static int? FindNested(
            IReadOnlyList<string> lines, int[] depths, int first, int end, IReadOnlyList<Regex> patterns)
        {
            var position = first;
            var depth = depths[first];

            for (var index = 0; index < patterns.Count; index++)
            {
                int? found = null;

                for (var line = position; line < end; line++)
                {
                    // The scope closed: what follows belongs to a sibling, not to this scope.
                    if (depths[line] < depth)
                    {
                        break;
                    }

                    if (depths[line] == depth && patterns[index].IsMatch(lines[line]))
                    {
                        found = line;
                        break;
                    }
                }

                if (found is null)
                {
                    return null;
                }

                if (index == patterns.Count - 1)
                {
                    return found;
                }

                position = found.Value + 1;
                depth++;
            }

            return null;
        }

        /// <summary>
        /// The brace depth at the start of each line of the source block: the number of bodies open
        /// there. Braces inside comments and quoted text do not count.
        /// </summary>
        private static int[] DepthsBefore(IReadOnlyList<string> lines, int first, int end)
        {
            var depths = new int[Math.Max(lines.Count, 1) + 1];
            var depth = 0;
            var inBlockComment = false;

            for (var index = first; index < end; index++)
            {
                depths[index] = depth;
                var text = lines[index];

                for (var at = 0; at < text.Length; at++)
                {
                    var character = text[at];
                    var next = at + 1 < text.Length ? text[at + 1] : '\0';

                    if (inBlockComment)
                    {
                        if (character == '*' && next == '/')
                        {
                            inBlockComment = false;
                            at++;
                        }

                        continue;
                    }

                    if (character == '/' && next == '/')
                    {
                        break;
                    }

                    if (character == '/' && next == '*')
                    {
                        inBlockComment = true;
                        at++;
                        continue;
                    }

                    if (character is '\'' or '"')
                    {
                        var close = text.IndexOf(character, at + 1);
                        at = close < 0 ? text.Length : close;
                        continue;
                    }

                    if (character == '{')
                    {
                        depth++;
                    }
                    else if (character == '}')
                    {
                        depth = Math.Max(0, depth - 1);
                    }
                }
            }

            depths[end] = depth;

            return depths;
        }

        /// <summary>The lines between the page's first code fence and its closing fence.</summary>
        private static (int First, int End) SourceBlock(IReadOnlyList<string> lines)
        {
            var open = -1;

            for (var index = 0; index < lines.Count; index++)
            {
                if (!lines[index].StartsWith("```", StringComparison.Ordinal))
                {
                    continue;
                }

                if (open < 0)
                {
                    open = index;
                    continue;
                }

                return (open + 1, index);
            }

            return (0, lines.Count);
        }

        private static int? Search(IReadOnlyList<string> lines, int from, int end, Regex pattern)
        {
            for (var index = from; index < end; index++)
            {
                if (pattern.IsMatch(lines[index]))
                {
                    return index;
                }
            }

            return null;
        }

        /// <summary>The name preceded by the keyword that declares it, or the bare name without a keyword.</summary>
        private static Regex Declared(string name, string keyword)
        {
            var pattern = string.IsNullOrWhiteSpace(keyword)
                ? NamePattern(name)
                : $@"(?<![\w]){KeywordPattern(keyword)}\s+{NamePattern(name)}";

            return new Regex(pattern, RegexOptions.None, MatchTimeout);
        }

        /// <summary>A name as declared: bare, or quoted when it is not a plain identifier.</summary>
        private static string NamePattern(string name)
        {
            var escaped = Regex.Escape(name.Trim('\''));

            return $@"(?:'{escaped}'|(?<![\w']){escaped}(?![\w']))";
        }

        /// <summary>The recorded keyword, with any run of whitespace allowed between its words.</summary>
        private static string KeywordPattern(string keyword) =>
            string.Join(
                @"\s+",
                keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape));
    }
}
