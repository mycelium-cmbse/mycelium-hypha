// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseStatus.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// One installed release: whether it is the default, whether its inputs were fetched, and which
    /// of its artifacts have been generated.
    /// </summary>
    /// <remarks>
    /// The single place that decides what "generated" means for a release, so a host reports
    /// readiness from here rather than probing the file layout itself. Part of the
    /// <c>hypha list --json</c> contract.
    /// </remarks>
    /// <param name="Tag">The release tag.</param>
    /// <param name="IsDefault">The release every lookup answers from when none is named.</param>
    /// <param name="SourcesFetched">The release's XMI or textual sources are present.</param>
    /// <param name="Artifacts">Which generated artifacts are present.</param>
    public sealed record ReleaseStatus(
        [property: JsonPropertyName("tag")] string Tag,
        [property: JsonPropertyName("isDefault")] bool IsDefault,
        [property: JsonPropertyName("sourcesFetched")] bool SourcesFetched,
        [property: JsonPropertyName("artifacts")] ReleaseArtifacts Artifacts)
    {
        /// <summary>The specification documents whose catalogs make up the specification text.</summary>
        public static readonly IReadOnlyList<string> SpecificationDocuments = ["kerml", "sysml2"];

        /// <summary>Reads the status of every installed release, in manifest order (newest first).</summary>
        public static IReadOnlyList<ReleaseStatus> ReadAll(IKnowledgeLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            return [.. layout.InstalledTags.Select(tag => Read(layout, tag))];
        }

        /// <summary>Reads the status of one release.</summary>
        public static ReleaseStatus Read(IKnowledgeLayout layout, string tag)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var artifacts = new ReleaseArtifacts(
                Present(layout.MetamodelIndex(tag)),
                Present(new FileInfo(Path.Combine(layout.TextualNotation(tag).FullName, "index.md"))),
                Present(layout.ModelLibraryIndex(tag)),
                Present(layout.CrossReferences(tag)),
                SpecificationDocuments.All(document => Present(layout.SpecificationCatalog(tag, document))));

            return new ReleaseStatus(
                tag,
                string.Equals(tag, layout.DefaultTag, StringComparison.Ordinal),
                layout.Xmi(tag).Exists || layout.TextualSources(tag).Exists,
                artifacts);
        }

        /// <summary>An empty file is what an interrupted write leaves, so it does not count.</summary>
        private static bool Present(FileInfo file)
        {
            file.Refresh();

            return file.Exists && file.Length > 0;
        }
    }
}
