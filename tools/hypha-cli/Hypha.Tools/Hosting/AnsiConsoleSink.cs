// ------------------------------------------------------------------------------------------------
// <copyright file="AnsiConsoleSink.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hosting
{
    using System;
    using System.IO;

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;

    using Spectre.Console;

    /// <summary>
    /// Writes each log line through <see cref="AnsiConsole"/>, the console the progress display draws
    /// on, so a line logged while the display runs is placed above its bars instead of over them.
    /// </summary>
    /// <remarks>
    /// Used only when the output is a terminal. The line is written as plain text, so brackets in a
    /// message (a path, a type name) are shown as they are and never read as markup.
    /// </remarks>
    public sealed class AnsiConsoleSink : ILogEventSink
    {
        /// <summary>Renders an event the way the console sink does.</summary>
        private readonly ITextFormatter formatter;

        /// <summary>Initializes a new instance of the <see cref="AnsiConsoleSink"/> class.</summary>
        /// <param name="outputTemplate">The Serilog output template each line is rendered with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="outputTemplate"/> is <c>null</c>.</exception>
        public AnsiConsoleSink(string outputTemplate)
        {
            ArgumentNullException.ThrowIfNull(outputTemplate);

            this.formatter = new MessageTemplateTextFormatter(outputTemplate);
        }

        /// <summary>Writes one log event to the console.</summary>
        /// <param name="logEvent">The event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="logEvent"/> is <c>null</c>.</exception>
        public void Emit(LogEvent logEvent)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            using var line = new StringWriter();
            this.formatter.Format(logEvent, line);

            // Read at each line rather than kept, so the console a test installs is the one written to.
            AnsiConsole.Console.Write(new Text(line.ToString()));
        }
    }
}
