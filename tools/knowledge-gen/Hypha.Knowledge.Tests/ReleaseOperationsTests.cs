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
        public async Task Install_fetches_then_generates_that_release_holding_the_lock_throughout()
        {
            var request = new ReleaseInstallRequest { Tag = Tag, IncludeSpecifications = true };
            var progress = new Mock<IProgress<KnowledgeProgress>>().Object;
            var installation = this.Installation();
            var build = new KnowledgeBuild([], null);
            var held = new List<string>();

            this.installer
                .Setup(i => i.InstallAsync(request, It.IsAny<CancellationToken>(), progress))
                .Callback(() => held.Add($"fetch:{this.LockHeld}"))
                .ReturnsAsync(installation);
            this.builder
                .Setup(b => b.BuildAsync(It.Is<IReadOnlyList<string>>(tags => tags.Count == 1 && tags[0] == Tag), null, progress, It.IsAny<CancellationToken>()))
                .Callback(() => held.Add($"generate:{this.LockHeld}"))
                .ReturnsAsync(build);

            var setup = await this.operations.InstallAsync(request, progress);

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.EqualTo(new[] { "fetch:True", "generate:True" }));
                Assert.That(setup, Is.EqualTo(new ReleaseSetup(installation, build)));
                Assert.That(this.LockHeld, Is.False, "the lock is released once the install is done");
            });
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
