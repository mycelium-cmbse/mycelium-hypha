// ------------------------------------------------------------------------------------------------
// <copyright file="ApiOperationsTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    [TestFixture]
    public class ApiOperationsTests
    {
        private const string Version = "2026-04";

        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private ApiLayout apiLayout = null!;
        private Mock<IApiInstaller> installer = null!;
        private Mock<IKnowledgeBuilder<IApiKnowledgeGenerator>> builder = null!;
        private Mock<IApiRemover> remover = null!;
        private ApiOperations operations = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"hypha-api-operations-{Guid.NewGuid():N}"));
            this.layout = new KnowledgeLayout(this.workspace);
            this.apiLayout = new ApiLayout(this.layout);
            this.installer = new Mock<IApiInstaller>(MockBehavior.Strict);
            this.builder = new Mock<IKnowledgeBuilder<IApiKnowledgeGenerator>>(MockBehavior.Strict);
            this.remover = new Mock<IApiRemover>(MockBehavior.Strict);

            this.operations = new ApiOperations(
                new ReleaseOperationLock(this.layout, new SystemProcessProbe(), NullLogger<ReleaseOperationLock>.Instance),
                this.installer.Object,
                this.builder.Object,
                new ApiSelector(this.apiLayout),
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

        private string? Default => ApiVersionManifestFile.ReadIfPresent(this.apiLayout.VersionManifest.FullName)?.Default;

        [Test]
        public async Task Install_fetches_generates_and_only_then_switches_the_default_holding_the_lock_throughout()
        {
            var request = new ApiInstallRequest { Tag = Version, IncludeSpecification = true };
            var fetched = request with { MakeDefault = false };
            var progress = new Mock<IProgress<KnowledgeProgress>>().Object;
            var build = new KnowledgeBuild([], null);
            var held = new List<string>();
            this.WriteManifest("2025-02", "2025-02");

            this.installer
                .Setup(i => i.InstallAsync(fetched, It.IsAny<CancellationToken>(), progress))
                .Callback(() =>
                {
                    held.Add($"fetch:{this.LockHeld}:{this.Default}");
                    this.WriteManifest("2025-02", "2025-02", Version);
                })
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.Is<IReadOnlyList<string>>(tags => tags.Count == 1 && tags[0] == Version), null, progress, It.IsAny<CancellationToken>()))
                .Callback(() => held.Add($"generate:{this.LockHeld}:{this.Default}"))
                .ReturnsAsync(build);

            var setup = await this.operations.InstallAsync(request, progress);

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.EqualTo(new[] { "fetch:True:2025-02", "generate:True:2025-02" }));
                Assert.That(this.Default, Is.EqualTo(Version), "the default switches once the knowledge is generated");
                Assert.That(setup, Is.EqualTo(new ApiSetup(setup.Installation, build, IsDefault: true)));
                Assert.That(this.LockHeld, Is.False);
            });
        }

        [Test]
        public async Task A_refused_generation_leaves_the_previous_default_answering()
        {
            this.installer
                .Setup(i => i.InstallAsync(It.Is<ApiInstallRequest>(r => !r.MakeDefault), It.IsAny<CancellationToken>(), null))
                .Callback(() => this.WriteManifest("2025-02", "2025-02", Version))
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KnowledgeBuild([], new ArtifactRefusal(Version, "cross-references", "a slug collision")));

            var setup = await this.operations.InstallAsync(new ApiInstallRequest { Tag = Version, MakeDefault = true });

            Assert.Multiple(() =>
            {
                Assert.That(setup.IsDefault, Is.False);
                Assert.That(this.Default, Is.EqualTo("2025-02"));
            });
        }

        [Test]
        public async Task An_install_not_asked_to_be_the_default_does_not_switch_it()
        {
            this.installer
                .Setup(i => i.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<CancellationToken>(), null))
                .Callback(() => this.WriteManifest("2025-02", "2025-02", Version))
                .ReturnsAsync(this.Installation());
            this.builder
                .Setup(b => b.BuildAsync(It.IsAny<IReadOnlyList<string>>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KnowledgeBuild([], null));

            var setup = await this.operations.InstallAsync(new ApiInstallRequest { Tag = Version, MakeDefault = false });

            Assert.Multiple(() =>
            {
                Assert.That(setup.IsDefault, Is.False);
                Assert.That(this.Default, Is.EqualTo("2025-02"));
            });
        }

        [Test]
        public void A_failed_fetch_releases_the_lock_and_generates_nothing()
        {
            this.installer
                .Setup(i => i.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<CancellationToken>(), null))
                .ThrowsAsync(new ArgumentException("2024-12 is older than 2025-02"));

            Assert.Multiple(() =>
            {
                Assert.That(() => this.operations.InstallAsync(new ApiInstallRequest { Tag = "2024-12" }), Throws.ArgumentException);
                Assert.That(this.LockHeld, Is.False);
            });

            this.builder.VerifyNoOtherCalls();
        }

        [Test]
        public void A_lock_another_process_holds_is_refused_before_anything_changes()
        {
            this.WriteManifest("2025-02", "2025-02", Version);
            File.WriteAllText(
                Path.Combine(this.workspace.FullName, ReleaseOperationLock.FileName),
                $"{{\"pid\": {Environment.ProcessId}, \"token\": \"other\"}}");

            Assert.Multiple(() =>
            {
                Assert.That(() => this.operations.InstallAsync(new ApiInstallRequest { Tag = Version }), Throws.TypeOf<ReleaseOperationLockException>());
                Assert.That(() => this.operations.UseAsync(Version), Throws.TypeOf<ReleaseOperationLockException>());
                Assert.That(() => this.operations.RemoveAsync(Version), Throws.TypeOf<ReleaseOperationLockException>());
                Assert.That(this.Default, Is.EqualTo("2025-02"));
            });

            this.installer.VerifyNoOtherCalls();
            this.remover.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Use_switches_the_default_API_version_and_says_what_it_was()
        {
            this.WriteManifest("2025-02", "2025-02", Version);

            var selection = await this.operations.UseAsync(Version);

            Assert.Multiple(() =>
            {
                Assert.That(selection, Is.EqualTo(new ReleaseSelection(Version, "2025-02")));
                Assert.That(this.Default, Is.EqualTo(Version));
                Assert.That(this.LockHeld, Is.False);
            });
        }

        [Test]
        public async Task Use_of_the_default_changes_nothing()
        {
            this.WriteManifest(Version, Version);

            var selection = await this.operations.UseAsync(Version);

            Assert.That(selection.Changed, Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Use_of_a_version_that_is_not_installed_names_the_installed_ones(bool anyInstalled)
        {
            if (anyInstalled)
            {
                this.WriteManifest("2025-02", "2025-02");
            }

            Assert.That(
                () => this.operations.UseAsync(Version),
                Throws.ArgumentException.With.Message.Contains($"API version '{Version}' is not installed")
                    .And.Message.Contains(anyInstalled ? "2025-02" : "no API version is installed"));
        }

        [Test]
        public async Task Remove_removes_the_API_version_holding_the_lock()
        {
            var held = false;
            this.remover
                .Setup(r => r.RemoveAsync("2025-02", true, It.IsAny<CancellationToken>()))
                .Callback(() => held = this.LockHeld)
                .Returns(Task.CompletedTask);

            await this.operations.RemoveAsync("2025-02", force: true);

            Assert.Multiple(() =>
            {
                Assert.That(held, Is.True);
                Assert.That(this.LockHeld, Is.False);
            });
        }

        [Test]
        public void Removing_an_API_version_that_is_not_installed_says_so_without_a_parameter_name()
        {
            this.WriteManifest(Version, Version);
            var operations = new ApiOperations(
                new ReleaseOperationLock(this.layout, new SystemProcessProbe(), NullLogger<ReleaseOperationLock>.Instance),
                this.installer.Object,
                this.builder.Object,
                new ApiSelector(this.apiLayout),
                new ApiRemover(this.apiLayout, NullLogger<ApiRemover>.Instance));

            Assert.Multiple(() =>
            {
                Assert.That(
                    () => operations.RemoveAsync("2026-09"),
                    Throws.ArgumentException
                        .With.Message.EqualTo($"API version '2026-09' is not installed; the installed API versions are {Version}"));
                Assert.That(
                    () => operations.RemoveAsync("not-a-tag"),
                    Throws.ArgumentException.With.Message.EqualTo("not an API version: not-a-tag"));
            });
        }

        private void WriteManifest(string defaultVersion, params string[] versions) =>
            ApiVersionManifestFile.Write(
                ApiVersionManifest.Build(defaultVersion, Array.ConvertAll(versions, Installed)),
                this.apiLayout.VersionManifest.FullName);

        private static InstalledApiVersion Installed(string version) =>
            new(
                version,
                new UpstreamReference(ApiUpstream.ServicesRepository, "abc"),
                new UpstreamReference(Upstream.ReleaseRepository, "def"),
                new UpstreamReference(Upstream.PilotRepository, "ghi"),
                ApiUpstream.PimDocument);

        private ApiInstallation Installation() =>
            new(Version, [], Installed(Version), this.apiLayout.VersionManifest);
    }
}
