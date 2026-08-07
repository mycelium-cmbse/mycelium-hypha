// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// The repository layout, resolved from one root.
    /// </summary>
    /// <remarks>
    /// This replaces three separate copies of the same path knowledge that had grown up in the test
    /// projects - one per project, plus a private one inside a fixture - because neither project could
    /// reach the other's. Path logic that lives in a test cannot be called by the CLI, so it gets
    /// written again; this is the one copy.
    /// </remarks>
    public sealed class KnowledgeLayout : IKnowledgeLayout
    {
        /// <summary>The folder holding the per-release inputs.</summary>
        public const string SourcesFolder = "sources";

        /// <summary>The folder holding the generated knowledge base.</summary>
        public const string KnowledgeFolder = "knowledge";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeLayout"/> class.
        /// </summary>
        /// <param name="root">The repository root: where the inputs are read from.</param>
        /// <param name="outputRoot">
        /// Where generated artifacts are written. Defaults to <paramref name="root"/>, which is the
        /// normal in-repository run; pass another folder to generate without touching the committed
        /// knowledge base.
        /// </param>
        public KnowledgeLayout(DirectoryInfo root, DirectoryInfo? outputRoot = null)
        {
            ArgumentNullException.ThrowIfNull(root);

            this.Root = root;
            this.OutputRoot = outputRoot ?? root;
        }

        /// <inheritdoc/>
        public DirectoryInfo Root { get; }

        /// <inheritdoc/>
        public DirectoryInfo OutputRoot { get; }

        /// <inheritdoc/>
        /// <remarks>
        /// Read on each access rather than cached: generation rewrites the manifest, and a cached list
        /// would be a stale answer for the rest of the run.
        /// </remarks>
        public IReadOnlyList<string> InstalledTags => this.Manifest()?.GetTags() ?? [];

        /// <inheritdoc/>
        public string? DefaultTag => this.Manifest()?.Default;

        /// <inheritdoc/>
        public FileInfo VersionManifest =>
            this.FileAt(KnowledgeFolder, Releases.VersionManifest.FileName);

        /// <inheritdoc/>
        public DirectoryInfo Sources => this.Folder(SourcesFolder);

        /// <inheritdoc/>
        public FileInfo SharedPrimitiveTypes => this.FileAt(SourcesFolder, ReleaseInputs.SharedXmi);

        /// <inheritdoc/>
        public FileInfo CrossReferenceSchema =>
            this.FileAt(KnowledgeFolder, "cross-references.schema.json");

        /// <summary>
        /// Walks up from <paramref name="start"/> looking for a folder that holds both
        /// <c>sources/</c> and <c>knowledge/</c>.
        /// </summary>
        /// <param name="start">Where to start; defaults to the running assembly's folder.</param>
        /// <returns>The layout, or <c>null</c> when there is no repository above the starting point.</returns>
        public static KnowledgeLayout? Discover(DirectoryInfo? start = null)
        {
            for (var directory = start ?? new DirectoryInfo(AppContext.BaseDirectory);
                 directory is not null;
                 directory = directory.Parent)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, SourcesFolder))
                    && Directory.Exists(Path.Combine(directory.FullName, KnowledgeFolder)))
                {
                    return new KnowledgeLayout(directory);
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public DirectoryInfo Xmi(string tag) => this.Folder(SourcesFolder, Tag(tag), "xmi");

        /// <inheritdoc/>
        public DirectoryInfo TextualSources(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "textual");

        /// <inheritdoc/>
        public DirectoryInfo Bnf(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "textual", "bnf");

        /// <inheritdoc/>
        public FileInfo Grammar(string tag, string fileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            return new FileInfo(Path.Combine(this.Bnf(tag).FullName, fileName));
        }

        /// <inheritdoc/>
        public DirectoryInfo Specifications(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "specs");

        /// <inheritdoc/>
        public DirectoryInfo Knowledge(string tag) => this.OutputFolder(KnowledgeFolder, Tag(tag));

        /// <inheritdoc/>
        public DirectoryInfo Metamodel(string tag) =>
            this.OutputFolder(KnowledgeFolder, Tag(tag), "metamodel");

        /// <inheritdoc/>
        public FileInfo MetamodelIndex(string tag) =>
            new(Path.Combine(this.Metamodel(tag).FullName, "index.json"));

        /// <inheritdoc/>
        public DirectoryInfo TextualNotation(string tag) =>
            this.OutputFolder(KnowledgeFolder, Tag(tag), "textual-notation");

        /// <inheritdoc/>
        public DirectoryInfo Examples(string tag) =>
            new(Path.Combine(this.TextualNotation(tag).FullName, "examples"));

        /// <inheritdoc/>
        /// <remarks>
        /// Built from <see cref="Root"/> rather than from <see cref="Knowledge"/>: the spec catalog is
        /// produced by the Python PDF chain and read here, so redirecting the output must not move it.
        /// </remarks>
        public FileInfo SpecificationCatalog(string tag, string document)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(document);

            return this.FileAt(KnowledgeFolder, Tag(tag), "spec", document, "index.json");
        }

        /// <inheritdoc/>
        public FileInfo CrossReferences(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, CrossReferenceDocument.FileName));

        /// <inheritdoc/>
        public FileInfo CommittedCrossReferences(string tag) =>
            this.FileAt(KnowledgeFolder, Tag(tag), CrossReferenceDocument.FileName);

        private static string Tag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return tag;
        }

        private VersionManifest? Manifest() =>
            VersionManifestFile.ReadIfPresent(this.VersionManifest.FullName);

        // Named Folder/FileAt rather than Directory/File so they do not shadow System.IO inside this
        // type - Discover() needs the real Directory.Exists.
        private DirectoryInfo Folder(params string[] segments) =>
            new(Path.Combine([this.Root.FullName, .. segments]));

        private FileInfo FileAt(params string[] segments) =>
            new(Path.Combine([this.Root.FullName, .. segments]));

        private DirectoryInfo OutputFolder(params string[] segments) =>
            new(Path.Combine([this.OutputRoot.FullName, .. segments]));
    }
}
