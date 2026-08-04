// ------------------------------------------------------------------------------------------------
// <copyright file="RepositoryLayout.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Finds this repository's per-release directories from a test binary's location.
    /// </summary>
    /// <remarks>
    /// Reading the manifest belongs to <see cref="VersionManifestFile"/>; this only knows the layout.
    /// It is deliberately small: the layout becomes a resolvable service when the CLI needs it (#81,
    /// #92), and duplicating it there would be worse than duplicating these few lines now.
    /// </remarks>
    internal static class RepositoryLayout
    {
        private static readonly Lazy<DirectoryInfo?> Root = new(Find);

        /// <summary>The repository root, or <c>null</c> when the tests run outside a checkout.</summary>
        public static DirectoryInfo? RepositoryRoot => Root.Value;

        /// <summary>The installed release tags, newest first. Empty when there is no manifest.</summary>
        public static IReadOnlyList<string> InstalledTags
        {
            get
            {
                var root = Root.Value;

                if (root is null)
                {
                    return [];
                }

                var manifest = VersionManifestFile.ReadIfPresent(
                    Path.Combine(root.FullName, "knowledge", VersionManifest.FileName));

                return manifest?.GetTags() ?? [];
            }
        }

        /// <summary>The grammar inputs for one tag: <c>sources/&lt;tag&gt;/textual/bnf</c>.</summary>
        public static DirectoryInfo BnfDirectory(string tag) =>
            new(Path.Combine(Require().FullName, "sources", tag, "textual", "bnf"));

        /// <summary>The generated notation for one tag: <c>knowledge/&lt;tag&gt;/textual-notation</c>.</summary>
        public static DirectoryInfo TextualNotationDirectory(string tag) =>
            new(Path.Combine(Require().FullName, "knowledge", tag, "textual-notation"));

        private static DirectoryInfo Require() =>
            Root.Value ?? throw new InvalidOperationException("not running inside the repository");

        private static DirectoryInfo? Find()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
                 directory is not null;
                 directory = directory.Parent)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "sources"))
                    && Directory.Exists(Path.Combine(directory.FullName, "knowledge")))
                {
                    return directory;
                }
            }

            return null;
        }
    }
}
