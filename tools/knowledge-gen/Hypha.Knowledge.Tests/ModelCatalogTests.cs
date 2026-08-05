// ------------------------------------------------------------------------------------------------
// <copyright file="ModelCatalogTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.TextualNotation;

    /// <summary>
    /// Tests for finding the models a release ships.
    /// </summary>
    [TestFixture]
    public class ModelCatalogTests
    {
        private DirectoryInfo workspace = null!;
        private IModelCatalog catalog = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(TestContext.CurrentContext.WorkDirectory, $"models-{Guid.NewGuid():N}"));

            this.catalog = new ModelCatalog();
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

        private void Write(string relativePath)
        {
            var file = new FileInfo(
                Path.Combine(this.workspace.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar)));

            file.Directory!.Create();
            File.WriteAllText(file.FullName, "x");
        }

        [Test]
        public void Finds_models_recursively_and_ignores_everything_else()
        {
            this.Write("sysml/b.sysml");
            this.Write("sysml/deep/a.kerml");
            this.Write("sysml/ignored.html");
            this.Write("bnf/KerML-textual-bnf.kebnf");

            Assert.That(
                this.catalog.Discover(this.workspace),
                Is.EqualTo(new[] { "sysml/b.sysml", "sysml/deep/a.kerml" }));
        }

        [Test]
        public void A_shorter_path_sorts_before_one_that_extends_it()
        {
            // Segment-wise ordering, not an ordinal sort of the whole string: '/' sits below the
            // letters, so comparing the raw strings would interleave folders with files.
            this.Write("sysml/a.sysml");
            this.Write("sysml/a b/c.sysml");

            Assert.That(
                this.catalog.Discover(this.workspace),
                Is.EqualTo(new[] { "sysml/a b/c.sysml", "sysml/a.sysml" }));
        }

        [Test]
        public void Ordering_ignores_case()
        {
            // "KerML Spec Annex A Examples" must sit among its lowercase siblings, not ahead of all
            // of them - which is what the committed index was generated with.
            this.Write("sysml/alpha.sysml");
            this.Write("sysml/Beta.sysml");
            this.Write("sysml/gamma.sysml");

            Assert.That(
                this.catalog.Discover(this.workspace),
                Is.EqualTo(new[] { "sysml/alpha.sysml", "sysml/Beta.sysml", "sysml/gamma.sysml" }));
        }

        [Test]
        public void A_missing_root_yields_nothing_rather_than_throwing()
        {
            var absent = new DirectoryInfo(Path.Combine(this.workspace.FullName, "not-fetched"));

            Assert.That(this.catalog.Discover(absent), Is.Empty);
        }
    }
}
