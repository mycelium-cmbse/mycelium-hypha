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

    internal sealed class RecordedConsole : IDisposable
    {
        private readonly IAnsiConsole previous;
        private readonly TestConsole console;

        // The test console wraps at 80 columns by default, where the line breaks fall wherever a path's length
        // puts them; a test asserting on whole sentences or paths asks for a width that never wraps them.
        public RecordedConsole(int width = 80)
        {
            this.previous = AnsiConsole.Console;
            this.console = new TestConsole();
            this.console.Profile.Width = width;

            AnsiConsole.Console = this.console;
        }

        public string Output => this.console.Output;

        public void Dispose() => AnsiConsole.Console = this.previous;
    }
}
