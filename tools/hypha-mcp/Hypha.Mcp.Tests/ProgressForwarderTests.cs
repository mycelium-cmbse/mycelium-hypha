// ------------------------------------------------------------------------------------------------
// <copyright file="ProgressForwarderTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;
    using Hypha.Mcp.Tools;

    using ModelContextProtocol;

    [TestFixture]
    public class ProgressForwarderTests
    {
        private ConcurrentQueue<ProgressNotificationValue> sent = null!;

        [SetUp]
        public void SetUp() => this.sent = new ConcurrentQueue<ProgressNotificationValue>();

        [Test]
        public async Task Progress_counts_every_unit_finished_over_all_steps()
        {
            await using (var forwarder = new ProgressForwarder(this.Send))
            {
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 2, "1 of 2 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 2, 2, "2 of 2 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 3, 300, "3 of 300 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Generate, "2026-05", 1, 6, "metamodel: 120 files"));
            }

            var values = this.sent.ToList();
            Assert.Multiple(() =>
            {
                Assert.That(values.Select(value => value.Progress), Is.EqualTo(new float[] { 1, 2, 5, 6 }));
                Assert.That(values.Select(value => value.Total), Is.All.Null);
                Assert.That(values[^1].Message, Is.EqualTo("generate 2026-05: metamodel: 120 files"));
            });
        }

        [Test]
        public async Task A_report_that_would_not_move_its_step_forward_is_not_sent()
        {
            await using (var forwarder = new ProgressForwarder(this.Send))
            {
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 5, 10, "5 of 10 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 4, 10, "4 of 10 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 5, 10, "5 of 10 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", 6, 10, "6 of 10 files"));
            }

            Assert.That(this.sent.Select(value => value.Progress), Is.EqualTo(new float[] { 5, 6 }));
        }

        [Test]
        public async Task Only_the_first_start_is_sent_and_only_before_any_unit()
        {
            await using (var forwarder = new ProgressForwarder(this.Send))
            {
                forwarder.Start("waiting for the lock");
                forwarder.Start("again");
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 2, "1 of 2 files"));
                forwarder.Start("too late");
            }

            Assert.Multiple(() =>
            {
                Assert.That(this.sent.Select(value => value.Message), Is.EqualTo(new[] { "waiting for the lock", "fetch metamodel: 1 of 2 files" }));
                Assert.That(this.sent.Select(value => value.Progress), Is.EqualTo(new float[] { 0, 1 }));
            });
        }

        [Test]
        public async Task Notifications_are_sent_one_at_a_time_in_order_even_from_several_downloads_at_once()
        {
            var sending = 0;
            var overlapped = false;

            async Task SlowSend(ProgressNotificationValue value)
            {
                if (Interlocked.Increment(ref sending) > 1)
                {
                    overlapped = true;
                }

                await Task.Yield();
                this.sent.Enqueue(value);
                Interlocked.Decrement(ref sending);
            }

            await using (var forwarder = new ProgressForwarder(SlowSend))
            {
                Parallel.For(1, 201, done => forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "textual", done, 200, $"{done} of 200 files")));
            }

            var progress = this.sent.Select(value => value.Progress).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(overlapped, Is.False, "a notification was sent before the one before it had gone");
                Assert.That(progress, Is.Ordered.Ascending);
                Assert.That(progress.Distinct().Count(), Is.EqualTo(progress.Count));
                Assert.That(progress[^1], Is.EqualTo(200));
            });
        }

        [Test]
        public async Task Disposing_waits_until_the_last_notification_has_gone()
        {
            var release = new TaskCompletionSource();

            var forwarder = new ProgressForwarder(async value =>
            {
                await release.Task;
                this.sent.Enqueue(value);
            });
            forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 1, "1 of 1 files"));

            var disposing = forwarder.DisposeAsync().AsTask();
            await Task.Delay(50);
            var waited = !disposing.IsCompleted;
            release.SetResult();
            await disposing;

            Assert.Multiple(() =>
            {
                Assert.That(waited, Is.True);
                Assert.That(this.sent, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public async Task A_notification_that_cannot_be_sent_does_not_stop_the_others_or_the_call()
        {
            var calls = 0;

            await using (var forwarder = new ProgressForwarder(value =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                {
                    throw new InvalidOperationException("the client went away");
                }

                this.sent.Enqueue(value);
                return Task.CompletedTask;
            }))
            {
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 2, "1 of 2 files"));
                forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 2, 2, "2 of 2 files"));
            }

            Assert.That(this.sent.Select(value => value.Progress), Is.EqualTo(new float[] { 2 }));
        }

        [Test]
        public void Without_a_client_progress_nothing_is_sent_and_nothing_fails()
        {
            Assert.That(
                async () =>
                {
                    await using var forwarder = new ProgressForwarder(null);
                    forwarder.Start("waiting");
                    forwarder.Report(new KnowledgeProgress(KnowledgeProgress.Fetch, "metamodel", 1, 1, "1 of 1 files"));
                },
                Throws.Nothing);
        }

        private Task Send(ProgressNotificationValue value)
        {
            this.sent.Enqueue(value);
            return Task.CompletedTask;
        }
    }
}
