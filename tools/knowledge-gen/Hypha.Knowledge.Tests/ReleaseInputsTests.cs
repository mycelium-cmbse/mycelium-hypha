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
            "sysml.library.xmi/Parts.xmi",
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
        public void Excludes_the_model_libraries()
        {
            // Ingesting the standard libraries is a separate decision (#80). A loose prefix match on
            // "sysml" would quietly pull in 24 MB of sysml.library.xmi.
            var selected = ReleaseInputs.SelectTextual(Tree);

            Assert.That(selected.Where(path => path.StartsWith("sysml.library")), Is.Empty);
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
