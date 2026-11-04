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
    ///     an enclosing scope is a declaration of that name, introduced by the keyword the index
    ///     recorded for that scope, that opens a body - not a mention in a comment, and not a
    ///     reference such as <c>: LengthValue</c>, <c>:&gt; Base</c> or <c>specializes Usage</c>;
    ///   </item>
    ///   <item>
    ///     each segment is looked for directly inside the body the previous one opened, tracked by
    ///     brace depth, so a member is told apart from a same-named member of a nested or sibling
    ///     scope;
    ///   </item>
    ///   <item>the last segment has to follow the keyword the index recorded for it.</item>
    /// </list>
    /// Between a keyword and its name, <c>all</c> (<c>feature all x</c>) and a short name
    /// (<c>attribute &lt;m&gt; mass</c>) are allowed, as the declaration scanner allows them.
    /// When the nesting cannot be followed, the segments are searched for in order regardless of
    /// depth; when even that fails, the caller starts at the top of the page.
    /// </remarks>
    internal static class DeclarationLocator
    {
        private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

        /// <summary>Finds the declaration.</summary>
        /// <param name="lines">The package page.</param>
        /// <param name="qualifiedName">The declaration's qualified name, as the index keys it.</param>
        /// <param name="keyword">The keyword the index recorded for the declaration.</param>
        /// <param name="keywordOf">
        /// The keyword the index recorded for an enclosing scope, by its qualified name, or
        /// <c>null</c> when the index does not have the scope; such a scope is matched by name.
        /// </param>
        /// <returns>The 1-based line number, or <c>null</c> when the declaration cannot be located.</returns>
        public static int? Find(
            IReadOnlyList<string> lines, string qualifiedName, string keyword, Func<string, string?> keywordOf)
        {
            ArgumentNullException.ThrowIfNull(lines);
            ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedName);
            ArgumentNullException.ThrowIfNull(keyword);
            ArgumentNullException.ThrowIfNull(keywordOf);

            var (first, end) = SourceBlock(lines);
            var source = Read(lines, first, end);
            var segments = qualifiedName.Split("::", StringSplitOptions.RemoveEmptyEntries);
            var scopes = segments[..^1]
                .Select((segment, index) => keywordOf(string.Join("::", segments[..(index + 1)])) is { } scopeKeyword
                    ? Declared(segment, scopeKeyword)
                    : Pattern(ScopeNamePattern(segment)))
                .ToList();
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
            Source source, int first, int end, List<Regex> scopes, Regex member)
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
        private static int? BodyStart(string[] code, int line, Match match, int end)
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
        /// each line.
        /// </summary>
        private static Source Read(IReadOnlyList<string> lines, int first, int end)
        {
            var code = lines.ToArray();
            var depths = new int[lines.Count + 1];
            var scanner = new BraceScanner();

            for (var index = first; index < end; index++)
            {
                depths[index] = scanner.Depth;
                code[index] = scanner.Scan(lines[index]);
            }

            depths[end] = scanner.Depth;

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

        private static int? Search(string[] lines, int from, int end, Regex pattern)
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

        /// <summary>
        /// The name preceded by the keyword that declares it - optionally with <c>all</c> or a short
        /// name in between - or the bare name without a keyword.
        /// </summary>
        private static Regex Declared(string name, string keyword) =>
            Pattern(
                string.IsNullOrWhiteSpace(keyword)
                    ? NamePattern(name)
                    : $@"(?<![\w]){KeywordPattern(keyword)}\s+(?:all\s+)?(?:<[^>]*>\s*)?{NamePattern(name)}");

        /// <summary>
        /// An enclosing scope the index has no keyword for: its name, but not where a relationship
        /// keyword (<c>specializes Usage</c>, <c>subsets x</c>, <c>import P::*</c>, ...) refers to it.
        /// </summary>
        private static string ScopeNamePattern(string name) =>
            @"(?<!\b(?:specializes|subsets|redefines|references|subclassifier|subtype|subset|unions|intersects"
            + @"|differences|conjugates|chains|crosses|disjoint\s+from|inverse\s+of|typed\s+by|defined\s+by"
            + @"|featured\s+by|for|import|alias|of|from|to|by)\s+)"
            + NamePattern(name);

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

        /// <summary>
        /// Reads source lines in order, blanking out comments and counting the braces of the code
        /// that is left. Braces inside comments and quoted text do not count; a block comment may
        /// span lines, which is why one scanner reads a whole block.
        /// </summary>
        private sealed class BraceScanner
        {
            private bool inBlockComment;

            /// <summary>The number of bodies open at the current position.</summary>
            public int Depth { get; private set; }

            /// <summary>Reads one line.</summary>
            /// <returns>The line with its comments replaced by spaces, so columns are kept.</returns>
            public string Scan(string line)
            {
                var text = line.ToCharArray();
                var at = 0;

                while (at < text.Length)
                {
                    at = this.inBlockComment ? this.SkipComment(text, at) : this.ScanCode(text, at);
                }

                return new string(text);
            }

            /// <summary>Blanks a block comment up to and including its end, or to the end of the line.</summary>
            /// <returns>Where the code resumes.</returns>
            private int SkipComment(char[] text, int at)
            {
                var close = new string(text).IndexOf("*/", at, StringComparison.Ordinal);
                var stop = close < 0 ? text.Length : close + 2;

                Array.Fill(text, ' ', at, stop - at);
                this.inBlockComment = close < 0;

                return stop;
            }

            /// <summary>Reads one piece of code: a comment opening, quoted text, or a single character.</summary>
            /// <returns>The position after it.</returns>
            private int ScanCode(char[] text, int at)
            {
                var character = text[at];
                var next = at + 1 < text.Length ? text[at + 1] : '\0';

                if (character == '/' && next == '/')
                {
                    Array.Fill(text, ' ', at, text.Length - at);
                    return text.Length;
                }

                if (character == '/' && next == '*')
                {
                    Array.Fill(text, ' ', at, 2);
                    this.inBlockComment = true;
                    return at + 2;
                }

                if (character is '\'' or '"')
                {
                    var close = Array.IndexOf(text, character, at + 1);
                    return close < 0 ? text.Length : close + 1;
                }

                if (character == '{')
                {
                    this.Depth++;
                }
                else if (character == '}')
                {
                    this.Depth = Math.Max(0, this.Depth - 1);
                }

                return at + 1;
            }
        }

        /// <summary>A source block read for searching.</summary>
        /// <param name="Code">Every line of the page, with comments blanked out inside the block.</param>
        /// <param name="Depths">The brace depth at the start of each line, and after the block.</param>
        private sealed record Source(string[] Code, int[] Depths);
    }
}
