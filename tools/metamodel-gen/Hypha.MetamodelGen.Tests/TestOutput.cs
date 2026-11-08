// ------------------------------------------------------------------------------------------------
// <copyright file="TestOutput.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.IO;

    internal static class TestOutput
    {
        public static DirectoryInfo Directory(params string[] segments) =>
            System.IO.Directory.CreateDirectory(
                Path.Combine([TestContext.CurrentContext.WorkDirectory, "generated", .. segments]));
    }
}
