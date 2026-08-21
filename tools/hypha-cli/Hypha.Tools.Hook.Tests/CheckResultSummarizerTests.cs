// ------------------------------------------------------------------------------------------------
// <copyright file="CheckResultSummarizerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    using System;
    using System.IO;

    /// <summary>
    /// Suite of tests for <see cref="CheckResultSummarizer"/>.
    /// </summary>
    [TestFixture]
    public class CheckResultSummarizerTests
    {
        private DirectoryInfo pluginRoot = null!;

        [SetUp]
        public void SetUp()
        {
            this.pluginRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"hypha-summarizer-{Guid.NewGuid():N}"));
        }

        [TearDown]
        public void TearDown()
        {
            this.pluginRoot.Refresh();
            if (this.pluginRoot.Exists)
            {
                this.pluginRoot.Delete(recursive: true);
            }
        }

        [Test]
        public void No_result_at_all_says_nothing()
        {
            Assert.That(CheckResultSummarizer.Summarize(null, this.pluginRoot), Is.Null);
        }

        [Test]
        public void Nothing_local_and_nothing_online_says_nothing()
        {
            var result = new HookCheckResult([], null, []);

            Assert.That(CheckResultSummarizer.Summarize(result, this.pluginRoot), Is.Null);
        }

        [Test]
        public void Nothing_local_names_what_is_available_online()
        {
            var result = new HookCheckResult([], null, ["2026-06", "2026-05", "2026-04"]);

            var context = CheckResultSummarizer.Summarize(result, this.pluginRoot);

            Assert.That(
                context,
                Does.Contain("2026-06").And.Contains("2026-05").And.Contains("2026-04")
                    .And.Contains("Ask which release"));
        }

        [Test]
        public void A_newer_release_online_is_named_and_offered()
        {
            var result = new HookCheckResult(["2026-05", "2026-04"], "2026-05", ["2026-06", "2026-05"]);

            var context = CheckResultSummarizer.Summarize(result, this.pluginRoot);

            Assert.That(
                context,
                Does.Contain("2026-06").And.Contains("2026-05").And.Contains("2026-04")
                    .And.Contains("hypha fetch --tag 2026-06"));
        }

        [Test]
        public void Already_on_the_newest_with_specs_present_says_nothing()
        {
            SeedSpecs("2026-05", allPresent: true);

            var result = new HookCheckResult(["2026-05"], "2026-05", ["2026-05", "2026-04"]);

            Assert.That(CheckResultSummarizer.Summarize(result, this.pluginRoot), Is.Null);
        }

        [Test]
        public void Already_on_the_newest_with_specs_missing_names_them()
        {
            // No PDFs seeded at all - sources/2026-05/specs/ does not even exist.
            var result = new HookCheckResult(["2026-05"], "2026-05", ["2026-05", "2026-04"]);

            var context = CheckResultSummarizer.Summarize(result, this.pluginRoot);

            Assert.That(
                context,
                Does.Contain("2026-05").And.Contains("1-Kernel_Modeling_Language.pdf")
                    .And.Contains("sysml-validation still work").And.Contains("tools/spec-extract"));
        }

        [Test]
        public void Already_on_the_newest_with_only_some_specs_missing_still_reports_it()
        {
            SeedSpecs("2026-05", allPresent: false);

            var result = new HookCheckResult(["2026-05"], "2026-05", ["2026-05"]);

            var context = CheckResultSummarizer.Summarize(result, this.pluginRoot);

            Assert.That(context, Does.Contain("3-Systems_Modeling_API_and_Services.pdf"));
        }

        [Test]
        public void No_default_tag_and_up_to_date_says_nothing()
        {
            // Defensive: DefaultTag is only ever null when nothing is installed, which the
            // nothing-local branch already handles - covered here in case that assumption ever slips.
            var result = new HookCheckResult(["2026-05"], null, ["2026-05"]);

            Assert.That(CheckResultSummarizer.Summarize(result, this.pluginRoot), Is.Null);
        }

        private void SeedSpecs(string tag, bool allPresent)
        {
            var specsDirectory = Directory.CreateDirectory(
                Path.Combine(this.pluginRoot.FullName, "sources", tag, "specs"));

            File.WriteAllText(Path.Combine(specsDirectory.FullName, "1-Kernel_Modeling_Language.pdf"), "%PDF");
            File.WriteAllText(
                Path.Combine(specsDirectory.FullName, "2a-OMG_Systems_Modeling_Language.pdf"), "%PDF");

            if (allPresent)
            {
                File.WriteAllText(
                    Path.Combine(specsDirectory.FullName, "3-Systems_Modeling_API_and_Services.pdf"), "%PDF");
            }
        }
    }
}
