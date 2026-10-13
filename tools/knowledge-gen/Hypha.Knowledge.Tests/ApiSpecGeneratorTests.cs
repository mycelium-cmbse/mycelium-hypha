// ------------------------------------------------------------------------------------------------
// <copyright file="ApiSpecGeneratorTests.cs" company="Starion Group S.A.">
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

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Toolchain;

    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    /// <summary>
    /// Tests for <see cref="ApiSpecGenerator"/> against mocked <see cref="IApiLayout"/>,
    /// <see cref="IKnowledgeLayout"/>, <see cref="IUvProvisioner"/> and <see cref="IProcessRunner"/> -
    /// no network, no real <c>uv</c>.
    /// </summary>
    [TestFixture]
    public class ApiSpecGeneratorTests
    {
        private DirectoryInfo root = null!;
        private DirectoryInfo outputRoot = null!;
        private Mock<IApiLayout> layout = null!;
        private Mock<IKnowledgeLayout> repository = null!;
        private Mock<IUvProvisioner> uv = null!;
        private Mock<IProcessRunner> processes = null!;
        private ApiSpecGenerator generator = null!;

        [SetUp]
        public void SetUp()
        {
            var unique = Guid.NewGuid().ToString("N");
            this.root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"hypha-api-spec-generator-{unique}-in"));
            this.root.Create();

            // Deliberately a *different* directory from root: the generator must honor `--output`, not
            // always write into the repository it reads inputs from.
            this.outputRoot = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), $"hypha-api-spec-generator-{unique}-out"));
            this.outputRoot.Create();

            this.repository = new Mock<IKnowledgeLayout>();
            this.repository.SetupGet(l => l.Root).Returns(this.root);
            this.repository.SetupGet(l => l.OutputRoot).Returns(this.outputRoot);

            this.layout = new Mock<IApiLayout>();
            this.layout.Setup(l => l.Specifications(It.IsAny<string>())).Returns(
                (string tag) => new DirectoryInfo(Path.Combine(this.root.FullName, "sources", "api", tag, "specs")));
            this.layout.Setup(l => l.SpecificationText(It.IsAny<string>())).Returns(
                (string tag) => new DirectoryInfo(
                    Path.Combine(this.outputRoot.FullName, "knowledge", "api", tag, "spec")));

            this.uv = new Mock<IUvProvisioner>();
            this.processes = new Mock<IProcessRunner>();

            this.generator = new ApiSpecGenerator(
                this.layout.Object,
                this.repository.Object,
                this.uv.Object,
                this.processes.Object,
                NullLogger<ApiSpecGenerator>.Instance);
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
        public void Describes_itself_as_the_spec_artifact()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.generator.Artifact, Is.EqualTo("spec"));
                Assert.That(this.generator.Order, Is.EqualTo(30));
            });
        }

        [Test]
        public async Task Skips_when_the_omg_pdf_is_not_present()
        {
            var result = await this.generator.GenerateAsync("1.0");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("1.0"));
                Assert.That(result.Reason, Does.Contain("hypha api fetch"));
            });
            this.uv.Verify(u => u.EnsureAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Skips_when_only_another_pdf_is_present()
        {
            // The API specification is one named file; a language specification beside it is not it.
            var specs = new DirectoryInfo(Path.Combine(this.root.FullName, "sources", "api", "1.0", "specs"));
            specs.Create();
            File.WriteAllText(Path.Combine(specs.FullName, "1-Kernel_Modeling_Language.pdf"), "stub");

            var result = await this.generator.GenerateAsync("1.0");

            Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
            this.uv.Verify(u => u.EnsureAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Skips_when_uv_cannot_be_provisioned()
        {
            this.SeedPdf("1.0");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync((FileInfo?)null);

            var result = await this.generator.GenerateAsync("1.0");

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
            this.SeedPdf("1.0");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.FakeUvExecutable());
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            var result = await this.generator.GenerateAsync("1.0");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("code 3"));
            });
        }

        [Test]
        public async Task Skips_when_extraction_reports_success_but_writes_nothing()
        {
            this.SeedPdf("1.0");
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.FakeUvExecutable());
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await this.generator.GenerateAsync("1.0");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Skipped));
                Assert.That(result.Reason, Does.Contain("wrote nothing"));
            });
        }

        [Test]
        public async Task Runs_spec_extract_for_the_api_version_and_never_for_a_language_release()
        {
            this.SeedPdf("1.0");
            var uvExe = this.FakeUvExecutable();
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(uvExe);

            string? fileName = null;
            string? arguments = null;
            DirectoryInfo? workingDirectory = null;
            this.processes
                .Setup(p => p.RunAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryInfo>(), It.IsAny<CancellationToken>()))
                .Callback((string file, string args, DirectoryInfo directory, CancellationToken _) =>
                {
                    fileName = file;
                    arguments = args;
                    workingDirectory = directory;
                })
                .ReturnsAsync(0);

            await this.generator.GenerateAsync("1.0");

            Assert.Multiple(() =>
            {
                Assert.That(fileName, Is.EqualTo(uvExe.FullName));
                Assert.That(arguments, Does.Contain("python -m spec_extract"));
                Assert.That(arguments, Does.Contain("--api-version 1.0"));

                // `--tag` selects a language release: passing it here would extract the wrong PDFs.
                Assert.That(arguments, Does.Not.Contain("--tag"));
                Assert.That(arguments, Does.Contain($"--repo-root \"{this.root.FullName}\""));
                Assert.That(arguments, Does.Contain($"--out-root \"{this.outputRoot.FullName}\""));
                Assert.That(
                    arguments,
                    Does.Contain($"--project \"{Path.Combine(this.root.FullName, "tools", "spec-extract")}\""));
                Assert.That(workingDirectory!.FullName, Is.EqualTo(this.root.FullName));
            });
        }

        [Test]
        public async Task Reports_what_extraction_wrote_on_success()
        {
            this.SeedPdf("1.0");
            var uvExe = this.FakeUvExecutable();
            this.uv.Setup(u => u.EnsureAsync(It.IsAny<CancellationToken>())).ReturnsAsync(uvExe);
            this.processes
                .Setup(p => p.RunAsync(
                    It.Is<string>(f => f == uvExe.FullName),
                    It.Is<string>(a => a.Contains("spec_extract", StringComparison.Ordinal)
                        && a.Contains("--api-version 1.0", StringComparison.Ordinal)
                        && a.Contains($"--out-root \"{this.outputRoot.FullName}\"", StringComparison.Ordinal)),
                    It.IsAny<DirectoryInfo>(),
                    It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // Simulates what the real `python -m spec_extract --out-root ...` process writes:
                    // under the *output* root, never the input root the PDF was read from.
                    var spec = new DirectoryInfo(
                        Path.Combine(this.outputRoot.FullName, "knowledge", "api", "1.0", "spec"));
                    spec.Create();
                    File.WriteAllText(Path.Combine(spec.FullName, "index.json"), "{}\n");
                    File.WriteAllText(Path.Combine(spec.FullName, "01-scope.md"), "---\n---\n");
                })
                .ReturnsAsync(0);

            var result = await this.generator.GenerateAsync("1.0");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(result.Written, Has.Count.EqualTo(2));
                Assert.That(
                    result.Written.Select(file => file.Name), Is.EquivalentTo(new[] { "01-scope.md", "index.json" }));
            });
        }

        private void SeedPdf(string tag)
        {
            var specs = new DirectoryInfo(Path.Combine(this.root.FullName, "sources", "api", tag, "specs"));
            specs.Create();
            File.WriteAllText(Path.Combine(specs.FullName, ApiInputs.Specification), "stub");
        }

        private FileInfo FakeUvExecutable() => new(Path.Combine(this.root.FullName, "uv.exe"));
    }
}
