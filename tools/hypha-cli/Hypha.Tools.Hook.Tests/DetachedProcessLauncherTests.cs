// ------------------------------------------------------------------------------------------------
// <copyright file="DetachedProcessLauncherTests.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Suite of tests for <see cref="DetachedProcessLauncher"/>.
    /// </summary>
    [TestFixture]
    public class DetachedProcessLauncherTests
    {
        [Test]
        public void Arguments_go_through_the_argument_list_not_a_hand_quoted_string()
        {
            var info = DetachedProcessLauncher.Build(
                "hypha", ["sync", "--status-file", @"C:\Users\a user\status.json"], @"C:\repo");

            Assert.Multiple(() =>
            {
                Assert.That(info.ArgumentList, Is.EqualTo(new[]
                {
                    "sync", "--status-file", @"C:\Users\a user\status.json",
                }));
                Assert.That(info.Arguments, Is.Empty);
            });
        }

        [Test]
        public void Nothing_is_inherited_from_this_process()
        {
            var info = DetachedProcessLauncher.Build("hypha", ["sync"], @"C:\repo");

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
        public void The_working_directory_and_file_name_are_carried_through_unchanged()
        {
            var info = DetachedProcessLauncher.Build("hypha", ["sync"], @"C:\repo");

            Assert.Multiple(() =>
            {
                Assert.That(info.FileName, Is.EqualTo("hypha"));
                Assert.That(info.WorkingDirectory, Is.EqualTo(@"C:\repo"));
            });
        }

        [Test]
        public void Starting_a_nonexistent_executable_fails_gracefully_rather_than_throwing()
        {
            var started = DetachedProcessLauncher.Start(
                Path.Combine(Path.GetTempPath(), $"hypha-does-not-exist-{Guid.NewGuid():N}"),
                ["sync"],
                Path.GetTempPath());

            Assert.That(started, Is.False);
        }

        [Test]
        public void Starting_a_real_short_lived_process_succeeds()
        {
            // A trivial, instantly-exiting command available on any OS - not `hypha` itself, since this
            // test only needs to prove Start() reports success for something real, not exercise sync.
            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", new[] { "/c", "exit", "0" })
                : ("/bin/sh", new[] { "-c", "exit 0" });

            var started = DetachedProcessLauncher.Start(fileName, arguments, Path.GetTempPath());

            Assert.That(started, Is.True);
        }
    }
}
