// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeReaderTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.CrossReferences;

    /// <summary>
    /// Tests for reading the generated knowledge trees off disk.
    /// </summary>
    [TestFixture]
    public class KnowledgeReaderTests
    {
        private DirectoryInfo workspace = null!;
        private IKnowledgeReader reader = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"knowledge-{Guid.NewGuid():N}"));

            this.reader = new KnowledgeReader();
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        private FileInfo Write(string name, string content)
        {
            var file = new FileInfo(Path.Combine(this.workspace.FullName, name));
            file.Directory!.Create();
            File.WriteAllText(file.FullName, content);

            return file;
        }

        [Test]
        public void Reads_the_elements_and_the_model_uri()
        {
            var index = this.Write(
                "index.json",
                """
                {
                  "modelVersionUri": "https://www.omg.org/spec/SysML/20250201",
                  "entries": {
                    "PartUsage": { "kind": "class", "file": "elements/PartUsage.md" },
                    "VisibilityKind": { "kind": "enumeration", "file": "elements/VisibilityKind.md" }
                  }
                }
                """);

            var (elements, modelVersionUri) = this.reader.ReadElements(index);

            Assert.Multiple(() =>
            {
                Assert.That(modelVersionUri, Is.EqualTo("https://www.omg.org/spec/SysML/20250201"));
                Assert.That(elements.Select(element => element.Name), Is.EqualTo(new[]
                {
                    "PartUsage", "VisibilityKind",
                }));
                Assert.That(elements[0].File, Is.EqualTo("elements/PartUsage.md"));
            });
        }

        [Test]
        public void Reads_the_clause_titles_and_the_document_metadata()
        {
            var index = this.Write(
                "spec.json",
                """
                {
                  "document": "KerML",
                  "version": "1.0",
                  "entries": {
                    "1": { "title": "Scope", "file": "01-scope.md" },
                    "7.4.2": { "title": "Feature", "file": "07-04-02.md" }
                  }
                }
                """);

            var (titles, document) = this.reader.ReadClauseTitles(index);

            Assert.Multiple(() =>
            {
                Assert.That(document, Is.EqualTo(new SpecificationDocument("KerML", "1.0")));
                Assert.That(titles["7.4.2"], Is.EqualTo("Feature"));
            });
        }

        [Test]
        public void Reads_the_elements_front_matter_of_every_example()
        {
            this.Write(
                Path.Combine("examples", "parts.md"),
                "---\nname: Parts\nelements: [PartUsage, \"PartDefinition\"]\n---\n\n# Parts\n");
            this.Write(Path.Combine("examples", "no-elements.md"), "---\nname: Nothing\n---\n");

            var loaded = this.reader.ReadExamples(
                new DirectoryInfo(Path.Combine(this.workspace.FullName, "examples")),
                "textual-notation/examples/");

            Assert.Multiple(() =>
            {
                Assert.That(loaded.Keys, Is.EqualTo(new[] { "textual-notation/examples/parts.md" }));
                Assert.That(
                    loaded["textual-notation/examples/parts.md"],
                    Is.EqualTo(new[] { "PartUsage", "PartDefinition" }));
            });
        }

        [Test]
        public void An_example_with_no_elements_yields_an_empty_list()
        {
            this.Write(Path.Combine("examples", "empty.md"), "---\nname: Empty\nelements: []\n---\n");

            var loaded = this.reader.ReadExamples(
                new DirectoryInfo(Path.Combine(this.workspace.FullName, "examples")), "prefix/");

            Assert.That(loaded["prefix/empty.md"], Is.Empty);
        }

        [Test]
        public void A_missing_examples_directory_yields_nothing_rather_than_throwing()
        {
            var absent = new DirectoryInfo(Path.Combine(this.workspace.FullName, "not-generated"));

            Assert.That(this.reader.ReadExamples(absent, "prefix/"), Is.Empty);
        }
    }
}
