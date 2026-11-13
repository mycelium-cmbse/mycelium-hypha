// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSelector.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Linq;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Switches the default release by rewriting the version manifest.
    /// </summary>
    public sealed class ReleaseSelector : IReleaseSelector
    {
        /// <summary>Where the version manifest is.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Initializes a new instance of the <see cref="ReleaseSelector"/> class.</summary>
        /// <param name="layout">Where the version manifest is.</param>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
        public ReleaseSelector(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <summary>Makes <paramref name="tag"/> the default release.</summary>
        /// <param name="tag">An installed release.</param>
        /// <returns>
        /// The new default and the one before it. When <paramref name="tag"/> already was the default,
        /// nothing is written and <see cref="ReleaseSelection.Changed"/> is <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, or is not installed; the message
        /// names the installed releases, or says that nothing is installed.
        /// </exception>
        public ReleaseSelection Use(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var path = this.layout.VersionManifest.FullName;
            var manifest = VersionManifestFile.ReadIfPresent(path);
            var installed = manifest?.GetTags() ?? [];

            if (manifest is null || !installed.Contains(tag, StringComparer.Ordinal))
            {
                // No parameter name: the message is shown as is, by the CLI and to a model, and a
                // "(Parameter 'tag')" suffix would only get in the way.
                throw new ArgumentException(
                    installed.Count > 0
                        ? $"'{tag}' is not installed; the installed releases are {string.Join(", ", installed)}"
                        : $"'{tag}' is not installed, and nothing is installed yet");
            }

            var selection = new ReleaseSelection(tag, manifest.Default);

            if (selection.Changed)
            {
                VersionManifestFile.Write(VersionManifest.Build(tag, manifest.Versions), path);
            }

            return selection;
        }
    }
}
