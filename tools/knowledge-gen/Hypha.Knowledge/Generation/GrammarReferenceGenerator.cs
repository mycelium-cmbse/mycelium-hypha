// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarReferenceGenerator.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/textual-notation/grammar-{kerml,sysml,graphical}.md</c>.
    /// </summary>
    public sealed class GrammarReferenceGenerator : IKnowledgeGenerator
    {
        /// <summary>The textual grammars, by their name in both the sources and the output.</summary>
        private static readonly string[] TextualGrammars = ["KerML", "SysML"];

        /// <summary>The graphical grammar's file name, rendered to <c>grammar-graphical.md</c> when present.</summary>
        private const string GraphicalGrammar = "SysML-graphical-bnf.kgbnf";

        /// <summary>Resolves the grammar inputs and the textual notation output folder.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Parses the textual and graphical grammars into productions.</summary>
        private readonly IGrammarParser parser;

        /// <summary>Renders the parsed productions as grammar reference pages.</summary>
        private readonly IGrammarReference reference;

        /// <summary>Receives the progress messages of <see cref="GenerationLog"/>.</summary>
        private readonly ILogger<GrammarReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarReferenceGenerator"/> class.
        /// </summary>
        /// <param name="layout">Resolves the grammar inputs and the output folder.</param>
        /// <param name="parser">Parses the grammars.</param>
        /// <param name="reference">Renders the grammar reference pages.</param>
        /// <param name="logger">Receives the progress messages.</param>
        public GrammarReferenceGenerator(
            IKnowledgeLayout layout,
            IGrammarParser parser,
            IGrammarReference reference,
            ILogger<GrammarReferenceGenerator> logger)
        {
            this.layout = layout;
            this.parser = parser;
            this.reference = reference;
            this.logger = logger;
        }

        /// <summary>Gets the artifact name, <c>grammar-references</c>, used when reporting progress.</summary>
        public string Artifact => "grammar-references";

        /// <summary>
        /// Gets this generator's place in a full run, <c>20</c>: after the metamodel, before the textual
        /// notation.
        /// </summary>
        public int Order => 20;

        /// <summary>
        /// Writes <c>grammar-kerml.md</c>, <c>grammar-sysml.md</c> and <c>grammar-graphical.md</c> under
        /// <c>knowledge/&lt;tag&gt;/textual-notation/</c>, one for each grammar the release ships.
        /// </summary>
        /// <param name="tag">The release to generate for.</param>
        /// <param name="cancellationToken">Cancels the generation.</param>
        /// <returns>
        /// The written files, or a <see cref="GenerationOutcome.Skipped"/> result when the release has no
        /// grammar folder or no grammar file in it.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        /// <exception cref="InvalidOperationException">
        /// A textual grammar has no productions, or a production with no clause attribution: the grammar
        /// changed shape and the reference would quietly lose its structure.
        /// </exception>
        public async Task<GenerationResult> GenerateAsync(
            string tag, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            GenerationLog.Started(this.logger, this.Artifact, tag);

            if (!this.layout.Bnf(tag).Exists)
            {
                return GenerationResult.Skipped($"no grammar fetched for {tag}")
                    .Report(this.logger, this.Artifact, tag);
            }

            var outputDirectory = this.layout.TextualNotation(tag);
            outputDirectory.Create();

            var written = new List<FileInfo>();

            foreach (var grammar in TextualGrammars)
            {
                var input = this.layout.Grammar(tag, $"{grammar}-textual-bnf.kebnf");
                if (!input.Exists)
                {
                    continue;
                }

                var productions = this.parser.Parse(
                    await File.ReadAllTextAsync(input.FullName, cancellationToken));

                // Every production is expected to carry a clause; one that does not means the grammar
                // changed shape and the reference would quietly lose its structure.
                if (productions.Count == 0 || productions.Any(p => string.IsNullOrEmpty(p.Clause)))
                {
                    throw new InvalidOperationException(
                        $"the {grammar} grammar at {tag} has productions with no clause attribution");
                }

                written.Add(await KnowledgeFile.WriteAsync(
                    Path.Combine(outputDirectory.FullName, $"grammar-{grammar.ToLowerInvariant()}.md"),
                    this.reference.Render(grammar, tag, productions),
                    cancellationToken));
            }

            var graphical = this.layout.Grammar(tag, GraphicalGrammar);

            if (graphical.Exists)
            {
                var productions = this.parser.ParseGraphical(
                    await File.ReadAllTextAsync(graphical.FullName, cancellationToken));

                written.Add(await KnowledgeFile.WriteAsync(
                    Path.Combine(outputDirectory.FullName, "grammar-graphical.md"),
                    this.reference.RenderGraphical(tag, Upstream.ReleaseRepository, productions),
                    cancellationToken));
            }

            return (written.Count == 0
                ? GenerationResult.Skipped($"no grammar files found under {this.layout.Bnf(tag).FullName}")
                : GenerationResult.Generated(written)).Report(this.logger, this.Artifact, tag);
        }
    }
}
