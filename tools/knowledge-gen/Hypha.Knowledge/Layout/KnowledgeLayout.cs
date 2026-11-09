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
    /// projects (one per project, plus a private one inside a fixture) because neither project could
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

        /// <summary>
        /// Gets the repository root, the folder holding <c>sources/</c> and <c>knowledge/</c>, which every
        /// input is read from.
        /// </summary>
        public DirectoryInfo Root { get; }

        /// <summary>
        /// Gets the folder generated artifacts are written to: the <c>outputRoot</c> passed to the
        /// constructor, or <see cref="Root"/> when none was.
        /// </summary>
        public DirectoryInfo OutputRoot { get; }

        /// <summary>
        /// Gets the installed release tags, newest first, as <c>knowledge/versions.json</c> under
        /// <see cref="Root"/> records them. Empty when there is no manifest.
        /// </summary>
        /// <remarks>
        /// Read on each access rather than cached: generation rewrites the manifest, and a cached list
        /// would be a stale answer for the rest of the run.
        /// </remarks>
        public IReadOnlyList<string> InstalledTags => this.Manifest()?.GetTags() ?? [];

        /// <summary>
        /// Gets the tag answered from by default, as the manifest records it, or <c>null</c> when there is
        /// no manifest. Read on each access, like <see cref="InstalledTags"/>.
        /// </summary>
        public string? DefaultTag => this.Manifest()?.Default;

        /// <summary>
        /// Gets <c>knowledge/versions.json</c>, the <see cref="Releases.VersionManifest"/> recording which
        /// releases are installed and which one is the default.
        /// </summary>
        /// <remarks>
        /// Resolved against <see cref="Root"/>, not <see cref="OutputRoot"/>: it records which releases have
        /// been fetched into <c>sources/</c>, so it belongs with them rather than with whatever a run happens
        /// to generate.
        /// </remarks>
        public FileInfo VersionManifest =>
            this.FileAt(KnowledgeFolder, Releases.VersionManifest.FileName);

        /// <summary>
        /// Gets <c>sources/</c> under <see cref="Root"/>: the folder every release's inputs are fetched
        /// into.
        /// </summary>
        public DirectoryInfo Sources => this.Folder(SourcesFolder);

        /// <summary>
        /// Gets <c>sources/PrimitiveTypes.xmi</c> under <see cref="Root"/>: the OMG UML primitives library,
        /// shared by every release rather than fetched per tag.
        /// </summary>
        public FileInfo SharedPrimitiveTypes => this.FileAt(SourcesFolder, ReleaseInputs.SharedXmi);

        /// <summary>
        /// Gets <c>knowledge/cross-references.schema.json</c> under <see cref="Root"/>, which is not per
        /// release.
        /// </summary>
        public FileInfo CrossReferenceSchema =>
            this.FileAt(KnowledgeFolder, "cross-references.schema.json");

        /// <summary>
        /// Gets <c>knowledge/model-library.schema.json</c> under <see cref="Root"/>, which is not per
        /// release.
        /// </summary>
        public FileInfo ModelLibrarySchema =>
            this.FileAt(KnowledgeFolder, "model-library.schema.json");

        /// <summary>
        /// Gets <c>knowledge/metamodel.schema.json</c> under <see cref="Root"/>, which is not per
        /// release.
        /// </summary>
        public FileInfo MetamodelSchema =>
            this.FileAt(KnowledgeFolder, "metamodel.schema.json");

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

        /// <summary>Resolves <c>sources/&lt;tag&gt;</c> under <see cref="Root"/>: everything fetched for one release.</summary>
        /// <param name="tag">The release tag, e.g. <c>2026-05</c>.</param>
        /// <returns>The release's sources folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo ReleaseSources(string tag) => this.Folder(SourcesFolder, Tag(tag));

        /// <summary>Resolves <c>sources/&lt;tag&gt;/xmi</c> under <see cref="Root"/>: the metamodel XMI.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's XMI folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Xmi(string tag) => this.Folder(SourcesFolder, Tag(tag), "xmi");

        /// <summary>
        /// Resolves <c>sources/&lt;tag&gt;/textual</c> under <see cref="Root"/>: the grammar and the models.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's textual sources folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo TextualSources(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "textual");

        /// <summary>Resolves <c>sources/&lt;tag&gt;/textual/bnf</c> under <see cref="Root"/>: the release's grammars.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's grammar folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Bnf(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "textual", "bnf");

        /// <summary>
        /// Resolves <c>sources/&lt;tag&gt;/textual/sysml.library</c> under <see cref="Root"/>: the normative
        /// standard libraries.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's model library sources folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo ModelLibrarySources(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "textual", "sysml.library");

        /// <summary>
        /// Resolves one of a release's grammars, a file directly under <see cref="Bnf(string)"/>.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <param name="fileName">The grammar's file name, e.g. <c>KerML-textual-bnf.kebnf</c>.</param>
        /// <returns>The grammar file, whether or not it exists.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> or <paramref name="fileName"/> is <c>null</c>, empty or white space.
        /// </exception>
        public FileInfo Grammar(string tag, string fileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            return new FileInfo(Path.Combine(this.Bnf(tag).FullName, fileName));
        }

        /// <summary>
        /// Resolves <c>sources/&lt;tag&gt;/specs</c> under <see cref="Root"/>: the OMG-copyrighted,
        /// git-ignored specification PDFs.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's specifications folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Specifications(string tag) =>
            this.Folder(SourcesFolder, Tag(tag), "specs");

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;</c> under <see cref="OutputRoot"/>, where a run writes the
        /// release's generated knowledge.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's knowledge folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Knowledge(string tag) => this.OutputFolder(KnowledgeFolder, Tag(tag));

        /// <summary>Resolves <c>knowledge/&lt;tag&gt;/metamodel</c> under <see cref="OutputRoot"/>.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's metamodel folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Metamodel(string tag) =>
            this.OutputFolder(KnowledgeFolder, Tag(tag), "metamodel");

        /// <summary>Resolves <c>knowledge/&lt;tag&gt;/metamodel/index.json</c> under <see cref="OutputRoot"/>.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's metamodel index, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo MetamodelIndex(string tag) =>
            new(Path.Combine(this.Metamodel(tag).FullName, "index.json"));

        /// <summary>Resolves <c>knowledge/&lt;tag&gt;/textual-notation</c> under <see cref="OutputRoot"/>.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's textual notation folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo TextualNotation(string tag) =>
            this.OutputFolder(KnowledgeFolder, Tag(tag), "textual-notation");

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/textual-notation/examples</c> under <see cref="OutputRoot"/>.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's examples folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo Examples(string tag) =>
            new(Path.Combine(this.TextualNotation(tag).FullName, "examples"));

        /// <summary>Resolves <c>knowledge/&lt;tag&gt;/model-library</c> under <see cref="OutputRoot"/>.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's model library folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo ModelLibrary(string tag) =>
            this.OutputFolder(KnowledgeFolder, Tag(tag), "model-library");

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/model-library/packages</c> under <see cref="OutputRoot"/>.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's model library packages folder, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public DirectoryInfo ModelLibraryPackages(string tag) =>
            new(Path.Combine(this.ModelLibrary(tag).FullName, "packages"));

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/model-library/index.json</c> under <see cref="OutputRoot"/>.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The release's model library index, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo ModelLibraryIndex(string tag) =>
            new(Path.Combine(this.ModelLibrary(tag).FullName, "index.json"));

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/spec/&lt;document&gt;/index.json</c> under <see cref="Root"/>;
        /// git-ignored, so it is routinely absent.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <param name="document">The specification's folder name, e.g. <c>kerml</c> or <c>sysml2</c>.</param>
        /// <returns>The specification's clause catalog, whether or not it exists.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> or <paramref name="document"/> is <c>null</c>, empty or white space.
        /// </exception>
        /// <remarks>
        /// Built from <see cref="Root"/> rather than from <see cref="Knowledge"/>: the spec catalog is
        /// produced by the Python PDF chain and read here, so redirecting the output must not move it.
        /// </remarks>
        public FileInfo SpecificationCatalog(string tag, string document)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(document);

            return this.FileAt(KnowledgeFolder, Tag(tag), "spec", document, "index.json");
        }

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/cross-references.json</c> under <see cref="OutputRoot"/>, where
        /// a run writes it.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The cross-reference document a run writes, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public FileInfo CrossReferences(string tag) =>
            new(Path.Combine(this.Knowledge(tag).FullName, CrossReferenceDocument.FileName));

        /// <summary>
        /// Resolves <c>knowledge/&lt;tag&gt;/cross-references.json</c> under <see cref="Root"/>: the
        /// document a previous run left there.
        /// </summary>
        /// <param name="tag">The release tag.</param>
        /// <returns>The previously written cross-reference document, whether or not it exists.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <remarks>
        /// The same file as <see cref="CrossReferences"/> in a normal run, and a different one as soon as the
        /// output is redirected: this is the carry-forward source the cross-references read when the
        /// specifications are absent, so it keeps pointing under <see cref="Root"/> rather than following the
        /// output to an empty folder.
        /// </remarks>
        public FileInfo CommittedCrossReferences(string tag) =>
            this.FileAt(KnowledgeFolder, Tag(tag), CrossReferenceDocument.FileName);

        /// <summary>Validates a release tag before it becomes a path segment.</summary>
        /// <param name="tag">The release tag.</param>
        /// <returns><paramref name="tag"/>, unchanged.</returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        private static string Tag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return tag;
        }

        /// <summary>Reads <see cref="VersionManifest"/> afresh.</summary>
        /// <returns>The manifest, or <c>null</c> when there is none.</returns>
        private VersionManifest? Manifest() =>
            VersionManifestFile.ReadIfPresent(this.VersionManifest.FullName);

        // Named Folder/FileAt rather than Directory/File so they do not shadow System.IO inside this
        // type: Discover() needs the real Directory.Exists.
        /// <summary>Resolves a folder under <see cref="Root"/>.</summary>
        /// <param name="segments">The path segments below the root.</param>
        /// <returns>The folder, whether or not it exists.</returns>
        private DirectoryInfo Folder(params string[] segments) =>
            new(Path.Combine([this.Root.FullName, .. segments]));

        /// <summary>Resolves a file under <see cref="Root"/>.</summary>
        /// <param name="segments">The path segments below the root.</param>
        /// <returns>The file, whether or not it exists.</returns>
        private FileInfo FileAt(params string[] segments) =>
            new(Path.Combine([this.Root.FullName, .. segments]));

        /// <summary>Resolves a folder under <see cref="OutputRoot"/>.</summary>
        /// <param name="segments">The path segments below the output root.</param>
        /// <returns>The folder, whether or not it exists.</returns>
        private DirectoryInfo OutputFolder(params string[] segments) =>
            new(Path.Combine([this.OutputRoot.FullName, .. segments]));
    }
}
