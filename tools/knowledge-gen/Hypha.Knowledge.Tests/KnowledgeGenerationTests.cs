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
    /// Asserts on how the generators this library owns are wired and what they do with a release they
    /// cannot generate.
    /// </summary>
    /// <remarks>
    /// This fixture used to run the generators over the committed knowledge base, which meant running
    /// the tests rewrote the repository. Generation is <c>hypha generate</c>'s job now; regenerating
    /// and comparing against the committed files is <c>KnowledgeRegenerationTests</c>'s, in the CLI's
    /// test project, where it can write to a scratch folder instead.
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
        public void Generation_writes_where_the_caller_asked_rather_than_into_the_repository()
        {
            // The seam that lets the CLI take --output and lets the golden tests regenerate without
            // rewriting the committed files. Inputs stay where they are: only the output moves.
            var elsewhere = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-elsewhere"));

            using var redirected = new ServiceCollection()
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = this.layout.Root;
                    options.OutputRoot = elsewhere;
                })
                .BuildServiceProvider();

            var redirectedLayout = redirected.GetRequiredService<IKnowledgeLayout>();

            Assert.Multiple(() =>
            {
                Assert.That(
                    redirectedLayout.TextualNotation("2026-05").FullName,
                    Does.StartWith(elsewhere.FullName),
                    "generated artifacts belong under the output root");
                Assert.That(
                    redirectedLayout.Bnf("2026-05").FullName,
                    Does.StartWith(this.layout.Root.FullName),
                    "inputs are still read from the repository");
                Assert.That(
                    redirectedLayout.SpecificationCatalog("2026-05", "kerml").FullName,
                    Does.StartWith(this.layout.Root.FullName),
                    "the spec catalog is written by the Python chain and read here, so it is an input");
            });
        }
    }
}
