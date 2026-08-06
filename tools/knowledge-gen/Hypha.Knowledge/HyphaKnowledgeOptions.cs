// ------------------------------------------------------------------------------------------------
// <copyright file="HyphaKnowledgeOptions.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge
{
    using System;
    using System.IO;

    using Hypha.Knowledge.Releases;

    /// <summary>
    /// Everything a caller can vary about how the knowledge base is fetched and generated.
    /// </summary>
    /// <remarks>
    /// One place for the settings that were accumulating as optional constructor parameters. It is
    /// also where the CLI's flags will land (#81): a <c>--token</c> or <c>--concurrency</c> now has an
    /// obvious home instead of needing another parameter threaded through
    /// <see cref="ServiceCollectionExtensions.AddHyphaKnowledge"/>.
    /// </remarks>
    public sealed class HyphaKnowledgeOptions
    {
        /// <summary>The configuration section these bind from, when bound from configuration.</summary>
        public const string SectionName = "Hypha";

        /// <summary>
        /// A GitHub token. Falls back to <c>GITHUB_TOKEN</c> / <c>GH_TOKEN</c> when unset.
        /// </summary>
        /// <remarks>
        /// Optional: every repository read is public. The anonymous API allows 60 requests an hour,
        /// which discovery alone can exhaust.
        /// </remarks>
        public string? Token { get; set; }

        /// <summary>The GitHub API host. Defaults to the public one.</summary>
        public Uri ApiBaseAddress { get; set; } = Upstream.DefaultApiBaseAddress;

        /// <summary>Where a file at a tag is served from. Defaults to the public one.</summary>
        public Uri RawContentBaseAddress { get; set; } = Upstream.DefaultRawContentBaseAddress;

        /// <summary>
        /// The folder holding <c>sources/</c> and <c>knowledge/</c>. Discovered from the running
        /// assembly when unset.
        /// </summary>
        public DirectoryInfo? RepositoryRoot { get; set; }

        /// <summary>
        /// How many files are downloaded at once.
        /// </summary>
        /// <remarks>
        /// Deliberately modest. The point is to stop opening a connection per file, not to saturate
        /// the host that was throttling us in the first place.
        /// </remarks>
        public int MaxDownloadConcurrency { get; set; } = 6;

        /// <summary>Throws when a value cannot be used, rather than failing later and further away.</summary>
        public void Validate()
        {
            ArgumentNullException.ThrowIfNull(this.ApiBaseAddress);
            ArgumentNullException.ThrowIfNull(this.RawContentBaseAddress);
            ArgumentOutOfRangeException.ThrowIfLessThan(this.MaxDownloadConcurrency, 1);
        }
    }
}
