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

        public RecordedConsole()
        {
            this.previous = AnsiConsole.Console;
            this.console = new TestConsole();

            AnsiConsole.Console = this.console;
        }

        public string Output => this.console.Output;

        public void Dispose() => AnsiConsole.Console = this.previous;
    }
}
