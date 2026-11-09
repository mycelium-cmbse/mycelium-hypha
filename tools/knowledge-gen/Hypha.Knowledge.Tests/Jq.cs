// ------------------------------------------------------------------------------------------------
// <copyright file="Jq.cs" company="Starion Group S.A.">
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Nodes;

    // Runs the jq queries the skills document, so the query API can be checked against them. jq is a
    // developer tool, not a dependency: without it these tests are ignored, unless HYPHA_REQUIRE_JQ is
    // set (CI sets it), when a missing jq fails the run instead of silently skipping the comparison.
    internal static class Jq
    {
        private static readonly Lazy<bool> Installed = new(Probe);

        public static IReadOnlyList<string> Lines(string filter, FileInfo file) =>
            Run("-r", filter, file).Split('\n', StringSplitOptions.RemoveEmptyEntries);

        public static JsonNode? Json(string filter, FileInfo file) => JsonNode.Parse(Run("-c", filter, file));

        private static string Run(string mode, string filter, FileInfo file)
        {
            if (!Installed.Value)
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HYPHA_REQUIRE_JQ")))
                {
                    Assert.Fail("jq is not on the PATH, and HYPHA_REQUIRE_JQ asks for the jq comparisons to run.");
                }

                Assert.Ignore("jq is not on the PATH; the comparison with the documented jq query is skipped.");
            }

            var start = new ProcessStartInfo("jq")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            start.ArgumentList.Add(mode);
            start.ArgumentList.Add(filter);
            start.ArgumentList.Add(file.FullName);

            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.That(process.ExitCode, Is.Zero, $"jq {filter} failed: {error}");

            return output.Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private static bool Probe()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("jq", "--version")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                });

                if (process is null)
                {
                    return false;
                }

                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        public static IReadOnlyList<string> Sorted(this IEnumerable<string> values) =>
            values.Order(StringComparer.Ordinal).ToList();
    }
}
