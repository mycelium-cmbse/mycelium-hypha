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

    using Hypha.Knowledge.Api;
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
        /// Where the Systems Modeling API PIM model is published. Defaults to the OMG document server.
        /// </summary>
        /// <remarks>
        /// The one input that is not read from a repository at a tag: no Systems-Modeling repository
        /// carries the PIM model.
        /// </remarks>
        public Uri PimAddress { get; set; } = ApiUpstream.DefaultPimAddress;

        /// <summary>
        /// The folder holding <c>sources/</c> and <c>knowledge/</c>. Discovered from the running
        /// assembly when unset.
        /// </summary>
        public DirectoryInfo? RepositoryRoot { get; set; }

        /// <summary>
        /// Where generated artifacts are written. Defaults to <see cref="RepositoryRoot"/>.
        /// </summary>
        /// <remarks>
        /// Setting this generates without touching the committed knowledge base, which is what the CLI's
        /// <c>--output</c> offers, and what lets the golden tests regenerate into a scratch folder and
        /// compare rather than overwrite. Inputs are still read from <see cref="RepositoryRoot"/>.
        /// </remarks>
        public DirectoryInfo? OutputRoot { get; set; }

        /// <summary>
        /// Where the managed toolchain lives: the <c>uv</c> executable, uv's package cache and the
        /// Python uv installs.
        /// </summary>
        /// <remarks>
        /// Unset, <c>uv</c> is cached under the per-user application data folder and uv keeps its own
        /// default locations for its cache and Python, which other uv users on the machine share. Set,
        /// all three move under this folder, so a host such as an editor extension can keep everything
        /// it provisions in its own storage.
        /// </remarks>
        public DirectoryInfo? ToolchainCacheRoot { get; set; }

        /// <summary>
        /// How many files are downloaded at once.
        /// </summary>
        /// <remarks>
        /// Deliberately modest. The point is to stop opening a connection per file, not to saturate
        /// the host that was throttling us in the first place.
        /// </remarks>
        public int MaxDownloadConcurrency { get; set; } = 6;

        /// <summary>
        /// How long taking the release-operation lock (<see cref="IReleaseOperationLock"/>) waits while
        /// another operation holds it, before it is refused.
        /// </summary>
        /// <remarks>
        /// Zero, the default, refuses at once, as the VS Code extension does. A host whose operations can
        /// overlap, such as an MCP server whose tool calls run alongside another server's or each other,
        /// sets a wait instead.
        /// </remarks>
        public TimeSpan ReleaseLockWait { get; set; } = TimeSpan.Zero;

        /// <summary>Throws when a value cannot be used, rather than failing later and further away.</summary>
        /// <exception cref="ArgumentNullException">
        /// <see cref="ApiBaseAddress"/> or <see cref="RawContentBaseAddress"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="MaxDownloadConcurrency"/> is less than 1, or <see cref="ReleaseLockWait"/> is negative.
        /// </exception>
        public void Validate()
        {
            ArgumentNullException.ThrowIfNull(this.ApiBaseAddress);
            ArgumentNullException.ThrowIfNull(this.RawContentBaseAddress);
            ArgumentNullException.ThrowIfNull(this.PimAddress);
            ArgumentOutOfRangeException.ThrowIfLessThan(this.MaxDownloadConcurrency, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(this.ReleaseLockWait, TimeSpan.Zero);
        }
    }
}
