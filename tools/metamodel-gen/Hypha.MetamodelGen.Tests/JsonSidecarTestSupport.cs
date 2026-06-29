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

    /// <summary>
    /// Shared helpers for the JSON-sidecar tests: the committed input/output locations and the
    /// provenance digest, so the generation, golden and invariant tests all agree.
    /// </summary>
    internal static class JsonSidecarTestSupport
    {
        /// <summary>The committed knowledge directory the sidecar files live in.</summary>
        public static DirectoryInfo KnowledgeDirectory() =>
            new(Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", "metamodel"));

        /// <summary>The committed input XMI files the provenance digest is computed over.</summary>
        public static IReadOnlyList<string> SourceXmiPaths()
        {
            var xmiDirectory = Path.Combine(TestModel.FindRepoRoot()!.FullName, "sources", "xmi");

            return new[]
            {
                Path.Combine(xmiDirectory, "SysML_only_xmi.uml"),
                Path.Combine(xmiDirectory, "KerML_only_xmi.uml"),
                Path.Combine(xmiDirectory, "PrimitiveTypes.xmi"),
            };
        }

        /// <summary>The deterministic provenance digest of the committed input XMI.</summary>
        public static string ComputeSourceHash() =>
            MetamodelJsonGenerator.ComputeSourceXmiSha256(SourceXmiPaths());

        /// <summary>Normalizes line endings / trailing newlines for cross-platform comparison.</summary>
        public static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
