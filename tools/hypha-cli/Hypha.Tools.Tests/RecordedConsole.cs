// ------------------------------------------------------------------------------------------------
// <copyright file="RecordedConsole.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;

    using Spectre.Console;
    using Spectre.Console.Testing;

    /// <summary>
    /// Redirects the console a handler writes to, and restores it afterwards.
    /// </summary>
    /// <remarks>
    /// The handlers write to the ambient <see cref="AnsiConsole"/> rather than to an injected one:
    /// what they print is a presentation detail, and threading a console through every constructor to
    /// assert on it would be tail wagging dog. Where the message <i>is</i> the behaviour - naming the
    /// artifacts that exist when the user asks for one that does not - this makes it assertable.
    /// </remarks>
    internal sealed class RecordedConsole : IDisposable
    {
        private readonly IAnsiConsole previous;
        private readonly TestConsole console;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordedConsole"/> class.
        /// </summary>
        public RecordedConsole()
        {
            this.previous = AnsiConsole.Console;
            this.console = new TestConsole();

            AnsiConsole.Console = this.console;
        }

        /// <summary>Everything written since this instance was created.</summary>
        public string Output => this.console.Output;

        /// <inheritdoc/>
        public void Dispose() => AnsiConsole.Console = this.previous;
    }
}
