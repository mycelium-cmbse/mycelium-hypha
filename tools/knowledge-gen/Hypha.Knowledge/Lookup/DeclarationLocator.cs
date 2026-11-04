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
    /// source block is searched for each segment of the qualified name in turn:
    /// <list type="bullet">
    ///   <item>
    ///     an enclosing scope is a declaration of that name that opens a body - not a mention in a
    ///     comment, and not a type reference such as <c>: LengthValue</c> or <c>:&gt; Base</c>;
    ///   </item>
    ///   <item>
    ///     each segment is looked for directly inside the body the previous one opened, tracked by
    ///     brace depth, so a member is told apart from a same-named member of a nested or sibling
    ///     scope;
    ///   </item>
    ///   <item>the last segment has to follow the keyword the index recorded for it.</item>
    /// </list>
    /// When the nesting cannot be followed, the segments are searched for in order regardless of
    /// depth; when even that fails, the caller starts at the top of the page.
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
            var source = Read(lines, first, end);
            var segments = qualifiedName.Split("::", StringSplitOptions.RemoveEmptyEntries);
            var scopes = segments[..^1].Select(segment => Pattern(NamePattern(segment))).ToList();
            var member = Declared(segments[^1], keyword);

            var nested = FindNested(source, first, end, scopes, member);
            if (nested is not null)
            {
                return nested + 1;
            }

            // The nesting could not be followed (an unusual layout of braces, say): settle for the
            // segments in order, ignoring depth, then for the bare name anywhere after the scopes.
            var position = first;
            foreach (var scope in scopes)
            {
                position = Search(source.Code, position, end, scope) ?? position;
            }

            var line = Search(source.Code, position, end, member)
                       ?? Search(source.Code, position, end, Pattern(NamePattern(segments[^1])));

            return line + 1;
        }

        /// <summary>Follows the segments down the scopes, each one directly inside the previous one.</summary>
        private static int? FindNested(
            Source source, int first, int end, IReadOnlyList<Regex> scopes, Regex member)
        {
            var position = first;
            var depth = source.Depths[first];

            for (var index = 0; index < scopes.Count; index++)
            {
                var found = Next(source, position, end, depth, scopes[index], requireBody: true);
                if (found is null)
                {
                    return null;
                }

                var (line, match) = found.Value;
                var text = source.Code[line];
                var brace = text.IndexOf('{', match.Index + match.Length);

                // The body opens and closes on this one line: the rest of the qualified name has to be
                // in what follows the brace.
                if (brace >= 0 && source.Depths[line + 1] <= source.Depths[line])
                {
                    return Inline(text[(brace + 1)..], scopes.Skip(index + 1), member) ? line : null;
                }

                position = BodyStart(source.Code, line, match, end)!.Value;
                depth++;
            }

            return Next(source, position, end, depth, member, requireBody: false)?.Line;
        }

        /// <summary>The next match at exactly <paramref name="depth"/>, before the scope closes.</summary>
        private static (int Line, Match Match)? Next(
            Source source, int position, int end, int depth, Regex pattern, bool requireBody)
        {
            for (var line = position; line < end; line++)
            {
                if (source.Depths[line] < depth)
                {
                    // The scope closed: what follows belongs to a sibling, not to this scope.
                    return null;
                }

                if (source.Depths[line] != depth)
                {
                    continue;
                }

                var match = pattern.Match(source.Code[line]);
                if (match.Success && (!requireBody || BodyStart(source.Code, line, match, end) is not null))
                {
                    return (line, match);
                }
            }

            return null;
        }

        /// <summary>
        /// The first line of the body a declaration opens: the next line when the brace follows the
        /// name on its own line, the line after the brace when the brace is on a line of its own.
        /// </summary>
        /// <returns><c>null</c> when the declaration opens no body.</returns>
        private static int? BodyStart(IReadOnlyList<string> code, int line, Match match, int end)
        {
            if (code[line].IndexOf('{', match.Index + match.Length) >= 0)
            {
                return line + 1;
            }

            for (var next = line + 1; next < end; next++)
            {
                var text = code[next].Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                return text.StartsWith('{') ? next + 1 : null;
            }

            return null;
        }

        /// <summary>Whether the remaining segments appear, in order, in one line's text.</summary>
        private static bool Inline(string text, IEnumerable<Regex> scopes, Regex member)
        {
            foreach (var scope in scopes)
            {
                var match = scope.Match(text);
                if (!match.Success)
                {
                    return false;
                }

                text = text[(match.Index + match.Length)..];
            }

            return member.IsMatch(text);
        }

        /// <summary>
        /// The source block's lines with comments blanked out, and the brace depth at the start of
        /// each line. Braces inside comments and quoted text do not count.
        /// </summary>
        private static Source Read(IReadOnlyList<string> lines, int first, int end)
        {
            var code = lines.ToArray();
            var depths = new int[lines.Count + 1];
            var depth = 0;
            var inBlockComment = false;

            for (var index = first; index < end; index++)
            {
                depths[index] = depth;
                var text = lines[index].ToCharArray();

                for (var at = 0; at < text.Length; at++)
                {
                    var next = at + 1 < text.Length ? text[at + 1] : '\0';

                    if (inBlockComment)
                    {
                        if (text[at] == '*' && next == '/')
                        {
                            inBlockComment = false;
                            text[at + 1] = ' ';
                        }

                        text[at] = ' ';
                        continue;
                    }

                    if (text[at] == '/' && next == '/')
                    {
                        Array.Fill(text, ' ', at, text.Length - at);
                        break;
                    }

                    if (text[at] == '/' && next == '*')
                    {
                        inBlockComment = true;
                        text[at] = ' ';
                        continue;
                    }

                    if (text[at] is '\'' or '"')
                    {
                        var close = Array.IndexOf(text, text[at], at + 1);
                        at = close < 0 ? text.Length : close;
                        continue;
                    }

                    if (text[at] == '{')
                    {
                        depth++;
                    }
                    else if (text[at] == '}')
                    {
                        depth = Math.Max(0, depth - 1);
                    }
                }

                code[index] = new string(text);
            }

            depths[end] = depth;

            return new Source(code, depths);
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

        private static Regex Pattern(string pattern) => new(pattern, RegexOptions.None, MatchTimeout);

        /// <summary>The name preceded by the keyword that declares it, or the bare name without a keyword.</summary>
        private static Regex Declared(string name, string keyword) =>
            Pattern(
                string.IsNullOrWhiteSpace(keyword)
                    ? NamePattern(name)
                    : $@"(?<![\w]){KeywordPattern(keyword)}\s+{NamePattern(name)}");

        /// <summary>
        /// A name as declared: bare, or quoted when it is not a plain identifier - and not a reference
        /// to it, which follows <c>:</c>, <c>:&gt;</c>, <c>:&gt;&gt;</c>, <c>::</c>, <c>=</c> or <c>~</c>.
        /// </summary>
        private static string NamePattern(string name)
        {
            var escaped = Regex.Escape(name.Trim('\''));

            return $@"(?<![:>=~]\s*)(?:'{escaped}'|(?<![\w']){escaped}(?![\w']))";
        }

        /// <summary>The recorded keyword, with any run of whitespace allowed between its words.</summary>
        private static string KeywordPattern(string keyword) =>
            string.Join(
                @"\s+",
                keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape));

        /// <summary>A source block read for searching.</summary>
        /// <param name="Code">Every line of the page, with comments blanked out inside the block.</param>
        /// <param name="Depths">The brace depth at the start of each line, and after the block.</param>
        private sealed record Source(string[] Code, int[] Depths);
    }
}
