// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeVersions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.MetamodelGen.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Locates the per-release directories this repository is laid out with, and exposes which
    /// releases are installed.
    /// </summary>
    /// <remarks>
    /// Reading the manifest itself belongs to <see cref="VersionManifestFile"/>; this type only knows
    /// the repository layout. The <b>tag</b> is the version identifier throughout — the model URI
    /// inside the XMI is deliberately not used, since it tracks neither the release nor the content.
    /// </remarks>
    internal static class KnowledgeVersions
    {
        /// <summary>The manifest file name, relative to the knowledge root.</summary>
        public const string ManifestFileName = VersionManifest.FileName;

        private static readonly Lazy<VersionManifest?> Manifest = new(Load);

        /// <summary>Gets the installed release tags, newest first. Empty when there is no manifest.</summary>
        public static IReadOnlyList<string> Tags => Manifest.Value?.Tags ?? [];

        /// <summary>Gets the tag answered from by default, or <c>null</c> when there is no manifest.</summary>
        public static string? DefaultTag => Manifest.Value?.Default;

        /// <summary>The knowledge directory for one tag, e.g. <c>knowledge/2026-05</c>.</summary>
        public static DirectoryInfo KnowledgeDirectory(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "knowledge", tag));
        }

        /// <summary>The metamodel output directory for one tag, e.g. <c>knowledge/2026-05/metamodel</c>.</summary>
        public static DirectoryInfo MetamodelDirectory(string tag) =>
            new(Path.Combine(KnowledgeDirectory(tag).FullName, "metamodel"));

        /// <summary>The XMI input directory for one tag, e.g. <c>sources/2026-05/xmi</c>.</summary>
        public static DirectoryInfo XmiDirectory(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            return new DirectoryInfo(
                Path.Combine(TestModel.FindRepoRoot()!.FullName, "sources", tag, "xmi"));
        }

        private static VersionManifest? Load()
        {
            var root = TestModel.FindRepoRoot();

            return root is null
                ? null
                : VersionManifestFile.ReadIfPresent(
                    Path.Combine(root.FullName, "knowledge", ManifestFileName));
        }
    }
}
