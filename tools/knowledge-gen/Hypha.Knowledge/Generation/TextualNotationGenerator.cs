// ------------------------------------------------------------------------------------------------
// <copyright file="TextualNotationGenerator.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.TextualNotation;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/textual-notation/</c>: one page per model the release ships,
    /// plus the index and its keyword reference.
    /// </summary>
    public sealed class TextualNotationGenerator : IKnowledgeGenerator
    {
        /// <summary>The textual grammars whose keywords the index lists, by their name in the sources.</summary>
        private static readonly string[] Grammars = ["KerML", "SysML"];

        /// <summary>Resolves the textual sources, the metamodel index, the grammars and the output folders.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Discovers the models the release ships.</summary>
        private readonly IModelCatalog catalog;

        /// <summary>Derives each metaclass's declaration keyword from the metamodel's element names.</summary>
        private readonly ISurfaceForms forms;

        /// <summary>Names and renders the example pages and the index.</summary>
        private readonly INotationRenderer renderer;

        /// <summary>Reads the reserved keywords out of the grammars.</summary>
        private readonly IGrammarParser parser;

        /// <summary>Reads the metamodel index the surface forms are derived from.</summary>
        private readonly IKnowledgeReader reader;

        /// <summary>Receives the progress messages of <see cref="GenerationLog"/>.</summary>
        private readonly ILogger<TextualNotationGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextualNotationGenerator"/> class.
        /// </summary>
        /// <param name="layout">Resolves the inputs and the output folders.</param>
        /// <param name="catalog">Discovers the models.</param>
        /// <param name="forms">Derives the metaclasses' declaration keywords.</param>
        /// <param name="renderer">Names and renders the pages.</param>
        /// <param name="parser">Reads the grammars' reserved keywords.</param>
        /// <param name="reader">Reads the metamodel index.</param>
        /// <param name="logger">Receives the progress messages.</param>
        public TextualNotationGenerator(
            IKnowledgeLayout layout,
            IModelCatalog catalog,
            ISurfaceForms forms,
            INotationRenderer renderer,
            IGrammarParser parser,
            IKnowledgeReader reader,
            ILogger<TextualNotationGenerator> logger)
        {
            this.layout = layout;
            this.catalog = catalog;
            this.forms = forms;
            this.renderer = renderer;
            this.parser = parser;
            this.reader = reader;
            this.logger = logger;
        }

        /// <summary>Gets the artifact name, <c>textual-notation</c>, used when reporting progress.</summary>
        public string Artifact => "textual-notation";

        /// <summary>
        /// Gets this generator's place in a full run, <c>30</c>: after the grammar references, before the
        /// model library.
        /// </summary>
        public int Order => 30;

        /// <summary>
        /// Writes one page per model the release ships (the standard library excepted, which
        /// <see cref="ModelLibraryGenerator"/> covers) under <c>textual-notation/examples/</c>, plus the
        /// <c>index.md</c> with the keyword reference. Pages left over from a previous run are deleted first.
        /// </summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels the generation.</param>
        /// <returns>
        /// The written files, or a <see cref="GenerationOutcome.Skipped"/> result when the release ships no
        /// model.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// The models are present but the metamodel index is not (without it there are no surface forms,
        /// so every page would come out claiming the model declares nothing, wrong rather than merely
        /// incomplete), or two models slugify to the same example file name.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var textualRoot = this.layout.TextualSources(tag);

            // The standard library gets its own artifact (ModelLibraryGenerator, #80) rather than being
            // rendered as an "example": it is normative model content, not a worked example.
            var models = this.catalog.Discover(textualRoot)
                .Where(model => !model.StartsWith("sysml.library/", StringComparison.Ordinal))
                .ToList();

            if (models.Count == 0)
            {
                return GenerationResult.Skipped($"no models fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var index = this.layout.MetamodelIndex(tag);
            if (!index.Exists)
            {
                throw new InvalidOperationException(
                    $"the metamodel index for {tag} has not been generated yet; run the metamodel "
                    + "generator first, or the examples would declare no elements at all");
            }

            var surfaceForms = this.forms.Index(
                this.reader.ReadElements(index).Elements.Select(element => element.Name));

            var examples = this.layout.Examples(tag);
            examples.Create();

            // Stale pages go first: a model removed upstream would otherwise linger forever.
            foreach (var stale in examples.EnumerateFiles("*.md"))
            {
                stale.Delete();
            }

            var written = new List<FileInfo>();
            var entries = new List<ExampleEntry>();
            var indexed = new SortedDictionary<string, ExampleIndexEntry>(StringComparer.Ordinal);

            foreach (var model in models)
            {
                var source = Path.Combine(
                    textualRoot.FullName, model.Replace('/', Path.DirectorySeparatorChar));

                var text = await File.ReadAllTextAsync(source, cancellationToken);
                var language = model.EndsWith(".kerml", StringComparison.OrdinalIgnoreCase)
                    ? "KerML"
                    : "SysML";

                var fileName = this.renderer.ExampleFileName(model);
                var elements = surfaceForms.ElementsIn(text);

                written.Add(await KnowledgeFile.WriteAsync(
                    Path.Combine(examples.FullName, fileName),
                    this.renderer.RenderExample(model, text, elements, language),
                    cancellationToken));

                entries.Add(new ExampleEntry(fileName, model));
                indexed.TryAdd(
                    fileName[..^ExampleDocument.PageExtension.Length],
                    new ExampleIndexEntry(ModelName(model), language, model, fileName, elements));
            }

            // The pages land in one flat folder, so a slug collision would silently lose a model:
            // two entries in the index, one file on disk.
            if (entries.Select(entry => entry.FileName).Distinct(StringComparer.Ordinal).Count() != entries.Count)
            {
                throw new InvalidOperationException(
                    $"two models at {tag} slugified to the same example file name");
            }

            // The index the readers find an example by: written after the collision check, so it never
            // names a page that another model overwrote.
            written.Add(await KnowledgeFile.WriteAsync(
                this.layout.ExamplesIndex(tag).FullName,
                ExampleFile.Render(new ExampleDocument(ExampleDocument.CurrentSchemaVersion, tag, indexed)),
                cancellationToken));

            written.Add(await KnowledgeFile.WriteAsync(
                Path.Combine(this.layout.TextualNotation(tag).FullName, "index.md"),
                this.renderer.RenderIndex(tag, entries, await this.KeywordsAsync(tag, cancellationToken)),
                cancellationToken));

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }

        /// <summary>A model's own name: the last segment of its path, without the extension.</summary>
        /// <param name="sourcePath">The model's path relative to the textual sources, <c>/</c>-separated.</param>
        /// <returns>The name, as the example's page titles it.</returns>
        private static string ModelName(string sourcePath)
        {
            var lastSegment = sourcePath[(sourcePath.LastIndexOf('/') + 1)..];
            var dot = lastSegment.LastIndexOf('.');

            return dot < 0 ? lastSegment : lastSegment[..dot];
        }

        /// <summary>The reserved keywords of each grammar the release ships.</summary>
        /// <param name="tag">The release whose grammars are read.</param>
        /// <param name="cancellationToken">Cancels reading the grammars.</param>
        /// <returns>The reserved keywords by grammar name; an absent grammar maps to an empty list.</returns>
        private async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> KeywordsAsync(
            string tag, CancellationToken cancellationToken)
        {
            var keywords = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            foreach (var grammar in Grammars)
            {
                var file = this.layout.Grammar(tag, $"{grammar}-textual-bnf.kebnf");

                keywords[grammar] = file.Exists
                    ? this.parser.ReservedKeywords(
                        await File.ReadAllTextAsync(file.FullName, cancellationToken))
                    : [];
            }

            return keywords;
        }
    }
}
