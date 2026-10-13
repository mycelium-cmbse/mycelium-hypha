// ------------------------------------------------------------------------------------------------
// <copyright file="ApiInstallerTests.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="ApiInstaller"/>: what it fetches, from where, and what it
    /// records.
    /// </summary>
    /// <remarks>
    /// Nothing here touches the network. The fetcher and the document downloader are stubs that report
    /// the file they would have written, which is enough to pin down the four sources, the flags and
    /// the manifest merge.
    /// </remarks>
    [TestFixture]
    public class ApiInstallerTests
    {
        private static readonly Uri PimAddress = new("https://documents.example.invalid/pim/Systems-Modeling-API.xmi");

        private DirectoryInfo workspace = null!;
        private ApiLayout layout = null!;
        private Mock<IReleaseFetcher> fetcher = null!;
        private Mock<IDocumentDownloader> documents = null!;
        private Mock<ICommitResolver> resolver = null!;
        private HyphaKnowledgeOptions options = null!;
        private List<string> downloads = null!;
        private ApiInstaller installer = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-install-{Guid.NewGuid():N}"));

            this.layout = new ApiLayout(new KnowledgeLayout(this.workspace));
            this.downloads = [];

            this.fetcher = new Mock<IReleaseFetcher>();
            this.fetcher
                .Setup(mock => mock.DownloadAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileInfo>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string repository, string tag, string path, FileInfo destination, bool _, CancellationToken _) =>
                {
                    this.downloads.Add($"{repository}@{tag}:{path}");
                    return destination;
                });

            this.documents = new Mock<IDocumentDownloader>();
            this.documents
                .Setup(mock => mock.DownloadAsync(
                    It.IsAny<Uri>(), It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Uri address, FileInfo destination, bool _, CancellationToken _) =>
                {
                    this.downloads.Add(address.ToString());
                    return destination;
                });

            this.resolver = new Mock<ICommitResolver>();
            this.resolver
                .Setup(mock => mock.ResolveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string repository, string tag, CancellationToken _) => $"commit-of-{repository}-at-{tag}");

            this.options = new HyphaKnowledgeOptions { PimAddress = PimAddress };

            this.installer = new ApiInstaller(
                this.fetcher.Object,
                this.documents.Object,
                this.resolver.Object,
                this.layout,
                this.options,
                NullLogger<ApiInstaller>.Instance);
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

        private string Relative(FileSystemInfo path) =>
            Path.GetRelativePath(this.workspace.FullName, path.FullName).Replace('\\', '/');

        [Test]
        public async Task An_api_version_is_fetched_and_recorded()
        {
            var installation = await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            Assert.Multiple(() =>
            {
                Assert.That(installation.Tag, Is.EqualTo("2026-04"));
                Assert.That(installation.Files, Has.Count.EqualTo(3));
                Assert.That(installation.Manifest.Exists, Is.True);
                Assert.That(
                    installation.Manifest.FullName, Is.EqualTo(this.layout.VersionManifest.FullName));
            });

            Assert.Multiple(() =>
            {
                Assert.That(this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04" }));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-04"));
            });
        }

        [Test]
        public async Task The_three_inputs_come_from_three_different_places()
        {
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            Assert.That(
                this.downloads,
                Is.EqualTo(new[]
                {
                    "Systems-Modeling/SysML-v2-API-Services@2026-04:public/docs/openapi.json",
                    "Systems-Modeling/SysML-v2-Pilot-Implementation@2026-04:org.omg.sysml/model/KerML_only_xmi.uml",
                    PimAddress.ToString(),
                }));
        }

        [Test]
        public async Task Every_input_lands_under_sources_api_for_its_version()
        {
            var installation = await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true });

            Assert.That(
                installation.Files.Select(this.Relative),
                Is.EqualTo(new[]
                {
                    "sources/api/2026-04/openapi/openapi.json",
                    "sources/api/2026-04/xmi/KerML_only_xmi.uml",
                    "sources/api/2026-04/xmi/Systems-Modeling-API.xmi",
                    "sources/api/2026-04/specs/3-Systems_Modeling_API_and_Services.pdf",
                }));
        }

        [Test]
        public async Task The_pim_model_is_read_from_the_address_the_options_carry()
        {
            // No Systems-Modeling repository carries the PIM, so it does not go through the fetcher -
            // and the address is a setting so a test can serve it from a stub.
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            this.documents.Verify(
                mock => mock.DownloadAsync(
                    PimAddress,
                    It.Is<FileInfo>(file => file.FullName == this.layout.PimModel("2026-04").FullName),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.Is<FileInfo>(file => file.Name == ApiInputs.PimModel),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task The_copyrighted_specification_is_left_alone_unless_asked_for()
        {
            var without = await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            Assert.That(without.Files.Select(file => file.Name), Does.Not.Contain(ApiInputs.Specification));

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    Upstream.ReleaseRepository, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileInfo>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);

            var with = await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true });

            Assert.Multiple(() =>
            {
                Assert.That(with.Files, Has.Count.EqualTo(4));
                Assert.That(with.Files[^1].Name, Is.EqualTo(ApiInputs.Specification));
                Assert.That(with.Files[^1].Directory!.Name, Is.EqualTo("specs"));
            });

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    Upstream.ReleaseRepository, "2026-04", ApiUpstream.SpecificationPath, It.IsAny<FileInfo>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task An_interrupted_fetch_is_resumed_rather_than_repeated()
        {
            await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true });

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<string>(), "2026-04", It.IsAny<string>(), It.IsAny<FileInfo>(),
                    true, It.IsAny<CancellationToken>()),
                Times.Exactly(3),
                "skipExisting defaults on, which is what makes an interrupted fetch resumable");

            this.documents.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<Uri>(), It.IsAny<FileInfo>(), true, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Turning_skip_existing_off_downloads_every_file_again()
        {
            await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true, SkipExisting = false });

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileInfo>(),
                    false, It.IsAny<CancellationToken>()),
                Times.Exactly(3));

            this.documents.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<Uri>(), It.IsAny<FileInfo>(), false, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task The_tag_is_resolved_in_all_three_repositories_and_the_pim_document_is_named()
        {
            var installation = await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            var expected = new InstalledApiVersion(
                "2026-04",
                new UpstreamReference(
                    ApiUpstream.ServicesRepository, $"commit-of-{ApiUpstream.ServicesRepository}-at-2026-04"),
                new UpstreamReference(
                    Upstream.ReleaseRepository, $"commit-of-{Upstream.ReleaseRepository}-at-2026-04"),
                new UpstreamReference(
                    Upstream.PilotRepository, $"commit-of-{Upstream.PilotRepository}-at-2026-04"),
                ApiUpstream.PimDocument);

            var recorded = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(installation.Version, Is.EqualTo(expected));
                Assert.That(recorded!.Versions, Is.EqualTo(new[] { expected }));
            });

            this.resolver.Verify(
                mock => mock.ResolveAsync(It.IsAny<string>(), "2026-04", It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Test]
        public async Task The_release_commit_is_recorded_even_when_the_specification_was_not_fetched()
        {
            // The version is a point on all three upstreams whichever of their files was read.
            var installation = await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            Assert.That(installation.Version.Release.Repo, Is.EqualTo(Upstream.ReleaseRepository));
        }

        [Test]
        public async Task Installing_a_version_again_replaces_its_record_rather_than_doubling_it()
        {
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            this.resolver
                .Setup(mock => mock.ResolveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("moved");

            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            var manifest = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest!.Versions, Has.Count.EqualTo(1));
                Assert.That(manifest.Versions[0].Services.Commit, Is.EqualTo("moved"));
            });
        }

        [Test]
        public async Task A_second_version_joins_the_first_and_the_newest_leads()
        {
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2025-09" });

            var manifest = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest!.GetTags(), Is.EqualTo(new[] { "2026-04", "2025-09" }));
                Assert.That(manifest.Default, Is.EqualTo("2025-09"), "the version just installed answers");
            });
        }

        [Test]
        public async Task Installing_without_making_it_default_leaves_the_answering_version_alone()
        {
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2025-09" });
            await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", MakeDefault = false });

            var manifest = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(manifest!.Default, Is.EqualTo("2025-09"));
                Assert.That(manifest.GetTags(), Has.Count.EqualTo(2));
            });
        }

        [Test]
        public async Task The_first_version_installed_is_the_default_even_when_not_asked_for()
        {
            // A manifest whose default is not installed is one ApiVersionManifest.Build refuses to
            // build, so there is no such thing as "installed but nothing answers".
            await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", MakeDefault = false });

            var manifest = ApiVersionManifestFile.ReadIfPresent(this.layout.VersionManifest.FullName);

            Assert.That(manifest!.Default, Is.EqualTo("2026-04"));
        }

        [Test]
        public async Task Installing_an_api_version_leaves_the_language_releases_manifest_alone()
        {
            await this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" });

            Assert.That(
                new KnowledgeLayout(this.workspace).VersionManifest.Exists, Is.False,
                "the API is versioned on its own axis");
        }

        [TestCase("2026-05-pre")]
        [TestCase("2021-08-internal")]
        [TestCase("2021-05a")]
        [TestCase("main")]
        [TestCase("")]
        public void A_tag_that_is_not_an_offerable_version_is_refused_before_anything_is_written(string tag)
        {
            Assert.That(
                () => this.installer.InstallAsync(new ApiInstallRequest { Tag = tag }),
                Throws.ArgumentException);

            Assert.Multiple(() =>
            {
                Assert.That(
                    this.layout.VersionManifest.Exists, Is.False,
                    "a refused tag must not leave a manifest behind");
                Assert.That(this.downloads, Is.Empty, "a refused tag must not be downloaded either");
                Assert.That(Directory.EnumerateFileSystemEntries(this.workspace.FullName), Is.Empty);
            });

            this.resolver.Verify(
                mock => mock.ResolveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void The_manifest_is_only_written_once_the_downloads_have_finished()
        {
            // A version recorded but half-fetched would generate a knowledge base that looks complete.
            this.documents
                .Setup(mock => mock.DownloadAsync(
                    It.IsAny<Uri>(), It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("the host reset the connection"));

            Assert.That(
                () => this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" }),
                Throws.InstanceOf<IOException>());

            this.layout.VersionManifest.Refresh();

            Assert.That(this.layout.VersionManifest.Exists, Is.False);
        }

        [Test]
        public void A_tag_that_cannot_be_resolved_is_not_recorded()
        {
            this.resolver
                .Setup(mock => mock.ResolveAsync(
                    ApiUpstream.ServicesRepository, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("no such tag"));

            Assert.That(
                () => this.installer.InstallAsync(new ApiInstallRequest { Tag = "2026-04" }),
                Throws.InvalidOperationException);

            Assert.That(this.layout.VersionManifest.Exists, Is.False);
        }

        [Test]
        public async Task The_cancellation_token_reaches_every_download_and_resolution()
        {
            using var source = new CancellationTokenSource();

            await this.installer.InstallAsync(
                new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true }, source.Token);

            this.fetcher.Verify(
                mock => mock.DownloadAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileInfo>(),
                    It.IsAny<bool>(), source.Token),
                Times.Exactly(3));
            this.documents.Verify(
                mock => mock.DownloadAsync(It.IsAny<Uri>(), It.IsAny<FileInfo>(), It.IsAny<bool>(), source.Token),
                Times.Once);
            this.resolver.Verify(
                mock => mock.ResolveAsync(It.IsAny<string>(), It.IsAny<string>(), source.Token),
                Times.Exactly(3));
        }

        [Test]
        public void A_missing_request_is_refused()
        {
            Assert.That(() => this.installer.InstallAsync(null!), Throws.ArgumentNullException);
        }
    }
}
