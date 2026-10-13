// ------------------------------------------------------------------------------------------------
// <copyright file="ApiFetchCommandTests.cs" company="Starion Group S.A.">
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
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands.Api;

    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="ApiFetchCommand"/>.
    /// </summary>
    [TestFixture]
    public class ApiFetchCommandTests
    {
        private RootCommand root = null!;
        private Mock<IApiInstaller> installer = null!;
        private ApiFetchCommand.Handler handler = null!;

        [SetUp]
        public void SetUp()
        {
            var api = new ApiCommand();
            api.Add(new ApiFetchCommand());

            this.root = new RootCommand();
            this.root.Add(api);

            this.installer = new Mock<IApiInstaller>();

            this.installer
                .Setup(mock => mock.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApiInstallRequest request, CancellationToken _) => Installation(request));

            this.handler = new ApiFetchCommand.Handler(this.installer.Object);
        }

        [Test]
        public async Task An_api_version_is_fetched_with_the_defaults_that_make_a_run_resumable()
        {
            var result = await this.Invoke("api fetch --tag 2026-04");

            Assert.That(result, Is.EqualTo(0));

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ApiInstallRequest>(request =>
                        request.Tag == "2026-04"
                        && request.SkipExisting
                        && request.MakeDefault
                        && request.IncludeSpecification),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task The_short_tag_option_is_accepted()
        {
            await this.Invoke("api fetch -t 2026-04");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ApiInstallRequest>(request => request.Tag == "2026-04"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task No_specs_skips_the_copyrighted_PDF()
        {
            await this.Invoke("api fetch --tag 2026-04 --no-specs");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ApiInstallRequest>(request => !request.IncludeSpecification),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Force_stops_an_existing_file_from_being_kept()
        {
            await this.Invoke("api fetch --tag 2026-04 --force");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ApiInstallRequest>(request => !request.SkipExisting),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task No_default_leaves_the_answering_version_alone()
        {
            await this.Invoke("api fetch --tag 2026-04 --no-default");

            this.installer.Verify(
                mock => mock.InstallAsync(
                    It.Is<ApiInstallRequest>(request => !request.MakeDefault),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task The_outcome_names_the_version_the_file_count_and_the_manifest()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("api fetch --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(console.Output, Does.Contain("Fetched API version 2026-04: 4 files."));
                Assert.That(console.Output, Does.Contain("Recorded in"));
            });
        }

        [Test]
        public async Task A_tag_that_is_not_a_version_is_a_usage_error_pointing_at_api_discover()
        {
            this.installer
                .Setup(mock => mock.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("not an offerable API version: 2026-04-pre"));

            using var console = new RecordedConsole();

            var result = await this.Invoke("api fetch --tag 2026-04-pre");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("2026-04-pre").And.Contains("hypha api discover"));
            });
        }

        [Test]
        public void A_failed_download_is_not_dressed_up_as_a_usage_error()
        {
            // Only the refusal of the tag is the user's to fix; anything else has to reach the caller.
            this.installer
                .Setup(mock => mock.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("the host reset the connection"));

            Assert.That(() => this.Invoke("api fetch --tag 2026-04"), Throws.InstanceOf<IOException>());
        }

        [Test]
        public async Task The_cancellation_token_reaches_the_installer()
        {
            using var source = new CancellationTokenSource();

            await this.handler.InvokeAsync(this.root.Parse("api fetch --tag 2026-04"), source.Token);

            this.installer.Verify(
                mock => mock.InstallAsync(It.IsAny<ApiInstallRequest>(), source.Token), Times.Once);
        }

        [Test]
        public void The_tag_is_required()
        {
            Assert.That(this.root.Parse("api fetch").Errors, Is.Not.Empty);
        }

        [Test]
        public void A_missing_installer_is_refused()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new ApiFetchCommand.Handler(null!));
        }

        private Task<int> Invoke(string commandLine) =>
            this.handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);

        private static ApiInstallation Installation(ApiInstallRequest request)
        {
            var files = new List<FileInfo>
            {
                new("openapi.json"), new("KerML_only_xmi.uml"), new("Systems-Modeling-API.xmi"),
            };

            if (request.IncludeSpecification)
            {
                files.Add(new FileInfo("3-Systems_Modeling_API_and_Services.pdf"));
            }

            return new ApiInstallation(
                request.Tag,
                files,
                new InstalledApiVersion(
                    request.Tag,
                    new UpstreamReference("Systems-Modeling/SysML-v2-API-Services", "abc123"),
                    new UpstreamReference("Systems-Modeling/SysML-v2-Release", "def456"),
                    new UpstreamReference("Systems-Modeling/SysML-v2-Pilot-Implementation", "789abc"),
                    "ptc/25-02-29"),
                new FileInfo("versions.json"));
        }
    }
}
