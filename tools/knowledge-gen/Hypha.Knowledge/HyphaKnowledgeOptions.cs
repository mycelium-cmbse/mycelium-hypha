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
        /// The per-user folder every hypha client shares. Defaults to <see cref="HyphaHome.Resolve"/>:
        /// the <c>HYPHA_HOME</c> folder, or <c>hypha</c> under the per-user local application data folder.
        /// </summary>
        /// <remarks>
        /// It holds <c>sources/</c> and <c>knowledge/</c> unless <see cref="RepositoryRoot"/> is set, and
        /// the managed toolchain unless <see cref="ToolchainCacheRoot"/> is set. Set here or through
        /// <c>HYPHA_HOME</c>, it also wins over the source checkout above the running assembly.
        /// </remarks>
        public DirectoryInfo? Home { get; set; }

        /// <summary>
        /// The folder holding <c>sources/</c> and <c>knowledge/</c>.
        /// </summary>
        /// <remarks>
        /// Unset, it is <see cref="Home"/> when that is set here or through <c>HYPHA_HOME</c>; otherwise
        /// the source checkout above the running assembly, which is what a maintainer runs from; and
        /// otherwise the home folder's default location.
        /// </remarks>
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
        /// Unset, it is <c>toolchain/</c> under <see cref="Home"/>. All three live under it either way,
        /// rather than in uv's own per-user locations, so hypha never changes another uv's cache and a
        /// host such as an editor extension can keep everything it provisions in its own storage.
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
