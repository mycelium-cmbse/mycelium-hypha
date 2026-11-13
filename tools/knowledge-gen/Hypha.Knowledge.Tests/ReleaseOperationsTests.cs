// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperationsTests.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    // The operations run against the real lock, so "held" means the lock file is on disk.
    [TestFixture]
    public class ReleaseOperationsTests
    {
        private const string Tag = "2026-05";

        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private Mock<IReleaseInstaller> installer = null!;
        private Mock<IKnowledgeBuilder<IKnowledgeGenerator>> builder = null!;
        private Mock<IReleaseSelector> selector = null!;
        private Mock<IReleaseRemover> remover = null!;
        private ReleaseOperations operations = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-operations-{Guid.NewGuid():N}"));
            this.layout = new KnowledgeLayout(this.workspace);
            this.installer = new Mock<IReleaseInstaller>(MockBehavior.Strict);
            this.builder = new Mock<IKnowledgeBuilder<IKnowledgeGenerator>>(MockBehavior.Strict);
            this.selector = new Mock<IReleaseSelector>(MockBehavior.Strict);
            this.remover = new Mock<IReleaseRemover>(MockBehavior.Strict);

            this.operations = new ReleaseOperations(
                new ReleaseOperationLock(this.layout, new SystemProcessProbe(), NullLogger<ReleaseOperationLock>.Instance),
                this.installer.Object,
                this.builder.Object,
                this.selector.Object,
                this.remover.Object);
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

        private bool LockHeld => File.Exists(Path.Combine(this.workspace.FullName, ReleaseOperationLock.FileName));

        [Test]
        public async Task Install_fetches_generates_and_only_then_switches_the_default_holding_the_lock_throughout()
        {
            var request = new ReleaseInstallRequest { Tag = Tag, IncludeSpecifications = true };
            var fetched = request with { MakeDefault = false };
            var progress = new Mock<IProgress<KnowledgeProgress>>().Object;
            var installation = this.Installation();
            var build = new KnowledgeBuild([], null);
            var held = new List<string>();
            this.WriteManifest("2026-04", "2026-04", Tag);

            this.installer
                .Setup(i => i.InstallAsync(fetched, It.IsAny<CancellationToken>(), progress))
                .Callback(() => held.Add($"fetch:{this.LockHeld}:{this.Default}"))
                .ReturnsAsync(installation);
            this.builder
                .Setup(b => b.BuildAsync(It.Is<IReadOnlyList<string>>(tags => tags.Count == 1 && tags[0] == Tag), null, progress, It.IsAny<CancellationToken>()))
                .Callback(() => held.Add($"generate:{this.LockHeld}:{this.Default}"))
                .ReturnsAsync(build);
            this.selector
                .Setup(s => s.Use(Tag))
                .Callback(() =>
                {
                    held.Add($"use:{this.LockHeld}");
                    this.WriteManifest(Tag, "2026-04", Tag);
                })
                .Returns(new ReleaseSelection(Tag, "2026-04"));

            var setup = await this.operations.InstallAsync(request, progress);

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.EqualTo(new[] { "fetch:True:2026-04", "generate:True:2026-04", "use:True" }));
                Assert.That(setup, Is.EqualTo(new ReleaseSetup(installation, build, IsDefault: true)));
                Assert.That(this.LockHeld, Is.False, "the lock is released once the install is done");
            });
        }

        [Test]
        public async Task A_refused_generation_leaves_the_previous_default_answering()
        {
            this.WriteManifest("2026-04", "2026-04", Tag);
            this.installer
                .Setup(i => i.InstallAsync(It.Is<ReleaseInstallRequest>(r => !r.MakeDefault), It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KnowledgeBuild([], new ArtifactRefusal(Tag, "cross-references", "a slug collision")));

            var setup = await this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag, MakeDefault = true });

            Assert.Multiple(() =>
            {
                Assert.That(setup.IsDefault, Is.False);
                Assert.That(this.Default, Is.EqualTo("2026-04"));
            });
            this.selector.VerifyNoOtherCalls();
        }

        [Test]
        public void A_generation_that_fails_or_is_cancelled_leaves_the_previous_default_and_releases_the_lock()
        {
            this.WriteManifest("2026-04", "2026-04", Tag);
            this.installer
                .Setup(i => i.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(this.Installation());
            this.builder
                .SetupSequence(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("spec-extract stopped"))
                .ThrowsAsync(new OperationCanceledException());

            Assert.Multiple(() =>
            {
                Assert.That(() => this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag }), Throws.TypeOf<IOException>());
                Assert.That(() => this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag }), Throws.InstanceOf<OperationCanceledException>());
                Assert.That(this.Default, Is.EqualTo("2026-04"));
                Assert.That(this.LockHeld, Is.False);
            });
            this.selector.VerifyNoOtherCalls();
        }

        [Test]
        public async Task An_install_not_asked_to_be_the_default_does_not_switch_it()
        {
            this.WriteManifest("2026-04", "2026-04", Tag);
            this.installer
                .Setup(i => i.InstallAsync(It.Is<ReleaseInstallRequest>(r => !r.MakeDefault), It.IsAny<CancellationToken>(), null))
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KnowledgeBuild([], null));

            var setup = await this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag, MakeDefault = false });

            Assert.That(setup.IsDefault, Is.False);
            this.selector.VerifyNoOtherCalls();
        }

        [Test]
        public async Task The_first_release_installed_is_reported_as_the_default_even_when_not_asked()
        {
            this.installer
                .Setup(i => i.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(), null))
                .Callback(() => this.WriteManifest(Tag, Tag))
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KnowledgeBuild([], null));

            var setup = await this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag, MakeDefault = false });

            Assert.That(setup.IsDefault, Is.True);
        }

        [Test]
        public void A_failed_fetch_releases_the_lock_and_generates_nothing()
        {
            this.installer
                .Setup(i => i.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>(), null))
                .ThrowsAsync(new ArgumentException("not an offerable release: 2026-13"));

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => this.operations.InstallAsync(new ReleaseInstallRequest { Tag = "2026-13" }),
                    Throws.ArgumentException.With.Message.Contains("2026-13"));
                Assert.That(this.LockHeld, Is.False);
            });

            this.builder.VerifyNoOtherCalls();
        }

        [Test]
        public void A_lock_another_process_holds_is_refused_before_anything_changes()
        {
            // The current process is alive, so its lock is not stale; without a wait it is refused at once.
            File.WriteAllText(
                Path.Combine(this.workspace.FullName, ReleaseOperationLock.FileName),
                $"{{\"pid\": {Environment.ProcessId}, \"token\": \"other\"}}");

            Assert.Multiple(() =>
            {
                Assert.That(() => this.operations.InstallAsync(new ReleaseInstallRequest { Tag = Tag }), Throws.TypeOf<ReleaseOperationLockException>());
                Assert.That(() => this.operations.UseAsync(Tag), Throws.TypeOf<ReleaseOperationLockException>());
                Assert.That(() => this.operations.RemoveAsync(Tag), Throws.TypeOf<ReleaseOperationLockException>());
            });

            this.installer.VerifyNoOtherCalls();
            this.selector.VerifyNoOtherCalls();
            this.remover.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Use_switches_the_default_holding_the_lock()
        {
            var held = false;
            this.selector
                .Setup(s => s.Use("2026-04"))
                .Callback(() => held = this.LockHeld)
                .Returns(new ReleaseSelection("2026-04", Tag));

            var selection = await this.operations.UseAsync("2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(selection, Is.EqualTo(new ReleaseSelection("2026-04", Tag)));
                Assert.That(held, Is.True);
                Assert.That(this.LockHeld, Is.False);
            });
        }

        [Test]
        public async Task Remove_removes_the_release_holding_the_lock()
        {
            var held = false;
            this.remover
                .Setup(r => r.RemoveAsync("2026-04", true, It.IsAny<CancellationToken>()))
                .Callback(() => held = this.LockHeld)
                .Returns(Task.CompletedTask);

            await this.operations.RemoveAsync("2026-04", force: true);

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.True);
                Assert.That(this.LockHeld, Is.False);
            });
        }

        [Test]
        public void A_removal_that_is_refused_releases_the_lock()
        {
            this.remover
                .Setup(r => r.RemoveAsync(Tag, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("'2026-05' is the default release"));

            Assert.Multiple(() =>
            {
                Assert.That(() => this.operations.RemoveAsync(Tag), Throws.InvalidOperationException.With.Message.Contains("default"));
                Assert.That(this.LockHeld, Is.False);
            });
        }

        private string? Default => VersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName)?.Default;

        private void WriteManifest(string defaultTag, params string[] tags) =>
            VersionManifestFile.Write(
                VersionManifest.Build(
                    defaultTag,
                    Array.ConvertAll(
                        tags,
                        tag => new InstalledVersion(tag, new UpstreamReference(Upstream.ReleaseRepository, "abc"), new UpstreamReference(Upstream.PilotRepository, "def")))),
                this.layout.VersionManifest.FullName);

        private ReleaseInstallation Installation() =>
            new(
                Tag,
                [],
                [],
                [],
                new InstalledVersion(Tag, new UpstreamReference(Upstream.ReleaseRepository, "abc"), new UpstreamReference(Upstream.PilotRepository, "def")),
                this.layout.VersionManifest);
    }
}
