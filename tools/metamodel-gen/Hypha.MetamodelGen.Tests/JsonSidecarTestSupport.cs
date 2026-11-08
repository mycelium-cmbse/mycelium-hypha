// ------------------------------------------------------------------------------------------------
// <copyright file="JsonSidecarTestSupport.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System.Collections.Generic;
    using System.IO;

    using Hypha.MetamodelGen.Generators;

    using NUnit.Framework;

    internal static class JsonSidecarTestSupport
    {
        public static string ExpectedDirectory =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Expected", "metamodel");

        public static IReadOnlyList<string> SourceXmiPaths()
        {
            var xmiDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "xmi");

            return new[]
            {
                Path.Combine(xmiDirectory, "SysML_only_xmi.uml"),
                Path.Combine(xmiDirectory, "KerML_only_xmi.uml"),
                Repository.Layout!.SharedPrimitiveTypes.FullName,
            };
        }

        public static string ComputeSourceHash() =>
            MetamodelJsonGenerator.ComputeSourceXmiSha256(SourceXmiPaths());

        public static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
