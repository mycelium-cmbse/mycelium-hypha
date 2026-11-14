// ------------------------------------------------------------------------------------------------
// <copyright file="SystemProcessRunnerTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Hosting;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Moq;

    [TestFixture]
    public class SystemProcessRunnerTests
    {
        private DirectoryInfo workingDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            this.workingDirectory = new DirectoryInfo(Path.GetTempPath());
        }

        [Test]
        public void Requires_a_file_name()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);

            Assert.That(() => runner.RunAsync(" ", "--version", this.workingDirectory), Throws.ArgumentException);
        }

        [Test]
        public void Requires_arguments()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);

            Assert.That(() => runner.RunAsync("dotnet", null!, this.workingDirectory), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void Requires_a_working_directory()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);

            Assert.That(() => runner.RunAsync("dotnet", "--version", null!), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public async Task Returns_the_exit_code_of_a_process_that_succeeds()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);

            var exitCode = await runner.RunAsync("dotnet", "--version", this.workingDirectory);

            Assert.That(exitCode, Is.EqualTo(0));
        }

        [Test]
        public async Task Returns_the_exit_code_of_a_process_that_fails()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);

            var exitCode = await runner.RunAsync("dotnet", "no-such-hypha-test-command", this.workingDirectory);

            Assert.That(exitCode, Is.Not.EqualTo(0));
        }

        [Test]
        public async Task Passes_the_given_environment_to_the_process()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);
            var (shell, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c exit %HYPHA_TEST_EXIT_CODE%")
                : ("/bin/sh", "-c \"exit $HYPHA_TEST_EXIT_CODE\"");

            var exitCode = await runner.RunAsync(
                shell, arguments, this.workingDirectory, new Dictionary<string, string> { ["HYPHA_TEST_EXIT_CODE"] = "7" });

            Assert.That(exitCode, Is.EqualTo(7));
        }

        [Test]
        public async Task The_given_environment_overrides_an_inherited_variable()
        {
            // A user may already have UV_CACHE_DIR set; the toolchain root still has to win.
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);
            var (shell, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c exit %HYPHA_TEST_INHERITED%")
                : ("/bin/sh", "-c \"exit $HYPHA_TEST_INHERITED\"");

            Environment.SetEnvironmentVariable("HYPHA_TEST_INHERITED", "3");

            try
            {
                var exitCode = await runner.RunAsync(
                    shell, arguments, this.workingDirectory, new Dictionary<string, string> { ["HYPHA_TEST_INHERITED"] = "7" });

                Assert.That(exitCode, Is.EqualTo(7));
            }
            finally
            {
                Environment.SetEnvironmentVariable("HYPHA_TEST_INHERITED", null);
            }
        }

        [Test]
        public async Task Streams_standard_output_through_the_logger()
        {
            var logger = new Mock<ILogger<SystemProcessRunner>>();
            var runner = new SystemProcessRunner(logger.Object);

            await runner.RunAsync("dotnet", "--version", this.workingDirectory);

            logger.Verify(
                mock => mock.Log(
                    LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((value, type) => true), null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task Streams_standard_error_through_the_logger_as_a_warning()
        {
            var logger = new Mock<ILogger<SystemProcessRunner>>();
            var runner = new SystemProcessRunner(logger.Object);

            await runner.RunAsync("dotnet", "no-such-hypha-test-command", this.workingDirectory);

            logger.Verify(
                mock => mock.Log(
                    LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((value, type) => true), null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task A_process_that_reads_its_standard_input_finds_it_closed_rather_than_waiting_on_ours()
        {
            // Under `hypha serve` this process's own input is the pipe an MCP client keeps open, so a child
            // that inherited it would wait for ever, or take a request meant for the server. Whatever the
            // test host's own input is, the child's is closed at once: a command that reads to the end
            // of its input returns.
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);
            using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var (command, arguments) = OperatingSystem.IsWindows() ? ("cmd", "/c findstr x") : ("cat", string.Empty);

            Assert.That(
                async () => await runner.RunAsync(command, arguments, this.workingDirectory, cancellationToken: limit.Token),
                Throws.Nothing);

            await Task.CompletedTask;
        }

        [Test]
        public void A_cancelled_run_kills_the_process_and_throws()
        {
            var runner = new SystemProcessRunner(NullLogger<SystemProcessRunner>.Instance);
            var (fileName, arguments) = LongRunningCommand();

            using var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromMilliseconds(200));

            var stopwatch = Stopwatch.StartNew();

            Assert.That(
                () => runner.RunAsync(fileName, arguments, this.workingDirectory, cancellationToken: source.Token),
                Throws.InstanceOf<OperationCanceledException>());

            stopwatch.Stop();

            // The command itself runs far longer than this; only a real kill of the process (rather
            // than politely waiting it out) explains returning this quickly.
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(30)));
        }

        private static (string FileName, string Arguments) LongRunningCommand() =>
            OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c ping 127.0.0.1 -n 60 > nul")
                : ("sleep", "60");
    }
}
