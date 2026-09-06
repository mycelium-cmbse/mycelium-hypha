// ------------------------------------------------------------------------------------------------
// <copyright file="SpecGeneratorTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Tests for <see cref="SpecGenerator"/> against mocked <see cref="IKnowledgeLayout"/>,
    /// <see cref="IUvProvisioner"/> and <see cref="IProcessRunner"/> - no network, no real <c>uv</c>.
    /// </summary>
    [TestFixture]
    public class SpecGeneratorTests
    {
        private DirectoryInfo root = null!;
        private DirectoryInfo outputRoot = null!;
        private Mock<IKnowledgeLayout> layout = null!;
        private Mock<IUvProvisioner> uv = null!;
        private Mock<IProcessRunner> processes = null!;
        private SpecGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            var unique = Guid.NewGuid().ToString("N");
            this.root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"hypha-spec-generator-{unique}-in"));
            this.root.Create();

            // Deliberately a *different* directory from root: SpecGenerator must honor `hypha
            // generate --output`, not always write into the repository it reads inputs from - this is
            // what caught the generator originally never passing --out-root at all.
            this.outputRoot = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), $"hypha-spec-generator-{unique}-out"));
            this.outputRoot.Create();

            this.layout = new Mock<IKnowledgeLayout>();
            this.layout.SetupGet(l => l.Root).Returns(this.root);
            this.layout.SetupGet(l => l.OutputRoot).Returns(this.outputRoot);
            this.layout.Setup(l => l.Specifications(It.IsAny<string>())).Returns(
                (string tag) => new DirectoryInfo(Path.Combine(this.root.FullName, "sources", tag, "specs")));
            this.layout.Setup(l => l.Knowledge(It.IsAny<string>())).Returns(
                (string tag) => new DirectoryInfo(Path.Combine(this.outputRoot.FullName, "knowledge", tag)));

            this.uv = new Mock<IUvProvisioner>();
            this.processes = new Mock<IProcessRunner>();

            this.generator = new SpecGenerator(
                this.layout.Object, this.uv.Object, this.processes.Object, NullLogger<SpecGenerator>.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var directory in new[] { this.root, this.outputRoot })
            {
                directory.Refresh();
                if (directory.Exists)
                {
                    directory.Delete(recursive: true);
                }
            }
        }

        [Test]
        public async Task Skips_when_the_omg_pdfs_are_not_present()
        {
            var result = await this.generator.GenerateAsync("2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("2026-05"));
            });
            this.uv.Verify(u => u.EnsureAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Skips_when_uv_cannot_be_provisioned()
        {
            this.SeedPdfs("2026-05");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync((FileInfo?)null);

            var result = await this.generator.GenerateAsync("2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("uv"));
            });
            this.processes.Verify(
                p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Skips_when_extraction_exits_non_zero()
        {
            this.SeedPdfs("2026-05");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.FakeUvExecutable());
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await this.generator.GenerateAsync("2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("1"));
            });
        }

        [Test]
        public async Task Skips_when_extraction_reports_success_but_writes_nothing()
        {
            this.SeedPdfs("2026-05");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.FakeUvExecutable());
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await this.generator.GenerateAsync("2026-05");

            Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
        }

        [Test]
        public async Task Reports_what_extraction_wrote_on_success()
        {
            this.SeedPdfs("2026-05");
            var uvExe = this.FakeUvExecutable();
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(uvExe);
            this.processes
                .Setup(p => p.RunAsync(
                    It.Is<string>(f => f == uvExe.FullName),
                    It.Is<string>(a => a.Contains("spec_extract", StringComparison.Ordinal)
                        && a.Contains("2026-05", StringComparison.Ordinal)
                        && a.Contains($"--out-root \"{this.outputRoot.FullName}\"", StringComparison.Ordinal)),
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // Simulates what the real `python -m spec_extract --out-root ...` process writes:
                    // under the *output* root, never the input root the PDFs were read from.
                    var kerml = new DirectoryInfo(
                        Path.Combine(this.outputRoot.FullName, "knowledge", "2026-05", "spec", "kerml"));
                    kerml.Create();
                    File.WriteAllText(Path.Combine(kerml.FullName, "01-scope.md"), "---\n---\n");
                })
                .ReturnsAsync(0);

            var result = await this.generator.GenerateAsync("2026-05");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(result.Written, Has.Count.EqualTo(1));
                Assert.That(result.Written[0].Name, Is.EqualTo("01-scope.md"));
            });
        }

        private void SeedPdfs(string tag)
        {
            var specs = new DirectoryInfo(Path.Combine(this.root.FullName, "sources", tag, "specs"));
            specs.Create();
            File.WriteAllText(Path.Combine(specs.FullName, "1-Kernel_Modeling_Language.pdf"), "stub");
            File.WriteAllText(Path.Combine(specs.FullName, "2a-OMG_Systems_Modeling_Language.pdf"), "stub");
        }

        private FileInfo FakeUvExecutable() => new(Path.Combine(this.root.FullName, "uv.exe"));
    }
}
