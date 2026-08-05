// ------------------------------------------------------------------------------------------------
// <copyright file="GrammarReferenceGenerationTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Grammar;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/textual-notation/grammar-*.md</c> for every installed release.
    /// </summary>
    /// <remarks>
    /// Generation is test-driven in this repository: running the tests <b>is</b> running the generator,
    /// and the committed files are the golden. Skips rather than fails when a release's grammar has not
    /// been fetched, so CI stays green without the optional inputs.
    /// </remarks>
    [TestFixture]
    public class GrammarReferenceGenerationTests
    {
        /// <summary>UTF-8 without a BOM; the knowledge base is pinned to LF by <c>.gitattributes</c>.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        private IGrammarParser parser = null!;
        private IGrammarReference reference = null!;

        [SetUp]
        public void SetUp()
        {
            this.parser = new GrammarParser();
            this.reference = new GrammarReference();
        }

        [Test]
        public void Regenerates_the_textual_grammar_references()
        {
            var generated = 0;

            foreach (var tag in InstalledTags())
            {
                var bnf = RepositoryLayout.BnfDirectory(tag);
                if (!bnf.Exists)
                {
                    continue;
                }

                var outputDirectory = RepositoryLayout.TextualNotationDirectory(tag);
                outputDirectory.Create();

                foreach (var grammar in new[] { "KerML", "SysML" })
                {
                    var input = new FileInfo(Path.Combine(bnf.FullName, $"{grammar}-textual-bnf.kebnf"));
                    if (!input.Exists)
                    {
                        continue;
                    }

                    var productions = this.parser.Parse(File.ReadAllText(input.FullName, Encoding.UTF8));

                    Assert.Multiple(() =>
                    {
                        Assert.That(productions, Is.Not.Empty, $"no productions parsed from {input.Name}");
                        Assert.That(
                            productions.Where(production => string.IsNullOrEmpty(production.Clause)),
                            Is.Empty,
                            $"every {grammar} production should be attributed to a clause");
                    });

                    Write(
                        Path.Combine(outputDirectory.FullName, $"grammar-{grammar.ToLowerInvariant()}.md"),
                        this.reference.Render(grammar, tag, productions));

                    TestContext.Out.WriteLine(
                        $"{tag} {grammar}: {productions.Count} productions, "
                        + $"{productions.Count(production => production.Produces is not null)} typed");

                    generated++;
                }
            }

            AssertGenerated(generated);
        }

        [Test]
        public void Regenerates_the_graphical_grammar_reference()
        {
            var generated = 0;

            foreach (var tag in InstalledTags())
            {
                var input = new FileInfo(
                    Path.Combine(RepositoryLayout.BnfDirectory(tag).FullName, "SysML-graphical-bnf.kgbnf"));

                if (!input.Exists)
                {
                    continue;
                }

                var outputDirectory = RepositoryLayout.TextualNotationDirectory(tag);
                outputDirectory.Create();

                var productions = this.parser.ParseGraphical(
                    File.ReadAllText(input.FullName, Encoding.UTF8));

                var withImages = productions.Count(production => production.Images.Count > 0);

                Assert.Multiple(() =>
                {
                    Assert.That(productions, Is.Not.Empty, "no productions parsed from the graphical grammar");
                    Assert.That(withImages, Is.GreaterThan(0), "no notation images referenced");
                });

                Write(
                    Path.Combine(outputDirectory.FullName, "grammar-graphical.md"),
                    this.reference.RenderGraphical(tag, Upstream.ReleaseRepository, productions));

                TestContext.Out.WriteLine(
                    $"{tag} graphical: {productions.Count} productions, {withImages} with images");

                generated++;
            }

            AssertGenerated(generated);
        }

        [Test]
        public void Regeneration_is_stable()
        {
            // Rendering the same grammar twice must give the same bytes, or the committed knowledge base
            // would churn on every run regardless of whether anything upstream moved.
            var tag = InstalledTags().FirstOrDefault();
            var input = tag is null
                ? null
                : new FileInfo(
                    Path.Combine(RepositoryLayout.BnfDirectory(tag).FullName, "KerML-textual-bnf.kebnf"));

            if (input is null || !input.Exists)
            {
                Assert.Ignore("no grammar fetched");
                return;
            }

            var text = File.ReadAllText(input.FullName, Encoding.UTF8);

            Assert.That(
                this.reference.Render("KerML", tag!, this.parser.Parse(text)),
                Is.EqualTo(this.reference.Render("KerML", tag!, this.parser.Parse(text))));
        }

        private static IReadOnlyList<string> InstalledTags() =>
            RepositoryLayout.RepositoryRoot is null ? [] : RepositoryLayout.InstalledTags;

        private static void AssertGenerated(int generated)
        {
            if (generated == 0)
            {
                Assert.Ignore("no grammar sources fetched for any installed release");
            }
        }

        private static void Write(string path, string content) =>
            File.WriteAllText(path, content, Utf8NoBom);
    }
}
