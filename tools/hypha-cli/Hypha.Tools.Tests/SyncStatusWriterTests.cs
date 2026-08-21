// ------------------------------------------------------------------------------------------------
// <copyright file="SyncStatusWriterTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.IO;

    using Hypha.Tools.Sync;

    /// <summary>
    /// Suite of tests for <see cref="SyncStatusWriter"/>.
    /// </summary>
    [TestFixture]
    public class SyncStatusWriterTests
    {
        private FileInfo path = null!;
        private SyncStatusWriter writer = null!;

        [SetUp]
        public void SetUp()
        {
            this.path = new FileInfo(
                Path.Combine(Path.GetTempPath(), $"hypha-status-{Guid.NewGuid():N}", "sync-status.json"));
            this.writer = new SyncStatusWriter(this.path);
        }

        [TearDown]
        public void TearDown()
        {
            this.path.Directory?.Refresh();
            if (this.path.Directory?.Exists == true)
            {
                this.path.Directory.Delete(recursive: true);
            }
        }

        [Test]
        public void Loading_before_anything_was_written_returns_null()
        {
            Assert.That(this.writer.Load(), Is.Null);
        }

        [Test]
        public void What_is_written_round_trips_through_load()
        {
            var status = new SyncStatus
            {
                Phase = SyncPhase.Fetching,
                TargetTag = "2026-06",
                Fetch = new SyncFetchProgress("textual", 12, 300),
                CommittedBaselineTags = ["2026-05", "2026-04"],
                LocallyAddedTag = "2026-05a",
            };

            this.writer.Write(status);
            var loaded = this.writer.Load();

            Assert.Multiple(() =>
            {
                Assert.That(loaded!.Phase, Is.EqualTo(SyncPhase.Fetching));
                Assert.That(loaded.TargetTag, Is.EqualTo("2026-06"));
                Assert.That(loaded.Fetch, Is.EqualTo(new SyncFetchProgress("textual", 12, 300)));
                Assert.That(loaded.CommittedBaselineTags, Is.EqualTo(new[] { "2026-05", "2026-04" }));
                Assert.That(loaded.LocallyAddedTag, Is.EqualTo("2026-05a"));
            });
        }

        [Test]
        public void A_later_write_leaves_no_leftover_temporary_file()
        {
            this.writer.Write(new SyncStatus { Phase = SyncPhase.Checking });
            this.writer.Write(new SyncStatus { Phase = SyncPhase.Done });

            Assert.That(File.Exists(this.path.FullName + ".tmp"), Is.False);
        }

        [Test]
        public void Writing_creates_the_directory_it_needs()
        {
            Assert.That(this.path.Directory!.Exists, Is.False);

            this.writer.Write(new SyncStatus { Phase = SyncPhase.Checking });

            Assert.That(File.Exists(this.path.FullName), Is.True);
        }
    }
}
