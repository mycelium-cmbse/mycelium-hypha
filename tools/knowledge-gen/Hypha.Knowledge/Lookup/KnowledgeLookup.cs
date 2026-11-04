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
    using Hypha.Knowledge.ModelLibrary;

    /// <inheritdoc cref="IKnowledgeLookup"/>
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

        private static readonly JsonSerializerOptions ModelLibraryFormat = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        private readonly IKnowledgeLayout layout;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeLookup"/> class.</summary>
        public KnowledgeLookup(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <inheritdoc/>
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
                throw new ArgumentException(
                    $"The page size must be between 1 and {LookupRequest.LargestPage} lines.", nameof(request));
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
                ? DeclarationLocator.Find(lines, entry.Key, entry.Keyword)
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

        private static (LookupStatus Status, IReadOnlyList<IndexEntry> Matches) Resolve(
            IReadOnlyList<IndexEntry> entries, string name)
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

        private static IReadOnlyList<IndexEntry> ReadIndex(KnowledgeKind kind, FileInfo index, string tag)
        {
            try
            {
                var text = ReadText(index);

                return kind == KnowledgeKind.ModelLibrary
                    ? ReadModelLibraryIndex(text)
                    : ReadMetamodelIndex(text);
            }
            catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException
                                                  and not KnowledgeLookupException)
            {
                throw new KnowledgeLookupException(
                    $"The {Describe(kind)} index of release {tag} is not valid. "
                    + $"Run 'hypha generate --tag {tag}' to regenerate it.",
                    exception);
            }
        }

        private static List<IndexEntry> ReadMetamodelIndex(string text)
        {
            using var document = JsonDocument.Parse(text);

            return
            [
                .. document.RootElement.GetProperty("entries").EnumerateObject().Select(entry =>
                {
                    var qualifiedName = entry.Value.TryGetProperty("qualifiedName", out var value)
                        ? value.GetString()
                        : null;

                    return new IndexEntry(
                        entry.Name,
                        qualifiedName is null ? [entry.Name] : [entry.Name, qualifiedName],
                        entry.Value.GetProperty("kind").GetString() ?? string.Empty,
                        entry.Value.GetProperty("file").GetString()
                            ?? throw new InvalidOperationException($"'{entry.Name}' has no file."));
                }),
            ];
        }

        private static List<IndexEntry> ReadModelLibraryIndex(string text)
        {
            var document = JsonSerializer.Deserialize<ModelLibraryDocument>(text, ModelLibraryFormat)
                           ?? throw new InvalidOperationException("The model-library index is empty.");

            return
            [
                .. (document.Declarations ?? throw new InvalidOperationException("No declarations."))
                    .Select(pair => new IndexEntry(pair.Key, [pair.Key], pair.Value.Kind, pair.Value.File)),
            ];
        }

        /// <summary>
        /// Resolves an index path, refusing one that leaves the artifact's folder - directly, through
        /// <c>..</c>, or through a symbolic link.
        /// </summary>
        private static FileInfo ContainedFile(DirectoryInfo root, string relative, string tag)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains('\\'))
            {
                throw new KnowledgeLookupException(
                    $"The index of release {tag} holds an unsafe page path '{relative}'.");
            }

            var file = new FileInfo(Path.GetFullPath(Path.Combine(root.FullName, relative)));
            if (!IsInside(root.FullName, file.FullName))
            {
                throw new KnowledgeLookupException(
                    $"The index of release {tag} points outside its folder: '{relative}'.");
            }

            if (!file.Exists)
            {
                throw new KnowledgeLookupException(
                    $"The page '{relative}' of release {tag} is missing. Run 'hypha generate --tag {tag}'.");
            }

            var target = file.ResolveLinkTarget(returnFinalTarget: true);
            if (target is not null)
            {
                var realRoot = root.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? root.FullName;

                if (!IsInside(realRoot, target.FullName))
                {
                    throw new KnowledgeLookupException(
                        $"The page '{relative}' of release {tag} links outside its folder.");
                }

                return new FileInfo(target.FullName);
            }

            return file;
        }

        private static bool IsInside(string root, string path)
        {
            var relative = Path.GetRelativePath(root, path);

            return relative != ".."
                   && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                   && !Path.IsPathRooted(relative);
        }

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

        private static string ReadText(FileInfo file)
        {
            file.Refresh();

            if (file.Length > MaxFileBytes)
            {
                throw new KnowledgeLookupException(
                    $"'{file.Name}' is larger than the {MaxFileBytes / (1024 * 1024)} MiB a lookup reads.");
            }

            return File.ReadAllText(file.FullName, Encoding.UTF8);
        }

        private static string Describe(KnowledgeKind kind) =>
            kind == KnowledgeKind.ModelLibrary ? "model library" : "metamodel";

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
        private sealed record IndexEntry(string Key, IReadOnlyList<string> Names, string Keyword, string File);
    }
}
