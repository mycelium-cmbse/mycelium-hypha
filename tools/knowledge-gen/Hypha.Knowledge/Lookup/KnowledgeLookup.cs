// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeLookup.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Resolves a metamodel or model-library name to its generated page in an installed release and
    /// returns one page of it, reading the release's <c>index.json</c> and never fetching or generating.
    /// </summary>
    /// <remarks>
    /// A name resolves in tiers, and the first tier with any match decides:
    /// <list type="number">
    ///   <item>the index key or qualified name, exactly;</item>
    ///   <item>the same, ignoring case;</item>
    ///   <item>a trailing part of the qualified name (<c>Real</c> for <c>ScalarValues::Real</c>), exactly;</item>
    ///   <item>the same, ignoring case.</item>
    /// </list>
    /// One match in a tier is the answer; several are <see cref="LookupStatus.Ambiguous"/>. Only when
    /// no tier matches are names that merely contain the request offered, as
    /// <see cref="LookupStatus.Candidates"/>: a substring hit is a suggestion, never an answer.
    /// </remarks>
    public sealed class KnowledgeLookup : IKnowledgeLookup
    {
        /// <summary>How many names a result lists at most.</summary>
        public const int MaxMatches = 20;

        /// <summary>The largest generated file a lookup reads.</summary>
        public const long MaxFileBytes = 64L * 1024 * 1024;

        /// <summary>How many links deep a path is followed before it is taken to be a loop.</summary>
        private const int MaxLinkDepth = 32;

        /// <summary>Where each release's generated knowledge lives, and which releases are installed.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeLookup"/> class.</summary>
        /// <param name="layout">Where each release's generated knowledge lives, and which releases are installed.</param>
        public KnowledgeLookup(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <summary>
        /// Looks up a name in the requested release, or the default one, and returns a page of the matched
        /// entry's generated file; for a model-library entry, the page starts at its declaration unless a start line
        /// is given.
        /// </summary>
        /// <param name="request">What to look up, in which release, and which page of its source to return.</param>
        /// <returns>
        /// One page of the matched entry's generated source when the name resolves to exactly one entry;
        /// otherwise the status and at most <see cref="MaxMatches"/> names to choose from, ordinally. A name
        /// that does not resolve is a normal result, not an exception.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The request has an empty name, a start line below 1 or past the end of the page, or a page size
        /// outside 1 to <see cref="LookupRequest.LargestPage"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The request's kind is not a <see cref="KnowledgeKind"/>.</exception>
        /// <exception cref="KnowledgeLookupException">
        /// The release is not installed, has no default, has not been generated, or its generated
        /// files are invalid, too large, or reached through a path that leaves the release's folder.
        /// </exception>
        public LookupResult Lookup(LookupRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("A lookup needs a non-empty name.", nameof(request));
            }

            if (request.StartLine is < 1)
            {
                throw new ArgumentException("The start line is 1-based and must be positive.", nameof(request));
            }

            if (request.MaxLines is < 1 or > LookupRequest.LargestPage)
            {
                throw new ArgumentException($"The page size must be between 1 and {LookupRequest.LargestPage} lines.", nameof(request));
            }

            var tag = this.Release(request.Tag);
            var (root, index) = request.Kind switch
            {
                KnowledgeKind.Metamodel => (this.layout.Metamodel(tag), this.layout.MetamodelIndex(tag)),
                KnowledgeKind.ModelLibrary => (this.layout.ModelLibrary(tag), this.layout.ModelLibraryIndex(tag)),
                _ => throw new ArgumentOutOfRangeException(nameof(request), request.Kind, "Unknown knowledge kind."),
            };

            if (!index.Exists)
            {
                throw new KnowledgeLookupException(
                    $"The {Describe(request.Kind)} of release {tag} has not been generated. "
                    + $"Run 'hypha generate --tag {tag}'.");
            }

            var entries = ReadIndex(request.Kind, index, tag);
            var name = request.Name.Trim();
            var (status, matches) = Resolve(entries, name);

            if (status != LookupStatus.Found)
            {
                var names = matches.Select(entry => entry.Key).Order(StringComparer.Ordinal).ToList();

                return new LookupResult
                {
                    Release = tag,
                    Kind = request.Kind,
                    Status = status,
                    Matches = [.. names.Take(MaxMatches)],
                    TotalMatches = names.Count,
                };
            }

            var entry = matches[0];
            var page = ContainedFile(root, entry.File, tag);
            var lines = ReadLines(page);

            var declarationLine = request.Kind == KnowledgeKind.ModelLibrary
                ? DeclarationLocator.Find(lines, entry.Key, entry.Keyword, KeywordsOf(entries))
                : null;

            var start = request.StartLine ?? declarationLine ?? 1;
            if (start > lines.Count)
            {
                throw new ArgumentException(
                    $"The start line {start} is past the end of {entry.Key}'s page, which has {lines.Count} lines.",
                    nameof(request));
            }

            var end = Math.Min(start + request.MaxLines - 1, lines.Count);

            return new LookupResult
            {
                Release = tag,
                Kind = request.Kind,
                Status = LookupStatus.Found,
                Name = entry.Key,
                Path = page.FullName,
                DeclarationLine = declarationLine,
                StartLine = start,
                EndLine = end,
                TotalLines = lines.Count,
                NextLine = end < lines.Count ? end + 1 : null,
                Content = string.Join('\n', lines.Skip(start - 1).Take(end - start + 1)),
                Matches = [entry.Key],
                TotalMatches = 1,
            };
        }

        /// <summary>The keyword the index recorded for each qualified name, for the enclosing scopes.</summary>
        /// <param name="entries">The index's entries.</param>
        /// <returns>
        /// A function from a qualified name to its recorded keyword, or to <c>null</c> when the index has no
        /// entry, or no keyword, for it; the first entry wins on a duplicate key.
        /// </returns>
        private static Func<string, string?> KeywordsOf(List<IndexEntry> entries)
        {
            var keywords = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var entry in entries.Where(entry => entry.Keyword.Length > 0))
            {
                keywords.TryAdd(entry.Key, entry.Keyword);
            }

            return name => keywords.GetValueOrDefault(name);
        }

        /// <summary>Resolves a requested name against the index, one tier at a time (see the class remarks).</summary>
        /// <param name="entries">The index's entries.</param>
        /// <param name="name">The requested name, trimmed.</param>
        /// <returns>
        /// <see cref="LookupStatus.Found"/> with the one match, <see cref="LookupStatus.Ambiguous"/> with the
        /// matches of the first tier that has several, <see cref="LookupStatus.Candidates"/> with the entries
        /// whose names merely contain the request, or <see cref="LookupStatus.NotFound"/> with none.
        /// </returns>
        private static (LookupStatus Status, List<IndexEntry> Matches) Resolve(List<IndexEntry> entries, string name)
        {
            var suffix = "::" + name;

            Func<IndexEntry, bool>[] tiers =
            [
                entry => entry.Names.Any(candidate => string.Equals(candidate, name, StringComparison.Ordinal)),
                entry => entry.Names.Any(candidate => string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase)),
                entry => entry.Names.Any(candidate => candidate.EndsWith(suffix, StringComparison.Ordinal)),
                entry => entry.Names.Any(candidate => candidate.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)),
            ];

            foreach (var tier in tiers)
            {
                var matches = entries.Where(tier).ToList();

                if (matches.Count == 1)
                {
                    return (LookupStatus.Found, matches);
                }

                if (matches.Count > 1)
                {
                    return (LookupStatus.Ambiguous, matches);
                }
            }

            var candidates = entries
                .Where(entry => entry.Names.Any(candidate => candidate.Contains(name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return (candidates.Count == 0 ? LookupStatus.NotFound : LookupStatus.Candidates, candidates);
        }

        /// <summary>Reads and parses an artifact's <c>index.json</c>.</summary>
        /// <param name="kind">Which artifact the index belongs to; it decides the index's shape.</param>
        /// <param name="index">The index file.</param>
        /// <param name="tag">The release the index belongs to, for the error message.</param>
        /// <returns>The index's entries, in the order the file lists them.</returns>
        /// <exception cref="KnowledgeLookupException">The index cannot be read or is not valid.</exception>
        private static List<IndexEntry> ReadIndex(KnowledgeKind kind, FileInfo index, string tag)
        {
            var text = ReadText(index);

            try
            {
                using var document = JsonDocument.Parse(text);

                return kind == KnowledgeKind.ModelLibrary
                    ? ReadModelLibraryIndex(document.RootElement)
                    : ReadMetamodelIndex(document.RootElement);
            }
            catch (Exception exception) when (exception is JsonException or InvalidDataException)
            {
                throw new KnowledgeLookupException(
                    $"The {Describe(kind)} index of release {tag} is not valid ({exception.Message}). "
                    + $"Run 'hypha generate --tag {tag}' to regenerate it.",
                    exception);
            }
        }

        /// <summary>
        /// Reads <c>metamodel/index.json</c>: <c>entries</c>, keyed by element name, each with a
        /// <c>file</c>, a <c>kind</c> and, for a metaclass, a <c>qualifiedName</c>.
        /// </summary>
        /// <param name="root">The index document's root element.</param>
        /// <returns>
        /// One entry per element, answering to its name and, when it has one, its qualified name; its
        /// keyword is the element's <c>kind</c>.
        /// </returns>
        private static List<IndexEntry> ReadMetamodelIndex(JsonElement root) =>
        [
            .. Entries(root, "entries").Select(pair =>
            {
                var qualifiedName = OptionalString(pair.Value, "qualifiedName", pair.Key);

                return new IndexEntry(
                    pair.Key,
                    qualifiedName is null ? [pair.Key] : [pair.Key, qualifiedName],
                    OptionalString(pair.Value, "kind", pair.Key) ?? string.Empty,
                    RequiredString(pair.Value, "file", pair.Key));
            }),
        ];

        /// <summary>
        /// Reads <c>model-library/index.json</c> (<c>knowledge/model-library.schema.json</c>):
        /// <c>declarations</c>, keyed by qualified name, each with a <c>kind</c> and a <c>file</c>.
        /// </summary>
        /// <param name="root">The index document's root element.</param>
        /// <returns>
        /// One entry per declaration, answering to its qualified name; its keyword is the declaration's
        /// <c>kind</c>, the keyword that introduces it in the source.
        /// </returns>
        private static List<IndexEntry> ReadModelLibraryIndex(JsonElement root) =>
        [
            .. Entries(root, "declarations").Select(pair => new IndexEntry(
                pair.Key,
                [pair.Key],
                RequiredString(pair.Value, "kind", pair.Key),
                RequiredString(pair.Value, "file", pair.Key))),
        ];

        /// <summary>The entries of an index, each checked to be an object.</summary>
        /// <remarks>Read eagerly, so a malformed entry is reported while the document is still open.</remarks>
        /// <param name="root">The index document's root element.</param>
        /// <param name="property">The root property that holds the entries, keyed by name.</param>
        /// <returns>Each entry's key and its object, in document order.</returns>
        /// <exception cref="InvalidDataException">The property is missing, or it or an entry is not an object.</exception>
        private static List<(string Key, JsonElement Value)> Entries(JsonElement root, string property)
        {
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty(property, out var entries)
                || entries.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"'{property}' is missing or not an object");
            }

            var found = new List<(string Key, JsonElement Value)>();

            foreach (var entry in entries.EnumerateObject())
            {
                if (entry.Value.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException($"entry '{entry.Name}' is not an object");
                }

                found.Add((entry.Name, entry.Value));
            }

            return found;
        }

        /// <summary>Reads a string property an index entry must have.</summary>
        /// <param name="entry">The index entry.</param>
        /// <param name="property">The property's name.</param>
        /// <param name="key">The entry's key, for the error message.</param>
        /// <returns>The property's value, never empty.</returns>
        /// <exception cref="InvalidDataException">The property is missing, <c>null</c>, empty or not a string.</exception>
        private static string RequiredString(JsonElement entry, string property, string key) =>
            OptionalString(entry, property, key) is { Length: > 0 } value
                ? value
                : throw new InvalidDataException($"entry '{key}' has no '{property}'");

        /// <summary>Reads a string property an index entry may have.</summary>
        /// <param name="entry">The index entry.</param>
        /// <param name="property">The property's name.</param>
        /// <param name="key">The entry's key, for the error message.</param>
        /// <returns>The property's value, or <c>null</c> when it is missing or <c>null</c>.</returns>
        /// <exception cref="InvalidDataException">The property is present but not a string.</exception>
        private static string? OptionalString(JsonElement entry, string property, string key)
        {
            if (!entry.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : throw new InvalidDataException($"'{property}' of entry '{key}' is not a string");
        }

        /// <summary>
        /// Resolves an index path, refusing one that leaves the artifact's folder, directly, through
        /// <c>..</c>, or through a symbolic link anywhere on the way.
        /// </summary>
        /// <param name="root">The artifact's folder, which the page has to stay inside.</param>
        /// <param name="relative">The page path the index recorded, relative to <paramref name="root"/>, with <c>/</c> separators.</param>
        /// <param name="tag">The release the index belongs to, for the error message.</param>
        /// <returns>The page, which exists and lies inside <paramref name="root"/>.</returns>
        /// <exception cref="KnowledgeLookupException">The path is unsafe, leaves the folder, or the page is missing.</exception>
        private static FileInfo ContainedFile(DirectoryInfo root, string relative, string tag)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains('\\'))
            {
                throw new KnowledgeLookupException($"The index of release {tag} holds an unsafe page path '{relative}'.");
            }

            var file = new FileInfo(Path.GetFullPath(Path.Combine(root.FullName, relative)));
            if (!IsInside(root.FullName, file.FullName))
            {
                throw new KnowledgeLookupException($"The index of release {tag} points outside its folder: '{relative}'.");
            }

            if (!file.Exists)
            {
                throw new KnowledgeLookupException(
                    $"The page '{relative}' of release {tag} is missing. Run 'hypha generate --tag {tag}'.");
            }

            // Compared after resolving every link on both paths: a linked folder on the way is caught
            // as well as a linked page, and a release folder that itself sits behind a link (as the
            // temporary folder does on macOS) is not mistaken for an escape.
            if (!IsInside(Canonical(root.FullName, tag), Canonical(file.FullName, tag)))
            {
                throw new KnowledgeLookupException($"The page '{relative}' of release {tag} links outside its folder.");
            }

            return file;
        }

        /// <summary>The path with every symbolic link or junction along it resolved.</summary>
        /// <param name="path">The path to resolve, absolute or relative to the current directory.</param>
        /// <param name="tag">The release being read, for the error message.</param>
        /// <param name="depth">How many links have been followed so far; at most <see cref="MaxLinkDepth"/>.</param>
        /// <returns>The full path with no link left on it.</returns>
        /// <exception cref="KnowledgeLookupException">A link cannot be followed, or there are too many of them.</exception>
        private static string Canonical(string path, string tag, int depth = 0)
        {
            if (depth > MaxLinkDepth)
            {
                throw new KnowledgeLookupException($"The knowledge of release {tag} is reached through too many links ('{path}').");
            }

            try
            {
                var full = Path.GetFullPath(path);
                var root = Path.GetPathRoot(full) ?? string.Empty;
                var current = root;

                foreach (var part in full[root.Length..].Split(
                             [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    current = Path.Combine(current, part);

                    FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
                    if (info.LinkTarget is null)
                    {
                        continue;
                    }

                    var target = info.ResolveLinkTarget(returnFinalTarget: true);
                    if (target is not null)
                    {
                        current = Canonical(target.FullName, tag, depth + 1);
                    }
                }

                return current;
            }
            catch (IOException exception)
            {
                throw new KnowledgeLookupException(
                    $"A link on the way to the knowledge of release {tag} cannot be followed: {exception.Message}",
                    exception);
            }
        }

        /// <summary>Whether a path lies inside a folder, or is the folder itself.</summary>
        /// <param name="root">The folder's full path.</param>
        /// <param name="path">The full path to test.</param>
        /// <returns><c>true</c> when <paramref name="path"/> does not leave <paramref name="root"/>.</returns>
        private static bool IsInside(string root, string path)
        {
            var relative = Path.GetRelativePath(root, path);

            return relative != ".."
                   && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                   && !Path.IsPathRooted(relative);
        }

        /// <summary>Reads a page as lines, whichever line endings it was written with.</summary>
        /// <param name="file">The page.</param>
        /// <returns>The page's lines, without their line endings and without an empty line for a final newline.</returns>
        private static List<string> ReadLines(FileInfo file)
        {
            var lines = ReadText(file).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();

            // A final newline ends the last line; it does not start another.
            if (lines.Count > 1 && lines[^1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        /// <summary>Reads a generated file as UTF-8 text, refusing one larger than <see cref="MaxFileBytes"/>.</summary>
        /// <param name="file">The file.</param>
        /// <returns>The file's text.</returns>
        /// <exception cref="KnowledgeLookupException">The file is too large or cannot be read.</exception>
        private static string ReadText(FileInfo file)
        {
            try
            {
                file.Refresh();

                if (file.Length > MaxFileBytes)
                {
                    throw new KnowledgeLookupException(
                        $"'{file.Name}' is larger than the {MaxFileBytes / (1024 * 1024)} MiB a lookup reads.");
                }

                return File.ReadAllText(file.FullName, Encoding.UTF8);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Most likely 'hypha generate' rewriting the file at this moment.
                throw new KnowledgeLookupException(
                    $"'{file.FullName}' could not be read: {exception.Message} "
                    + "If 'hypha generate' is running, try again once it has finished.",
                    exception);
            }
        }

        /// <summary>Names an artifact the way an error message refers to it.</summary>
        /// <param name="kind">The artifact.</param>
        /// <returns><c>model library</c> or <c>metamodel</c>.</returns>
        private static string Describe(KnowledgeKind kind) =>
            kind == KnowledgeKind.ModelLibrary ? "model library" : "metamodel";

        /// <summary>Settles which release a lookup reads.</summary>
        /// <param name="requested">The release asked for, or <c>null</c> for the default release.</param>
        /// <returns>The tag of an installed release.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The requested release is not installed, or none was requested and there is no default.
        /// </exception>
        private string Release(string? requested)
        {
            var installed = this.layout.InstalledTags;

            if (requested is null)
            {
                return this.layout.DefaultTag
                       ?? throw new KnowledgeLookupException(
                           installed.Count == 0
                               ? "No release is installed. Run 'hypha fetch --tag <release>' and 'hypha generate'."
                               : "No default release is selected. Run 'hypha use --tag <release>'.");
            }

            if (!installed.Contains(requested, StringComparer.Ordinal))
            {
                throw new KnowledgeLookupException(
                    $"Release {requested} is not installed. Installed: "
                    + (installed.Count == 0 ? "none" : string.Join(", ", installed)) + ".");
            }

            return requested;
        }

        /// <summary>One index entry, with every name it answers to.</summary>
        /// <param name="Key">The entry's key in the index: an element name, or a declaration's qualified name.</param>
        /// <param name="Names">Every name the entry answers to, the key included.</param>
        /// <param name="Keyword">
        /// The entry's <c>kind</c> as the index recorded it; for a model-library declaration, the keyword that
        /// introduces it; empty when the index has none.
        /// </param>
        /// <param name="File">The entry's page, relative to the artifact's folder, with <c>/</c> separators.</param>
        private sealed record IndexEntry(string Key, IReadOnlyList<string> Names, string Keyword, string File);
    }
}
