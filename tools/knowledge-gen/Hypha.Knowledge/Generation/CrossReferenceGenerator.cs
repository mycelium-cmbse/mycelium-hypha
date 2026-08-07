// ------------------------------------------------------------------------------------------------
// <copyright file="CrossReferenceGenerator.cs" company="Starion Group S.A.">
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

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/cross-references.json</c>.
    /// </summary>
    /// <remarks>
    /// Runs last: it reads the metamodel index and the generated example pages, so both have to exist
    /// first. It does <b>not</b> need the OMG specifications - see #88 for how the title-matched clause
    /// edges survive without them.
    /// </remarks>
    public sealed class CrossReferenceGenerator : IKnowledgeGenerator
    {
        /// <summary>The grammar file, its key in the output, and the document its clauses belong to.</summary>
        private static readonly (string File, string Key, string Document)[] Grammars =
        [
            ("KerML", "kerml", "kerml"),
            ("SysML", "sysml", "sysml2"),
        ];

        /// <summary>Where the example paths are recorded, relative to the release's knowledge folder.</summary>
        private const string ExamplePrefix = "textual-notation/examples/";

        private readonly IKnowledgeLayout layout;
        private readonly IGrammarParser parser;
        private readonly IGrammarLinks links;
        private readonly IKnowledgeReader reader;
        private readonly ICrossReferenceBuilder builder;
        private readonly ILogger<CrossReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossReferenceGenerator"/> class.
        /// </summary>
        public CrossReferenceGenerator(
            IKnowledgeLayout layout,
            IGrammarParser parser,
            IGrammarLinks links,
            IKnowledgeReader reader,
            ICrossReferenceBuilder builder,
            ILogger<CrossReferenceGenerator> logger)
        {
            this.layout = layout;
            this.parser = parser;
            this.links = links;
            this.reader = reader;
            this.builder = builder;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Artifact => "cross-references";

        /// <inheritdoc/>
        public int Order => 40;

        /// <inheritdoc/>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            if (!this.layout.MetamodelIndex(tag).Exists)
            {
                return GenerationResult.Skipped($"no metamodel index generated for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            if (!this.layout.Bnf(tag).Exists)
            {
                return GenerationResult.Skipped($"no grammar fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var document = this.builder.Build(await this.InputsAsync(tag, cancellationToken));
            var rendered = CrossReferenceFile.Render(document);

            // The licensing guarantee, checked where the bytes are produced rather than in a test that
            // could be deleted: this file records clause identifiers and is committed, so a clause
            // title reaching it would put OMG wording in the repository.
            if (rendered.Contains("\"title\"", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "the cross-references would carry a clause title; they must record identifiers only");
            }

            var path = this.layout.CrossReferences(tag);
            var written = await KnowledgeFile.WriteAsync(path.FullName, rendered, cancellationToken);

            return GenerationResult.Generated([written]).Report(this.logger, this.Artifact, tag);
        }

        private async Task<CrossReferenceInputs> InputsAsync(string tag, CancellationToken cancellationToken)
        {
            var (elements, modelVersionUri) = this.reader.ReadElements(this.layout.MetamodelIndex(tag));
            var names = elements.Select(element => element.Name).ToHashSet(StringComparer.Ordinal);

            var parsed = new Dictionary<string, IReadOnlyList<Production>>(StringComparer.Ordinal);

            foreach (var (file, key, _) in Grammars)
            {
                var grammar = this.layout.Grammar(tag, $"{file}-textual-bnf.kebnf");

                parsed[key] = grammar.Exists
                    ? this.parser.Parse(await File.ReadAllTextAsync(grammar.FullName, cancellationToken))
                    : [];
            }

            var documents = new Dictionary<string, SpecificationDocument>(StringComparer.Ordinal);
            var clauseTitles = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);

            foreach (var (_, _, key) in Grammars)
            {
                var catalog = this.layout.SpecificationCatalog(tag, key);
                if (!catalog.Exists)
                {
                    continue;
                }

                var (titles, document) = this.reader.ReadClauseTitles(catalog);
                clauseTitles[key] = titles;
                documents[key] = document;
            }

            // Without the specifications, the title-matched edges - and the document names they refer
            // to - come from the document a previous run committed. See #88. Deliberately the
            // committed path rather than the output one: generating elsewhere must still carry
            // forward from the repository, or a redirected run would silently drop these edges.
            var previous = CrossReferenceFile.ReadIfPresent(
                this.layout.CommittedCrossReferences(tag).FullName);
            var carried = clauseTitles.Count == 0
                ? CrossReferenceFile.TitleMatchedEdges(previous)
                : new Dictionary<string, IReadOnlyList<ClauseEdge>>();

            if (clauseTitles.Count == 0 && previous is not null)
            {
                documents = new Dictionary<string, SpecificationDocument>(
                    previous.Documents.ToDictionary(entry => entry.Key, entry => entry.Value),
                    StringComparer.Ordinal);
            }

            return new CrossReferenceInputs
            {
                Elements = elements,
                ModelVersionUri = modelVersionUri,
                Documents = documents,
                ClauseTitles = clauseTitles,
                CarriedClauseEdges = carried,
                Productions = Project(parsed, value => this.links.MetaclassLinks(value, names)),
                GrammarClauses = Project(parsed, value => this.links.ClauseLinks(value, names)),
                GrammarFeatures = Project(parsed, value => this.links.FeatureLinks(value, names)),
                GrammarDocuments = Grammars.ToDictionary(
                    grammar => grammar.Key, grammar => grammar.Document, StringComparer.Ordinal),
                Examples = this.reader.ReadExamples(this.layout.Examples(tag), ExamplePrefix),
            };
        }

        private static Dictionary<string, TResult> Project<TResult>(
            Dictionary<string, IReadOnlyList<Production>> parsed,
            Func<IReadOnlyList<Production>, TResult> select) =>
            parsed.ToDictionary(entry => entry.Key, entry => select(entry.Value), StringComparer.Ordinal);
    }
}
