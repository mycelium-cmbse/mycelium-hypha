// ------------------------------------------------------------------------------------------------
// <copyright file="ApiVersionStatus.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// One installed Systems Modeling API version: whether it is the default, whether its inputs were
    /// fetched, and which of its artifacts have been generated.
    /// </summary>
    /// <remarks>
    /// The API counterpart of <see cref="Layout.ReleaseStatus"/>, and the single place that decides
    /// what "generated" means for an API version. Part of the <c>hypha api list --json</c> contract.
    /// </remarks>
    /// <param name="Tag">The API version tag.</param>
    /// <param name="IsDefault">The API version every lookup answers from when none is named.</param>
    /// <param name="SourcesFetched">The API version's source folder is present.</param>
    /// <param name="Artifacts">Which generated artifacts are present.</param>
    public sealed record ApiVersionStatus(
        [property: JsonPropertyName("tag")] string Tag,
        [property: JsonPropertyName("isDefault")] bool IsDefault,
        [property: JsonPropertyName("sourcesFetched")] bool SourcesFetched,
        [property: JsonPropertyName("artifacts")] ApiArtifacts Artifacts)
    {
        /// <summary>Reads the status of every installed API version, in manifest order (newest first).</summary>
        /// <param name="layout">Where the API versions' inputs and generated knowledge live.</param>
        /// <returns>One status per installed API version; empty when none is installed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
        public static IReadOnlyList<ApiVersionStatus> ReadAll(IApiLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            var defaultTag = layout.DefaultTag;

            return [.. layout.InstalledTags.Select(tag => Read(layout, tag, defaultTag))];
        }

        /// <summary>Reads the status of one API version.</summary>
        /// <param name="layout">Where the API version's inputs and generated knowledge live.</param>
        /// <param name="tag">
        /// The API version; it does not have to be installed, so a missing one reads as nothing present.
        /// </param>
        /// <returns>The API version's status, read from disk now.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is <c>null</c>, empty or white space.</exception>
        public static ApiVersionStatus Read(IApiLayout layout, string tag)
        {
            ArgumentNullException.ThrowIfNull(layout);

            return Read(layout, tag, layout.DefaultTag);
        }

        /// <summary>Reads one API version's status against a default the caller has already read.</summary>
        /// <param name="layout">Where the API version's inputs and generated knowledge live.</param>
        /// <param name="tag">The API version.</param>
        /// <param name="defaultTag">The default API version, or <c>null</c> when there is none.</param>
        /// <returns>The API version's status, read from disk now.</returns>
        private static ApiVersionStatus Read(IApiLayout layout, string tag, string? defaultTag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var artifacts = new ApiArtifacts(
                Present(layout.PimGraph(tag)),
                Present(layout.OpenApiIndex(tag)),
                Present(layout.CrossReferences(tag)),
                Present(layout.SpecificationCatalog(tag)));

            var sources = layout.Sources(tag);
            sources.Refresh();

            return new ApiVersionStatus(
                tag, string.Equals(tag, defaultTag, StringComparison.Ordinal), sources.Exists, artifacts);
        }

        /// <summary>An empty file is what an interrupted write leaves, so it does not count.</summary>
        /// <param name="file">The generated file; its cached state is refreshed first.</param>
        /// <returns><c>true</c> when the file exists and is not empty.</returns>
        private static bool Present(FileInfo file)
        {
            file.Refresh();

            return file.Exists && file.Length > 0;
        }
    }
}
