// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryGenerator.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.ModelLibrary;
    using Hypha.Knowledge.TextualNotation;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/model-library/</c>: one page per standard-library file the
    /// release ships, plus a qualified-name index over what each one declares.
    /// </summary>
    /// <remarks>
    /// The standard libraries (ISQ, ScalarValues, SysML.sysml, ...) are normative model content a
    /// user's own model specialises with <c>:&gt;</c>, not a worked example (see #80). This generator
    /// does not resolve <c>:&gt;</c>/<c>:&gt;&gt;</c> chains or compute an effective feature set the way
    /// the metamodel pages do: it only answers "what names does this file declare and where."
    /// </remarks>
    public sealed class ModelLibraryGenerator : IKnowledgeGenerator
    {
        /// <summary>The textual grammars whose reserved words a library declaration name can never be.</summary>
        private static readonly string[] Grammars = ["KerML-textual-bnf.kebnf", "SysML-textual-bnf.kebnf"];

        /// <summary>Resolves the library sources, the metamodel index, the grammars and the output folders.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Discovers the standard-library files the release ships.</summary>
        private readonly IModelCatalog catalog;

        /// <summary>Derives each metaclass's declaration keyword from the metamodel's element names.</summary>
        private readonly ISurfaceForms forms;

        /// <summary>Finds the qualified names each library file declares.</summary>
        private readonly IDeclarationScanner scanner;

        /// <summary>Renders the package pages and the library's <c>index.md</c>.</summary>
        private readonly IModelLibraryRenderer renderer;

        /// <summary>Names each package page the way the example pages are named.</summary>
        private readonly INotationRenderer notation;

        /// <summary>Reads the reserved keywords out of the grammars.</summary>
        private readonly IGrammarParser parser;

        /// <summary>Reads the metamodel index the surface forms are derived from.</summary>
        private readonly IKnowledgeReader reader;

        /// <summary>Receives the progress messages of <see cref="GenerationLog"/>.</summary>
        private readonly ILogger<ModelLibraryGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLibraryGenerator"/> class.
        /// </summary>
        /// <param name="layout">Resolves the inputs and the output folders.</param>
        /// <param name="catalog">Discovers the standard-library files.</param>
        /// <param name="forms">Derives the metaclasses' declaration keywords.</param>
        /// <param name="scanner">Finds the declarations in each library file.</param>
        /// <param name="renderer">Renders the package pages and the index page.</param>
        /// <param name="notation">Names the package pages.</param>
        /// <param name="parser">Reads the grammars' reserved keywords.</param>
        /// <param name="reader">Reads the metamodel index.</param>
        /// <param name="logger">Receives the progress messages.</param>
        public ModelLibraryGenerator(
            IKnowledgeLayout layout,
            IModelCatalog catalog,
            ISurfaceForms forms,
            IDeclarationScanner scanner,
            IModelLibraryRenderer renderer,
            INotationRenderer notation,
            IGrammarParser parser,
            IKnowledgeReader reader,
            ILogger<ModelLibraryGenerator> logger)
        {
            this.layout = layout;
            this.catalog = catalog;
            this.forms = forms;
            this.scanner = scanner;
            this.renderer = renderer;
            this.notation = notation;
            this.parser = parser;
            this.reader = reader;
            this.logger = logger;
        }

        /// <summary>Gets the artifact name, <c>model-library</c>, used when reporting progress.</summary>
        public string Artifact => "model-library";

        /// <summary>
        /// Gets this generator's place in a full run, <c>35</c>: after the textual notation, before the
        /// cross-references.
        /// </summary>
        public int Order => 35;

        /// <summary>
        /// Writes <c>knowledge/&lt;tag&gt;/model-library/</c>: one page per standard-library file under
        /// <c>packages/</c>, the qualified-name index <c>index.json</c> and the <c>index.md</c> page. Pages
        /// left over from a previous run are deleted first.
        /// </summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels the generation.</param>
        /// <returns>
        /// The written files, or a <see cref="GenerationOutcome.Skipped"/> result when the release ships no
        /// standard-library file.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// The library is present but the metamodel index is not (there would be no surface forms), or
        /// two files slugify to the same page name, or two <b>different</b> files declare the same
        /// qualified name. A same-file collision keeps its first occurrence instead, since both still
        /// point at the correct page.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var librarySources = this.layout.ModelLibrarySources(tag);
            var files = this.catalog.Discover(librarySources);

            if (files.Count == 0)
            {
                return GenerationResult.Skipped($"no standard library fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var index = this.layout.MetamodelIndex(tag);
            if (!index.Exists)
            {
                throw new InvalidOperationException(
                    $"the metamodel index for {tag} has not been generated yet; run the metamodel "
                    + "generator first, or every declaration would come out unrecognised");
            }

            var surfaceForms = this.forms.Index(
                this.reader.ReadElements(index).Elements.Select(element => element.Name)).Forms.Values;

            var reservedKeywords = await this.ReservedKeywordsAsync(tag, cancellationToken);

            var packages = this.layout.ModelLibraryPackages(tag);
            packages.Create();

            // Stale pages go first: a file removed upstream would otherwise linger forever.
            foreach (var stale in packages.EnumerateFiles("*.md"))
            {
                stale.Delete();
            }

            var written = new List<FileInfo>();
            var packageEntries = new List<ModelLibraryPackageEntry>();
            var declarations = new Dictionary<string, ModelLibraryDeclarationEntry>(StringComparer.Ordinal);
            var declaredBy = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                var sourcePath = "sysml.library/" + file;
                var source = Path.Combine(
                    librarySources.FullName, file.Replace('/', Path.DirectorySeparatorChar));

                var text = await File.ReadAllTextAsync(source, cancellationToken);
                var language = file.EndsWith(".kerml", StringComparison.OrdinalIgnoreCase) ? "KerML" : "SysML";

                var found = this.scanner.Scan(text, surfaceForms, reservedKeywords);
                var fileName = this.notation.ExampleFileName(sourcePath);
                var pagePath = $"packages/{fileName}";

                written.Add(await KnowledgeFile.WriteAsync(
                    Path.Combine(packages.FullName, fileName),
                    this.renderer.RenderPackage(sourcePath, text, found, language),
                    cancellationToken));

                packageEntries.Add(new ModelLibraryPackageEntry(fileName, sourcePath));

                foreach (var declaration in found)
                {
                    if (declaredBy.TryGetValue(declaration.QualifiedName, out var existingSource))
                    {
                        if (!string.Equals(existingSource, sourcePath, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                $"'{declaration.QualifiedName}' is declared in both {existingSource} and "
                                + $"{sourcePath} at {tag} - the qualified-name index cannot point at both");
                        }

                        continue;
                    }

                    declaredBy[declaration.QualifiedName] = sourcePath;
                    declarations[declaration.QualifiedName] = new ModelLibraryDeclarationEntry(
                        declaration.Kind, pagePath, sourcePath);
                }
            }

            // The pages land in one flat folder, so a slug collision would silently lose a file: two
            // entries in the index, one page on disk.
            if (packageEntries.Select(entry => entry.FileName).Distinct(StringComparer.Ordinal).Count()
                != packageEntries.Count)
            {
                throw new InvalidOperationException(
                    $"two standard-library files at {tag} slugified to the same page name");
            }

            var document = new ModelLibraryDocument(
                ModelLibraryDocument.CurrentSchemaVersion,
                tag,
                new ModelLibraryCounts(files.Count, declarations.Count),
                declarations);

            written.Add(await KnowledgeFile.WriteAsync(
                this.layout.ModelLibraryIndex(tag).FullName, ModelLibraryFile.Render(document), cancellationToken));

            written.Add(await KnowledgeFile.WriteAsync(
                Path.Combine(this.layout.ModelLibrary(tag).FullName, "index.md"),
                this.renderer.RenderIndex(tag, packageEntries),
                cancellationToken));

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }

        /// <summary>Both grammars' reserved words, merged: a word reserved in either can never be a name.</summary>
        /// <param name="tag">The release whose grammars are read; a grammar that is absent contributes nothing.</param>
        /// <param name="cancellationToken">Cancels reading the grammars.</param>
        /// <returns>The reserved words, compared ordinally.</returns>
        private async Task<IReadOnlyCollection<string>> ReservedKeywordsAsync(
            string tag, CancellationToken cancellationToken)
        {
            var reserved = new HashSet<string>(StringComparer.Ordinal);

            foreach (var grammarFile in Grammars)
            {
                var file = this.layout.Grammar(tag, grammarFile);

                if (file.Exists)
                {
                    reserved.UnionWith(
                        this.parser.ReservedKeywords(await File.ReadAllTextAsync(file.FullName, cancellationToken)));
                }
            }

            return reserved;
        }
    }
}
