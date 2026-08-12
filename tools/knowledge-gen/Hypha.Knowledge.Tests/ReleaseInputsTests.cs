// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInputsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Tests for selecting a release's inputs, on a listing shaped like the Release repository's.
    /// </summary>
    [TestFixture]
    public class ReleaseInputsTests
    {
        private static readonly string[] Tree =
        [
            "bnf/KerML-textual-bnf.kebnf",
            "bnf/SysML-textual-bnf.kebnf",
            "bnf/SysML-graphical-bnf.kgbnf",
            "bnf/KerML-textual-bnf.html",
            "bnf/bnf_styles.css",
            "bnf/images/part-def.svg",
            "kerml/Kernel/Connections.kerml",
            "sysml/training/02. Part Definitions/Part Definition Example.sysml",
            "sysml.library/Systems Library/Parts.sysml",
            "sysml.library/Domain Libraries/Quantities and Units/ISQ.kerml",
            "sysml.library/Systems Library/.meta.json",
            "sysml.library/Systems Library/.project.json",
            "sysml.library/.gitignore",
            "sysml.library/.project",
            "sysml.library.xmi/Parts.xmi",
            "sysml.library.kpar/Parts.kpar",
            "doc/1-Kernel_Modeling_Language.pdf",
            "README.adoc",
        ];

        [Test]
        public void Selects_the_grammar_but_not_its_rendering()
        {
            var selected = ReleaseInputs.SelectTextual(Tree);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Does.Contain("bnf/KerML-textual-bnf.kebnf"));
                Assert.That(selected, Does.Contain("bnf/SysML-graphical-bnf.kgbnf"));
                Assert.That(
                    selected.Where(path => path.EndsWith(".html") || path.EndsWith(".css") || path.EndsWith(".svg")),
                    Is.Empty);
            });
        }

        [Test]
        public void Selects_the_kerml_and_sysml_models()
        {
            var selected = ReleaseInputs.SelectTextual(Tree);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Does.Contain("kerml/Kernel/Connections.kerml"));
                Assert.That(
                    selected, Does.Contain("sysml/training/02. Part Definitions/Part Definition Example.sysml"));
            });
        }

        [Test]
        public void Excludes_pdfs_and_repository_chrome()
        {
            var selected = ReleaseInputs.SelectTextual(Tree);

            Assert.Multiple(() =>
            {
                Assert.That(selected.Where(path => path.StartsWith("doc/")), Is.Empty);
                Assert.That(selected, Does.Not.Contain("README.adoc"));
            });
        }

        [Test]
        public void Selects_the_model_library_but_not_its_xmi_kpar_siblings_or_chrome()
        {
            // As of #80: the standard libraries are normative model content, not merely an example,
            // so they are selected - but only the .sysml/.kerml models, never the XMI/kpar forms of
            // the same content or the per-package project chrome that ships alongside them.
            var selected = ReleaseInputs.SelectTextual(Tree);

            Assert.Multiple(() =>
            {
                Assert.That(selected, Does.Contain("sysml.library/Systems Library/Parts.sysml"));
                Assert.That(
                    selected, Does.Contain("sysml.library/Domain Libraries/Quantities and Units/ISQ.kerml"));
                Assert.That(selected.Where(path => path.StartsWith("sysml.library.xmi/")), Is.Empty);
                Assert.That(selected.Where(path => path.StartsWith("sysml.library.kpar/")), Is.Empty);
                Assert.That(
                    selected.Where(path => path.StartsWith("sysml.library/")
                        && !(path.EndsWith(".sysml") || path.EndsWith(".kerml"))),
                    Is.Empty,
                    "chrome files (.meta.json, .project.json, .gitignore, .project) must stay out");
            });
        }

        [Test]
        public void Selection_is_deterministic_and_grammar_first()
        {
            var first = ReleaseInputs.SelectTextual(Tree);
            var second = ReleaseInputs.SelectTextual(Tree.Reverse().ToList());

            Assert.Multiple(() =>
            {
                Assert.That(second, Is.EqualTo(first));
                Assert.That(first[0], Does.StartWith("bnf/"));
            });
        }

        [Test]
        public void Primitive_types_is_not_fetched_per_tag()
        {
            // It is the OMG UML primitives library: from neither upstream, identical every release.
            Assert.Multiple(() =>
            {
                Assert.That(ReleaseInputs.SharedXmi, Is.EqualTo("PrimitiveTypes.xmi"));
                Assert.That(ReleaseInputs.XmiFiles.Values, Does.Not.Contain(ReleaseInputs.SharedXmi));
            });
        }
    }
}
