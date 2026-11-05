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
    using System.Collections.Generic;
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
        /// <summary>The scratch repository root the generator reads its specification PDFs from.</summary>
        private DirectoryInfo root = null!;

        /// <summary>The separate scratch output root the generator must write the extracted text under.</summary>
        private DirectoryInfo outputRoot = null!;

        /// <summary>
        /// The mocked layout, resolving specifications under <see cref="root"/> and knowledge under <see cref="outputRoot"/>.
        /// </summary>
        private Mock<IKnowledgeLayout> layout = null!;

        /// <summary>The mocked <c>uv</c> provisioner, so no test downloads or runs the real <c>uv</c>.</summary>
        private Mock<IUvProvisioner> uv = null!;

        /// <summary>The mocked process runner standing in for the <c>spec_extract</c> run.</summary>
        private Mock<IProcessRunner> processes = null!;

        /// <summary>The generator under test, composed from the mocks above.</summary>
        private SpecGenerator generator = null!;

        /// <summary>
        /// Creates distinct scratch input and output roots, wires the mocked layout to them and composes the generator
        /// under test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var unique = Guid.NewGuid().ToString("N");
            this.root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"hypha-spec-generator-{unique}-in"));
            this.root.Create();

            // Deliberately a *different* directory from root: SpecGenerator must honor `hypha
            // generate --output`, not always write into the repository it reads inputs from - this is
            // what caught the generator originally never passing --out-root at all.
            this.outputRoot = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"hypha-spec-generator-{unique}-out"));
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

        /// <summary>Deletes both scratch roots, whichever of them a test left behind.</summary>
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
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Skips_when_extraction_exits_non_zero()
        {
            this.SeedPdfs("2026-05");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.FakeUvExecutable());
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
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
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
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
                    It.IsAny<DirectoryInfo>(), It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // Simulates what the real `python -m spec_extract --out-root ...` process writes:
                    // under the *output* root, never the input root the PDFs were read from.
                    var kerml = new DirectoryInfo(Path.Combine(this.outputRoot.FullName, "knowledge", "2026-05", "spec", "kerml"));
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

        [Test]
        public async Task Runs_uv_with_the_environment_the_provisioner_gives_it()
        {
            this.SeedPdfs("2026-05");
            var uvExe = this.FakeUvExecutable();
            var environment = new Dictionary<string, string> { ["UV_CACHE_DIR"] = "cache" };
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(uvExe);
            this.uv.SetupGet(u => u.Environment).Returns(environment);
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(),
                    It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await this.generator.GenerateAsync("2026-05");

            this.processes.Verify(
                p => p.RunAsync(uvExe.FullName, It.IsAny<string>(), It.IsAny<DirectoryInfo>(), environment, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Writes stub KerML and SysML specification PDFs under the release's <c>sources/&lt;tag&gt;/specs/</c>, which is
        /// all the generator checks before it provisions <c>uv</c>.
        /// </summary>
        /// <param name="tag">The release to seed the PDFs for.</param>
        private void SeedPdfs(string tag)
        {
            var specs = new DirectoryInfo(Path.Combine(this.root.FullName, "sources", tag, "specs"));
            specs.Create();
            File.WriteAllText(Path.Combine(specs.FullName, "1-Kernel_Modeling_Language.pdf"), "stub");
            File.WriteAllText(Path.Combine(specs.FullName, "2a-OMG_Systems_Modeling_Language.pdf"), "stub");
        }

        /// <summary>Names a <c>uv</c> executable under the scratch root; it is never created nor run.</summary>
        /// <returns>The would-be <c>uv</c> executable the mocked provisioner hands back.</returns>
        private FileInfo FakeUvExecutable() => new(Path.Combine(this.root.FullName, "uv.exe"));
    }
}
