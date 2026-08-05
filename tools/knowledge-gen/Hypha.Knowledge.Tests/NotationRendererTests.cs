// ------------------------------------------------------------------------------------------------
// <copyright file="NotationRendererTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;

    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Tests for the generated example pages and the per-release index.
    /// </summary>
    [TestFixture]
    public class NotationRendererTests
    {
        private INotationRenderer renderer = null!;

        [SetUp]
        public void SetUp() => this.renderer = new NotationRenderer();

        [Test]
        public void The_example_file_name_is_deterministic_and_flat()
        {
            var name = this.renderer.ExampleFileName(
                "sysml/training/02. Part Definitions/Part Definition Example.sysml");

            Assert.Multiple(() =>
            {
                Assert.That(
                    name, Is.EqualTo("sysml-training-02-part-definitions-part-definition-example.md"));
                Assert.That(name, Does.Not.Contain("/"));
                Assert.That(name, Does.Not.Contain("\\"));
            });
        }

        [Test]
        public void The_whole_path_is_slugified_so_same_named_models_do_not_collide()
        {
            // A release ships several models called Example.sysml in different folders, and every page
            // lands in one flat directory.
            Assert.That(
                this.renderer.ExampleFileName("sysml/a/Example.sysml"),
                Is.Not.EqualTo(this.renderer.ExampleFileName("sysml/b/Example.sysml")));
        }

        [Test]
        public void The_example_embeds_the_source_verbatim()
        {
            var source = "part def Vehicle {\r\n\tattribute mass : Real;\r\n}\r\n";

            var page = this.renderer.RenderExample("sysml/x.sysml", source, ["PartDefinition"], "SysML");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("```sysml\npart def Vehicle {\n\tattribute mass : Real;\n}\n```"));
                Assert.That(page, Does.Contain("source: sysml/x.sysml"));
                Assert.That(page, Does.Contain("elements: [PartDefinition]"));
                Assert.That(page, Does.Contain("- [PartDefinition](../metamodel/elements/PartDefinition.md)"));
            });
        }

        [Test]
        public void The_example_carries_no_carriage_returns()
        {
            // The committed knowledge base is pinned to LF; a \r inside the fenced block would show up
            // as a whole-file diff on a Windows checkout.
            var page = this.renderer.RenderExample(
                "sysml/x.sysml", "part def A;\r\npart def B;\r\n", [], "SysML");

            Assert.That(page, Does.Not.Contain("\r"));
        }

        [Test]
        public void The_example_omits_the_elements_section_when_there_are_none()
        {
            var page = this.renderer.RenderExample("kerml/x.kerml", "package Empty;", [], "KerML");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Not.Contain("## Elements"));
                Assert.That(page, Does.Contain("elements: []"));
            });
        }

        [Test]
        public void The_example_names_itself_after_the_model()
        {
            var page = this.renderer.RenderExample(
                "kerml/src/examples/Address Book Example/AddressBookModel.kerml",
                "package X;",
                [],
                "KerML");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("name: AddressBookModel"));
                Assert.That(page, Does.Contain("# AddressBookModel"));
            });
        }

        [Test]
        public void The_index_lists_keywords_and_examples()
        {
            var index = this.renderer.RenderIndex(
                "2026-05",
                [new ExampleEntry("a.md", "sysml/a.sysml")],
                new Dictionary<string, IReadOnlyList<string>>
                {
                    ["SysML"] = ["part", "def"],
                    ["KerML"] = ["feature"],
                });

            Assert.Multiple(() =>
            {
                Assert.That(index, Does.Contain("textual notation — 2026-05"));
                Assert.That(index, Does.Contain("## KerML keywords (1)"));
                Assert.That(index, Does.Contain("## SysML keywords (2)"));
                Assert.That(index, Does.Contain("- [a](examples/a.md) — `sysml/a.sysml`"));
            });
        }

        [Test]
        public void The_index_orders_the_grammars_so_the_page_is_stable()
        {
            var index = this.renderer.RenderIndex(
                "2026-05",
                [],
                new Dictionary<string, IReadOnlyList<string>>
                {
                    ["SysML"] = ["part"],
                    ["KerML"] = ["feature"],
                });

            Assert.That(
                index.IndexOf("## KerML keywords", System.StringComparison.Ordinal),
                Is.LessThan(index.IndexOf("## SysML keywords", System.StringComparison.Ordinal)));
        }

        [Test]
        public void Rendering_is_deterministic()
        {
            Assert.That(
                this.renderer.RenderExample("sysml/x.sysml", "part def A;", ["PartDefinition"], "SysML"),
                Is.EqualTo(
                    this.renderer.RenderExample("sysml/x.sysml", "part def A;", ["PartDefinition"], "SysML")));
        }
    }
}
