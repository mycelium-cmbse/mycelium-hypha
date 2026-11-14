// ------------------------------------------------------------------------------------------------
// <copyright file="ApiSelector.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Linq;

    using Hypha.Knowledge.Releases;

    /// <summary>Switches the default API version, in <c>knowledge/api/versions.json</c>.</summary>
    /// <remarks>
    /// It takes no lock of its own: <see cref="ApiOperations"/> calls it under the release-operation lock.
    /// </remarks>
    public sealed class ApiSelector : IApiSelector
    {
        /// <summary>Where the API version manifest is.</summary>
        private readonly IApiLayout layout;

        /// <summary>Initializes a new instance of the <see cref="ApiSelector"/> class.</summary>
        /// <param name="layout">Where the API version manifest is.</param>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
        public ApiSelector(IApiLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            this.layout = layout;
        }

        /// <summary>Makes <paramref name="tag"/> the default API version.</summary>
        /// <param name="tag">An installed API version.</param>
        /// <returns>
        /// The new default and the one before it. When <paramref name="tag"/> already was the default,
        /// nothing is written and <see cref="ReleaseSelection.Changed"/> is <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, or is not installed; the message
        /// names the installed API versions, or says that none is installed.
        /// </exception>
        public ReleaseSelection Use(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var path = this.layout.VersionManifest.FullName;
            var manifest = ApiVersionManifestFile.ReadIfPresent(path);
            var installed = manifest?.GetTags() ?? [];

            if (manifest is null || !installed.Contains(tag, StringComparer.Ordinal))
            {
                // No parameter name: the message is shown as is, by the CLI and to a model.
                throw new ArgumentException(
                    installed.Count > 0
                        ? $"API version '{tag}' is not installed; the installed API versions are {string.Join(", ", installed)}"
                        : $"API version '{tag}' is not installed, and no API version is installed yet");
            }

            var selection = new ReleaseSelection(tag, manifest.Default);

            if (selection.Changed)
            {
                ApiVersionManifestFile.Write(ApiVersionManifest.Build(tag, manifest.Versions), path);
            }

            return selection;
        }
    }
}
