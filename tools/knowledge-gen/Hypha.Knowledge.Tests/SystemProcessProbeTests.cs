// ------------------------------------------------------------------------------------------------
// <copyright file="SystemProcessProbeTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Diagnostics;

    using Hypha.Knowledge.Hosting;

    [TestFixture]
    public class SystemProcessProbeTests
    {
        private readonly SystemProcessProbe probe = new();

        [Test]
        public void This_process_is_running()
        {
            Assert.That(this.probe.IsRunning(Environment.ProcessId), Is.True);
        }

        [Test]
        public void A_process_that_has_exited_is_not_running()
        {
            var start = OperatingSystem.IsWindows()
                ? new ProcessStartInfo("cmd.exe", "/c exit 0")
                : new ProcessStartInfo("/bin/sh", "-c \"exit 0\"");
            start.UseShellExecute = false;
            start.CreateNoWindow = true;

            using var process = Process.Start(start)!;
            var processId = process.Id;
            process.WaitForExit();

            Assert.That(this.probe.IsRunning(processId), Is.False);
        }

        [Test]
        public void A_process_this_one_may_not_inspect_is_running()
        {
            // The System process on Windows and init on Unix: always there, and on Windows not open to
            // a normal user, which is the branch that must count it as running.
            var processId = OperatingSystem.IsWindows() ? 4 : 1;

            Assert.That(this.probe.IsRunning(processId), Is.True);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void An_id_no_process_has_is_not_running(int processId)
        {
            Assert.That(this.probe.IsRunning(processId), Is.False);
        }
    }
}
