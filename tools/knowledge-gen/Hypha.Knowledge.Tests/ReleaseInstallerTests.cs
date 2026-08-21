// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInstallerTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="ReleaseInstaller"/>: what it fetches, and what it records.
    /// </summary>
    /// <remarks>
    /// Nothing here touches the network. The fetcher is a stub that reports the files it would have
    /// written, which is enough to pin down the ordering, the flags and the manifest merge - the parts
    /// a live run would be a slow and flaky way to check.
    /// </remarks>
    [TestFixture]
    public class ReleaseInstallerTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private Mock<IReleaseFetcher> fetcher = null!;
        private Mock<ICommitResolver> resolver = null!;
        private ReleaseInstaller installer = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-install-{Guid.NewGuid():N}"));

            this.layout = new KnowledgeLayout(this.workspace);

            this.fetcher = new Mock<IReleaseFetcher>();

            this.fetcher
                .Setup(mock => mock.FetchMetamodelAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Files("KerML.uml", "SysML.uml"));

            this.fetcher
                .Setup(mock => mock.FetchTextualAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Files("model.sysml"));

            this.fetcher
                .Setup(mock => mock.FetchSpecificationsAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Files("KerML.pdf"));

            this.resolver = new Mock<ICommitResolver>();
            this.resolver
                .Setup(mock => mock.ResolveVersionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string tag, CancellationToken _) => Version(tag));

            this.installer = new ReleaseInstaller(
                this.fetcher.Object,
                this.resolver.Object,
                this.layout,
                NullLogger<ReleaseInstaller>.Instance);
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
        public async Task A_release_is_fetched_and_recorded()
        {
            var installation = await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });

            Assert.Multiple(() =>
            {
                Assert.That(installation.Metamodel, Has.Count.EqualTo(2));
                Assert.That(installation.Textual, Has.Count.EqualTo(1));
                Assert.That(installation.FileCount, Is.EqualTo(3));
                Assert.That(installation.Manifest.Exists, Is.True);
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public async Task The_copyrighted_specifications_are_left_alone_unless_asked_for()
        {
            var without = await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });

            Assert.That(without.Specifications, Is.Empty);

            this.fetcher.Verify(
                mock => mock.FetchSpecificationsAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);

            var with = await this.installer.InstallAsync(
                new ReleaseInstallRequest { Tag = "2026-05", IncludeSpecifications = true });

            Assert.That(with.Specifications, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task An_interrupted_fetch_is_resumed_rather_than_repeated()
        {
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });

            this.fetcher.Verify(
                mock => mock.FetchTextualAsync(
                    "2026-05", It.IsAny<DirectoryInfo>(), true, It.IsAny<CancellationToken>()),
                Times.Once,
                "skipExisting defaults on, which is what makes an interrupted fetch resumable");
        }

        [Test]
        public async Task Installing_a_release_again_replaces_its_record_rather_than_doubling_it()
        {
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.That(manifest.Versions, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task A_second_release_joins_the_first_and_the_newest_leads()
        {
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-04" });
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetTags(), Is.EqualTo(new[] { "2026-05", "2026-04" }));
                Assert.That(manifest.Default, Is.EqualTo("2026-05"));
            });
        }

        [Test]
        public async Task Installing_without_making_it_default_leaves_the_answering_release_alone()
        {
            await this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" });
            await this.installer.InstallAsync(
                new ReleaseInstallRequest { Tag = "2026-04", MakeDefault = false });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest.Default, Is.EqualTo("2026-05"));
                Assert.That(manifest.GetTags(), Has.Count.EqualTo(2));
            });
        }

        [Test]
        public async Task The_first_release_installed_is_the_default_even_when_not_asked_for()
        {
            // A manifest whose default is not installed is one VersionManifest.Build refuses to build,
            // so there is no such thing as "installed but nothing answers".
            await this.installer.InstallAsync(
                new ReleaseInstallRequest { Tag = "2026-05", MakeDefault = false });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);

            Assert.That(manifest.Default, Is.EqualTo("2026-05"));
        }

        [TestCase("2026-05-pre")]
        [TestCase("2021-08-internal")]
        [TestCase("2021-05a")]
        [TestCase("main")]
        public void A_tag_that_is_not_an_offerable_release_is_refused_before_anything_is_written(string tag)
        {
            Assert.That(
                () => this.installer.InstallAsync(new ReleaseInstallRequest { Tag = tag }),
                Throws.ArgumentException);

            Assert.That(
                this.layout.VersionManifest.Exists, Is.False,
                "a refused tag must not leave a manifest behind");
        }

        [Test]
        public void The_manifest_is_only_written_once_the_downloads_have_finished()
        {
            // A release recorded but half-fetched would generate a knowledge base that looks complete.
            this.fetcher
                .Setup(mock => mock.FetchTextualAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("the host reset the connection"));

            Assert.That(
                () => this.installer.InstallAsync(new ReleaseInstallRequest { Tag = "2026-05" }),
                Throws.InstanceOf<IOException>());

            this.layout.VersionManifest.Refresh();

            Assert.That(this.layout.VersionManifest.Exists, Is.False);
        }

        [Test]
        public async Task Progress_reaches_the_metamodel_and_textual_fetches_but_never_specifications()
        {
            IProgress<FetchProgress>? seenByMetamodel = null;
            IProgress<FetchProgress>? seenByTextual = null;

            this.fetcher
                .Setup(mock => mock.FetchMetamodelAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(), It.IsAny<IProgress<FetchProgress>>()))
                .Callback<string, DirectoryInfo, bool, CancellationToken, IProgress<FetchProgress>?>(
                    (_, _, _, _, progress) => seenByMetamodel = progress)
                .ReturnsAsync(Files("KerML.uml", "SysML.uml"));

            this.fetcher
                .Setup(mock => mock.FetchTextualAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(), It.IsAny<IProgress<FetchProgress>>()))
                .Callback<string, DirectoryInfo, bool, CancellationToken, IProgress<FetchProgress>?>(
                    (_, _, _, _, progress) => seenByTextual = progress)
                .ReturnsAsync(Files("model.sysml"));

            this.fetcher
                .Setup(mock => mock.FetchSpecificationsAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Files("KerML.pdf"));

            var reporter = new Progress<FetchProgress>();

            await this.installer.InstallAsync(
                new ReleaseInstallRequest { Tag = "2026-05", IncludeSpecifications = true },
                CancellationToken.None,
                reporter);

            Assert.Multiple(() =>
            {
                Assert.That(seenByMetamodel, Is.SameAs(reporter));
                Assert.That(seenByTextual, Is.SameAs(reporter));
            });

            this.fetcher.Verify(
                mock => mock.FetchSpecificationsAsync(
                    It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "specifications are only ever called through the 4-argument overload - no progress reporter reaches it");
        }

        private static IReadOnlyList<FileInfo> Files(params string[] names) =>
            Array.ConvertAll(names, name => new FileInfo(name));

        private static InstalledVersion Version(string tag) =>
            new(
                tag,
                new UpstreamReference(Upstream.ReleaseRepository, "abc123"),
                new UpstreamReference(Upstream.PilotRepository, "def456"));
    }
}
