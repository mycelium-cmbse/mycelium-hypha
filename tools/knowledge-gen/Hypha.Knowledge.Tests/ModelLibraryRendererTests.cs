// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryRendererTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using Hypha.Knowledge.ModelLibrary;

    /// <summary>
    /// Tests for the generated standard-library package pages and the per-release index.
    /// </summary>
    [TestFixture]
    public class ModelLibraryRendererTests
    {
        private IModelLibraryRenderer renderer = null!;

        [SetUp]
        public void SetUp() => this.renderer = new ModelLibraryRenderer();

        [Test]
        public void The_package_embeds_the_source_verbatim()
        {
            var source = "standard library package ScalarValues {\r\n\tdatatype Real;\r\n}\r\n";

            var page = this.renderer.RenderPackage(
                "sysml.library/Kernel Libraries/Kernel Data Type Library/ScalarValues.kerml",
                source,
                [new LibraryDeclaration("ScalarValues", "standard library package"),
                 new LibraryDeclaration("ScalarValues::Real", "datatype")],
                "KerML");

            Assert.Multiple(() =>
            {
                Assert.That(
                    page,
                    Does.Contain(
                        "```kerml\nstandard library package ScalarValues {\n\tdatatype Real;\n}\n```"));
                Assert.That(
                    page,
                    Does.Contain(
                        "source: sysml.library/Kernel Libraries/Kernel Data Type Library/ScalarValues.kerml"));
                Assert.That(page, Does.Contain("declares: [ScalarValues, ScalarValues::Real]"));
                Assert.That(page, Does.Contain("- `ScalarValues::Real` — datatype"));
            });
        }

        [Test]
        public void The_package_carries_no_carriage_returns()
        {
            var page = this.renderer.RenderPackage(
                "sysml.library/x.sysml", "package A;\r\npackage B;\r\n", [], "SysML");

            Assert.That(page, Does.Not.Contain("\r"));
        }

        [Test]
        public void The_package_omits_the_declarations_section_when_there_are_none()
        {
            var page = this.renderer.RenderPackage("sysml.library/x.sysml", "package Empty;", [], "SysML");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Not.Contain("## Declarations"));
                Assert.That(page, Does.Contain("declares: []"));
            });
        }

        [Test]
        public void The_package_names_itself_after_the_file()
        {
            var page = this.renderer.RenderPackage(
                "sysml.library/Domain Libraries/Quantities and Units/ISQBase.sysml",
                "package X;",
                [],
                "SysML");

            Assert.Multiple(() =>
            {
                Assert.That(page, Does.Contain("name: ISQBase"));
                Assert.That(page, Does.Contain("# ISQBase"));
            });
        }

        [Test]
        public void The_index_groups_packages_by_their_upstream_top_level_folder()
        {
            var index = this.renderer.RenderIndex(
                "2026-05",
                [
                    new ModelLibraryPackageEntry(
                        "isq.md", "sysml.library/Domain Libraries/Quantities and Units/ISQ.sysml"),
                    new ModelLibraryPackageEntry(
                        "parts.md", "sysml.library/Systems Library/Parts.sysml"),
                ]);

            Assert.Multiple(() =>
            {
                Assert.That(index, Does.Contain("standard model library — 2026-05"));
                Assert.That(index, Does.Contain("## Domain Libraries"));
                Assert.That(index, Does.Contain("## Systems Library"));
                Assert.That(
                    index,
                    Does.Contain(
                        "- [ISQ](packages/isq.md) — "
                        + "`sysml.library/Domain Libraries/Quantities and Units/ISQ.sysml`"));
                Assert.That(
                    index,
                    Does.Contain("- [Parts](packages/parts.md) — `sysml.library/Systems Library/Parts.sysml`"));
            });
        }

        [Test]
        public void The_index_orders_groups_so_the_page_is_stable()
        {
            var index = this.renderer.RenderIndex(
                "2026-05",
                [
                    new ModelLibraryPackageEntry("a.md", "sysml.library/Systems Library/A.sysml"),
                    new ModelLibraryPackageEntry("b.md", "sysml.library/Domain Libraries/B.sysml"),
                ]);

            Assert.That(
                index.IndexOf("## Domain Libraries", System.StringComparison.Ordinal),
                Is.LessThan(index.IndexOf("## Systems Library", System.StringComparison.Ordinal)));
        }

        [Test]
        public void Rendering_is_deterministic()
        {
            Assert.That(
                this.renderer.RenderPackage(
                    "sysml.library/x.sysml", "package A;", [new LibraryDeclaration("A", "package")], "SysML"),
                Is.EqualTo(
                    this.renderer.RenderPackage(
                        "sysml.library/x.sysml", "package A;", [new LibraryDeclaration("A", "package")], "SysML")));
        }
    }
}
