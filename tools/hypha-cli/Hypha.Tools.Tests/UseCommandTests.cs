// ------------------------------------------------------------------------------------------------
// <copyright file="UseCommandTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Commands;

    /// <summary>
    /// Suite of tests for <see cref="UseCommand"/>.
    /// </summary>
    /// <remarks>
    /// Against a real <see cref="KnowledgeLayout"/> over a temp directory, the same style
    /// <c>ReleaseWindowEvictorTests</c>/<c>ReleaseRemoverTests</c> use - switching the default is a
    /// direct manifest rewrite, not worth mocking behind an interface for this alone.
    /// </remarks>
    [TestFixture]
    public class UseCommandTests
    {
        private DirectoryInfo workspace = null!;
        private KnowledgeLayout layout = null!;
        private RootCommand root = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-use-{Guid.NewGuid():N}"));

            this.layout = new KnowledgeLayout(this.workspace);

            this.root = new RootCommand();
            this.root.Add(new UseCommand());
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
                tag => new InstalledVersion(
                    tag,
                    new UpstreamReference(Upstream.ReleaseRepository, "abc"),
                    new UpstreamReference(Upstream.PilotRepository, "def")));

            VersionManifestFile.Write(
                VersionManifest.Build(defaultTag, versions), this.layout.VersionManifest.FullName);
        }

        [Test]
        public async Task Switches_the_default_to_an_installed_tag()
        {
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            var result = await this.Invoke("use --tag 2026-04");

            Assert.That(result, Is.EqualTo(0));

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.Default, Is.EqualTo("2026-04"));
        }

        [Test]
        public async Task Switching_to_the_current_default_is_a_no_op_success()
        {
            this.WriteManifest("2026-05", "2026-05", "2026-04");

            var result = await this.Invoke("use --tag 2026-05");

            Assert.That(result, Is.EqualTo(0));

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.Default, Is.EqualTo("2026-05"));
        }

        [Test]
        public async Task An_uninstalled_tag_is_a_usage_error()
        {
            this.WriteManifest("2026-05", "2026-05");

            using var console = new RecordedConsole();

            var result = await this.Invoke("use --tag 2026-06");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("2026-06").And.Contains("hypha fetch"));
            });

            var manifest = VersionManifestFile.Read(this.layout.VersionManifest.FullName);
            Assert.That(manifest.Default, Is.EqualTo("2026-05"), "an unknown tag must not change the default");
        }

        [Test]
        public async Task Nothing_installed_is_a_usage_error()
        {
            using var console = new RecordedConsole();

            var result = await this.Invoke("use --tag 2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(2));
                Assert.That(console.Output, Does.Contain("Nothing is installed"));
            });
        }

        private Task<int> Invoke(string commandLine)
        {
            var handler = new UseCommand.Handler(this.layout);

            return handler.InvokeAsync(this.root.Parse(commandLine), CancellationToken.None);
        }
    }
}
