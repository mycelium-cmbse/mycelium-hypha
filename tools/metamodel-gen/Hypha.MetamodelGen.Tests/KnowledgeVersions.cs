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
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Reads <c>knowledge/versions.json</c>, the committed record of which release tags this
    /// checkout carries and which one answers by default.
    /// </summary>
    /// <remarks>
    /// The <b>tag</b> is the version identifier throughout. The model URI inside the XMI
    /// (<c>…/SysML/20250201</c>) is deliberately not used: it tracks neither the release nor the
    /// content, and the same model may legitimately appear under several tags.
    /// </remarks>
    internal static class KnowledgeVersions
    {
        /// <summary>The manifest file name, relative to the knowledge root.</summary>
        public const string ManifestFileName = "versions.json";

        private static readonly Lazy<ManifestData> Manifest = new(Load);

        /// <summary>Gets the installed release tags, newest first. Empty when there is no manifest.</summary>
        public static IReadOnlyList<string> Tags => Manifest.Value.Tags;

        /// <summary>Gets the tag answered from by default, or <c>null</c> when there is no manifest.</summary>
        public static string? DefaultTag => Manifest.Value.Default;

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

        private static ManifestData Load()
        {
            var root = TestModel.FindRepoRoot();
            if (root is null)
            {
                return new ManifestData(Array.Empty<string>(), null);
            }

            var path = Path.Combine(root.FullName, "knowledge", ManifestFileName);
            if (!File.Exists(path))
            {
                return new ManifestData(Array.Empty<string>(), null);
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var tags = document.RootElement
                .GetProperty("versions")
                .EnumerateArray()
                .Select(version => version.GetProperty("tag").GetString()!)
                .ToList();

            return new ManifestData(tags, document.RootElement.GetProperty("default").GetString());
        }

        private sealed record ManifestData(IReadOnlyList<string> Tags, string? Default);
    }
}
