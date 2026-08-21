// ------------------------------------------------------------------------------------------------
// <copyright file="SyncTagPruner.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Removes the one release <c>hypha sync</c> previously added on its own, once a newer one has
    /// replaced it.
    /// </summary>
    /// <remarks>
    /// Deliberately separate from <see cref="IReleaseWindowEvictor"/>, which evicts indiscriminately
    /// by tag age and has no notion of "committed vs. locally fetched" - calling it from unattended
    /// background code risks deleting one of the releases a plugin install actually ships with. This
    /// type only ever touches the single tag <c>sync</c> itself is tracking, and refuses outright if
    /// that tag happens to be one of the committed baseline.
    /// </remarks>
    public static class SyncTagPruner
    {
        /// <summary>
        /// Deletes <paramref name="tag"/>'s <c>sources/</c> and <c>knowledge/</c> folders and removes
        /// it from the version manifest, leaving the manifest's default release untouched.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="tag"/> is in <paramref name="committedBaselineTags"/>, or is the manifest's
        /// current default. Neither should be reachable given how <c>SyncCommand</c> calls this - both
        /// are asserted anyway, because a silent prune of either would be exactly the mistake this
        /// type exists to prevent.
        /// </exception>
        public static void Prune(
            IKnowledgeLayout layout, string tag, IReadOnlyList<string>? committedBaselineTags)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            if (committedBaselineTags?.Contains(tag, StringComparer.Ordinal) == true)
            {
                throw new InvalidOperationException(
                    $"refusing to prune '{tag}': it is part of the committed baseline");
            }

            var path = layout.VersionManifest.FullName;
            var manifest = VersionManifestFile.ReadIfPresent(path);

            if (manifest is null)
            {
                return;
            }

            if (string.Equals(manifest.Default, tag, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"refusing to prune '{tag}': it is the manifest's default release");
            }

            var kept = manifest.Versions
                .Where(version => !string.Equals(version.Tag, tag, StringComparison.Ordinal))
                .ToList();

            if (kept.Count != manifest.Versions.Count)
            {
                VersionManifestFile.Write(VersionManifest.Build(manifest.Default, kept), path);
            }

            Delete(layout.ReleaseSources(tag));
            Delete(layout.Knowledge(tag));
        }

        private static void Delete(DirectoryInfo directory)
        {
            directory.Refresh();

            if (directory.Exists)
            {
                directory.Delete(recursive: true);
            }
        }
    }
}
