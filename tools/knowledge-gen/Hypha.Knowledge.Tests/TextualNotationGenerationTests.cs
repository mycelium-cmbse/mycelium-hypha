// ------------------------------------------------------------------------------------------------
// <copyright file="TextualNotationGenerationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/textual-notation/</c> for every installed release.
    /// </summary>
    /// <remarks>
    /// Generation is test-driven in this repository: running the tests <b>is</b> running the
    /// generator, and the committed files are the golden. Skips rather than fails when a release's
    /// models have not been fetched.
    /// </remarks>
    [TestFixture]
    public class TextualNotationGenerationTests
    {
        /// <summary>UTF-8 without a BOM; the knowledge base is pinned to LF by <c>.gitattributes</c>.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        private static readonly string[] Grammars = ["KerML", "SysML"];

        private IGrammarParser parser = null!;
        private ISurfaceForms forms = null!;
        private INotationRenderer renderer = null!;
        private IModelCatalog catalog = null!;

        [SetUp]
        public void SetUp()
        {
            this.parser = new GrammarParser();
            this.forms = new SurfaceForms();
            this.renderer = new NotationRenderer();
            this.catalog = new ModelCatalog();
        }

        [Test]
        public void Regenerates_the_textual_notation()
        {
            var generated = 0;

            foreach (var tag in Repository.Layout?.InstalledTags ?? [])
            {
                var textualRoot = Repository.Layout!.TextualSources(tag);
                var models = this.catalog.Discover(textualRoot);

                if (models.Count == 0)
                {
                    continue;
                }

                var written = this.Generate(tag, textualRoot, models);

                Assert.Multiple(() =>
                {
                    Assert.That(written, Has.Count.GreaterThan(100), $"expected every model at {tag}");
                    Assert.That(
                        written.Select(example => example.FileName).Distinct(StringComparer.Ordinal).Count(),
                        Is.EqualTo(written.Count),
                        "example file names must be unique");
                });

                TestContext.Out.WriteLine($"{tag}: {written.Count} example pages");

                generated++;
            }

            if (generated == 0)
            {
                Assert.Ignore("no textual sources fetched for any installed release");
            }
        }

        private List<ExampleEntry> Generate(
            string tag, DirectoryInfo textualRoot, IReadOnlyList<string> models)
        {
            var index = this.forms.Index(MetaclassNames(tag));

            var outputDirectory = Repository.Layout!.TextualNotation(tag);
            var examplesDirectory = new DirectoryInfo(Path.Combine(outputDirectory.FullName, "examples"));
            examplesDirectory.Create();

            // Stale pages are deleted first: a model removed upstream would otherwise linger forever.
            foreach (var stale in examplesDirectory.EnumerateFiles("*.md"))
            {
                stale.Delete();
            }

            var written = new List<ExampleEntry>();

            foreach (var model in models)
            {
                var path = Path.Combine(
                    textualRoot.FullName, model.Replace('/', Path.DirectorySeparatorChar));

                var text = File.ReadAllText(path, Encoding.UTF8);
                var language = model.EndsWith(".kerml", StringComparison.OrdinalIgnoreCase)
                    ? "KerML"
                    : "SysML";

                var fileName = this.renderer.ExampleFileName(model);

                Write(
                    Path.Combine(examplesDirectory.FullName, fileName),
                    this.renderer.RenderExample(model, text, index.ElementsIn(text), language));

                written.Add(new ExampleEntry(fileName, model));
            }

            Write(
                Path.Combine(outputDirectory.FullName, "index.md"),
                this.renderer.RenderIndex(tag, written, this.Keywords(textualRoot)));

            return written;
        }

        private Dictionary<string, IReadOnlyList<string>> Keywords(DirectoryInfo textualRoot)
        {
            var keywords = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

            foreach (var grammar in Grammars)
            {
                var path = Path.Combine(
                    textualRoot.FullName, "bnf", $"{grammar}-textual-bnf.kebnf");

                keywords[grammar] = File.Exists(path)
                    ? this.parser.ReservedKeywords(File.ReadAllText(path, Encoding.UTF8))
                    : [];
            }

            Assert.That(keywords["SysML"], Is.Not.Empty, "no reserved keywords read from the SysML grammar");

            return keywords;
        }

        /// <summary>
        /// The metaclass names of one release, from the generated metamodel index.
        /// </summary>
        /// <remarks>
        /// Read from <c>index.json</c> rather than from the XMI: this project does not reference
        /// uml4net, and the index is the metamodel generator's own committed output for that tag. It
        /// is also what bounds the surface forms, so the mapping cannot invent an element.
        /// </remarks>
        private static IReadOnlyList<string> MetaclassNames(string tag)
        {
            var path = Path.Combine(
                Repository.Layout!.Knowledge(tag).FullName, "metamodel", "index.json");

            // Unlike the cross-references (#96) there is no way to degrade quietly here - without the
            // index there are no surface forms and every page would come out with an empty elements
            // list. Say so, rather than letting a bare FileNotFoundException explain it.
            Assert.That(
                File.Exists(path), Is.True,
                $"the metamodel index for {tag} has not been generated yet; run metamodel-gen first");

            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));

            return document.RootElement
                .GetProperty("entries")
                .EnumerateObject()
                .Select(entry => entry.Name)
                .ToList();
        }

        private static void Write(string path, string content) =>
            File.WriteAllText(path, content, Utf8NoBom);
    }
}
