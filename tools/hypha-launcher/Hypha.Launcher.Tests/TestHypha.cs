// ------------------------------------------------------------------------------------------------
// <copyright file="TestHypha.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Launcher.Tests
{
    using System;
    using System.IO;

    // The hypha this solution built, which the launcher tests start through HYPHA_BIN or hand over to,
    // since no released hypha has serve yet.
    internal static class TestHypha
    {
        public static FileInfo Executable()
        {
            var output = new DirectoryInfo(AppContext.BaseDirectory);
            var configuration = output.Parent!.Name;
            var framework = output.Name;
            var root = output;
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "mycelium-hypha.sln")))
            {
                root = root.Parent;
            }

            var hypha = new FileInfo(Path.Combine(
                root!.FullName, "tools", "hypha-cli", "Hypha.Tools", "bin", configuration, framework, OperatingSystem.IsWindows() ? "hypha.exe" : "hypha"));

            Assert.That(hypha.Exists, Is.True, $"the solution builds {hypha.FullName}");

            return hypha;
        }
    }
}
