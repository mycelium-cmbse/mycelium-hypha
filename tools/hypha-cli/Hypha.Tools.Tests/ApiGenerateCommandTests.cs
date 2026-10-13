// ------------------------------------------------------------------------------------------------
// <copyright file="ApiGenerateCommandTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Commands.Api;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="ApiGenerateCommand"/>: the same loop <c>hypha generate</c>
    /// runs, over the API generators and the installed API versions.
    /// </summary>
    /// <remarks>
    /// The loop itself is <see cref="GenerateCommand.Handler"/>'s and is covered by
    /// <see cref="GenerateCommandTests"/>. What is pinned down here is what the
    /// <see cref="GenerateCommand.GenerationScope"/> changes: which versions it runs for, which
    /// argument and option it reads, and the words it reports in.
    /// </remarks>
    [TestFixture]
    public class ApiGenerateCommandTests
    {
        private RootCommand root = null!;
        private Mock<IApiLayout> layout = null!;
        private Mock<IApiKnowledgeGenerator> pim = null!;
        private Mock<IApiKnowledgeGenerator> openApi = null!;
        private List<string> order = null!;
        private DirectoryInfo location = null!;

        [SetUp]
        public void SetUp()
        {
            var api = new ApiCommand();
            api.Add(new ApiGenerateCommand());

            // The language verb sits beside it, as it does in the real command line: the two declare
            // an argument and a --tag option of the same names, and must not read each other's.
            this.root = new RootCommand();
            this.root.Add(new GenerateCommand());
            this.root.Add(api);

            this.location = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "hypha-api-generate", "knowledge", "api"));

            this.layout = new Mock<IApiLayout>();
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-04", "2025-09"]);
            this.layout.SetupGet(mock => mock.VersionManifest)
                .Returns(new FileInfo(Path.Combine(this.location.FullName, "versions.json")));

            this.order = [];
            this.pim = Generator<IApiKnowledgeGenerator>("pim", 10, this.order);
            this.openApi = Generator<IApiKnowledgeGenerator>("openapi", 20, this.order);
        }

        [Test]
        public async Task Every_artifact_runs_for_every_installed_api_version()
        {
            var result = await this.Invoke("api generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[]
                    {
                        "pim:2026-04", "openapi:2026-04",
                        "pim:2025-09", "openapi:2025-09",
                    }));
            });
        }

        [Test]
        public async Task The_generators_run_in_order_however_they_were_registered()
        {
            var handler = new GenerateCommand.Handler(
                [this.openApi.Object, this.pim.Object], ApiGenerateCommand.Scope(this.layout.Object));

            await handler.InvokeAsync(this.root.Parse("api generate --tag 2026-04"), CancellationToken.None);

            Assert.That(this.order, Is.EqualTo(new[] { "pim:2026-04", "openapi:2026-04" }));
        }

        [Test]
        public async Task Naming_an_artifact_runs_only_that_one()
        {
            var result = await this.Invoke("api generate pim --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.order, Is.EqualTo(new[] { "pim:2026-04" }));
            });
        }

        [Test]
        public async Task An_unknown_artifact_is_a_usage_error_that_names_the_api_artifacts()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("api generate metamodel --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2), "a language artifact is not an API artifact");
                Assert.That(this.order, Is.Empty, "nothing should run when the selection is rejected");
                Assert.That(console.Output, Does.Contain("No generator produces 'metamodel'"));
                Assert.That(console.Output, Does.Contain("Known artifacts: pim, openapi."));
            });
        }

        [Test]
        public async Task An_explicit_tag_overrides_the_installed_api_versions()
        {
            var result = await this.Invoke("api generate --tag 2025-07 --tag 2025-02");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[] { "pim:2025-07", "openapi:2025-07", "pim:2025-02", "openapi:2025-02" }));
            });
        }

        [Test]
        public async Task The_installed_versions_are_read_when_the_run_starts_not_when_it_is_composed()
        {
            // The scope holds a function rather than a list: a version installed between composing
            // the handler and invoking it is still generated for.
            var scope = ApiGenerateCommand.Scope(this.layout.Object);
            var handler = new GenerateCommand.Handler([this.pim.Object], scope);

            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2025-07"]);

            await handler.InvokeAsync(this.root.Parse("api generate"), CancellationToken.None);

            Assert.That(this.order, Is.EqualTo(new[] { "pim:2025-07" }));
        }

        [Test]
        public async Task Nothing_installed_is_reported_in_the_words_of_the_api()
        {
            this.layout.SetupGet(mock => mock.InstalledTags).Returns([]);

            using var console = new RecordedConsole();

            var result = await this.Invoke("api generate");

            var output = Unwrapped(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(this.order, Is.Empty);
                Assert.That(output, Does.Contain("No API version is installed under"));
                Assert.That(output, Does.Contain("Run 'hypha api fetch --tag <version>' first."));
                Assert.That(output, Does.Not.Contain("hypha fetch --tag <release>"));
            });
        }

        [Test]
        public async Task The_outcome_counts_api_versions_not_releases()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("api generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(Unwrapped(console.Output), Does.Contain("Generated 4 files across 2 API version(s)."));
            });
        }

        [Test]
        public async Task A_run_in_which_everything_was_skipped_fails()
        {
            this.pim
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GenerationResult.Skipped("nothing fetched"));
            this.openApi
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GenerationResult.Skipped("nothing fetched"));

            var result = await this.Invoke("api generate --tag 2026-04");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task A_skipped_artifact_is_counted_beside_the_ones_that_were_written()
        {
            this.openApi
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GenerationResult.Skipped("no OpenAPI document fetched"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api generate --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(
                    Unwrapped(console.Output),
                    Does.Contain("Generated 1 files across 1 API version(s), 1 artifacts skipped."));
            });
        }

        [Test]
        public async Task A_generator_that_refuses_stops_the_run()
        {
            this.pim
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("the PIM model declares two classes named Commit"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api generate");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(Unwrapped(console.Output), Does.Contain("pim refused for 2026-04"));
                Assert.That(
                    Unwrapped(console.Output), Does.Contain("the PIM model declares two classes named Commit"));
            });

            this.openApi.Verify(
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
                [this.pim.Object, this.openApi.Object], ApiGenerateCommand.Scope(this.layout.Object));

            Assert.That(
                () => handler.InvokeAsync(this.root.Parse("api generate"), source.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public void The_scope_describes_the_api_axis()
        {
            var scope = ApiGenerateCommand.Scope(this.layout.Object);

            Assert.Multiple(() =>
            {
                Assert.That(scope.InstalledTags(), Is.EqualTo(new[] { "2026-04", "2025-09" }));
                Assert.That(
                    scope.Location, Is.EqualTo(this.location.FullName),
                    "the folder the API manifest lives in, which is where an install would show up");
                Assert.That(scope.Noun, Is.EqualTo("API version"));
                Assert.That(scope.FetchHint, Is.EqualTo("hypha api fetch --tag <version>"));
                Assert.That(scope.Artifact, Is.SameAs(ApiGenerateCommand.Artifact));
                Assert.That(scope.Tag, Is.SameAs(ApiGenerateCommand.Tag));
            });
        }

        [Test]
        public void The_api_verb_declares_its_own_argument_and_options()
        {
            // Sharing the language verb's symbols would let one parse result answer for the other.
            Assert.Multiple(() =>
            {
                Assert.That(ApiGenerateCommand.Artifact, Is.Not.SameAs(GenerateCommand.Artifact));
                Assert.That(ApiGenerateCommand.Tag, Is.Not.SameAs(GenerateCommand.Tag));
                Assert.That(ApiGenerateCommand.Output, Is.Not.SameAs(GenerateCommand.Output));
            });
        }

        [Test]
        public async Task The_language_verb_is_unchanged_by_the_scope_it_now_runs_through()
        {
            // `hypha generate` builds its own scope from the knowledge layout. Its words, its hint
            // and the symbols it reads are what they were before the API axis existed.
            var knowledge = new Mock<IKnowledgeLayout>();
            knowledge.SetupGet(mock => mock.InstalledTags).Returns([]);
            knowledge.SetupGet(mock => mock.Root).Returns(new DirectoryInfo(Path.GetTempPath()));

            var metamodel = Generator<IKnowledgeGenerator>("metamodel", 10, this.order);
            var handler = new GenerateCommand.Handler([metamodel.Object], knowledge.Object);

            using var console = new RecordedConsole();

            var nothingInstalled = await handler.InvokeAsync(this.root.Parse("generate"), CancellationToken.None);
            var generated = await handler.InvokeAsync(
                this.root.Parse("generate metamodel --tag 2026-05"), CancellationToken.None);

            var output = Unwrapped(console.Output);

            Assert.Multiple(() =>
            {
                Assert.That(nothingInstalled, Is.EqualTo(1));
                Assert.That(output, Does.Contain("No release is installed under"));
                Assert.That(output, Does.Contain("Run 'hypha fetch --tag <release>' first."));
                Assert.That(generated, Is.EqualTo(0));
                Assert.That(output, Does.Contain("Generated 1 files across 1 release(s)."));
                Assert.That(output, Does.Not.Contain("API version"));
                Assert.That(this.order, Is.EqualTo(new[] { "metamodel:2026-05" }));
            });
        }

        [Test]
        public async Task The_language_verb_does_not_read_the_api_verbs_tags()
        {
            // Parsed as `api generate`, the language handler finds neither its artifact nor its
            // --tag, so it falls back to every installed release - never to the API versions typed.
            var knowledge = new Mock<IKnowledgeLayout>();
            knowledge.SetupGet(mock => mock.InstalledTags).Returns(["2026-05"]);
            knowledge.SetupGet(mock => mock.Root).Returns(new DirectoryInfo(Path.GetTempPath()));

            var metamodel = Generator<IKnowledgeGenerator>("metamodel", 10, this.order);
            var handler = new GenerateCommand.Handler([metamodel.Object], knowledge.Object);

            await handler.InvokeAsync(this.root.Parse("api generate pim --tag 2026-04"), CancellationToken.None);

            Assert.That(this.order, Is.EqualTo(new[] { "metamodel:2026-05" }));
        }

        [Test]
        public void A_missing_scope_or_generator_collection_is_refused()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(
                    () => _ = new GenerateCommand.Handler([this.pim.Object], (GenerateCommand.GenerationScope)null!));
                Assert.Throws<ArgumentNullException>(
                    () => _ = new GenerateCommand.Handler(null!, ApiGenerateCommand.Scope(this.layout.Object)));
            });
        }

        private Task<int> Invoke(string commandLine)
        {
            var handler = new GenerateCommand.Handler(
                [this.pim.Object, this.openApi.Object], ApiGenerateCommand.Scope(this.layout.Object));

            return handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }

        /// <summary>
        /// The console output as one line: the recorded console wraps at a fixed width, and a message
        /// that names a path can be wrapped anywhere.
        /// </summary>
        private static string Unwrapped(string output) =>
            string.Join(' ', output.Split(['\r', '\n', ' '], StringSplitOptions.RemoveEmptyEntries));

        /// <summary>A generator that records the order it was called in and reports one written file.</summary>
        private static Mock<T> Generator<T>(string artifact, int order, List<string> log)
            where T : class, IKnowledgeGenerator
        {
            var mock = new Mock<T>();

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
