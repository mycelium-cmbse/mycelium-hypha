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

    /// <summary>
    /// Shared helpers for the JSON-sidecar tests: the fixture input, the committed expected output and
    /// the provenance digest, so the generation, golden and invariant tests all agree.
    /// </summary>
    internal static class JsonSidecarTestSupport
    {
        /// <summary>
        /// The committed expected-output directory the JSON sidecar golden files live in, under this
        /// test project rather than the (no longer committed) <c>knowledge/</c> tree.
        /// </summary>
        public static string ExpectedDirectory =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Expected", "metamodel");

        /// <summary>
        /// The fixture's input XMI files, that its provenance digest is computed over. The primitive
        /// types are shared across releases (the OMG UML library), so they are read from the
        /// repository's <c>sources/</c> root rather than the fixture folder.
        /// </summary>
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

        /// <summary>The deterministic provenance digest of the fixture's input XMI.</summary>
        public static string ComputeSourceHash() =>
            MetamodelJsonGenerator.ComputeSourceXmiSha256(SourceXmiPaths());

        /// <summary>Normalizes line endings / trailing newlines for cross-platform comparison.</summary>
        public static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
