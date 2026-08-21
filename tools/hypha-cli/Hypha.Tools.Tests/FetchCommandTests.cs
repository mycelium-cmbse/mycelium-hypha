// ------------------------------------------------------------------------------------------------
// <copyright file="FetchCommandTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="FetchCommand"/>.
    /// </summary>
    [TestFixture]
    public class FetchCommandTests
    {
        private RootCommand root = null!;
        private Mock<IReleaseInstaller> installer = null!;
        private FetchCommand.Handler handler = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = new RootCommand();
            this.root.Add(new FetchCommand());

            this.installer = new Mock<IReleaseInstaller>();
            this.installer
                .Setup(mock => mock.InstallAsync(
                    It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReleaseInstallRequest request, CancellationToken _, IProgress<FetchProgress>? _) =>
                    Installation(request));

            this.handler = new FetchCommand.Handler(this.installer.Object);
        }

        [Test]
        public async Task A_release_is_fetched_with_the_defaults_that_make_a_run_resumable()
        {
            var result = await this.Invoke("fetch --tag 2026-05");

            Assert.That(result, Is.EqualTo(0));

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ReleaseInstallRequest>(request =>
                        request.Tag == "2026-05"
                        && request.SkipExisting
                        && request.MakeDefault
                        && !request.IncludeSpecifications),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task The_copyrighted_specifications_are_only_fetched_when_asked_for()
        {
            await this.Invoke("fetch --tag 2026-05 --include-specs");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ReleaseInstallRequest>(request => request.IncludeSpecifications),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Force_stops_an_existing_file_from_being_kept()
        {
            await this.Invoke("fetch --tag 2026-05 --force");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ReleaseInstallRequest>(request => !request.SkipExisting),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task No_default_leaves_the_answering_release_alone()
        {
            await this.Invoke("fetch --tag 2026-05 --no-default");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ReleaseInstallRequest>(request => !request.MakeDefault),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task A_tag_that_is_not_a_release_is_a_usage_error_pointing_at_discover()
        {
            this.installer
                .Setup(mock => mock.InstallAsync(
                    It.IsAny<ReleaseInstallRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("not an offerable release tag: 2026-05-pre"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("fetch --tag 2026-05-pre");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("2026-05-pre").And.Contains("hypha discover"));
            });
        }

        [Test]
        public void The_tag_is_required()
        {
            Assert.That(this.root.Parse("fetch").Errors, Is.Not.Empty);
        }

        private Task<int> Invoke(string commandLine) =>
            this.handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);

        private static ReleaseInstallation Installation(ReleaseInstallRequest request) =>
            new(
                request.Tag,
                [new FileInfo("KerML.uml"), new FileInfo("SysML.uml")],
                [new FileInfo("model.sysml")],
                request.IncludeSpecifications ? [new FileInfo("KerML.pdf")] : [],
                new InstalledVersion(
                    request.Tag,
                    new UpstreamReference("Systems-Modeling/SysML-v2-Release", "abc123"),
                    new UpstreamReference("Systems-Modeling/SysML-v2-Pilot-Implementation", "def456")),
                new FileInfo("versions.json"));
    }
}
