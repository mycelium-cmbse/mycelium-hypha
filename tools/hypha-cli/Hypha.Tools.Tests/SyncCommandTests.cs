// ------------------------------------------------------------------------------------------------
// <copyright file="SyncCommandTests.cs" company="Starion Group S.A.">
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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;
    using Hypha.Tools.Sync;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Suite of tests for <see cref="SyncCommand"/>: the unattended verb the plugin's hook launches.
    /// </summary>
    /// <remarks>
    /// Every collaborator is faked at its own seam - nothing here touches the network or a real
    /// generator. <see cref="SyncTagPrunerTests"/> covers the on-disk prune/manifest behaviour that
    /// only fires once a previous run has already added a release.
    /// </remarks>
    [TestFixture]
    public class SyncCommandTests
    {
        private Mock<IReleaseDiscovery> discovery = null!;
        private Mock<IReleaseInstaller> installer = null!;
        private Mock<IKnowledgeGenerator> generatorA = null!;
        private Mock<IKnowledgeGenerator> generatorB = null!;
        private Mock<IKnowledgeLayout> layout = null!;
        private InMemoryStatusWriter statusWriter = null!;
        private FakeSyncLock syncLock = null!;
        private List<string> generatorCalls = null!;

        [SetUp]
        public void SetUp()
        {
            this.generatorCalls = [];

            this.discovery = new Mock<IReleaseDiscovery>();
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-05"]);

            this.installer = new Mock<IReleaseInstaller>();
            this.installer
                .Setup(mock => mock.InstallAsync(
                    It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(),
                    It.IsAny<IProgress<FetchProgress>>()))
                .ReturnsAsync((ReleaseInstallRequest request, CancellationToken _, IProgress<FetchProgress>? _) =>
                    Installation(request));

            this.generatorA = Generator("metamodel", 10, this.generatorCalls);
            this.generatorB = Generator("cross-references", 40, this.generatorCalls);

            this.layout = new Mock<IKnowledgeLayout>();
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-05", "2026-04"]);

            this.statusWriter = new InMemoryStatusWriter();
            this.syncLock = new FakeSyncLock();
        }

        [Test]
        public async Task Already_installed_release_short_circuits_without_fetching_anything()
        {
            var result = await this.Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.statusWriter.Last!.Phase, Is.EqualTo(SyncPhase.UpToDate));
                Assert.That(this.generatorCalls, Is.Empty);
            });

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(),
                    It.IsAny<IProgress<FetchProgress>>()),
                Times.Never);
        }

        [Test]
        public async Task A_newer_release_is_fetched_with_the_request_a_plugin_install_needs()
        {
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06", "2026-05"]);

            var result = await this.Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.statusWriter.Last!.Phase, Is.EqualTo(SyncPhase.Done));
                Assert.That(this.statusWriter.Last!.LocallyAddedTag, Is.EqualTo("2026-06"));
            });

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ReleaseInstallRequest>(request =>
                        request.Tag == "2026-06"
                        && !request.IncludeSpecifications
                        && request.SkipExisting
                        && request.MakeDefault),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IProgress<FetchProgress>>()),
                Times.Once);
        }

        [Test]
        public async Task Every_registered_generator_runs_once_for_the_new_release()
        {
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06"]);

            await this.Invoke();

            Assert.That(this.generatorCalls, Is.EqualTo(new[] { "metamodel:2026-06", "cross-references:2026-06" }));
        }

        [Test]
        public async Task The_committed_baseline_is_captured_once_and_never_recomputed()
        {
            // First run: nothing installed yet beyond the two committed releases.
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06"]);

            await this.Invoke();

            Assert.That(this.statusWriter.Last!.CommittedBaselineTags, Is.EqualTo(new[] { "2026-05", "2026-04" }));

            // Second run: as if 2026-06 really did get installed - InstalledTags has grown. If the
            // baseline were recomputed here, 2026-06 would wrongly join the permanent baseline the
            // moment it is later pruned.
            this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-06", "2026-05", "2026-04"]);
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06"]);

            await this.Invoke();

            Assert.That(
                this.statusWriter.Last!.CommittedBaselineTags, Is.EqualTo(new[] { "2026-05", "2026-04" }),
                "the baseline must not grow just because InstalledTags did");
        }

        [Test]
        public async Task Nothing_is_pruned_on_the_very_first_run()
        {
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)["2026-06"]);

            // The layout is deliberately left unconfigured beyond InstalledTags: if pruning were
            // attempted with nothing to prune, touching VersionManifest/ReleaseSources/Knowledge on an
            // unconfigured mock would throw, and this test would fail rather than pass by accident.
            var result = await this.Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.statusWriter.Last!.LocallyAddedTag, Is.EqualTo("2026-06"));
            });
        }

        [Test]
        public async Task Another_run_already_holding_the_lock_exits_without_writing_status()
        {
            this.syncLock.Held = true;

            var result = await this.Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(3));
                Assert.That(this.statusWriter.WriteCount, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task A_failure_is_reported_in_the_status_file_and_the_lock_is_still_released()
        {
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("disk full"));

            var result = await this.Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(1));
                Assert.That(this.statusWriter.Last!.Phase, Is.EqualTo(SyncPhase.Failed));
                Assert.That(this.statusWriter.Last!.Error, Is.Not.Null);
                Assert.That(this.statusWriter.Last!.Error!.Kind, Is.EqualTo("disk"));
                Assert.That(this.syncLock.ReleaseCount, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task The_lock_is_released_on_a_successful_run_too()
        {
            await this.Invoke();

            Assert.That(this.syncLock.ReleaseCount, Is.EqualTo(1));
        }

        [TestCase(typeof(HttpRequestException), "network")]
        [TestCase(typeof(UnauthorizedAccessException), "disk")]
        [TestCase(typeof(InvalidOperationException), "unexpected")]
        public async Task Failures_are_classified_by_kind(Type exceptionType, string expectedKind)
        {
            this.discovery
                .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType, "boom")!);

            await this.Invoke();

            Assert.That(this.statusWriter.Last!.Error!.Kind, Is.EqualTo(expectedKind));
        }

        [Test]
        public async Task A_second_sync_prunes_the_release_the_first_one_added()
        {
            var workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-sync-second-{Guid.NewGuid():N}"));

            try
            {
                var realLayout = new KnowledgeLayout(workspace);
                Seed(realLayout, "2026-05");
                Seed(realLayout, "2026-06");
                WriteManifest(realLayout, "2026-06", "2026-05", "2026-06");

                this.layout.SetupGet(mock => mock.InstalledTags).Returns(["2026-06", "2026-05"]);
                this.layout.Setup(mock => mock.VersionManifest).Returns(realLayout.VersionManifest);
                this.layout.Setup(mock => mock.ReleaseSources(It.IsAny<string>()))
                    .Returns((string tag) => realLayout.ReleaseSources(tag));
                this.layout.Setup(mock => mock.Knowledge(It.IsAny<string>()))
                    .Returns((string tag) => realLayout.Knowledge(tag));

                // Seed the status file as if a first sync already ran and added 2026-06 on top of the
                // committed 2026-05 baseline.
                this.statusWriter.Write(new SyncStatus
                {
                    Phase = SyncPhase.Done,
                    CommittedBaselineTags = ["2026-05"],
                    LocallyAddedTag = "2026-06",
                });

                this.discovery
                    .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<string>)["2026-07"]);

                // The installer is mocked, so it never touches the real manifest on its own - fetch
                // "2026-07" must also make it the real default, exactly like ReleaseInstaller does,
                // otherwise pruning "2026-06" would find it still recorded as the default and refuse.
                this.installer
                    .Setup(mock => mock.InstallAsync(
                        It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(),
                        It.IsAny<IProgress<FetchProgress>>()))
                    .ReturnsAsync((ReleaseInstallRequest request, CancellationToken _, IProgress<FetchProgress>? _) =>
                    {
                        var existing = VersionManifestFile.ReadIfPresent(realLayout.VersionManifest.FullName);
                        var versions = (existing?.Versions ?? []).ToList();
                        versions.Add(new InstalledVersion(
                            request.Tag,
                            new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                            new UpstreamReference(Upstream.PilotRepository, "def")));
                        VersionManifestFile.Write(
                            VersionManifest.Build(request.Tag, versions), realLayout.VersionManifest.FullName);

                        return Installation(request);
                    });

                var result = await this.Invoke();

                Assert.Multiple(() =>
                {
                    Assert.That(result, Is.EqualTo(0));
                    Assert.That(this.statusWriter.Last!.LocallyAddedTag, Is.EqualTo("2026-07"));

                    // The committed baseline is untouched; the previously-added 2026-06 is gone.
                    Assert.That(realLayout.ReleaseSources("2026-06").Exists, Is.False);
                    Assert.That(realLayout.ReleaseSources("2026-05").Exists, Is.True);
                });
            }
            finally
            {
                workspace.Refresh();
                if (workspace.Exists)
                {
                    workspace.Delete(recursive: true);
                }
            }
        }

        [Test]
        public async Task Fetch_progress_reported_by_the_installer_reaches_the_status_file()
        {
            // Progress<T> posts through a SynchronizationContext, defaulting to the thread pool when
            // none is set (as in this test) - making the callback's timing nondeterministic relative
            // to the test's own assertions. Installing an immediate, run-it-now context makes
            // Report() synchronous for the duration of this test, without changing SyncCommand itself.
            var originalContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

            try
            {
                this.discovery
                    .Setup(mock => mock.AvailableAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<string>)["2026-06"]);

                this.installer
                    .Setup(mock => mock.InstallAsync(
                        It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(),
                        It.IsAny<IProgress<FetchProgress>>()))
                    .Returns<ReleaseInstallRequest, CancellationToken, IProgress<FetchProgress>?>(
                        async (request, _, progress) =>
                        {
                            progress?.Report(new FetchProgress("textual", 1, 2));
                            await Task.Yield();

                            return Installation(request);
                        });

                await this.Invoke();

                Assert.That(
                    this.statusWriter.History,
                    Has.Some.Matches<SyncStatus>(status =>
                        status.Fetch is { Kind: "textual", Done: 1, Total: 2 }));
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        private sealed class ImmediateSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object? state) => d(state);
        }

        private static void Seed(KnowledgeLayout layout, string tag)
        {
            var xmi = layout.Xmi(tag);
            xmi.Create();
            File.WriteAllText(Path.Combine(xmi.FullName, "KerML.uml"), "<xmi/>");
        }

        private static void WriteManifest(KnowledgeLayout layout, string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledVersion(
                    tag,
                    new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                    new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(VersionManifest.Build(defaultTag, versions), layout.VersionManifest.FullName);
        }

        private Task<int> Invoke()
        {
            var handler = new SyncCommand.Handler(
                this.discovery.Object,
                this.installer.Object,
                [this.generatorA.Object, this.generatorB.Object],
                this.layout.Object,
                this.statusWriter,
                this.syncLock,
                NullLogger<SyncCommand.Handler>.Instance);

            var root = new System.CommandLine.RootCommand();
            root.Add(new SyncCommand());

            var statusFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"sync-status-{Guid.NewGuid():N}.json"));

            return handler.InvokeAsync(
                root.Parse($"sync --status-file \"{statusFile.FullName}\""), CancellationToken.None);
        }

        private static ReleaseInstallation Installation(ReleaseInstallRequest request) =>
            new(
                request.Tag,
                [new FileInfo("KerML.uml"), new FileInfo("SysML.uml")],
                [new FileInfo("model.sysml")],
                [],
                new InstalledVersion(
                    request.Tag,
                    new UpstreamReference("Systems-Modeling/SysML-v2-Release", "abc123"),
                    new UpstreamReference("Systems-Modeling/SysML-v2-Pilot-Implementation", "def456")),
                new FileInfo("versions.json"));

        private static Mock<IKnowledgeGenerator> Generator(string artifact, int order, List<string> log)
        {
            var mock = new Mock<IKnowledgeGenerator>();

            mock.SetupGet(generator => generator.Artifact).Returns(artifact);
            mock.SetupGet(generator => generator.Order).Returns(order);
            mock
                .Setup(generator => generator.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string tag, CancellationToken _) =>
                {
                    log.Add($"{artifact}:{tag}");

                    return GenerationResult.Generated(
                        [new FileInfo(Path.Combine(Path.GetTempPath(), $"{artifact}-{tag}.md"))]);
                });

            return mock;
        }

        /// <summary>An <see cref="ISyncStatusWriter"/> that keeps everything in memory.</summary>
        private sealed class InMemoryStatusWriter : ISyncStatusWriter
        {
            public SyncStatus? Last { get; private set; }

            public int WriteCount { get; private set; }

            public List<SyncStatus> History { get; } = [];

            public SyncStatus? Load() => this.Last;

            public void Write(SyncStatus status)
            {
                this.Last = status;
                this.WriteCount++;
                this.History.Add(status);
            }
        }

        /// <summary>An <see cref="ISyncLock"/> whose held/release behaviour a test controls directly.</summary>
        private sealed class FakeSyncLock : ISyncLock
        {
            public bool Held { get; set; }

            public int ReleaseCount { get; private set; }

            public bool TryAcquire() => !this.Held;

            public void Release() => this.ReleaseCount++;
        }
    }
}
