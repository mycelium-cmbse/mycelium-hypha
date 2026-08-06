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
        private static readonly string[] Grammars = ["KerML", "SysML"];

        private readonly IKnowledgeLayout layout;
        private readonly IModelCatalog catalog;
        private readonly ISurfaceForms forms;
        private readonly INotationRenderer renderer;
        private readonly IGrammarParser parser;
        private readonly IKnowledgeReader reader;
        private readonly ILogger<TextualNotationGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextualNotationGenerator"/> class.
        /// </summary>
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

        /// <inheritdoc/>
        public string Artifact => "textual-notation";

        /// <inheritdoc/>
        public int Order => 30;

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// The models are present but the metamodel index is not. Without it there are no surface
        /// forms, so every page would come out claiming the model declares nothing - wrong rather than
        /// merely incomplete.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var textualRoot = this.layout.TextualSources(tag);
            var models = this.catalog.Discover(textualRoot);

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

            foreach (var model in models)
            {
                var source = Path.Combine(
                    textualRoot.FullName, model.Replace('/', Path.DirectorySeparatorChar));

                var text = await File.ReadAllTextAsync(source, cancellationToken);
                var language = model.EndsWith(".kerml", StringComparison.OrdinalIgnoreCase)
                    ? "KerML"
                    : "SysML";

                var fileName = this.renderer.ExampleFileName(model);

                written.Add(await KnowledgeFile.WriteAsync(
                    Path.Combine(examples.FullName, fileName),
                    this.renderer.RenderExample(model, text, surfaceForms.ElementsIn(text), language),
                    cancellationToken));

                entries.Add(new ExampleEntry(fileName, model));
            }

            // The pages land in one flat folder, so a slug collision would silently lose a model:
            // two entries in the index, one file on disk.
            if (entries.Select(entry => entry.FileName).Distinct(StringComparer.Ordinal).Count() != entries.Count)
            {
                throw new InvalidOperationException(
                    $"two models at {tag} slugified to the same example file name");
            }

            written.Add(await KnowledgeFile.WriteAsync(
                Path.Combine(this.layout.TextualNotation(tag).FullName, "index.md"),
                this.renderer.RenderIndex(tag, entries, await this.KeywordsAsync(tag, cancellationToken)),
                cancellationToken));

            return GenerationResult.Generated(written).Report(this.logger, this.Artifact, tag);
        }

        /// <summary>The reserved keywords of each grammar the release ships.</summary>
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
