// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordIndexer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Hypha.Knowledge.Grammar;

    /// <summary>
    /// Builds one grammar's keyword table from its productions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A literal is every single-quoted text in a production's body. Words are <c>reserved</c> when the
    /// grammar's <c>RESERVED_KEYWORD</c> production declares them and <c>contextual</c> otherwise;
    /// anything else is an <c>operator</c>, except plain punctuation (<c>; , { } ( ) [ ]</c>), which is
    /// left out. The lexical productions (names, numbers, strings and comments) come before
    /// <c>RESERVED_KEYWORD</c> in both grammars, and their literals are character patterns rather than
    /// syntax, so they are left out too, as are <c>RESERVED_KEYWORD</c> and <c>RESERVED_SYMBOL</c>.
    /// </para>
    /// <para>
    /// A metaclass's syntax is what its productions write plus what they reach through <b>bare calls</b>:
    /// a reference to another production that is not the value of an assignment. A bare call goes on
    /// building the same element (<c>PartUsage = OccurrenceUsagePrefix 'part' Usage</c>), while an
    /// assigned one builds another (<c>ownedRelationship += OwnedRedefinition</c>), so only bare calls
    /// are followed. A bare call into a production that builds a metaclass is followed only when that
    /// metaclass is the caller's own or one of its supertypes: <c>BasicUsagePrefix : Usage</c> fills in
    /// a part usage, but <c>StructureUsageElement : Usage = ... | PartUsage</c> hands over to a part
    /// usage, which is not a usage's syntax. Visits are tracked, so a recursive grammar cannot loop.
    /// </para>
    /// <para>
    /// A grammar can call productions it does not define: the SysML grammar calls KerML's
    /// <c>OwnedExpression</c>, for example, so SysML text holds KerML's expressions. Given that
    /// <see cref="SharedGrammar"/>, the table also lists every literal the shared productions reachable
    /// from those calls write. Every reference is followed, assigned or not, since an expression is made
    /// of assigned subexpressions, but only within the clauses that define the called productions (and
    /// their subclauses): KerML's expression bodies refer on to its function bodies and from there to all
    /// of its declarations, which SysML text does not hold. Such an entry names the shared grammar's
    /// productions and metaclasses, and the shared grammar in <see cref="KeywordEntry.SyntaxFrom"/>. A literal the grammar writes in a
    /// production of its own keeps its own entry; one it only reserves takes the shared syntax.
    /// </para>
    /// </remarks>
    public sealed partial class KeywordIndexer : IKeywordIndexer
    {
        /// <summary>The production whose literals are the reserved keywords.</summary>
        private const string ReservedKeywordProduction = "RESERVED_KEYWORD";

        /// <summary>The production whose literals are the reserved symbols.</summary>
        private const string ReservedSymbolProduction = "RESERVED_SYMBOL";

        /// <summary>
        /// What a literal, cross-reference or action is replaced with before calls are read: a token that is
        /// neither a production reference nor an assignment operator.
        /// </summary>
        private const string Placeholder = " _ ";

        /// <summary>Literals that only delimit, and so introduce nothing.</summary>
        private static readonly HashSet<string> Punctuation = new(StringComparer.Ordinal) { ";", ",", "{", "}", "(", ")", "[", "]" };

        /// <summary>Matches a production header at the start of its body, so the header is not read as a call.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*[ \t]*(?::[ \t]*[A-Za-z][A-Za-z0-9_]*[ \t]*)?=")]
        private static partial Regex Header();

        /// <summary>Matches a single-quoted literal, capturing its text in group 1.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"'([^']+)'")]
        private static partial Regex Literal();

        /// <summary>Matches a word: a literal that is a keyword rather than an operator.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
        private static partial Regex Word();

        /// <summary>Matches a cross-reference, <c>[QualifiedName]</c>, or an action, <c>{ isPortion = true }</c>.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"\[[^\]]*\]|\{[^}]*\}")]
        private static partial Regex CrossReferenceOrAction();

        /// <summary>Matches a capitalised identifier, a production reference, capturing it in group 1.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"\b([A-Z][A-Za-z0-9_]*)\b")]
        private static partial Regex Reference();

        /// <summary>Matches an assignment operator that ends just before the given position.</summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"(?:\+=|\?=|=)[ \t\r\n]*$")]
        private static partial Regex AssignedBefore();

        /// <summary>Builds the keyword table of one grammar.</summary>
        /// <param name="grammar">The grammar's key, such as <c>sysml</c>, written as the <see cref="KeywordEntry.SyntaxFrom"/> of its own entries.</param>
        /// <param name="productions">The grammar's parsed productions, in document order.</param>
        /// <param name="reserved">The keywords its <c>RESERVED_KEYWORD</c> production declares.</param>
        /// <param name="ancestors">
        /// Each metaclass of the metamodel with the names of all its supertypes; nothing outside these is
        /// reported as a metaclass.
        /// </param>
        /// <param name="shared">
        /// The grammar whose productions this one calls without defining them, or <c>null</c> for none.
        /// </param>
        /// <returns>The grammar's entries, keyed and ordered ordinally by literal, with their counts.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="productions"/>, <paramref name="reserved"/> or <paramref name="ancestors"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="grammar"/> is <c>null</c>, empty or white space.</exception>
        public KeywordGrammar Build(
            string grammar,
            IReadOnlyList<Production> productions,
            IReadOnlyList<string> reserved,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors,
            SharedGrammar? shared = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(grammar);
            ArgumentNullException.ThrowIfNull(productions);
            ArgumentNullException.ThrowIfNull(reserved);
            ArgumentNullException.ThrowIfNull(ancestors);

            var reservedWords = reserved.ToHashSet(StringComparer.Ordinal);
            var own = Analyse(productions, ancestors);
            var writers = Writers(own, own.Syntax.Keys);

            foreach (var word in reservedWords)
            {
                Bucket(writers, word);
            }

            var entries = Entries(grammar, writers, Reached(own, own.Syntax.Keys, ancestors), reservedWords);

            if (shared is not null)
            {
                var lender = Analyse(shared.Productions, ancestors);
                var defined = productions.Select(production => production.Name).ToHashSet(StringComparer.Ordinal);
                var clauses = shared.Productions
                    .GroupBy(production => production.Name, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Clause, StringComparer.Ordinal);
                var reachable = Closure(
                    own.Syntax.Values.SelectMany(References).Where(name => !defined.Contains(name)),
                    lender,
                    clauses);

                var borrowed = Entries(
                    shared.Name,
                    Writers(lender, reachable),
                    Reached(lender, reachable, ancestors),
                    reservedWords);

                foreach (var (literal, entry) in borrowed)
                {
                    if (!entries.TryGetValue(literal, out var current) || current.Productions.Count == 0)
                    {
                        entries[literal] = entry;
                    }
                }
            }

            return new KeywordGrammar(
                new KeywordCounts(
                    entries.Values.Count(entry => entry.Kind == KeywordEntry.Reserved),
                    entries.Values.Count(entry => entry.Kind == KeywordEntry.Contextual),
                    entries.Values.Count(entry => entry.Kind == KeywordEntry.Operator)),
                entries);
        }

        /// <summary>What a grammar's productions write, call bare and build.</summary>
        /// <param name="productions">The productions, in document order.</param>
        /// <param name="ancestors">The metaclasses.</param>
        /// <returns>The analysis.</returns>
        private static Analysis Analyse(
            IReadOnlyList<Production> productions,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors)
        {
            var syntax = Syntax(productions);

            return new Analysis(
                syntax,
                syntax.ToDictionary(
                    entry => entry.Key,
                    entry => Literals(entry.Value).Where(literal => !Punctuation.Contains(literal)).ToHashSet(StringComparer.Ordinal),
                    StringComparer.Ordinal),
                syntax.ToDictionary(
                    entry => entry.Key,
                    entry => BareCalls(entry.Value).Where(syntax.ContainsKey).Distinct(StringComparer.Ordinal).ToList(),
                    StringComparer.Ordinal),
                productions
                    .GroupBy(production => production.Name, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => Built(group.Last(), ancestors), StringComparer.Ordinal));
        }

        /// <summary>The productions, among some, that write each literal.</summary>
        /// <param name="analysis">The grammar.</param>
        /// <param name="among">The productions to consider.</param>
        /// <returns>The writing productions, by literal.</returns>
        private static SortedDictionary<string, SortedSet<string>> Writers(Analysis analysis, IEnumerable<string> among)
        {
            var writers = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

            foreach (var production in among)
            {
                foreach (var literal in analysis.Literals[production])
                {
                    Bucket(writers, literal).Add(production);
                }
            }

            return writers;
        }

        /// <summary>The metaclasses whose syntax includes each literal, starting from some productions.</summary>
        /// <param name="analysis">The grammar.</param>
        /// <param name="starts">The productions whose metaclasses are considered.</param>
        /// <param name="ancestors">The supertypes of every metaclass.</param>
        /// <returns>Literal, then metaclass, then the building productions, the writing productions reached and whether one writes it directly.</returns>
        private static Dictionary<string, Dictionary<string, (SortedSet<string> Productions, SortedSet<string> Through, bool Direct)>> Reached(
            Analysis analysis,
            IEnumerable<string> starts,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors)
        {
            var reached = new Dictionary<string, Dictionary<string, (SortedSet<string> Productions, SortedSet<string> Through, bool Direct)>>(
                StringComparer.Ordinal);

            foreach (var start in starts)
            {
                var metaclass = analysis.Builds[start];
                if (metaclass is null)
                {
                    continue;
                }

                foreach (var visited in Reach(start, metaclass, analysis.Calls, analysis.Builds, ancestors))
                {
                    foreach (var literal in analysis.Literals[visited])
                    {
                        if (!reached.TryGetValue(literal, out var byMetaclass))
                        {
                            byMetaclass = new(StringComparer.Ordinal);
                            reached[literal] = byMetaclass;
                        }

                        if (!byMetaclass.TryGetValue(metaclass, out var row))
                        {
                            row = (new SortedSet<string>(StringComparer.Ordinal), new SortedSet<string>(StringComparer.Ordinal), false);
                        }

                        row.Productions.Add(start);
                        row.Through.Add(visited);
                        byMetaclass[metaclass] = row with { Direct = row.Direct || string.Equals(visited, start, StringComparison.Ordinal) };
                    }
                }
            }

            return reached;
        }

        /// <summary>The entries of a table.</summary>
        /// <param name="syntaxFrom">The grammar whose productions write the literals.</param>
        /// <param name="writers">The writing productions, by literal.</param>
        /// <param name="reached">The metaclasses reached, by literal.</param>
        /// <param name="reserved">The reserved keywords of the grammar the table is for.</param>
        /// <returns>The entries, keyed ordinally by literal.</returns>
        private static SortedDictionary<string, KeywordEntry> Entries(
            string syntaxFrom,
            SortedDictionary<string, SortedSet<string>> writers,
            Dictionary<string, Dictionary<string, (SortedSet<string> Productions, SortedSet<string> Through, bool Direct)>> reached,
            HashSet<string> reserved)
        {
            var entries = new SortedDictionary<string, KeywordEntry>(StringComparer.Ordinal);

            foreach (var (literal, written) in writers)
            {
                var metaclasses = reached.TryGetValue(literal, out var byMetaclass)
                    ? byMetaclass
                        .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                        .Select(entry => new KeywordMetaclass(
                            entry.Key,
                            entry.Value.Direct ? KeywordMetaclass.Direct : KeywordMetaclass.Helper,
                            [.. entry.Value.Productions],
                            [.. entry.Value.Through]))
                        .ToList()
                    : [];

                entries[literal] = new KeywordEntry(Kind(literal, reserved), syntaxFrom, [.. written], metaclasses);
            }

            return entries;
        }

        /// <summary>
        /// The productions of a grammar reachable from some names through any reference, within the
        /// clauses that define those names.
        /// </summary>
        /// <param name="roots">The names called from outside; those the grammar does not define are ignored.</param>
        /// <param name="analysis">The grammar.</param>
        /// <param name="clauses">The clause each of its productions is defined in, or <c>null</c>.</param>
        /// <returns>The reachable productions.</returns>
        private static HashSet<string> Closure(IEnumerable<string> roots, Analysis analysis, Dictionary<string, string?> clauses)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Stack<string>();

            foreach (var root in roots.Where(analysis.Syntax.ContainsKey))
            {
                if (seen.Add(root))
                {
                    pending.Push(root);
                }
            }

            var allowed = seen.Select(root => clauses[root]).ToHashSet();

            while (pending.Count > 0)
            {
                foreach (var callee in References(analysis.Syntax[pending.Pop()]).Where(analysis.Syntax.ContainsKey))
                {
                    if (Within(clauses[callee], allowed) && seen.Add(callee))
                    {
                        pending.Push(callee);
                    }
                }
            }

            return seen;
        }

        /// <summary>Whether a clause is one of some clauses or below one of them.</summary>
        /// <param name="clause">The clause, such as <c>8.2.5.8.1</c>, or <c>null</c> when the grammar gives none.</param>
        /// <param name="allowed">The clauses.</param>
        /// <returns><c>true</c> when it is, a missing clause matching only a missing clause.</returns>
        private static bool Within(string? clause, HashSet<string?> allowed) =>
            allowed.Contains(clause)
            || (clause is not null
                && allowed.Any(parent => parent is not null && clause.StartsWith(parent + ".", StringComparison.Ordinal)));

        /// <summary>
        /// Every production a production's syntax refers to, assigned or not, outside literals,
        /// cross-references and actions.
        /// </summary>
        /// <param name="syntax">The production's syntax.</param>
        /// <returns>The referenced names, repeats included.</returns>
        private static IEnumerable<string> References(string syntax) =>
            Reference().Matches(Masked(syntax)).Select(match => match.Groups[1].Value);

        /// <summary>
        /// A production's syntax with each literal, cross-reference and action replaced by a placeholder
        /// rather than a space, so the call after an assigned one
        /// (<c>isComposite ?= 'merge' UsageDeclaration</c>) does not look assigned too.
        /// </summary>
        /// <param name="syntax">The production's syntax.</param>
        /// <returns>The masked syntax.</returns>
        private static string Masked(string syntax) =>
            CrossReferenceOrAction().Replace(Literal().Replace(syntax, Placeholder), Placeholder);

        /// <summary>
        /// The syntax of every production that is not lexical: its body without the header line's
        /// <c>Name : Type =</c> and without comment lines. A later production with the same name replaces
        /// an earlier one.
        /// </summary>
        /// <param name="productions">The productions, in document order.</param>
        /// <returns>The syntax, by production name.</returns>
        private static Dictionary<string, string> Syntax(IReadOnlyList<Production> productions)
        {
            var reservedAt = -1;

            for (var index = 0; index < productions.Count; index++)
            {
                if (string.Equals(productions[index].Name, ReservedKeywordProduction, StringComparison.Ordinal))
                {
                    reservedAt = index;
                    break;
                }
            }

            var syntax = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var production in productions.Skip(reservedAt + 1))
            {
                if (string.Equals(production.Name, ReservedSymbolProduction, StringComparison.Ordinal))
                {
                    continue;
                }

                var lines = Header().Replace(production.Body, string.Empty, 1)
                    .Split('\n')
                    .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal));

                syntax[production.Name] = string.Join('\n', lines);
            }

            return syntax;
        }

        /// <summary>The literals a production's syntax writes.</summary>
        /// <param name="syntax">The production's syntax.</param>
        /// <returns>The literals, repeats included.</returns>
        private static IEnumerable<string> Literals(string syntax) =>
            Literal().Matches(syntax).Select(match => match.Groups[1].Value);

        /// <summary>
        /// The productions a production calls bare: capitalised references outside literals,
        /// cross-references and actions that are not the value of an assignment.
        /// </summary>
        /// <param name="syntax">The production's syntax.</param>
        /// <returns>The called production names, repeats included.</returns>
        private static IEnumerable<string> BareCalls(string syntax)
        {
            var text = Masked(syntax);
            var calls = new List<string>();
            var depth = 0;
            var assignedDepth = -1;

            // An assigned group, "ownedRelatedElement += ( LiteralExpression | FeatureReferenceExpression )",
            // is assigned as a whole, so nothing inside it is a bare call.
            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];

                if (character == '(')
                {
                    if (assignedDepth < 0 && AssignedBefore().IsMatch(text[..index]))
                    {
                        assignedDepth = depth;
                    }

                    depth++;
                }
                else if (character == ')')
                {
                    depth--;

                    if (depth == assignedDepth)
                    {
                        assignedDepth = -1;
                    }
                }
                else if (char.IsAsciiLetterUpper(character) && (index == 0 || !IsIdentifier(text[index - 1])))
                {
                    var match = Reference().Match(text, index);

                    if (assignedDepth < 0 && !AssignedBefore().IsMatch(text[..index]))
                    {
                        calls.Add(match.Groups[1].Value);
                    }

                    index += match.Length - 1;
                }
            }

            return calls;
        }

        /// <summary>Whether a character can be part of an identifier.</summary>
        /// <param name="character">The character.</param>
        /// <returns><c>true</c> for a letter, a digit or an underscore.</returns>
        private static bool IsIdentifier(char character) => char.IsAsciiLetterOrDigit(character) || character == '_';

        /// <summary>The metaclass a production builds: its declared type, else its own name, else nothing.</summary>
        /// <param name="production">The production.</param>
        /// <param name="ancestors">The metaclasses.</param>
        /// <returns>The metaclass, or <c>null</c> when neither candidate is one.</returns>
        private static string? Built(Production production, IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors) =>
            new[] { production.Produces, production.Name }
                .FirstOrDefault(candidate => !string.IsNullOrEmpty(candidate) && ancestors.ContainsKey(candidate));

        /// <summary>The productions a production reaches through bare calls that go on building its metaclass.</summary>
        /// <param name="start">The production that builds the metaclass.</param>
        /// <param name="metaclass">The metaclass it builds.</param>
        /// <param name="calls">The bare calls of every production.</param>
        /// <param name="builds">The metaclass every production builds, or <c>null</c>.</param>
        /// <param name="ancestors">The supertypes of every metaclass.</param>
        /// <returns><paramref name="start"/> and every production it reaches.</returns>
        private static HashSet<string> Reach(
            string start,
            string metaclass,
            Dictionary<string, List<string>> calls,
            Dictionary<string, string?> builds,
            IReadOnlyDictionary<string, IReadOnlySet<string>> ancestors)
        {
            var supertypes = ancestors[metaclass];
            var seen = new HashSet<string>(StringComparer.Ordinal) { start };
            var pending = new Stack<string>();
            pending.Push(start);

            while (pending.Count > 0)
            {
                foreach (var callee in calls[pending.Pop()])
                {
                    var built = builds[callee];
                    var continues = built is null
                        || string.Equals(built, metaclass, StringComparison.Ordinal)
                        || supertypes.Contains(built);

                    if (continues && seen.Add(callee))
                    {
                        pending.Push(callee);
                    }
                }
            }

            return seen;
        }

        /// <summary>The kind of a literal.</summary>
        /// <param name="literal">The literal.</param>
        /// <param name="reserved">The reserved keywords.</param>
        /// <returns>Its <see cref="KeywordEntry.Kind"/>.</returns>
        private static string Kind(string literal, HashSet<string> reserved) =>
            reserved.Contains(literal) ? KeywordEntry.Reserved
            : Word().IsMatch(literal) ? KeywordEntry.Contextual
            : KeywordEntry.Operator;

        /// <summary>What a grammar's productions write, call bare and build.</summary>
        /// <param name="Syntax">The syntax of every production that is not lexical, by name.</param>
        /// <param name="Literals">The literals each production writes, punctuation left out.</param>
        /// <param name="Calls">The productions each production calls bare.</param>
        /// <param name="Builds">The metaclass every production builds, or <c>null</c>.</param>
        private sealed record Analysis(
            Dictionary<string, string> Syntax,
            Dictionary<string, HashSet<string>> Literals,
            Dictionary<string, List<string>> Calls,
            Dictionary<string, string?> Builds);

        /// <summary>Gets the set of productions writing a literal, creating it when absent.</summary>
        /// <param name="writers">The productions writing each literal.</param>
        /// <param name="literal">The literal.</param>
        /// <returns>The literal's set.</returns>
        private static SortedSet<string> Bucket(SortedDictionary<string, SortedSet<string>> writers, string literal)
        {
            if (!writers.TryGetValue(literal, out var bucket))
            {
                bucket = new SortedSet<string>(StringComparer.Ordinal);
                writers[literal] = bucket;
            }

            return bucket;
        }
    }
}
