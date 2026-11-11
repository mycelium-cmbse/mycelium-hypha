// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Hypha.Knowledge.CrossReferences;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Keywords;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging.Abstractions;

    [TestFixture]
    public class KeywordGeneratorTests
    {
        private const string Tag = "2026-05";

        private const string KerML =
            """
            RESERVED_KEYWORD =
                'feature' | 'abstract'

            Feature =
                FeaturePrefix 'feature'

            FeaturePrefix =
                ( isAbstract ?= 'abstract' )? ':>'
            """;

        private DirectoryInfo root = null!;
        private KnowledgeLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-keywords-{Guid.NewGuid():N}"));
            this.layout = new KnowledgeLayout(this.root);
        }

        [TearDown]
        public void TearDown()
        {
            this.root.Refresh();
            if (this.root.Exists)
            {
                this.root.Delete(recursive: true);
            }
        }

        [Test]
        public void A_release_without_a_textual_grammar_is_skipped()
        {
            var result = this.Generator().GenerateAsync(Tag).GetAwaiter().GetResult();

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(this.layout.Keywords(Tag).Exists, Is.False);
            });
        }

        [Test]
        public void A_grammar_without_the_metamodel_graph_is_refused()
        {
            this.WriteGrammar("KerML-textual-bnf.kebnf", KerML);

            Assert.That(
                () => this.Generator().GenerateAsync(Tag),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("metamodel graph"));
        }

        [Test]
        public async Task The_document_names_the_release_and_holds_only_the_grammars_it_ships()
        {
            this.WriteGrammar("KerML-textual-bnf.kebnf", KerML);
            this.WriteGraph();

            var result = await this.Generator().GenerateAsync(Tag);
            var text = await File.ReadAllTextAsync(this.layout.Keywords(Tag).FullName);
            using var document = JsonDocument.Parse(text);
            var kerml = document.RootElement.GetProperty("grammars").GetProperty("kerml");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(result.Written.Select(file => file.FullName), Is.EqualTo(new[] { this.layout.Keywords(Tag).FullName }));
                Assert.That(document.RootElement.GetProperty("schemaVersion").GetString(), Is.EqualTo(KeywordDocument.CurrentSchemaVersion));
                Assert.That(document.RootElement.GetProperty("tag").GetString(), Is.EqualTo(Tag));
                Assert.That(document.RootElement.GetProperty("grammars").EnumerateObject().Select(grammar => grammar.Name), Is.EqualTo(new[] { "kerml" }));
                Assert.That(kerml.GetProperty("entries").EnumerateObject().Select(entry => entry.Name), Is.EqualTo(new[] { ":>", "abstract", "feature" }));
                Assert.That(
                    kerml.GetProperty("entries").GetProperty(":>").GetProperty("metaclasses")[0].GetProperty("via").GetString(),
                    Is.EqualTo(KeywordMetaclass.Helper));
                Assert.That(text, Does.Not.Contain("\r"));
                Assert.That(text, Does.EndWith("}\n"));
            });
        }

        [Test]
        public async Task SysML_lists_what_the_KerML_productions_it_calls_write_and_without_KerML_nothing()
        {
            const string sysml =
                """
                RESERVED_KEYWORD =
                    'part' | 'and'

                PartUsage =
                    'part' value = OwnedExpression
                """;

            this.WriteGrammar("SysML-textual-bnf.kebnf", sysml);
            this.WriteGraph();

            await this.Generator().GenerateAsync(Tag);
            var alone = Entry(await File.ReadAllTextAsync(this.layout.Keywords(Tag).FullName), "and");

            this.WriteGrammar("KerML-textual-bnf.kebnf", KerML + "\n\nOwnedExpression =\n    'and'\n");
            await this.Generator().GenerateAsync(Tag);
            var text = await File.ReadAllTextAsync(this.layout.Keywords(Tag).FullName);

            Assert.Multiple(() =>
            {
                Assert.That(alone, Is.EqualTo("reserved sysml "));
                Assert.That(Entry(text, "and"), Is.EqualTo("reserved kerml OwnedExpression"));
                Assert.That(Entry(text, "part"), Is.EqualTo("reserved sysml PartUsage"));
                Assert.That(text, Does.Contain("\":>\"").And.Not.Contain("\\u003E"));
            });

            static string Entry(string text, string word)
            {
                using var document = JsonDocument.Parse(text);
                var entry = document.RootElement.GetProperty("grammars").GetProperty("sysml").GetProperty("entries").GetProperty(word);

                return $"{entry.GetProperty("kind").GetString()} {entry.GetProperty("syntaxFrom").GetString()} "
                       + string.Join(",", entry.GetProperty("productions").EnumerateArray().Select(production => production.GetString()));
            }
        }

        [Test]
        public async Task Two_runs_over_the_same_inputs_write_the_same_bytes()
        {
            this.WriteGrammar("KerML-textual-bnf.kebnf", KerML);
            this.WriteGrammar("SysML-textual-bnf.kebnf", KerML.Replace("'feature'", "'part'", StringComparison.Ordinal));
            this.WriteGraph();

            await this.Generator().GenerateAsync(Tag);
            var first = await File.ReadAllBytesAsync(this.layout.Keywords(Tag).FullName);
            await this.Generator().GenerateAsync(Tag);

            Assert.That(await File.ReadAllBytesAsync(this.layout.Keywords(Tag).FullName), Is.EqualTo(first));
        }

        [Test]
        public void The_generator_runs_after_the_metamodel_and_needs_every_collaborator()
        {
            var parser = new GrammarParser();
            var indexer = new KeywordIndexer();
            var reader = new KnowledgeReader();
            var logger = NullLogger<KeywordGenerator>.Instance;

            Assert.Multiple(() =>
            {
                Assert.That(this.Generator().Artifact, Is.EqualTo("keywords"));
                Assert.That(this.Generator().Order, Is.EqualTo(33));
                Assert.That(() => new KeywordGenerator(null!, parser, indexer, reader, logger), Throws.ArgumentNullException);
                Assert.That(() => new KeywordGenerator(this.layout, null!, indexer, reader, logger), Throws.ArgumentNullException);
                Assert.That(() => new KeywordGenerator(this.layout, parser, null!, reader, logger), Throws.ArgumentNullException);
                Assert.That(() => new KeywordGenerator(this.layout, parser, indexer, null!, logger), Throws.ArgumentNullException);
                Assert.That(() => new KeywordGenerator(this.layout, parser, indexer, reader, null!), Throws.ArgumentNullException);
            });
        }

        private KeywordGenerator Generator() =>
            new(this.layout, new GrammarParser(), new KeywordIndexer(), new KnowledgeReader(), NullLogger<KeywordGenerator>.Instance);

        private void WriteGrammar(string fileName, string text)
        {
            var file = this.layout.Grammar(Tag, fileName);
            file.Directory!.Create();
            File.WriteAllText(file.FullName, text);
        }

        private void WriteGraph()
        {
            var graph = this.layout.MetamodelGraph(Tag);
            graph.Directory!.Create();
            File.WriteAllText(
                graph.FullName,
                """{ "classes": [ { "name": "Element", "allAncestors": [] }, { "name": "Feature", "allAncestors": ["Element"] } ] }""");
        }
    }
}
