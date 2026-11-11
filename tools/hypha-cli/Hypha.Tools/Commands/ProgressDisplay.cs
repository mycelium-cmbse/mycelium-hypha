// ------------------------------------------------------------------------------------------------
// <copyright file="ProgressDisplay.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Progress;

    using Spectre.Console;

    /// <summary>
    /// Shows the <see cref="KnowledgeProgress"/> of a fetch or a generation as a live progress display,
    /// one bar per step, when the console is a terminal.
    /// </summary>
    /// <remarks>
    /// When the output is redirected (an editor extension, a script, a test) there is no display and the
    /// work gets no reporter, so nothing but the verb's own lines reaches standard output.
    /// </remarks>
    public static class ProgressDisplay
    {
        /// <summary>Runs work, showing its progress when the console is interactive.</summary>
        /// <typeparam name="T">What the work returns.</typeparam>
        /// <param name="work">The work, given a reporter, or <c>null</c> when nothing is shown.</param>
        /// <returns>What the work returned.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> is <c>null</c>.</exception>
        public static async Task<T> RunAsync<T>(Func<IProgress<KnowledgeProgress>?, Task<T>> work)
        {
            ArgumentNullException.ThrowIfNull(work);

            if (!AnsiConsole.Profile.Capabilities.Interactive)
            {
                return await work(null);
            }

            T result = default!;

            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new TaskDescriptionColumn { Alignment = Justify.Left }, new ProgressBarColumn(), new PercentageColumn())
                .StartAsync(async context => result = await work(new Reporter(context)));

            return result;
        }

        /// <summary>Turns each report into the bar of its step.</summary>
        private sealed class Reporter : IProgress<KnowledgeProgress>
        {
            /// <summary>The display the bars belong to.</summary>
            private readonly ProgressContext context;

            /// <summary>The bar of each step, by step.</summary>
            private readonly Dictionary<string, ProgressTask> bars = new(StringComparer.Ordinal);

            /// <summary>Guards <see cref="bars"/>: a fetch reports from several downloads at once.</summary>
            private readonly object gate = new();

            /// <summary>Initializes a new instance of the <see cref="Reporter"/> class.</summary>
            /// <param name="context">The display the bars belong to.</param>
            public Reporter(ProgressContext context) => this.context = context;

            /// <summary>Moves the step's bar, adding it on the step's first report.</summary>
            /// <param name="value">The report.</param>
            public void Report(KnowledgeProgress value)
            {
                lock (this.gate)
                {
                    if (!this.bars.TryGetValue(value.Step, out var bar))
                    {
                        bar = this.context.AddTask(Markup.Escape(value.Step), maxValue: Math.Max(value.Total, 1));
                        this.bars[value.Step] = bar;
                    }

                    bar.MaxValue = Math.Max(value.Total, 1);
                    bar.Value = value.Done;
                    bar.Description = Markup.Escape($"{value.Step}: {value.Message}");
                }
            }
        }
    }
}
