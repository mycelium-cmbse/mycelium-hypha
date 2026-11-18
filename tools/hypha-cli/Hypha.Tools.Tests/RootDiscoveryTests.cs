// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Tools.Tests
{
    using System.IO;

    using Hypha.Knowledge.Layout;
    using Hypha.Tools.Hosting;

    using NUnit.Framework;

    public class RootDiscoveryTests
    {
        private DirectoryInfo workspace = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateTempSubdirectory("hypha-root-discovery-");
        }

        [TearDown]
        public void TearDown()
        {
            this.workspace.Delete(recursive: true);
        }

        [Test]
        public void A_verb_started_inside_a_checkout_uses_that_checkout()
        {
            var project = this.Checkout();

            var root = KnowledgeServices.DiscoverRoot(Program.BuildRootCommand().Parse(["list"]), project);

            Assert.That(root?.FullName, Is.EqualTo(this.workspace.FullName));
        }

        [Test]
        public void Serve_started_inside_a_checkout_does_not_use_it()
        {
            // An AI client chooses the server's working directory; what it installs belongs in the
            // folder every client shares, so only --repository-root makes a checkout the root.
            var project = this.Checkout();

            var root = KnowledgeServices.DiscoverRoot(Program.BuildRootCommand().Parse(["serve"]), project);

            Assert.That(root, Is.Null);
        }

        [Test]
        public void A_project_with_sources_and_knowledge_folders_is_not_a_checkout()
        {
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, KnowledgeLayout.SourcesFolder));
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, KnowledgeLayout.KnowledgeFolder));

            var root = KnowledgeServices.DiscoverRoot(Program.BuildRootCommand().Parse(["list"]), this.workspace);

            Assert.That(root?.FullName, Is.Not.EqualTo(this.workspace.FullName));
        }

        private DirectoryInfo Checkout()
        {
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, KnowledgeLayout.SourcesFolder));
            Directory.CreateDirectory(Path.Combine(this.workspace.FullName, KnowledgeLayout.KnowledgeFolder));
            File.WriteAllText(Path.Combine(this.workspace.FullName, KnowledgeLayout.CheckoutMarker), string.Empty);

            return Directory.CreateDirectory(Path.Combine(this.workspace.FullName, "models", "vehicle"));
        }
    }
}
