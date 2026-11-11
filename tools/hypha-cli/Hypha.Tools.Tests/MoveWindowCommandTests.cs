// ------------------------------------------------------------------------------------------------
// <copyright file="MoveWindowCommandTests.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    [TestFixture]
    public class MoveWindowCommandTests
    {
        private RootCommand root = null!;

        private Mock<IReleaseInstaller> installer = null!;

        private Mock<IKnowledgeGenerator> generator = null!;

        private Mock<IKnowledgeLayout> layout = null!;

        private Mock<IReleaseWindowEvictor> evictor = null!;

        private Mock<IProcessRunner> processes = null!;

        private List<string> order = null!;

        private List<string> dotnetArguments = null!;

        private int specExtractExitCode;

        private int reblessExitCode;

        private int verifyExitCode;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new MoveWindowCommand());

            this.order = [];
            this.dotnetArguments = [];
            this.specExtractExitCode = 0;
            this.reblessExitCode = 0;
            this.verifyExitCode = 0;

            this.installer = new Mock<IReleaseInstaller>();
            this.installer
                .Setup(mock => mock.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReleaseInstallRequest request, CancellationToken _, IProgress<KnowledgeProgress>? _) =>
                {
                    this.order.Add($"fetch:{request.Tag}");

                    return Installation(request);
                });

            this.generator = new Mock<IKnowledgeGenerator>();
            this.generator.SetupGet(mock => mock.Artifact).Returns("metamodel");
            this.generator.SetupGet(mock => mock.Order).Returns(10);
            this.generator
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string tag, CancellationToken _) =>
                {
                    this.order.Add($"generate:{tag}");

                    return GenerationResult.Generated([new FileInfo(Path.Combine(Path.GetTempPath(), "m.md"))]);
                });

            // A real checkout: move-window's pre-flight checks (mycelium-hypha.sln, tools/spec-extract,
            // its .venv) are real filesystem checks, so Root has to point at one for a test to get past
            // them without faking a whole checkout on disk.
            var repositoryRoot = KnowledgeLayout.Discover()?.Root
                ?? throw new InvalidOperationException("tests must run inside the repository checkout");

            this.layout = new Mock<IKnowledgeLayout>();
            this.layout.SetupGet(mock => mock.Root).Returns(repositoryRoot);

            this.evictor = new Mock<IReleaseWindowEvictor>();
            this.evictor
                .Setup(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int keep, CancellationToken _) =>
                {
                    this.order.Add($"evict:{keep}");

                    return (IReadOnlyList<string>)[];
                });

            this.processes = new Mock<IProcessRunner>();
            this.processes
                .Setup(mock => mock.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string fileName, string arguments, DirectoryInfo _, IReadOnlyDictionary<string, string>? _,
                    CancellationToken _) =>
                {
                    if (fileName == "dotnet")
                    {
                        this.dotnetArguments.Add(arguments);
                    }

                    if (arguments.Contains("pytest", StringComparison.Ordinal))
                    {
                        this.order.Add("extract-specifications");

                        return this.specExtractExitCode;
                    }

                    if (arguments.Contains("Bless_expected_files", StringComparison.Ordinal))
                    {
                        this.order.Add("rebless-fixtures");

                        return this.reblessExitCode;
                    }

                    if (arguments.Contains("KnowledgeRegenerationTests", StringComparison.Ordinal))
                    {
                        this.order.Add("verify");

                        return this.verifyExitCode;
                    }

                    throw new InvalidOperationException($"unexpected process call: {fileName} {arguments}");
                });
        }

        [Test]
        public async Task Every_step_runs_in_order_and_the_window_is_reported()
        {
            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[]
                    {
                        "fetch:2026-06", "generate:2026-06", "extract-specifications",
                        "rebless-fixtures", "evict:2", "verify",
                    }));
            });
        }

        [Test]
        public async Task The_test_projects_it_runs_exist_in_the_checkout()
        {
            // The paths are hard-coded, and the process runner is mocked everywhere else, so a stale
            // path after a rename would only show when a maintainer runs move-window.
            await this.Invoke("move-window --tag 2026-06");

            var projects = this.dotnetArguments
                .Select(arguments => arguments.Split(' ')[1])
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(projects, Has.Count.EqualTo(2));
                Assert.That(projects, Has.All.EndsWith(".csproj"));

                foreach (var project in projects)
                {
                    Assert.That(
                        File.Exists(Path.Combine(this.layout.Object.Root.FullName, project)), Is.True, project);
                }
            });
        }

        [Test]
        public async Task Keep_defaults_to_two_and_is_passed_through_unchanged()
        {
            await this.Invoke("move-window --tag 2026-06 --keep 3");

            this.evictor.Verify(mock => mock.EvictAsync(3, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase("0")]
        [TestCase("-1")]
        public async Task A_keep_smaller_than_one_is_a_usage_error_and_nothing_runs(string keep)
        {
            var result = await this.Invoke($"move-window --tag 2026-06 --keep {keep}");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(this.order, Is.Empty);
            });
        }

        [Test]
        public async Task A_generator_that_refuses_stops_the_run_before_anything_downstream()
        {
            this.generator
                .Setup(mock => mock.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("the metamodel index is missing"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(this.order, Is.EqualTo(new[] { "fetch:2026-06" }));
                Assert.That(console.Output, Does.Contain("the metamodel index is missing"));
            });

            this.evictor.Verify(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            this.processes.Verify(
                mock => mock.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task A_failed_specification_extraction_stops_the_run_before_rebless_and_evict()
        {
            this.specExtractExitCode = 1;

            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[] { "fetch:2026-06", "generate:2026-06", "extract-specifications" }));
            });

            this.evictor.Verify(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task A_failed_rebless_stops_the_run_before_evict()
        {
            this.reblessExitCode = 1;

            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[]
                    {
                        "fetch:2026-06", "generate:2026-06", "extract-specifications", "rebless-fixtures",
                    }));
            });

            this.evictor.Verify(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task An_evictor_that_refuses_stops_the_run_before_verify()
        {
            this.evictor
                .Setup(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("'2026-06' is both the default release and outside the newest 2"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(
                    this.order,
                    Is.EqualTo(new[]
                    {
                        "fetch:2026-06", "generate:2026-06", "extract-specifications", "rebless-fixtures",
                    }));
                Assert.That(console.Output, Does.Contain("outside the newest 2"));
            });

            this.processes.Verify(
                mock => mock.RunAsync(
                    It.IsAny<string>(),
                    It.Is<string>(arguments => arguments.Contains("KnowledgeRegenerationTests", StringComparison.Ordinal)),
                    It.IsAny<DirectoryInfo>(), It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task A_failed_final_verification_still_reports_failure_and_names_what_was_evicted()
        {
            this.evictor
                .Setup(mock => mock.EvictAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-04"]);
            this.verifyExitCode = 1;

            using var console = new RecordedConsole();

            var result = await this.Invoke("move-window --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1), "eviction already happening does not make the run a success");
                Assert.That(console.Output, Does.Contain("2026-04"));
            });
        }

        [Test]
        public void A_cancelled_run_does_not_report_success()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            var handler = new MoveWindowCommand.Handler(
                this.installer.Object, new KnowledgeBuilder<IKnowledgeGenerator>([this.generator.Object]), this.layout.Object, this.evictor.Object, this.processes.Object,
                NullLogger<MoveWindowCommand.Handler>.Instance);

            this.installer
                .Setup(mock => mock.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            Assert.That(
                () => handler.InvokeAsync(this.root.Parse("move-window --tag 2026-06"), source.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        private Task<int> Invoke(string commandLine)
        {
            var handler = new MoveWindowCommand.Handler(
                this.installer.Object, new KnowledgeBuilder<IKnowledgeGenerator>([this.generator.Object]), this.layout.Object, this.evictor.Object, this.processes.Object,
                NullLogger<MoveWindowCommand.Handler>.Instance);

            return handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }

        private static ReleaseInstallation Installation(ReleaseInstallRequest request)
        {
            var release = new UpstreamReference("Systems-Modeling/SysML-v2-Release", "abc123");
            var pilot = new UpstreamReference("Systems-Modeling/SysML-v2-Pilot-Implementation", "def456");

            return new ReleaseInstallation(
                request.Tag, [new FileInfo("KerML.uml"), new FileInfo("SysML.uml")], [new FileInfo("model.sysml")],
                [new FileInfo("KerML.pdf")], new InstalledVersion(request.Tag, release, pilot), new FileInfo("versions.json"));
        }
    }
}
