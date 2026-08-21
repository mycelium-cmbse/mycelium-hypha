// ------------------------------------------------------------------------------------------------
// <copyright file="StatusSummarizerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    /// <summary>
    /// Suite of tests for <see cref="StatusSummarizer"/>.
    /// </summary>
    [TestFixture]
    public class StatusSummarizerTests
    {
        private const string LogPath = "/tmp/sync.log";

        [Test]
        public void No_status_yet_says_nothing()
        {
            Assert.That(StatusSummarizer.Summarize(null, LogPath), Is.Null);
        }

        [Test]
        public void An_unrecognised_schema_version_says_nothing()
        {
            var status = new HookSyncStatus("9.9.9", "fetching", "2026-06", null, null, null);

            Assert.That(StatusSummarizer.Summarize(status, LogPath), Is.Null);
        }

        [TestCase("up-to-date")]
        [TestCase("done")]
        [TestCase("checking")]
        [TestCase("pruning")]
        public void Quiet_phases_say_nothing(string phase)
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, phase, "2026-06", null, null, null);

            Assert.That(StatusSummarizer.Summarize(status, LogPath), Is.Null);
        }

        [Test]
        public void Fetching_reports_the_kind_count_and_percentage()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "fetching", "2026-06",
                new HookFetchProgress("textual", 159, 318), null, null);

            var context = StatusSummarizer.Summarize(status, LogPath);

            Assert.That(context, Does.Contain("2026-06").And.Contains("textual").And.Contains("159/318")
                .And.Contains("50%"));
        }

        [Test]
        public void Fetching_with_no_progress_yet_says_nothing()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "fetching", "2026-06", null, null, null);

            Assert.That(StatusSummarizer.Summarize(status, LogPath), Is.Null);
        }

        [Test]
        public void Generating_reports_which_generator_and_its_position()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "generating", "2026-06", null,
                new HookGenerateProgress("cross-references", 4, 5), null);

            var context = StatusSummarizer.Summarize(status, LogPath);

            Assert.That(context, Does.Contain("2026-06").And.Contains("cross-references").And.Contains("4/5"));
        }

        [Test]
        public void Failed_points_at_the_log_and_includes_the_error_message()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "failed", "2026-06", null, null,
                new HookSyncError("network", "the upstream could not be reached"));

            var context = StatusSummarizer.Summarize(status, LogPath);

            Assert.That(
                context,
                Does.Contain("2026-06").And.Contains(LogPath).And.Contains("the upstream could not be reached"));
        }

        [Test]
        public void Failed_with_no_error_message_still_points_at_the_log()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "failed", "2026-06", null, null, null);

            Assert.That(StatusSummarizer.Summarize(status, LogPath), Does.Contain(LogPath));
        }

        [Test]
        public void An_unrecognised_phase_says_nothing_rather_than_guessing()
        {
            var status = new HookSyncStatus(
                HookSyncStatus.KnownSchemaVersion, "something-a-newer-cli-added", "2026-06", null, null, null);

            Assert.That(StatusSummarizer.Summarize(status, LogPath), Is.Null);
        }
    }
}
