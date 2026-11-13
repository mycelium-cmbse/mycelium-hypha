// ------------------------------------------------------------------------------------------------
// <copyright file="ProgressForwarder.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Channels;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;

    using ModelContextProtocol;

    /// <summary>
    /// Sends the progress of a fetch and a generation to the client as MCP progress notifications, one
    /// after the other.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The protocol wants one number that only grows over the whole call, while a fetch and a generation
    /// report each step's units on their own (<see cref="KnowledgeProgress.Done"/> of
    /// <see cref="KnowledgeProgress.Total"/>, step after step). So the notification's progress is every
    /// unit finished so far, over all steps, and its message says which step and unit it was. No total
    /// is sent: the later steps' units are not known until they start.
    /// </para>
    /// <para>
    /// The fetch reports from several downloads at once, so reports may arrive together and out of
    /// order; one that would not move a step forward is dropped rather than sent backwards, and only the
    /// first <see cref="Start"/> is sent, at zero. The notifications are queued in the order numbered and
    /// sent one at a time, each once the one before it has gone: sent side by side, a later one could
    /// overtake an earlier one on the way to the client. <see cref="DisposeAsync"/> waits for the queue,
    /// so the last notification goes before the tool's answer.
    /// </para>
    /// </remarks>
    internal sealed class ProgressForwarder : IProgress<KnowledgeProgress>, IAsyncDisposable
    {
        /// <summary>Guards <see cref="done"/>, <see cref="finished"/> and <see cref="started"/>, and the order of the queue.</summary>
        private readonly object gate = new();

        /// <summary>The units finished so far in each step, by operation and step.</summary>
        private readonly Dictionary<(string Operation, string Step), int> done = [];

        /// <summary>The notifications not sent yet, in the order numbered; <c>null</c> when nothing is sent.</summary>
        private readonly Channel<ProgressNotificationValue>? queue;

        /// <summary>Sends the queue, one notification at a time; completed at once when nothing is sent.</summary>
        private readonly Task sending;

        /// <summary>Every unit finished so far, over all steps: the progress sent.</summary>
        private int finished;

        /// <summary>Whether anything has been numbered yet.</summary>
        private bool started;

        /// <summary>Initializes a new instance of the <see cref="ProgressForwarder"/> class.</summary>
        /// <param name="send">
        /// Sends one notification to the client; <c>null</c> when the client asked for no progress, and
        /// nothing is sent. A failure to send is ignored, since progress is only ever a courtesy.
        /// </param>
        public ProgressForwarder(Func<ProgressNotificationValue, Task>? send)
        {
            if (send is null)
            {
                this.sending = Task.CompletedTask;

                return;
            }

            this.queue = Channel.CreateUnbounded<ProgressNotificationValue>(new UnboundedChannelOptions { SingleReader = true });
            this.sending = SendAsync(this.queue.Reader, send);
        }

        /// <summary>
        /// Says what the call is doing before any unit has finished, such as waiting for the lock; ignored
        /// once anything has been sent, since progress may not stand still.
        /// </summary>
        /// <param name="message">What it is doing, for a person.</param>
        public void Start(string message)
        {
            lock (this.gate)
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;
                this.queue?.Writer.TryWrite(new ProgressNotificationValue { Progress = 0, Message = message });
            }
        }

        /// <summary>Sends one finished unit of a step.</summary>
        /// <param name="value">The step, and how far it has come.</param>
        public void Report(KnowledgeProgress value)
        {
            lock (this.gate)
            {
                var step = (value.Operation, value.Step);
                var before = this.done.GetValueOrDefault(step);

                if (value.Done <= before)
                {
                    return;
                }

                this.done[step] = value.Done;
                this.finished += value.Done - before;
                this.started = true;

                this.queue?.Writer.TryWrite(new ProgressNotificationValue
                {
                    Progress = this.finished,
                    Message = $"{value.Operation} {value.Step}: {value.Message}",
                });
            }
        }

        /// <summary>Stops taking reports and waits until every queued notification has been sent.</summary>
        /// <returns>A task that completes once the queue is empty.</returns>
        public async ValueTask DisposeAsync()
        {
            this.queue?.Writer.TryComplete();

            await this.sending;
        }

        /// <summary>Sends the queued notifications one at a time, until the queue is completed.</summary>
        /// <param name="queue">The notifications, in the order numbered.</param>
        /// <param name="send">Sends one notification.</param>
        /// <returns>A task that completes once the queue is completed and empty.</returns>
        private static async Task SendAsync(ChannelReader<ProgressNotificationValue> queue, Func<ProgressNotificationValue, Task> send)
        {
            await foreach (var value in queue.ReadAllAsync())
            {
                try
                {
                    await send(value);
                }
                catch (Exception)
                {
                    // The client may have gone, or cancelled the call; the call itself decides what that
                    // means, and progress it cannot see is worth nothing.
                }
            }
        }
    }
}
