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
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Writes <c>knowledge/&lt;tag&gt;/metamodel/</c> for every installed release.
    /// </summary>
    /// <remarks>
    /// The only fixture in this project that writes the committed knowledge base. The rest exercise a
    /// generator and assert on scratch output, so a change to one of them cannot quietly rewrite what
    /// ships.
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
        public async Task Regenerates_the_metamodel_for_every_installed_release()
        {
            var generated = 0;

            foreach (var tag in this.layout.InstalledTags)
            {
                var result = await this.generator.GenerateAsync(tag);

                if (result.Outcome == GenerationOutcome.Skipped)
                {
                    TestContext.Out.WriteLine($"{tag}: skipped - {result.Reason}");
                    continue;
                }

                var metamodel = this.layout.Metamodel(tag);

                Assert.Multiple(() =>
                {
                    Assert.That(
                        File.Exists(Path.Combine(metamodel.FullName, "elements", "PartUsage.md")), Is.True);
                    Assert.That(File.Exists(Path.Combine(metamodel.FullName, "index.md")), Is.True);
                    Assert.That(File.Exists(this.layout.MetamodelIndex(tag).FullName), Is.True);
                    Assert.That(File.Exists(Path.Combine(metamodel.FullName, "metamodel.json")), Is.True);
                    Assert.That(
                        Directory.Exists(Path.Combine(metamodel.FullName, "diagrams")), Is.True);
                    Assert.That(result.Written, Is.Not.Empty);
                });

                TestContext.Out.WriteLine($"{tag}: {result.Written.Count} files");

                generated++;
            }

            if (generated == 0)
            {
                Assert.Ignore("no release has its metamodel XMI fetched");
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
