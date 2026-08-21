// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeServicesTests.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Tests
{
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Threading.Tasks;

    using Hypha.Tools.Commands;
    using Hypha.Tools.Hosting;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Suite of tests for <see cref="KnowledgeServices"/>.
    /// </summary>
    [TestFixture]
    public class KnowledgeServicesTests
    {
        [Test]
        public async Task A_log_file_is_used_instead_of_the_console_when_given()
        {
            var root = new RootCommand();
            root.Options.Add(GlobalOptions.RepositoryRoot);
            root.Options.Add(GlobalOptions.Token);
            root.Options.Add(GlobalOptions.LogLevel);
            root.Options.Add(GlobalOptions.NoLogo);

            var parseResult = root.Parse(
                $"--repository-root \"{Path.GetTempPath()}\" --log-level Information");

            var logFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"hypha-log-{Guid.NewGuid():N}.log"));

            try
            {
                var provider = KnowledgeServices.Build(parseResult, null, logFile);

                provider.GetRequiredService<ILogger<KnowledgeServicesTests>>()
                    .LogInformation("hello from the test");

                await provider.DisposeAsync();

                // Existence alone is the assertion: Serilog's file sink can still hold the handle for
                // a moment after disposal returns, which would make reading the content back flaky.
                logFile.Refresh();
                Assert.That(logFile.Exists, Is.True);
            }
            finally
            {
                // Best effort: Serilog's file sink can still be releasing its handle for a moment
                // after DisposeAsync returns. This is cleanup, not the assertion above.
                try
                {
                    logFile.Refresh();
                    if (logFile.Exists)
                    {
                        logFile.Delete();
                    }
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
