// ------------------------------------------------------------------------------------------------
// <copyright file="DeclarationScanner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Finds named declarations by walking the token stream once, tracking nesting depth and the
    /// enclosing scope's name - not by parsing to a syntax tree.
    /// </summary>
    /// <remarks>
    /// This deliberately stops well short of a real parser: it does not resolve <c>:&gt;</c>/<c>:&gt;&gt;</c>
    /// chains, does not know a feature's type, and cannot tell a public member from a private one. It
    /// only answers "what names does this file declare and under what path" - enough to build a
    /// qualified-name index, nothing more.
    /// </remarks>
    public sealed class DeclarationScanner : IDeclarationScanner
    {
        /// <summary>
        /// KerML's own classifier/feature/package vocabulary (Clause 8.2.2, 8.2.4-8.2.6). None of the
        /// metaclasses these build - Classifier, DataType, Class, Structure, Association,
        /// AssociationStructure, Behavior, Function, Predicate, Interaction, Metaclass, Feature, Step,
        /// Expression, BooleanExpression, Invariant, Connector, BindingConnector, Succession, Package,
        /// LibraryPackage - end in "Definition"/"Usage", so <see cref="TextualNotation.ISurfaceForms"/>
        /// cannot derive them. Verified against <c>KerML-textual-bnf.kebnf</c>'s own concrete syntax
        /// productions (Clause 8.2.4.3, 8.2.5); longest phrases are listed first only for readability;
        /// <see cref="Scan"/> re-sorts regardless.
        /// </summary>
        private static readonly string[] CoreKeywords =
        [
            "standard library package",
            "library package",
            "package",
            "assoc struct",
            "assoc",
            "classifier",
            "datatype",
            "class",
            "struct",
            "behavior",
            "function",
            "predicate",
            "interaction",
            "metaclass",
            "feature",
            "step",
            "expr",
            "bool",
            "inv",
            "connector",
            "binding",
            "succession",
            "flow",
        ];

        /// <summary>
        /// SysML keywords the grammar spells differently than <see cref="TextualNotation.ISurfaceForms"/>'s
        /// mechanical CamelCase-word derivation would produce, so they are missed unless listed here
        /// too. Verified against <c>SysML-textual-bnf.kebnf</c>: <c>'calc' 'def'</c> builds
        /// <c>CalculationDefinition</c> (not "calculation def"), <c>'enum' 'def'</c> builds
        /// <c>EnumerationDefinition</c> (not "enumeration def").
        /// </summary>
        private static readonly string[] IrregularSysmlKeywords =
        [
            "calc def",
            "calc",
            "enum def",
            "enum",
        ];

        /// <summary>
        /// SysML's named-membership keywords (Clause 8.2.2.20-8.2.2.24: Requirements, Cases, Analysis
        /// Cases, Verification Cases): each wraps a specific <c>Usage</c> subtype
        /// (<c>SubjectUsage : ReferenceUsage</c>, <c>ActorUsage : PartUsage</c>, ...) rather than
        /// following the plain Definition/Usage convention, so neither <see cref="TextualNotation.ISurfaceForms"/>
        /// nor <see cref="CoreKeywords"/> covers them. Verified against <c>SysML-textual-bnf.kebnf</c>.
        /// </summary>
        private static readonly string[] MembershipKeywords =
        [
            "subject",
            "actor",
            "stakeholder",
            "objective",
            "frame",
            "verify",
        ];

        /// <summary>
        /// Connecting keywords whose body-less form links two <b>existing</b> features by reference
        /// (<c>succession source then self;</c>, from Transfers.kerml) rather than declaring a new
        /// one. The word right after the keyword is indistinguishable, at this scanner's level, from a
        /// fresh name in that form - every genuinely named connector/succession/binding/flow observed
        /// in the standard library carries a body, so a body is required before one of these is
        /// trusted as a declaration rather than discarded as a reference.
        /// </summary>
        private static readonly HashSet<string> RequiresBodyToDeclare =
            new(StringComparer.Ordinal) { "connector", "binding", "succession", "flow" };

        /// <inheritdoc/>
        public IReadOnlyList<LibraryDeclaration> Scan(
            string modelText, IEnumerable<string> surfaceForms, IEnumerable<string> reservedKeywords)
        {
            ArgumentNullException.ThrowIfNull(modelText);
            ArgumentNullException.ThrowIfNull(surfaceForms);
            ArgumentNullException.ThrowIfNull(reservedKeywords);

            var keywords = BuildKeywordTable(surfaceForms);
            var reserved = new HashSet<string>(reservedKeywords, StringComparer.Ordinal);
            var tokens = Tokenize(modelText);

            var declarations = new List<LibraryDeclaration>();
            var scope = new List<(int Depth, string Name)>();
            var depth = 0;
            var index = 0;

            while (index < tokens.Count)
            {
                var token = tokens[index];

                switch (token.Kind)
                {
                    case TokenKind.OpenBrace:
                        depth++;
                        index++;
                        continue;

                    case TokenKind.CloseBrace:
                        if (scope.Count > 0 && scope[^1].Depth == depth)
                        {
                            scope.RemoveAt(scope.Count - 1);
                        }

                        depth = Math.Max(0, depth - 1);
                        index++;
                        continue;

                    case TokenKind.Word:
                        var match = MatchKeyword(tokens, index, keywords);
                        if (match is not null)
                        {
                            index += match.Value.Words;
                            var name = ReadDeclaredName(tokens, ref index, keywords, reserved);

                            if (name is not null)
                            {
                                index = SkipToBodyOrStatementEnd(tokens, index, out var opensBody);

                                if (opensBody || !RequiresBodyToDeclare.Contains(match.Value.Keyword))
                                {
                                    var qualifiedName = scope.Count == 0
                                        ? name
                                        : string.Join("::", scope.Select(frame => frame.Name)) + "::" + name;

                                    declarations.Add(new LibraryDeclaration(qualifiedName, match.Value.Keyword));
                                }

                                if (opensBody)
                                {
                                    depth++;
                                    scope.Add((depth, name));
                                }

                                continue;
                            }

                            // No name followed - a redefinition target, or another keyword right
                            // behind this one. `index` already sits past the matched keyword; let
                            // whatever is there get its own turn at the top of the loop rather than
                            // skipping it with a further increment.
                            continue;
                        }

                        index++;
                        continue;

                    default:
                        index++;
                        continue;
                }
            }

            return declarations;
        }

        /// <summary>
        /// This release's surface forms plus the always-on KerML core vocabulary, split into words and
        /// ordered longest-first so e.g. "standard library package" is tried before "package".
        /// </summary>
        private static List<(string[] Words, string Keyword)> BuildKeywordTable(IEnumerable<string> surfaceForms)
        {
            return CoreKeywords
                .Concat(IrregularSysmlKeywords)
                .Concat(MembershipKeywords)
                .Concat(surfaceForms)
                .Where(form => !string.IsNullOrWhiteSpace(form))
                .Distinct(StringComparer.Ordinal)
                .Select(form => (Words: form.Split(' ', StringSplitOptions.RemoveEmptyEntries), Keyword: form))
                .OrderByDescending(entry => entry.Words.Length)
                .ToList();
        }

        /// <summary>The longest keyword phrase starting exactly at <paramref name="index"/>, if any.</summary>
        private static (string Keyword, int Words)? MatchKeyword(
            IReadOnlyList<Token> tokens, int index, List<(string[] Words, string Keyword)> keywords)
        {
            foreach (var (words, keyword) in keywords)
            {
                if (index + words.Length > tokens.Count)
                {
                    continue;
                }

                var matches = true;
                for (var offset = 0; offset < words.Length; offset++)
                {
                    var candidate = tokens[index + offset];
                    if (candidate.Kind != TokenKind.Word
                        || !string.Equals(candidate.Text, words[offset], StringComparison.Ordinal))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    return (keyword, words.Length);
                }
            }

            return null;
        }

        /// <summary>
        /// The identifier or quoted name right after a matched keyword, or <c>null</c> when none
        /// follows - a redefinition-only declaration (<c>feature :&gt;&gt; mass;</c>), a keyword used
        /// only as a modifier before another keyword (<c>succession flow x { ... }</c>), or a bare
        /// <c>attribute</c> immediately followed by the reserved word <c>def</c> because "attribute
        /// def" was not registered as its own two-word phrase - a reserved word can never be an
        /// unquoted identifier, so seeing one here is proof this is not the name. <paramref
        /// name="index"/> is left unmoved when there is no name, so the token that stopped the search -
        /// very often another keyword - gets its own turn at the top of the scan.
        /// </summary>
        /// <remarks>
        /// One reserved word is skipped rather than treated as disqualifying: <c>all</c>, the
        /// <c>isSufficient</c> marker the grammar allows directly before a classifier's or feature's
        /// own identification (<c>(isSufficient ?= 'all')? Identification</c>, repeated across
        /// <c>ClassifierDeclaration</c>, <c>FeatureDeclaration</c> and the anonymous connector/binding/
        /// succession forms) - unlike every other reserved word, it is a prefix flag on the name that
        /// follows, not a sign that there is no name at all.
        /// </remarks>
        private static string? ReadDeclaredName(
            IReadOnlyList<Token> tokens,
            ref int index,
            List<(string[] Words, string Keyword)> keywords,
            HashSet<string> reservedKeywords)
        {
            if (index < tokens.Count
                && tokens[index] is { Kind: TokenKind.Word, Text: "all" })
            {
                index++;
            }

            if (index >= tokens.Count)
            {
                return null;
            }

            var token = tokens[index];

            if (token.Kind == TokenKind.Quoted)
            {
                index++;
                return token.Text;
            }

            if (token.Kind == TokenKind.Word
                && !reservedKeywords.Contains(token.Text)
                && MatchKeyword(tokens, index, keywords) is null)
            {
                index++;
                return token.Text;
            }

            return null;
        }

        /// <summary>
        /// Advances past everything between a declared name and its first <c>{</c> or <c>;</c> at the
        /// current depth - the supertype list, multiplicity and modifiers a name can be followed by
        /// before its body (or the lack of one) is known.
        /// </summary>
        private static int SkipToBodyOrStatementEnd(IReadOnlyList<Token> tokens, int index, out bool opensBody)
        {
            while (index < tokens.Count
                && tokens[index].Kind is not (TokenKind.OpenBrace or TokenKind.Semicolon))
            {
                index++;
            }

            if (index >= tokens.Count)
            {
                opensBody = false;
                return index;
            }

            opensBody = tokens[index].Kind == TokenKind.OpenBrace;

            return index + 1;
        }

        private static List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var length = text.Length;
            var position = 0;

            while (position < length)
            {
                var current = text[position];

                if (char.IsWhiteSpace(current))
                {
                    position++;
                    continue;
                }

                if (current == '/' && position + 1 < length && text[position + 1] == '/')
                {
                    while (position < length && text[position] != '\n')
                    {
                        position++;
                    }

                    continue;
                }

                if (current == '/' && position + 1 < length && text[position + 1] == '*')
                {
                    var end = text.IndexOf("*/", position + 2, StringComparison.Ordinal);
                    position = end < 0 ? length : end + 2;
                    continue;
                }

                if (current is '\'' or '"')
                {
                    var end = text.IndexOf(current, position + 1);
                    var content = end < 0
                        ? text[(position + 1)..]
                        : text[(position + 1)..end];

                    tokens.Add(new Token(TokenKind.Quoted, content));
                    position = end < 0 ? length : end + 1;
                    continue;
                }

                if (current == '{')
                {
                    tokens.Add(new Token(TokenKind.OpenBrace, "{"));
                    position++;
                    continue;
                }

                if (current == '}')
                {
                    tokens.Add(new Token(TokenKind.CloseBrace, "}"));
                    position++;
                    continue;
                }

                if (current == ';')
                {
                    tokens.Add(new Token(TokenKind.Semicolon, ";"));
                    position++;
                    continue;
                }

                if (char.IsAsciiLetter(current) || current == '_')
                {
                    var start = position;
                    while (position < length
                        && (char.IsAsciiLetterOrDigit(text[position]) || text[position] == '_'))
                    {
                        position++;
                    }

                    tokens.Add(new Token(TokenKind.Word, text[start..position]));
                    continue;
                }

                // Everything else - ':', '>', '=', '?', '[', ']', ',', digits, ... - becomes its own
                // token rather than being skipped. That is what lets ReadDeclaredName tell "attribute
                // def LengthUnit" (a fresh name, directly adjacent) from "attribute :>> quantityDimension"
                // (a redefinition target, separated by operator tokens) apart: silently skipping
                // punctuation would make the two indistinguishable.
                tokens.Add(new Token(TokenKind.Other, current.ToString()));
                position++;
            }

            return tokens;
        }

        private enum TokenKind
        {
            Word,
            Quoted,
            OpenBrace,
            CloseBrace,
            Semicolon,
            Other,
        }

        private readonly record struct Token(TokenKind Kind, string Text);
    }
}
