// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordGenerator.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/textual-notation/keywords.json</c>: each textual grammar's keywords
    /// and operators, with the productions that write them and the metaclasses they introduce.
    /// </summary>
    public sealed class KeywordGenerator : IKnowledgeGenerator
    {
        /// <summary>The textual grammars, by their key in the document and their file in the sources.</summary>
        private static readonly (string Key, string File)[] Grammars =
        [
            ("kerml", "KerML-textual-bnf.kebnf"),
            ("sysml", "SysML-textual-bnf.kebnf"),
        ];

        /// <summary>Resolves the grammars, the metamodel graph and the output file.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Parses the grammars into productions and reads their reserved keywords.</summary>
        private readonly IGrammarParser parser;

        /// <summary>Builds each grammar's keyword table.</summary>
        private readonly IKeywordIndexer indexer;

        /// <summary>Reads the metaclasses and their supertypes from the metamodel graph.</summary>
        private readonly IKnowledgeReader reader;

        /// <summary>Receives the progress messages of <see cref="GenerationLog"/>.</summary>
        private readonly ILogger<KeywordGenerator> logger;

        /// <summary>Initializes a new instance of the <see cref="KeywordGenerator"/> class.</summary>
        /// <param name="layout">Resolves the inputs and the output file.</param>
        /// <param name="parser">Parses the grammars.</param>
        /// <param name="indexer">Builds each grammar's keyword table.</param>
        /// <param name="reader">Reads the metamodel graph.</param>
        /// <param name="logger">Receives the progress messages.</param>
        /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
        public KeywordGenerator(
            IKnowledgeLayout layout,
            IGrammarParser parser,
            IKeywordIndexer indexer,
            IKnowledgeReader reader,
            ILogger<KeywordGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(indexer);
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.parser = parser;
            this.indexer = indexer;
            this.reader = reader;
            this.logger = logger;
        }

        /// <summary>Gets the artifact name, <c>keywords</c>, used when reporting progress.</summary>
        public string Artifact => "keywords";

        /// <summary>
        /// Gets this generator's place in a full run, <c>33</c>: after the textual notation, before the
        /// model library. It needs the metamodel graph, written at <c>10</c>.
        /// </summary>
        public int Order => 33;

        /// <summary>Writes the release's <c>keywords.json</c>.</summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels the generation.</param>
        /// <returns>
        /// The written file, or a <see cref="GenerationOutcome.Skipped"/> result when the release ships
        /// neither textual grammar.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// A grammar is present but the metamodel graph is not: without the metaclasses and their
        /// supertypes every keyword would come out introducing no metaclass, wrong rather than merely
        /// incomplete.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            var present = Array.FindAll(Grammars, grammar => this.layout.Grammar(tag, grammar.File).Exists);

            if (present.Length == 0)
            {
                return GenerationResult.Skipped($"no textual grammar fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var graph = this.layout.MetamodelGraph(tag);
            if (!graph.Exists)
            {
                throw new InvalidOperationException(
                    $"the metamodel graph for {tag} has not been generated yet; run the metamodel generator "
                    + "first, or no keyword would introduce a metaclass");
            }

            var ancestors = this.reader.ReadAncestors(graph);
            var grammars = new SortedDictionary<string, KeywordGrammar>(StringComparer.Ordinal);

            foreach (var (key, file) in present)
            {
                var text = await File.ReadAllTextAsync(this.layout.Grammar(tag, file).FullName, cancellationToken);

                grammars[key] = this.indexer.Build(this.parser.Parse(text), this.parser.ReservedKeywords(text), ancestors);
            }

            var written = await KnowledgeFile.WriteAsync(
                this.layout.Keywords(tag).FullName,
                KeywordFile.Render(new KeywordDocument(KeywordDocument.CurrentSchemaVersion, tag, grammars)),
                cancellationToken);

            return GenerationResult.Generated([written]).Report(this.logger, this.Artifact, tag);
        }
    }
}
