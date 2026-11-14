// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseChangeToolsTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Progress;
    using Hypha.Knowledge.Releases;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;

    using Moq;

    [TestFixture]
    public class ReleaseChangeToolsTests
    {
        private const string Tag = "2026-05";

        private Mock<IReleaseOperations> operations = null!;
        private Mock<IReleaseDiscovery> discovery = null!;
        private Mock<IApiOperations> api = null!;
        private Mock<IApiDiscovery> apiDiscovery = null!;
        private ReleaseChangeTools tools = null!;

        [SetUp]
        public void SetUp()
        {
            this.operations = new Mock<IReleaseOperations>(MockBehavior.Strict);
            this.discovery = new Mock<IReleaseDiscovery>(MockBehavior.Strict);
            this.api = new Mock<IApiOperations>(MockBehavior.Strict);
            this.apiDiscovery = new Mock<IApiDiscovery>(MockBehavior.Strict);
            this.tools = new ReleaseChangeTools(this.operations.Object, this.discovery.Object, this.api.Object, this.apiDiscovery.Object);
        }

        [Test]
        public async Task Install_asks_for_the_release_as_given_and_says_what_each_artifact_did()
        {
            ReleaseInstallRequest? asked = null;
            this.operations
                .Setup(o => o.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .Callback<ReleaseInstallRequest, IProgress<KnowledgeProgress>?, CancellationToken>((request, _, _) => asked = request)
                .ReturnsAsync(Setup(
                    new KnowledgeBuild(
                        [
                            new ArtifactBuild(Tag, "metamodel", GenerationResult.Generated([new FileInfo("a.md"), new FileInfo("b.md")])),
                            new ArtifactBuild(Tag, "spec", GenerationResult.Skipped("the specification PDFs are not in sources/2026-05/specs/")),
                        ],
                        null)));

            using var answer = JsonDocument.Parse(await this.tools.InstallRelease(" 2026-05 ", includeSpecifications: false, makeDefault: false));
            var root = answer.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(asked, Is.EqualTo(new ReleaseInstallRequest { Tag = Tag, IncludeSpecifications = false, SkipExisting = true, MakeDefault = false }));
                Assert.That(root.GetProperty("tag").GetString(), Is.EqualTo(Tag));
                Assert.That(root.GetProperty("isDefault").GetBoolean(), Is.False);
                Assert.That(root.GetProperty("fetched").GetProperty("metamodel").GetInt32(), Is.EqualTo(1));
                Assert.That(root.GetProperty("fetched").GetProperty("specifications").GetInt32(), Is.Zero);
                Assert.That(root.GetProperty("artifacts")[0].GetProperty("outcome").GetString(), Is.EqualTo("generated"));
                Assert.That(root.GetProperty("artifacts")[0].GetProperty("files").GetInt32(), Is.EqualTo(2));
                Assert.That(root.GetProperty("artifacts")[1].GetProperty("outcome").GetString(), Is.EqualTo("skipped"));
                Assert.That(root.GetProperty("artifacts")[1].GetProperty("reason").GetString(), Does.Contain("specification PDFs"));
                Assert.That(root.GetProperty("refusal").ValueKind, Is.EqualTo(JsonValueKind.Null));
            });
        }

        [Test]
        public async Task Install_without_a_tag_installs_the_newest_offerable_release_by_default_with_its_specifications()
        {
            this.discovery.Setup(d => d.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(["2026-06", Tag]);
            this.operations
                .Setup(o => o.InstallAsync(
                    new ReleaseInstallRequest { Tag = "2026-06", IncludeSpecifications = true, SkipExisting = true, MakeDefault = true },
                    It.IsAny<IProgress<KnowledgeProgress>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Setup(new KnowledgeBuild([], null), "2026-06", isDefault: true));

            using var answer = JsonDocument.Parse(await this.tools.InstallRelease());

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("tag").GetString(), Is.EqualTo("2026-06"));
                Assert.That(answer.RootElement.GetProperty("isDefault").GetBoolean(), Is.True);
            });
        }

        [Test]
        public void Install_without_a_tag_when_nothing_is_offered_is_a_tool_error()
        {
            this.discovery.Setup(d => d.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Assert.That(() => this.tools.InstallRelease(), Throws.TypeOf<McpException>().With.Message.Contains("No release is offered upstream"));
        }

        [Test]
        public async Task Install_forwards_its_progress_to_the_client_after_saying_what_it_waits_for()
        {
            var sent = new List<ProgressNotificationValue>();
            this.operations
                .Setup(o => o.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .Returns<ReleaseInstallRequest, IProgress<KnowledgeProgress>?, CancellationToken>((_, progress, _) =>
                {
                    progress!.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 1, "1 of 1 files"));
                    progress.Report(new KnowledgeProgress(KnowledgeProgress.Generate, Tag, 1, 6, "metamodel: 120 files"));

                    return Task.FromResult(Setup(new KnowledgeBuild([], null)));
                });

            await this.tools.InstallAsync(Tag, true, true, VersionAxis.Release, value => { sent.Add(value); return Task.CompletedTask; }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(sent.Select(value => value.Progress), Is.EqualTo(new float[] { 0, 1, 2 }));
                Assert.That(sent[0].Message, Does.Contain(Tag).And.Contain("other release change"));
                Assert.That(sent[2].Message, Is.EqualTo("generate 2026-05: metamodel: 120 files"));
            });
        }

        [Test]
        public async Task A_generator_that_refused_is_named_in_the_answer()
        {
            this.operations
                .Setup(o => o.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Setup(new KnowledgeBuild([], new ArtifactRefusal(Tag, "cross-references", "a clause title reached the cross-references"))));

            using var answer = JsonDocument.Parse(await this.tools.InstallRelease(Tag));
            var refusal = answer.RootElement.GetProperty("refusal");

            Assert.Multiple(() =>
            {
                Assert.That(refusal.GetProperty("artifact").GetString(), Is.EqualTo("cross-references"));
                Assert.That(refusal.GetProperty("outcome").GetString(), Is.EqualTo("refused"));
                Assert.That(refusal.GetProperty("reason").GetString(), Does.Contain("clause title"));
            });
        }

        [Test]
        public void A_failed_install_is_a_tool_error_the_model_can_act_on()
        {
            this.operations
                .SetupSequence(o => o.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("not an offerable release: 2026-13"))
                .ThrowsAsync(new ReleaseOperationLockException("Another hypha is changing releases; try again once it has finished."))
                .ThrowsAsync(new HttpRequestException("Response status code does not indicate success: 403 (rate limit exceeded)."));

            Assert.Multiple(() =>
            {
                Assert.That(() => this.tools.InstallRelease("2026-13"), Throws.TypeOf<McpException>().With.Message.Contains("2026-13"));
                Assert.That(() => this.tools.InstallRelease(Tag), Throws.TypeOf<McpException>().With.Message.Contains("Another hypha is changing releases"));
                Assert.That(() => this.tools.InstallRelease(Tag), Throws.TypeOf<McpException>().With.Message.Contains("403").And.Message.Contains("GITHUB_TOKEN"));
            });
        }

        [Test]
        public void A_cancelled_install_stays_cancelled()
        {
            this.operations
                .Setup(o => o.InstallAsync(It.IsAny<ReleaseInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            Assert.That(() => this.tools.InstallRelease(Tag), Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task Use_says_what_the_default_is_now_and_was()
        {
            this.operations.Setup(o => o.UseAsync("2026-04", It.IsAny<CancellationToken>())).ReturnsAsync(new ReleaseSelection("2026-04", Tag));

            using var answer = JsonDocument.Parse(await this.tools.UseRelease("2026-04"));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("default").GetString(), Is.EqualTo("2026-04"));
                Assert.That(answer.RootElement.GetProperty("previous").GetString(), Is.EqualTo(Tag));
                Assert.That(answer.RootElement.GetProperty("changed").GetBoolean(), Is.True);
            });
        }

        [Test]
        public void Use_of_a_release_that_is_not_installed_points_at_the_install_tool()
        {
            this.operations
                .Setup(o => o.UseAsync("2026-06", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("'2026-06' is not installed; the installed releases are 2026-05"));

            Assert.That(
                () => this.tools.UseRelease("2026-06"),
                Throws.TypeOf<McpException>().With.Message.EqualTo("'2026-06' is not installed; the installed releases are 2026-05. Install it with hypha_install_release first."));
        }

        [Test]
        public async Task Remove_passes_force_and_names_the_release_removed()
        {
            this.operations.Setup(o => o.RemoveAsync("2026-02", true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            using var answer = JsonDocument.Parse(await this.tools.RemoveRelease("2026-02", force: true));

            Assert.That(answer.RootElement.GetProperty("removed").GetString(), Is.EqualTo("2026-02"));
        }

        [Test]
        public void A_refused_removal_is_a_tool_error_with_the_reason()
        {
            this.operations
                .Setup(o => o.RemoveAsync(Tag, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("'2026-05' is the default release"));

            Assert.That(() => this.tools.RemoveRelease(Tag), Throws.TypeOf<McpException>().With.Message.EqualTo("'2026-05' is the default release"));
        }

        [Test]
        public async Task Install_on_the_api_axis_installs_the_API_version_and_counts_its_documents()
        {
            ApiInstallRequest? asked = null;
            this.api
                .Setup(o => o.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .Callback<ApiInstallRequest, IProgress<KnowledgeProgress>?, CancellationToken>((request, _, _) => asked = request)
                .ReturnsAsync(ApiSetup(new KnowledgeBuild([new ArtifactBuild("2026-04", "pim", GenerationResult.Generated([new FileInfo("pim.md")]))], null)));

            using var answer = JsonDocument.Parse(await this.tools.InstallRelease(" 2026-04 ", includeSpecifications: false, makeDefault: false, axis: VersionAxis.Api));
            var root = answer.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(asked, Is.EqualTo(new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = false, SkipExisting = true, MakeDefault = false }));
                Assert.That(root.GetProperty("tag").GetString(), Is.EqualTo("2026-04"));
                Assert.That(root.GetProperty("axis").GetString(), Is.EqualTo("api"));
                Assert.That(root.GetProperty("fetched").GetProperty("documents").GetInt32(), Is.EqualTo(3));
                Assert.That(root.GetProperty("artifacts")[0].GetProperty("artifact").GetString(), Is.EqualTo("pim"));
            });
        }

        [Test]
        public async Task Install_on_the_api_axis_without_a_tag_installs_the_newest_offerable_API_version()
        {
            this.apiDiscovery.Setup(d => d.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(["2026-04", "2025-02"]);
            this.api
                .Setup(o => o.InstallAsync(
                    new ApiInstallRequest { Tag = "2026-04", IncludeSpecification = true, SkipExisting = true, MakeDefault = true },
                    It.IsAny<IProgress<KnowledgeProgress>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiSetup(new KnowledgeBuild([], null), isDefault: true));

            using var answer = JsonDocument.Parse(await this.tools.InstallRelease(axis: VersionAxis.Api));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("tag").GetString(), Is.EqualTo("2026-04"));
                Assert.That(answer.RootElement.GetProperty("isDefault").GetBoolean(), Is.True);
                this.discovery.VerifyNoOtherCalls();
            });
        }

        [Test]
        public void Install_on_the_api_axis_without_a_tag_when_nothing_is_offered_is_a_tool_error()
        {
            this.apiDiscovery.Setup(d => d.AvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

            Assert.That(
                () => this.tools.InstallRelease(axis: VersionAxis.Api),
                Throws.TypeOf<McpException>().With.Message.Contains("No Systems Modeling API version is offered upstream"));
        }

        [Test]
        public async Task Install_on_the_api_axis_says_it_installs_an_API_version()
        {
            var sent = new List<ProgressNotificationValue>();
            this.api
                .Setup(o => o.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ApiSetup(new KnowledgeBuild([], null)));

            await this.tools.InstallAsync("2026-04", true, true, VersionAxis.Api, value => { sent.Add(value); return Task.CompletedTask; }, CancellationToken.None);

            Assert.That(sent[0].Message, Does.Contain("API version 2026-04"));
        }

        [Test]
        public async Task Install_on_the_api_axis_forwards_the_installers_and_builders_progress()
        {
            var sent = new List<ProgressNotificationValue>();
            this.api
                .Setup(o => o.InstallAsync(It.IsAny<ApiInstallRequest>(), It.IsAny<IProgress<KnowledgeProgress>>(), It.IsAny<CancellationToken>()))
                .Returns<ApiInstallRequest, IProgress<KnowledgeProgress>?, CancellationToken>((_, progress, _) =>
                {
                    progress!.Report(new KnowledgeProgress(KnowledgeProgress.ApiFetch, "documents", 1, 3, "OpenAPI document, 1 of 3"));
                    progress.Report(new KnowledgeProgress(KnowledgeProgress.Generate, "2026-04", 1, 4, "pim: 12 files"));

                    return Task.FromResult(ApiSetup(new KnowledgeBuild([], null)));
                });

            await this.tools.InstallAsync("2026-04", true, true, VersionAxis.Api, value => { sent.Add(value); return Task.CompletedTask; }, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(sent.Select(value => value.Progress), Is.EqualTo(new float[] { 0, 1, 2 }));
                Assert.That(sent[2].Message, Is.EqualTo("generate 2026-04: pim: 12 files"));
            });
        }

        [Test]
        public void Removing_an_API_version_that_is_not_installed_is_a_tool_error_the_model_can_read()
        {
            this.api
                .Setup(o => o.RemoveAsync("2026-09", false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("API version '2026-09' is not installed; the installed API versions are 2026-04"));

            Assert.That(
                () => this.tools.RemoveRelease("2026-09", axis: VersionAxis.Api),
                Throws.TypeOf<McpException>().With.Message.EqualTo("API version '2026-09' is not installed; the installed API versions are 2026-04"));
        }

        [Test]
        public async Task Use_on_the_api_axis_switches_the_default_API_version()
        {
            this.api.Setup(o => o.UseAsync("2026-04", It.IsAny<CancellationToken>())).ReturnsAsync(new ReleaseSelection("2026-04", "2025-02"));

            using var answer = JsonDocument.Parse(await this.tools.UseRelease("2026-04", VersionAxis.Api));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("default").GetString(), Is.EqualTo("2026-04"));
                Assert.That(answer.RootElement.GetProperty("previous").GetString(), Is.EqualTo("2025-02"));
                Assert.That(answer.RootElement.GetProperty("axis").GetString(), Is.EqualTo("api"));
                this.operations.VerifyNoOtherCalls();
            });
        }

        [Test]
        public void Use_of_an_API_version_that_is_not_installed_points_at_the_install_tool_on_the_api_axis()
        {
            this.api
                .Setup(o => o.UseAsync("2026-09", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("API version '2026-09' is not installed; the installed API versions are 2026-04"));

            Assert.That(
                () => this.tools.UseRelease("2026-09", VersionAxis.Api),
                Throws.TypeOf<McpException>().With.Message.EqualTo(
                    "API version '2026-09' is not installed; the installed API versions are 2026-04. Install it with hypha_install_release with axis 'api' first."));
        }

        [Test]
        public async Task Remove_on_the_api_axis_removes_the_API_version()
        {
            this.api.Setup(o => o.RemoveAsync("2025-02", false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            using var answer = JsonDocument.Parse(await this.tools.RemoveRelease("2025-02", axis: VersionAxis.Api));

            Assert.Multiple(() =>
            {
                Assert.That(answer.RootElement.GetProperty("removed").GetString(), Is.EqualTo("2025-02"));
                Assert.That(answer.RootElement.GetProperty("axis").GetString(), Is.EqualTo("api"));
                this.operations.VerifyNoOtherCalls();
            });
        }

        [Test]
        public async Task Every_answer_on_the_release_axis_says_so()
        {
            this.operations.Setup(o => o.RemoveAsync("2026-02", false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            using var answer = JsonDocument.Parse(await this.tools.RemoveRelease("2026-02"));

            Assert.That(answer.RootElement.GetProperty("axis").GetString(), Is.EqualTo("release"));
        }

        private static ApiSetup ApiSetup(KnowledgeBuild build, bool isDefault = false) =>
            new(
                new ApiInstallation(
                    "2026-04",
                    [new FileInfo("openapi.json"), new FileInfo("KerML.uml"), new FileInfo("pim.xmi")],
                    new InstalledApiVersion(
                        "2026-04",
                        new UpstreamReference(ApiUpstream.ServicesRepository, "abc"),
                        new UpstreamReference(Upstream.ReleaseRepository, "def"),
                        new UpstreamReference(Upstream.PilotRepository, "ghi"),
                        ApiUpstream.PimDocument),
                    new FileInfo("versions.json")),
                build,
                isDefault);

        private static ReleaseSetup Setup(KnowledgeBuild build, string tag = Tag, bool isDefault = false) =>
            new(
                new ReleaseInstallation(
                    tag,
                    [new FileInfo("KerML.uml")],
                    [new FileInfo("a.sysml"), new FileInfo("b.sysml")],
                    [],
                    new InstalledVersion(tag, new UpstreamReference(Upstream.ReleaseRepository, "abc"), new UpstreamReference(Upstream.PilotRepository, "def")),
                    new FileInfo("versions.json")),
                build,
                isDefault);
    }
}
