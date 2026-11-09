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
    using Hypha.Knowledge.Releases;
    using Hypha.Knowledge.Metamodel;

    using Microsoft.Extensions.DependencyInjection;

    [TestFixture]
    [Explicit("Hits the network; run by hand when changing a generator.")]
    [Category("Live")]
    public class KnowledgeRegenerationTests
    {
        private const string Tag = "2026-05";

        private DirectoryInfo workspace = null!;
        private DirectoryInfo outputA = null!;
        private DirectoryInfo outputB = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-regen-{Guid.NewGuid():N}"));

            this.outputA = this.workspace.CreateSubdirectory("output-a");
            this.outputB = this.workspace.CreateSubdirectory("output-b");
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

        [Test]
        public async Task Two_generation_runs_from_the_same_fetch_are_byte_identical()
        {
            using (var fetchProvider = this.BuildProvider(outputRoot: null))
            {
                await fetchProvider.GetRequiredService<IReleaseInstaller>().InstallAsync(
                    new ReleaseInstallRequest { Tag = Tag, IncludeSpecifications = false });
            }

            var writtenA = await this.GenerateAllAsync(this.outputA);
            var writtenB = await this.GenerateAllAsync(this.outputB);

            Assert.That(writtenA, Is.Not.Empty, "the fetch above should have given every generator something to work with");

            var relativeA = writtenA
                .Select(file => Path.GetRelativePath(this.outputA.FullName, file.FullName))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            var relativeB = writtenB
                .Select(file => Path.GetRelativePath(this.outputB.FullName, file.FullName))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();

            Assert.That(relativeB, Is.EqualTo(relativeA), "the two runs wrote a different set of files");

            var differing = new List<string>();

            foreach (var relative in relativeA)
            {
                var bytesA = File.ReadAllBytes(Path.Combine(this.outputA.FullName, relative));
                var bytesB = File.ReadAllBytes(Path.Combine(this.outputB.FullName, relative));

                if (!bytesA.AsSpan().SequenceEqual(bytesB))
                {
                    differing.Add(relative);
                }
            }

            Assert.That(differing, Is.Empty, "regeneration is not deterministic for these files");

            TestContext.Out.WriteLine($"{relativeA.Count} files compared");
        }

        private async Task<IReadOnlyList<FileInfo>> GenerateAllAsync(DirectoryInfo outputRoot)
        {
            using var provider = this.BuildProvider(outputRoot);

            var generators = provider.GetServices<IKnowledgeGenerator>()
                .OrderBy(generator => generator.Order);

            var written = new List<FileInfo>();

            foreach (var generator in generators)
            {
                var result = await generator.GenerateAsync(Tag);
                written.AddRange(result.Written);
            }

            return written;
        }

        private ServiceProvider BuildProvider(DirectoryInfo? outputRoot) =>
            new ServiceCollection()
                .AddHyphaKnowledge(options =>
                {
                    options.RepositoryRoot = this.workspace;
                    options.OutputRoot = outputRoot;
                })
                .AddHyphaMetamodel()
                .BuildServiceProvider();
    }
}
