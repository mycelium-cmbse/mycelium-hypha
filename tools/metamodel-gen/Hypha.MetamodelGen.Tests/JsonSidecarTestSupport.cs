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
        /// <summary>The committed knowledge directory one release's sidecar files live in.</summary>
        public static DirectoryInfo KnowledgeDirectory(string tag) =>
            KnowledgeVersions.MetamodelDirectory(tag);

        /// <summary>
        /// The input XMI files one release's provenance digest is computed over. The primitive types
        /// are shared across releases (the OMG UML library), so they are read from the common
        /// <c>sources/</c> root rather than the tag folder.
        /// </summary>
        public static IReadOnlyList<string> SourceXmiPaths(string tag)
        {
            var xmiDirectory = KnowledgeVersions.XmiDirectory(tag).FullName;
            var sourcesRoot = Path.Combine(TestModel.FindRepoRoot()!.FullName, "sources");

            return new[]
            {
                Path.Combine(xmiDirectory, "SysML_only_xmi.uml"),
                Path.Combine(xmiDirectory, "KerML_only_xmi.uml"),
                Path.Combine(sourcesRoot, "PrimitiveTypes.xmi"),
            };
        }

        /// <summary>The deterministic provenance digest of one release's input XMI.</summary>
        public static string ComputeSourceHash(string tag) =>
            MetamodelJsonGenerator.ComputeSourceXmiSha256(SourceXmiPaths(tag));

        /// <summary>Normalizes line endings / trailing newlines for cross-platform comparison.</summary>
        public static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
    }
}
