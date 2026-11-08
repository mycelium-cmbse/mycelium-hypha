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
                    Has.All.Matches<FileInfo>(file => file is not null && File.ReadAllText(file.FullName).StartsWith("---\n")),
                    "every clause file should open with YAML front matter");
            });
        }
    }
}
