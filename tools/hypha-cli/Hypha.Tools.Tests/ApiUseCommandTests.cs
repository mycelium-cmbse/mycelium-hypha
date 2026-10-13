// ------------------------------------------------------------------------------------------------
// <copyright file="ApiUseCommandTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands.Api;

    /// <summary>
    /// Suite of tests for <see cref="ApiUseCommand"/>.
    /// </summary>
    /// <remarks>
    /// Against a real <see cref="ApiLayout"/> over a temp directory, the same style
    /// <c>UseCommandTests</c> uses - switching the default is a direct manifest rewrite, not worth
    /// mocking behind an interface for this alone.
    /// </remarks>
    [TestFixture]
    public class ApiUseCommandTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout knowledge = null!;
        private ApiLayout layout = null!;
        private RootCommand root = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-api-use-{Guid.NewGuid():N}"));

            this.knowledge = new KnowledgeLayout(this.workspace);
            this.layout = new ApiLayout(this.knowledge);

            var api = new ApiCommand();
            api.Add(new ApiUseCommand());

            this.root = new RootCommand();
            this.root.Add(api);
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

        private void WriteManifest(string defaultTag, params string[] tags)
        {
            var versions = Array.ConvertAll(
                tags,
                tag => new InstalledApiVersion(
                    tag,
                    new UpstreamReference(ApiUpstream.ServicesRepository, "abc"),
                    new UpstreamReference(Upstream.ReleaseRepository, "def"),
                    new UpstreamReference(Upstream.PilotRepository, "ghi"),
                    ApiUpstream.PimDocument));

            ApiVersionManifestFile.Write(
                ApiVersionManifest.Build(defaultTag, versions), this.layout.VersionManifest.FullName);
        }

        [Test]
        public async Task Switches_the_default_to_an_installed_tag()
        {
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            using var console = new RecordedConsole();

            var result = await this.Invoke("api use --tag 2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2025-09"));
                Assert.That(
                    this.layout.InstalledTags, Is.EqualTo(new[] { "2026-04", "2025-09" }),
                    "switching the default installs and removes nothing");
                Assert.That(console.Output, Does.Contain("2025-09 is now the default API version"));
                Assert.That(console.Output, Does.Contain("was 2026-04"));
            });
        }

        [Test]
        public async Task Switching_to_the_current_default_is_a_no_op_success()
        {
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            var before = File.ReadAllBytes(this.layout.VersionManifest.FullName);
            var stamp = File.GetLastWriteTimeUtc(this.layout.VersionManifest.FullName);

            using var console = new RecordedConsole();

            var result = await this.Invoke("api use --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(0));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-04"));
                Assert.That(File.ReadAllBytes(this.layout.VersionManifest.FullName), Is.EqualTo(before));
                Assert.That(
                    File.GetLastWriteTimeUtc(this.layout.VersionManifest.FullName), Is.EqualTo(stamp),
                    "the manifest is not rewritten when nothing changes");
                Assert.That(console.Output, Does.Contain("already the default API version"));
            });
        }

        [Test]
        public async Task An_uninstalled_tag_is_a_usage_error()
        {
            this.WriteManifest("2026-04", "2026-04");

            using var console = new RecordedConsole();

            var result = await this.Invoke("api use --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("2026-05").And.Contains("hypha api fetch"));
                Assert.That(console.Output, Does.Contain("Installed: 2026-04"));
            });

            Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-04"), "an unknown tag must not change the default");
        }

        [Test]
        public async Task Nothing_installed_is_a_usage_error()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("api use --tag 2026-04");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("Nothing is installed"));
                Assert.That(console.Output, Does.Contain("hypha api fetch"));
                Assert.That(this.layout.VersionManifest.Exists, Is.False, "a refusal must not create a manifest");
            });
        }

        [Test]
        public async Task An_installed_language_release_is_not_an_installed_api_version()
        {
            // The two axes are switched independently: `hypha use` and `hypha api use` read different
            // manifests, and a release fetched for one is not installed for the other.
            VersionManifestFile.Write(
                VersionManifest.Build(
                    "2026-05",
                    [
                        new InstalledVersion(
                            "2026-05",
                            new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                            new UpstreamReference(Upstream.PilotRepository, "def")),
                    ]),
                this.knowledge.VersionManifest.FullName);

            this.WriteManifest("2026-04", "2026-04", "2025-09");

            var refused = await this.Invoke("api use --tag 2026-05");
            var switched = await this.Invoke("api use --tag 2025-09");

            Assert.Multiple(() =>
            {
                Assert.That(refused, Is.EqualTo(2));
                Assert.That(switched, Is.EqualTo(0));
                Assert.That(this.layout.DefaultTag, Is.EqualTo("2025-09"));
                Assert.That(
                    this.knowledge.DefaultTag, Is.EqualTo("2026-05"),
                    "the language releases' default is not this verb's to change");
            });
        }

        [Test]
        public void A_cancelled_run_does_not_touch_the_manifest()
        {
            this.WriteManifest("2026-04", "2026-04", "2025-09");

            using var source = new CancellationTokenSource();
            source.Cancel();

            var handler = new ApiUseCommand.Handler(this.layout);

            Assert.That(
                () => handler.InvokeAsync(this.root.Parse("api use --tag 2025-09"), source.Token),
                Throws.InstanceOf<OperationCanceledException>());

            Assert.That(this.layout.DefaultTag, Is.EqualTo("2026-04"));
        }

        [Test]
        public void The_tag_is_required()
        {
            Assert.That(this.root.Parse("api use").Errors, Is.Not.Empty);
        }

        private Task<int> Invoke(string commandLine)
        {
            var handler = new ApiUseCommand.Handler(this.layout);

            return handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }
    }
}
