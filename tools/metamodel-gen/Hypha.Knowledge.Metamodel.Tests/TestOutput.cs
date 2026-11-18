// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.Metamodel.Tests
{
    using System.IO;

    internal static class TestOutput
    {
        public static DirectoryInfo Directory(params string[] segments) =>
            System.IO.Directory.CreateDirectory(
                Path.Combine([TestContext.CurrentContext.WorkDirectory, "generated", .. segments]));
    }
}
