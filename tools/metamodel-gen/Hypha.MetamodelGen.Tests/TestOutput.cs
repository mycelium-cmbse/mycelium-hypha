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

    /// <summary>
    /// A scratch directory for tests that exercise a generator's output.
    /// </summary>
    /// <remarks>
    /// These fixtures used to write straight into <c>knowledge/</c>, which made every one of them a
    /// generator as well as a test. Producing the committed artifact is now
    /// <see cref="MetamodelGenerationTests"/>'s job alone, so the rest can assert on output nobody
    /// else is reading.
    /// </remarks>
    internal static class TestOutput
    {
        /// <summary>A directory under the test work directory, created if needed.</summary>
        public static DirectoryInfo Directory(params string[] segments) =>
            System.IO.Directory.CreateDirectory(
                Path.Combine([TestContext.CurrentContext.WorkDirectory, "generated", .. segments]));
    }
}
