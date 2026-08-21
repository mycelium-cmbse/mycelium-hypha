// ------------------------------------------------------------------------------------------------
// <copyright file="FileSyncLockTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;

    using Hypha.Tools.Sync;

    /// <summary>
    /// Suite of tests for <see cref="FileSyncLock"/>.
    /// </summary>
    [TestFixture]
    public class FileSyncLockTests
    {
        private FileInfo lockFile = null!;

        [SetUp]
        public void SetUp()
        {
            this.lockFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"hypha-lock-{Guid.NewGuid():N}.lock"));
        }

        [TearDown]
        public void TearDown()
        {
            this.lockFile.Refresh();
            if (this.lockFile.Exists)
            {
                this.lockFile.Delete();
            }
        }

        [Test]
        public void The_first_acquire_succeeds()
        {
            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void A_second_acquire_while_the_first_is_still_live_fails()
        {
            var first = new FileSyncLock(this.lockFile);
            var second = new FileSyncLock(this.lockFile);

            Assert.That(first.TryAcquire(), Is.True);
            Assert.That(second.TryAcquire(), Is.False, "this process is alive, so the lock is genuinely held");
        }

        [Test]
        public void Releasing_lets_a_later_acquire_succeed()
        {
            var first = new FileSyncLock(this.lockFile);
            first.TryAcquire();
            first.Release();

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void Release_without_ever_acquiring_does_not_throw()
        {
            Assert.That(() => new FileSyncLock(this.lockFile).Release(), Throws.Nothing);
        }

        [Test]
        public void A_lock_left_by_a_dead_process_is_reclaimed()
        {
            // A pid that (almost certainly) does not exist, with a fresh timestamp - the dead-process
            // check, not the age check, must be what reclaims this one.
            WriteLockContents(this.lockFile, pid: int.MaxValue - 1, startedAt: DateTimeOffset.UtcNow);

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void A_lock_older_than_the_staleness_ceiling_is_reclaimed_even_for_a_live_pid()
        {
            // This test process's own pid is definitely alive - only the age makes this stale.
            WriteLockContents(
                this.lockFile, pid: Environment.ProcessId, startedAt: DateTimeOffset.UtcNow.AddHours(-2));

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void A_recent_lock_for_a_live_pid_is_not_reclaimed()
        {
            WriteLockContents(
                this.lockFile, pid: Environment.ProcessId, startedAt: DateTimeOffset.UtcNow);

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.False);
        }

        [Test]
        public void Unreadable_lock_content_is_treated_as_stale_rather_than_jamming_the_lock_forever()
        {
            this.lockFile.Directory?.Create();
            File.WriteAllText(this.lockFile.FullName, "not json");

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void A_lock_file_containing_the_JSON_null_literal_is_treated_as_stale()
        {
            this.lockFile.Directory?.Create();
            File.WriteAllText(this.lockFile.FullName, "null");

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.True);
        }

        [Test]
        public void A_lock_file_that_cannot_be_read_at_all_is_not_treated_as_stale()
        {
            WriteLockContents(this.lockFile, pid: Environment.ProcessId, startedAt: DateTimeOffset.UtcNow);

            // Held exclusively so IsStale's own read fails with an IOException, distinct from "not
            // json" (a readable-but-meaningless file) - a real read failure must not be reclaimed.
            using var exclusive = new FileStream(
                this.lockFile.FullName, FileMode.Open, FileAccess.Read, FileShare.None);

            Assert.That(new FileSyncLock(this.lockFile).TryAcquire(), Is.False);
        }

        private static void WriteLockContents(FileInfo path, int pid, DateTimeOffset startedAt)
        {
            path.Directory?.Create();
            File.WriteAllText(
                path.FullName,
                JsonSerializer.Serialize(new { pid, startedAt }));
        }
    }
}
