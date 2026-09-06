// ------------------------------------------------------------------------------------------------
// <copyright file="SpecGeneratorLiveTests.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Threading.Tasks;

    using Hypha.Knowledge;
    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A real end-to-end run of <see cref="SpecGenerator"/>: downloads and caches the real <c>uv</c>,
    /// which resolves/fetches a real Python and installs <c>pdfplumber</c> into a managed venv, then
    /// runs real extraction against the OMG PDFs already present locally. <b>Explicit</b> - never part
    /// of a normal run or CI.
    /// </summary>
    /// <remarks>
    /// No mock proves the actual point of this feature: that an installed plugin, with nothing but the
    /// OMG PDFs already fetched, can produce the same clause text the maintainer <c>pytest</c> flow
    /// does, without a provisioned <c>.venv</c>. Run by hand:
    /// <code>
    /// dotnet test --filter "FullyQualifiedName~SpecGeneratorLiveTests"
    /// </code>
    /// Output is redirected to a scratch folder (never <c>knowledge/</c> itself), so a run never
    /// replaces whatever a maintainer's own Python flow already generated there.
    /// </remarks>
    [TestFixture]
    [Explicit("Downloads and runs uv for real, and shells out to a real Python; run by hand.")]
    [Category("Live")]
    public class SpecGeneratorLiveTests
    {
        private const string Tag = "2026-05";

        private DirectoryInfo workspace = null!;
        private ServiceProvider provider = null!;

        [SetUp]
        public void SetUp()
        {
            this.workspace = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-spec-live-{Guid.NewGuid():N}"));

            this.provider = new ServiceCollection()
                .AddHyphaKnowledge(options => options.OutputRoot = this.workspace)
                .BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            this.provider?.Dispose();

            this.workspace.Refresh();
            if (this.workspace.Exists)
            {
                this.workspace.Delete(recursive: true);
            }
        }

        [Test]
        public async Task Extracts_the_real_kerml_and_sysml_specs()
        {
            var layout = this.provider.GetRequiredService<IKnowledgeLayout>();
            var specs = layout.Specifications(Tag);

            if (!File.Exists(Path.Combine(specs.FullName, "1-Kernel_Modeling_Language.pdf"))
                || !File.Exists(Path.Combine(specs.FullName, "2a-OMG_Systems_Modeling_Language.pdf")))
            {
                Assert.Ignore(
                    $"OMG spec PDFs not present locally for {Tag} (git-ignored); "
                    + $"run 'hypha fetch --tag {Tag}' first.");

                return;
            }

            var generator = this.provider.GetServices<IKnowledgeGenerator>().Single(g => g.Artifact == "spec");

            var result = await generator.GenerateAsync(Tag);

            await TestContext.Out.WriteLineAsync(
                result.Outcome == GenerationOutcome.Skipped
                    ? $"skipped: {result.Reason}"
                    : $"wrote {result.Written.Count} files");

            Assert.Multiple(() =>
            {
                Assert.That(result.Outcome, Is.EqualTo(GenerationOutcome.Generated));
                Assert.That(
                    result.Written, Has.Count.GreaterThan(500),
                    "KerML + SysML together carry hundreds of clauses");
                Assert.That(
                    result.Written.Where(file => file.Extension == ".md"),
                    Has.All.Matches<FileInfo>(file => File.ReadAllText(file.FullName).StartsWith("---\n")),
                    "every clause file should open with YAML front matter");
            });
        }
    }
}
