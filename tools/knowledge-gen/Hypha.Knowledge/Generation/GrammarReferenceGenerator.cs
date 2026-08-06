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

        private const string GraphicalGrammar = "SysML-graphical-bnf.kgbnf";

        private readonly IKnowledgeLayout layout;
        private readonly IGrammarParser parser;
        private readonly IGrammarReference reference;
        private readonly ILogger<GrammarReferenceGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarReferenceGenerator"/> class.
        /// </summary>
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

        /// <inheritdoc/>
        public string Artifact => "grammar-references";

        /// <inheritdoc/>
        public int Order => 20;

        /// <inheritdoc/>
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
