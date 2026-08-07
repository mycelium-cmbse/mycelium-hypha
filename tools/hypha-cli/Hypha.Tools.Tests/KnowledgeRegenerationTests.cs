// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeRegenerationTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.MetamodelGen;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Regenerates the whole knowledge base into a scratch folder and compares it, byte for byte,
    /// against the committed one.
    /// </summary>
    /// <remarks>
    /// This is the fixture that replaced "running the tests writes the knowledge base". Generation is
    /// the CLI's job now; what a test can still do - and what nothing else does - is prove that the
    /// generators are deterministic and that the committed files are what they produce. Writing to a
    /// scratch root is what makes both true at once: a failure here is a real diff, not a repository
    /// that has quietly been rewritten.
    /// </remarks>
    [TestFixture]
    public class KnowledgeRegenerationTests
    {
        /// <summary>The folders under <c>knowledge/&lt;tag&gt;/</c> this toolchain does not produce.</summary>
        /// <remarks>
        /// <c>spec/</c> is written by the Python PDF chain from the OMG PDFs, is git-ignored, and is an
        /// input here rather than an output.
        /// </remarks>
        private static readonly string[] NotGeneratedHere = ["spec"];

        private DirectoryInfo scratch = null!;
        private ServiceProvider provider = null!;
        private IKnowledgeLayout committed = null!;
        private readonly Dictionary<string, IReadOnlyList<GenerationResult>> results = [];

        [OneTimeSetUp]
        public async Task Regenerate()
        {
            var repository = KnowledgeLayout.Discover();
            if (repository is null)
            {
                Assert.Ignore("not running inside a checkout");
                return;
            }

            this.committed = repository;

            this.scratch = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-regen-{Guid.NewGuid():N}"));

            this.provider = new ServiceCollection()
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = repository.Root;
                    options.OutputRoot = this.scratch;
                })
                .AddHyphaMetamodelGen()
                .BuildServiceProvider();

            var generators = this.provider.GetServices<IKnowledgeGenerator>()
                .OrderBy(generator => generator.Order)
                .ToList();

            foreach (var tag in repository.InstalledTags)
            {
                var forTag = new List<GenerationResult>();

                foreach (var generator in generators)
                {
                    forTag.Add(await generator.GenerateAsync(tag));
                }

                this.results[tag] = forTag;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            this.provider?.Dispose();

            this.scratch?.Refresh();
            if (this.scratch?.Exists == true)
            {
                this.scratch.Delete(recursive: true);
            }
        }

        [Test]
        public void Every_regenerated_file_is_byte_identical_to_the_committed_one()
        {
            var compared = 0;
            var differing = new List<string>();

            foreach (var (tag, forTag) in this.results)
            {
                foreach (var written in forTag.SelectMany(result => result.Written))
                {
                    var relative = Path.GetRelativePath(this.scratch.FullName, written.FullName);
                    var committedFile = Path.Combine(this.committed.Root.FullName, relative);

                    compared++;

                    if (!File.Exists(committedFile)
                        || !File.ReadAllBytes(committedFile).AsSpan()
                            .SequenceEqual(File.ReadAllBytes(written.FullName)))
                    {
                        differing.Add($"{tag}: {relative}");
                    }
                }
            }

            if (compared == 0)
            {
                Assert.Ignore("no release has its sources fetched");
            }

            Assert.That(
                differing,
                Is.Empty,
                "regeneration no longer reproduces the committed knowledge base; if the change is "
                + "intended, regenerate with 'hypha generate' and commit the result");

            TestContext.Out.WriteLine($"{compared} files compared");
        }

        [Test]
        public void Regeneration_leaves_nothing_committed_behind()
        {
            // A file that generation stopped producing would otherwise sit in the knowledge base
            // forever: the byte comparison above only looks at what was written this time.
            var stale = new List<string>();
            var checkedTags = 0;

            foreach (var (tag, forTag) in this.results)
            {
                if (forTag.Any(result => result.Outcome == GenerationOutcome.Skipped))
                {
                    // A skipped artifact means its committed files were legitimately not rewritten.
                    continue;
                }

                checkedTags++;

                var regenerated = forTag
                    .SelectMany(result => result.Written)
                    .Select(file => Path.GetRelativePath(this.scratch.FullName, file.FullName))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var relative in this.CommittedArtifactsOf(tag))
                {
                    if (!regenerated.Contains(relative))
                    {
                        stale.Add($"{tag}: {relative}");
                    }
                }
            }

            if (checkedTags == 0)
            {
                Assert.Ignore("no release generated every artifact");
            }

            Assert.That(
                stale, Is.Empty, "these committed files are no longer produced by any generator");
        }

        [Test]
        public async Task Regeneration_is_stable_within_a_run()
        {
            // The comparison above proves this run matches an earlier one. This proves two runs in the
            // same process match each other, which is where a cached comparer or a reused buffer would
            // show up.
            var tag = this.results.Keys.FirstOrDefault();
            if (tag is null)
            {
                Assert.Ignore("no release has its sources fetched");
                return;
            }

            var grammar = this.provider.GetServices<IKnowledgeGenerator>()
                .Single(generator => generator.Artifact == "grammar-references");

            var first = await grammar.GenerateAsync(tag);
            if (first.Outcome == GenerationOutcome.Skipped)
            {
                Assert.Ignore($"no grammar fetched for {tag}");
                return;
            }

            var before = await File.ReadAllTextAsync(first.Written[0].FullName);

            await grammar.GenerateAsync(tag);

            Assert.That(await File.ReadAllTextAsync(first.Written[0].FullName), Is.EqualTo(before));
        }

        /// <summary>
        /// The committed files under <c>knowledge/&lt;tag&gt;/</c> that this toolchain is responsible
        /// for, relative to the repository root.
        /// </summary>
        private IEnumerable<string> CommittedArtifactsOf(string tag)
        {
            var knowledge = new DirectoryInfo(
                Path.Combine(this.committed.Root.FullName, KnowledgeLayout.KnowledgeFolder, tag));

            if (!knowledge.Exists)
            {
                yield break;
            }

            foreach (var file in knowledge.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(this.committed.Root.FullName, file.FullName);
                var segments = relative.Split(Path.DirectorySeparatorChar);

                // knowledge/<tag>/<segment>/... - the third segment names the artifact folder.
                if (segments.Length > 3 && NotGeneratedHere.Contains(segments[2], StringComparer.Ordinal))
                {
                    continue;
                }

                yield return relative;
            }
        }
    }
}
