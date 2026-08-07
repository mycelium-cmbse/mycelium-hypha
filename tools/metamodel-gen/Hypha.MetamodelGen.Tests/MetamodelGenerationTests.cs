// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelGenerationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Asserts on how the metamodel generator is wired and what it does with a release it cannot
    /// generate.
    /// </summary>
    /// <remarks>
    /// No fixture in this project writes the committed knowledge base any more: they exercise a
    /// generator and assert on scratch output. Producing <c>knowledge/&lt;tag&gt;/metamodel/</c> is
    /// <c>hypha generate metamodel</c>'s job, and proving it reproduces the committed files is
    /// <c>KnowledgeRegenerationTests</c>'s.
    /// </remarks>
    [TestFixture]
    public class MetamodelGenerationTests
    {
        private ServiceProvider provider = null!;
        private IKnowledgeLayout layout = null!;
        private IKnowledgeGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            this.provider = new ServiceCollection()
                .AddHyphaKnowledge()
                .AddHyphaMetamodelGen()
                .BuildServiceProvider();

            this.layout = this.provider.GetRequiredService<IKnowledgeLayout>();
            this.generator = this.provider.GetServices<IKnowledgeGenerator>()
                .Single(candidate => candidate.Artifact == "metamodel");
        }

        [TearDown]
        public void TearDown() => this.provider?.Dispose();

        [Test]
        public async Task The_metamodel_is_generated_into_the_folder_the_caller_asked_for()
        {
            // Everything this generator writes has to land under the output root, or the CLI's
            // --output would silently write half its files into the repository.
            var tag = this.layout.InstalledTags.FirstOrDefault();
            if (tag is null || !this.layout.Xmi(tag).Exists)
            {
                Assert.Ignore("no release has its metamodel XMI fetched");
                return;
            }

            var scratch = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-metamodel-{Guid.NewGuid():N}"));

            try
            {
                using var redirected = new ServiceCollection()
                    .AddHyphaKnowledge(options =>
                    {
                        options.RepositoryRoot = this.layout.Root;
                        options.OutputRoot = scratch;
                    })
                    .AddHyphaMetamodelGen()
                    .BuildServiceProvider();

                var result = await redirected.GetServices<IKnowledgeGenerator>()
                    .Single(candidate => candidate.Artifact == "metamodel")
                    .GenerateAsync(tag);

                var metamodel = Path.Combine(scratch.FullName, "knowledge", tag, "metamodel");

                Assert.Multiple(() =>
                {
                    Assert.That(result.Written, Is.Not.Empty);
                    Assert.That(
                        result.Written,
                        Has.All.Matches<FileInfo>(file => file.FullName.StartsWith(scratch.FullName, StringComparison.Ordinal)),
                        "nothing may be written outside the requested output folder");
                    Assert.That(File.Exists(Path.Combine(metamodel, "elements", "PartUsage.md")), Is.True);
                    Assert.That(File.Exists(Path.Combine(metamodel, "index.md")), Is.True);
                    Assert.That(File.Exists(Path.Combine(metamodel, "index.json")), Is.True);
                    Assert.That(File.Exists(Path.Combine(metamodel, "metamodel.json")), Is.True);
                    Assert.That(Directory.Exists(Path.Combine(metamodel, "diagrams")), Is.True);
                });
            }
            finally
            {
                scratch.Refresh();
                if (scratch.Exists)
                {
                    scratch.Delete(recursive: true);
                }
            }
        }

        [Test]
        public async Task An_unfetched_release_is_skipped_with_a_reason()
        {
            var result = await this.generator.GenerateAsync("1999-01");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("1999-01"));
                Assert.That(result.Written, Is.Empty);
            });
        }

        [Test]
        public void The_metamodel_runs_before_everything_that_reads_its_index()
        {
            // The textual notation derives surface forms from the element names, and the
            // cross-references key every entry on them.
            var others = this.provider.GetServices<IKnowledgeGenerator>()
                .Where(candidate => candidate.Artifact != "metamodel");

            Assert.That(others.Select(other => other.Order), Has.All.GreaterThan(this.generator.Order));
        }
    }
}
