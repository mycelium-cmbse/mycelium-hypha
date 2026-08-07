// ------------------------------------------------------------------------------------------------
// <copyright file="GenerateCommandTests.cs" company="Starion Group S.A.">
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
    using System.CommandLine;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Tools.Commands;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="GenerateCommand"/>.
    /// </summary>
    [TestFixture]
    public class GenerateCommandTests
    {
        private RootCommand root = null!;
        private Mock<IKnowledgeLayout> layout = null!;
        private Mock<IKnowledgeGenerator> metamodel = null!;
        private Mock<IKnowledgeGenerator> crossReferences = null!;
        private List<string> order = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new GenerateCommand());

            this.layout = new Mock<IKnowledgeLayout>();
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-05", "2026-04"]);
            this.layout.SetupGet(mock => mock.Root).Returns(new DirectoryInfo(Path.GetTempPath()));

            this.order = [];

            this.metamodel = Generator("metamodel", 10, this.order);
            this.crossReferences = Generator("cross-references", 40, this.order);
        }

        [Test]
        public async Task Every_artifact_runs_for_every_installed_release()
        {
            var result = await this.Invoke("generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[]
                    {
                        "metamodel:2026-05", "cross-references:2026-05",
                        "metamodel:2026-04", "cross-references:2026-04",
                    }));
            });
        }

        [Test]
        public async Task The_generators_run_in_order_however_they_were_registered()
        {
            // Registration order is a DI detail; the cross-references read what the metamodel writes.
            var handler = new GenerateCommand.Handler(
                [this.crossReferences.Object, this.metamodel.Object], this.layout.Object);

            await handler.InvokeAsync(this.root.Parse("generate --tag 2026-05"), CancellationToken.None);

            Assert.That(this.order, Is.EqualTo(new[] { "metamodel:2026-05", "cross-references:2026-05" }));
        }

        [Test]
        public async Task Naming_an_artifact_runs_only_that_one()
        {
            var result = await this.Invoke("generate metamodel --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.order, Is.EqualTo(new[] { "metamodel:2026-05" }));
            });
        }

        [Test]
        public async Task An_unknown_artifact_is_a_usage_error_that_names_the_known_ones()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("generate metamodl --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2), "an unknown artifact is a usage error, not a failure");
                Assert.That(this.order, Is.Empty, "nothing should run when the selection is rejected");
                Assert.That(console.Output, Does.Contain("metamodel").And.Contains("cross-references"));
            });
        }

        [Test]
        public async Task An_explicit_tag_overrides_the_installed_releases()
        {
            var result = await this.Invoke("generate --tag 2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.order, Has.All.EndsWith(":2025-09"));
            });
        }

        [Test]
        public async Task Nothing_installed_is_reported_rather_than_silently_succeeding()
        {
            this.layout.SetupGet(mock => mock.InstalledTags).Returns([]);

            using var console = new RecordedConsole();

            var result = await this.Invoke("generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("hypha fetch"));
            });
        }

        [Test]
        public async Task A_run_in_which_everything_was_skipped_fails()
        {
            // Every artifact skipped means no knowledge base was produced. Reporting success would
            // let an install that fetched nothing look like one that worked.
            this.metamodel
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GenerationResult.Skipped("nothing fetched"));
            this.crossReferences
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GenerationResult.Skipped("nothing fetched"));

            var result = await this.Invoke("generate --tag 2026-05");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task A_generator_that_refuses_stops_the_run()
        {
            // The deliberate refusals (#96/#97) mean the output would be wrong. Carrying on to the
            // next release would leave a knowledge base that is partly right, which is worse.
            this.metamodel
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("the metamodel index is missing"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain("the metamodel index is missing"));
                Assert.That(this.order, Has.Count.EqualTo(0).Or.Count.EqualTo(1));
            });

            this.crossReferences.Verify(
                mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "the run should stop rather than press on to the artifacts that read the failed one");
        }

        [Test]
        public void A_cancelled_run_does_not_report_success()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            var handler = new GenerateCommand.Handler(
                [this.metamodel.Object, this.crossReferences.Object], this.layout.Object);

            Assert.That(
                () => handler.InvokeAsync(this.root.Parse("generate"), source.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        private Task<int> Invoke(string commandLine)
        {
            var handler = new GenerateCommand.Handler(
                [this.metamodel.Object, this.crossReferences.Object], this.layout.Object);

            return handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }

        /// <summary>A generator that records the order it was called in and reports one written file.</summary>
        private static Mock<IKnowledgeGenerator> Generator(string artifact, int order, List<string> log)
        {
            var mock = new Mock<IKnowledgeGenerator>();

            mock.SetupGet(generator => generator.Artifact).Returns(artifact);
            mock.SetupGet(generator => generator.Order).Returns(order);
            mock
                .Setup(generator => generator.GenerateAsync(
                    It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string tag, CancellationToken _) =>
                {
                    log.Add($"{artifact}:{tag}");

                    return GenerationResult.Generated(
                        [new FileInfo(Path.Combine(Path.GetTempPath(), $"{artifact}-{tag}.md"))]);
                });

            return mock;
        }
    }
}
