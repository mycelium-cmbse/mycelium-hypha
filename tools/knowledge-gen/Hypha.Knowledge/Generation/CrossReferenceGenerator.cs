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
    /// first. It does <b>not</b> need the OMG specifications; see #88 for how the title-matched clause
    /// edges survive without them, and #113 for why that no longer depends on a committed prior
    /// document to carry them from.
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

        /// <summary>Resolves the metamodel index, the grammars, the spec catalogs, the examples and the output.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Parses the textual grammars into productions.</summary>
        private readonly IGrammarParser parser;

        /// <summary>Links the parsed productions to metaclasses, clauses and metamodel features.</summary>
        private readonly IGrammarLinks links;

        /// <summary>Reads the metamodel index, the clause catalogs and the generated example pages.</summary>
        private readonly IKnowledgeReader reader;

        /// <summary>Assembles the cross-reference document from the gathered inputs.</summary>
        private readonly ICrossReferenceBuilder builder;

        /// <summary>Receives the progress messages of <see cref="GenerationLog"/>.</summary>
        private readonly ILogger<CrossReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossReferenceGenerator"/> class.
        /// </summary>
        /// <param name="layout">Resolves the inputs and the output file.</param>
        /// <param name="parser">Parses the textual grammars.</param>
        /// <param name="links">Links productions to metaclasses, clauses and features.</param>
        /// <param name="reader">Reads the generated knowledge the cross-references are built from.</param>
        /// <param name="builder">Assembles the cross-reference document.</param>
        /// <param name="logger">Receives the progress messages.</param>
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

        /// <summary>Gets the artifact name, <c>cross-references</c>, used when reporting progress.</summary>
        public string Artifact => "cross-references";

        /// <summary>
        /// Gets this generator's place in a full run, <c>40</c>: last, because it reads the metamodel index and
        /// the generated example pages.
        /// </summary>
        public int Order => 40;

        /// <summary>
        /// Writes <c>knowledge/&lt;tag&gt;/cross-references.json</c> from the metamodel index, the textual
        /// grammars, the generated examples and, when present, the specification clause catalogs.
        /// </summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels the generation.</param>
        /// <returns>
        /// The written document, or a <see cref="GenerationOutcome.Skipped"/> result when the release has no
        /// metamodel index or no grammar folder.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// The rendered document would carry a clause title, which would put OMG wording in it.
        /// </exception>
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

        /// <summary>
        /// Gathers everything the <see cref="ICrossReferenceBuilder"/> needs for one release.
        /// </summary>
        /// <param name="tag">The release to gather the inputs of.</param>
        /// <param name="cancellationToken">Cancels reading the grammars.</param>
        /// <returns>
        /// The inputs. Without a clause catalog, the title-matched clause edges and the document names are
        /// carried from the document a previous run left under the repository root; when neither exists, the
        /// inputs allow a grammar-only document and a <see cref="GenerationLog.Degraded"/> warning is logged.
        /// </returns>
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

            // Without the specifications, the title-matched edges (and the document names they refer
            // to) come from the document a previous run committed. See #88. Deliberately the
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

            // Neither source of title-matched edges exists: no specification PDFs were extracted (the
            // ordinary case for every install without a maintainer source checkout, see #113) and
            // nothing was carried forward (nothing is committed for any release any more, see #106).
            // That combination is no longer the rare accident CrossReferenceInputs.AllowGrammarOnly's
            // doc comment describes; it is the default state of a real install, so state the
            // grammar-only document is intended rather than let the builder refuse it.
            var grammarOnly = clauseTitles.Count == 0 && carried.Count == 0;

            if (grammarOnly)
            {
                GenerationLog.Degraded(this.logger, this.Artifact, tag);
            }

            return new CrossReferenceInputs
            {
                Elements = elements,
                ModelVersionUri = modelVersionUri,
                Documents = documents,
                ClauseTitles = clauseTitles,
                CarriedClauseEdges = carried,
                AllowGrammarOnly = grammarOnly,
                Productions = Project(parsed, value => this.links.MetaclassLinks(value, names)),
                GrammarClauses = Project(parsed, value => this.links.ClauseLinks(value, names)),
                GrammarFeatures = Project(parsed, value => this.links.FeatureLinks(value, names)),
                GrammarDocuments = Grammars.ToDictionary(
                    grammar => grammar.Key, grammar => grammar.Document, StringComparer.Ordinal),
                Examples = this.reader.ReadExamples(this.layout.Examples(tag), ExamplePrefix),
            };
        }

        /// <summary>Applies <paramref name="select"/> to each grammar's productions, keeping the grammar keys.</summary>
        /// <typeparam name="TResult">What each grammar's productions are projected to.</typeparam>
        /// <param name="parsed">The parsed productions, by grammar key.</param>
        /// <param name="select">The projection.</param>
        /// <returns>The projected values, by grammar key, with ordinal keys.</returns>
        private static Dictionary<string, TResult> Project<TResult>(
            Dictionary<string, IReadOnlyList<Production>> parsed,
            Func<IReadOnlyList<Production>, TResult> select) =>
            parsed.ToDictionary(entry => entry.Key, entry => select(entry.Value), StringComparer.Ordinal);
    }
}
