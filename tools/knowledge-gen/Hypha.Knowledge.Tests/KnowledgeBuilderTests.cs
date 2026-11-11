// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeBuilderTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Progress;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    [TestFixture]
    public class KnowledgeBuilderTests
    {
        private readonly List<string> ran = [];

        private readonly List<KnowledgeProgress> reports = [];

        [SetUp]
        public void SetUp()
        {
            this.ran.Clear();
            this.reports.Clear();
        }

        [Test]
        public async Task Every_artifact_runs_in_its_order_for_each_tag_one_tag_after_the_other()
        {
            var builder = new KnowledgeBuilder<IKnowledgeGenerator>(
                [this.Generator("cross-references", 40).Object, this.Generator("metamodel", 10).Object, this.Generator("keywords", 33, skipped: true).Object]);

            var build = await builder.BuildAsync(["2026-05", "2026-02"]);

            Assert.Multiple(() =>
            {
                Assert.That(builder.Artifacts, Is.EqualTo(new[] { "metamodel", "keywords", "cross-references" }));
                Assert.That(
                    this.ran,
                    Is.EqualTo(new[]
                    {
                        "metamodel 2026-05", "keywords 2026-05", "cross-references 2026-05",
                        "metamodel 2026-02", "keywords 2026-02", "cross-references 2026-02",
                    }));
                Assert.That(build.Artifacts, Has.Count.EqualTo(6));
                Assert.That(build.Refusal, Is.Null);
                Assert.That(build.Written, Is.EqualTo(4));
                Assert.That(build.Skipped, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task Each_artifact_is_reported_as_it_finishes_counting_within_its_tag()
        {
            var builder = new KnowledgeBuilder<IKnowledgeGenerator>(
                [this.Generator("metamodel", 10).Object, this.Generator("keywords", 33, skipped: true).Object]);

            await builder.BuildAsync(["2026-05", "2026-02"], progress: new Collector(this.reports));

            Assert.That(
                this.reports,
                Is.EqualTo(new[]
                {
                    new KnowledgeProgress(KnowledgeProgress.Generate, "2026-05", 1, 2, "metamodel: 1 files"),
                    new KnowledgeProgress(KnowledgeProgress.Generate, "2026-05", 2, 2, "keywords: skipped, no keywords input"),
                    new KnowledgeProgress(KnowledgeProgress.Generate, "2026-02", 1, 2, "metamodel: 1 files"),
                    new KnowledgeProgress(KnowledgeProgress.Generate, "2026-02", 2, 2, "keywords: skipped, no keywords input"),
                }));
        }

        [Test]
        public async Task One_artifact_can_be_asked_for_and_an_unknown_one_is_refused_naming_the_known_ones()
        {
            var builder = new KnowledgeBuilder<IKnowledgeGenerator>(
                [this.Generator("metamodel", 10).Object, this.Generator("keywords", 33).Object]);

            var build = await builder.BuildAsync(["2026-05"], "keywords", new Collector(this.reports));

            Assert.Multiple(() =>
            {
                Assert.That(this.ran, Is.EqualTo(new[] { "keywords 2026-05" }));
                Assert.That(build.Artifacts.Select(artifact => artifact.Artifact), Is.EqualTo(new[] { "keywords" }));
                Assert.That(this.reports.Select(report => $"{report.Done}/{report.Total}"), Is.EqualTo(new[] { "1/1" }));
                Assert.That(
                    () => builder.BuildAsync(["2026-05"], "nonsense"),
                    Throws.ArgumentException.With.Message.Contains("'nonsense'").And.Message.Contains("metamodel, keywords"));
            });
        }

        [Test]
        public async Task A_refusal_stops_the_run_and_keeps_what_was_done_before_it()
        {
            var refusing = new Mock<IKnowledgeGenerator>();
            refusing.SetupGet(generator => generator.Artifact).Returns("cross-references");
            refusing.SetupGet(generator => generator.Order).Returns(40);
            refusing.Setup(generator => generator.GenerateAsync("2026-05", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("a clause title reached the cross-references"));

            var builder = new KnowledgeBuilder<IKnowledgeGenerator>(
                [this.Generator("metamodel", 10).Object, refusing.Object, this.Generator("last", 50).Object]);

            var build = await builder.BuildAsync(["2026-05", "2026-02"], progress: new Collector(this.reports));

            Assert.Multiple(() =>
            {
                Assert.That(build.Refusal, Is.EqualTo(new ArtifactRefusal("2026-05", "cross-references", "a clause title reached the cross-references")));
                Assert.That(build.Artifacts.Select(artifact => artifact.Artifact), Is.EqualTo(new[] { "metamodel" }));
                Assert.That(this.ran, Is.EqualTo(new[] { "metamodel 2026-05" }));
                Assert.That(this.reports, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void A_cancelled_run_stops_before_the_next_artifact()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            var builder = new KnowledgeBuilder<IKnowledgeGenerator>([this.Generator("metamodel", 10).Object]);

            Assert.Multiple(() =>
            {
                Assert.That(() => builder.BuildAsync(["2026-05"], cancellationToken: source.Token), Throws.InstanceOf<OperationCanceledException>());
                Assert.That(this.ran, Is.Empty);
            });
        }

        [Test]
        public void The_builder_needs_its_generators_and_its_tags()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => new KnowledgeBuilder<IKnowledgeGenerator>(null!), Throws.ArgumentNullException);
                Assert.That(() => new KnowledgeBuilder<IKnowledgeGenerator>([]).BuildAsync(null!), Throws.ArgumentNullException);
            });
        }

        [Test]
        public void The_release_and_the_api_builders_run_their_own_collections()
        {
            var services = new ServiceCollection();
            services.AddSingleton(this.Generator("metamodel", 10).Object);
            services.AddSingleton<IApiKnowledgeGenerator>(this.Generator<IApiKnowledgeGenerator>("openapi", 20).Object);
            services.AddSingleton(typeof(IKnowledgeBuilder<>), typeof(KnowledgeBuilder<>));
            using var provider = services.BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetRequiredService<IKnowledgeBuilder<IKnowledgeGenerator>>().Artifacts, Is.EqualTo(new[] { "metamodel" }));
                Assert.That(provider.GetRequiredService<IKnowledgeBuilder<IApiKnowledgeGenerator>>().Artifacts, Is.EqualTo(new[] { "openapi" }));

                // A builder of API generators is a builder of generators, which is what a caller that only builds takes.
                Assert.That(provider.GetRequiredService<IKnowledgeBuilder<IApiKnowledgeGenerator>>(), Is.InstanceOf<IKnowledgeBuilder<IKnowledgeGenerator>>());
            });
        }

        private Mock<IKnowledgeGenerator> Generator(string artifact, int order, bool skipped = false) =>
            this.Generator<IKnowledgeGenerator>(artifact, order, skipped);

        private Mock<TGenerator> Generator<TGenerator>(string artifact, int order, bool skipped = false)
            where TGenerator : class, IKnowledgeGenerator
        {
            var generator = new Mock<TGenerator>();
            generator.SetupGet(mock => mock.Artifact).Returns(artifact);
            generator.SetupGet(mock => mock.Order).Returns(order);
            generator.Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string tag, CancellationToken _) =>
                {
                    this.ran.Add($"{artifact} {tag}");

                    return skipped
                        ? GenerationResult.Skipped($"no {artifact} input")
                        : GenerationResult.Generated([new FileInfo($"{artifact}-{tag}.json")]);
                });

            return generator;
        }

        // Collects the reports as they are made, unlike Progress<T>, which posts them to the thread pool.
        private sealed class Collector(List<KnowledgeProgress> reports) : IProgress<KnowledgeProgress>
        {
            public void Report(KnowledgeProgress value) => reports.Add(value);
        }
    }
}
