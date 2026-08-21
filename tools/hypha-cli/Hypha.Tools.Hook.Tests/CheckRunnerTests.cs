// ------------------------------------------------------------------------------------------------
// <copyright file="CheckRunnerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Hook.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Suite of tests for <see cref="CheckRunner"/>.
    /// </summary>
    [TestFixture]
    public class CheckRunnerTests
    {
        [Test]
        public void Arguments_go_through_the_argument_list_not_a_hand_quoted_string()
        {
            var info = CheckRunner.Build(
                "hypha", ["check", "--repository-root", @"C:\Users\a user\plugin"], @"C:\repo");

            Assert.Multiple(() =>
            {
                Assert.That(info.ArgumentList, Is.EqualTo(new[]
                {
                    "check", "--repository-root", @"C:\Users\a user\plugin",
                }));
                Assert.That(info.Arguments, Is.Empty);
            });
        }

        [Test]
        public void Output_and_error_are_redirected_and_nothing_is_shell_executed()
        {
            var info = CheckRunner.Build("hypha", ["check"], @"C:\repo");

            Assert.Multiple(() =>
            {
                Assert.That(info.UseShellExecute, Is.False);
                Assert.That(info.CreateNoWindow, Is.True);
                Assert.That(info.RedirectStandardOutput, Is.True);
                Assert.That(info.RedirectStandardError, Is.True);
                Assert.That(info.RedirectStandardInput, Is.True);
            });
        }

        [Test]
        public async Task Captures_stdout_from_a_real_process_that_exits_zero()
        {
            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", new[] { "/c", "echo hello" })
                : ("/bin/sh", new[] { "-c", "echo hello" });

            var output = await CheckRunner.RunAndCaptureOutputAsync(
                fileName, arguments, Path.GetTempPath(), CancellationToken.None);

            Assert.That(output, Does.Contain("hello"));
        }

        [Test]
        public async Task A_non_zero_exit_code_is_reported_as_no_output()
        {
            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", new[] { "/c", "exit 1" })
                : ("/bin/sh", new[] { "-c", "exit 1" });

            var output = await CheckRunner.RunAndCaptureOutputAsync(
                fileName, arguments, Path.GetTempPath(), CancellationToken.None);

            Assert.That(output, Is.Null);
        }

        [Test]
        public async Task A_nonexistent_executable_fails_gracefully_rather_than_throwing()
        {
            var output = await CheckRunner.RunAndCaptureOutputAsync(
                Path.Combine(Path.GetTempPath(), $"hypha-does-not-exist-{Guid.NewGuid():N}"),
                ["check"],
                Path.GetTempPath(),
                CancellationToken.None);

            Assert.That(output, Is.Null);
        }
    }
}
