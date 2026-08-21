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
    /// <summary>
    /// Suite of tests for <see cref="DetachedProcessLauncher"/>'s pure <c>ProcessStartInfo</c> building.
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
    }
}
