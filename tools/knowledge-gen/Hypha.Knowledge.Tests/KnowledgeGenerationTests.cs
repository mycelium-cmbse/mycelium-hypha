// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeGenerationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Writes the knowledge base this library owns, for every installed release.
    /// </summary>
    /// <remarks>
    /// Generation is test-driven in this repository: running the tests <b>is</b> running the
    /// generators, and the committed files are the golden. The orchestration itself lives in the
    /// library now, so this fixture only resolves, runs and asserts - and #81 can swap the driver
    /// without touching any of it.
    /// </remarks>
    [TestFixture]
    public class KnowledgeGenerationTests
    {
        private ServiceProvider provider = null!;
        private IKnowledgeLayout layout = null!;
        private IReadOnlyList<IKnowledgeGenerator> generators = null!;

        [SetUp]
        public void SetUp()
        {
            this.provider = new ServiceCollection().AddHyphaKnowledge().BuildServiceProvider();

            this.layout = this.provider.GetRequiredService<IKnowledgeLayout>();
            this.generators = [.. this.provider.GetServices<IKnowledgeGenerator>().OrderBy(g => g.Order)];
        }

        [TearDown]
        public void TearDown() => this.provider?.Dispose();

        [Test]
        public async Task Regenerates_every_artifact_for_every_installed_release()
        {
            var generated = 0;

            foreach (var tag in this.layout.InstalledTags)
            {
                foreach (var generator in this.generators)
                {
                    var result = await generator.GenerateAsync(tag);

                    if (result.Outcome == GenerationOutcome.Skipped)
                    {
                        TestContext.Out.WriteLine($"{tag} {generator.Artifact}: skipped - {result.Reason}");
                        continue;
                    }

                    Assert.That(
                        result.Written, Is.Not.Empty,
                        $"{generator.Artifact} reported success for {tag} but wrote nothing");
                    Assert.That(
                        result.Written, Has.All.Matches<FileInfo>(file => file.Exists && file.Length > 0));

                    TestContext.Out.WriteLine(
                        $"{tag} {generator.Artifact}: {result.Written.Count} files");

                    generated++;
                }
            }

            if (generated == 0)
            {
                Assert.Ignore("no release has its sources fetched");
            }
        }

        [Test]
        public void The_cross_references_run_last()
        {
            // They read the metamodel index and the generated example pages, so everything that
            // produces those has to have run.
            Assert.That(
                this.generators[^1].Artifact, Is.EqualTo("cross-references"),
                "cross-references must be ordered after every generator it reads from");
        }

        [Test]
        public async Task An_unfetched_release_is_skipped_with_a_reason_rather_than_failing()
        {
            foreach (var generator in this.generators)
            {
                var result = await generator.GenerateAsync("1999-01");

                Assert.Multiple(() =>
                {
                    Assert.That(
                        result.Outcome, Is.EqualTo(GenerationOutcome.Skipped),
                        $"{generator.Artifact} should skip a release that was never fetched");
                    Assert.That(result.Reason, Is.Not.Null.And.Contains("1999-01"));
                });
            }
        }

        [Test]
        public async Task Regeneration_is_stable()
        {
            // The committed knowledge base must not churn: running twice has to leave the same bytes,
            // which is also what makes "is the working tree clean?" a meaningful check afterwards.
            var tag = this.layout.DefaultTag;
            if (tag is null || !this.layout.Bnf(tag).Exists)
            {
                Assert.Ignore("no fetched release to regenerate");
                return;
            }

            var grammar = this.generators.Single(g => g.Artifact == "grammar-references");

            var first = await grammar.GenerateAsync(tag);
            var before = await File.ReadAllTextAsync(first.Written[0].FullName);

            await grammar.GenerateAsync(tag);

            Assert.That(await File.ReadAllTextAsync(first.Written[0].FullName), Is.EqualTo(before));
        }
    }
}
