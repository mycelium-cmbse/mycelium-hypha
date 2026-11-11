// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInstaller.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Progress;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Fetches a release's inputs into <c>sources/</c> and records it in the version manifest.
    /// </summary>
    /// <remarks>
    /// The manifest is written <b>after</b> the downloads, so an interrupted fetch leaves a release
    /// unrecorded rather than recorded and half-present. Generation then skips it with a reason
    /// instead of producing a knowledge base that looks complete.
    /// </remarks>
    public sealed class ReleaseInstaller : IReleaseInstaller
    {
        /// <summary>The compiled <c>Fetching the inputs for {Tag}</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, Exception?> StartedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Started"), "Fetching the inputs for {Tag}");

        /// <summary>The compiled <c>Fetched {Kind}: {Files} files</c> message (event 2, information).</summary>
        private static readonly Action<ILogger, string, int, Exception?> FetchedMessage =
            LoggerMessage.Define<string, int>(
                LogLevel.Information, new EventId(2, "Fetched"), "Fetched {Kind}: {Files} files");

        /// <summary>The compiled <c>Recorded {Tag} in {Manifest}</c> message (event 3, information).</summary>
        private static readonly Action<ILogger, string, string, Exception?> RecordedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(3, "Recorded"), "Recorded {Tag} in {Manifest}");

        /// <summary>Downloads the release's inputs.</summary>
        private readonly IReleaseFetcher fetcher;

        /// <summary>Resolves the tag to the upstream commits it points at, for the manifest.</summary>
        private readonly ICommitResolver resolver;

        /// <summary>Resolves the <c>sources/</c> folder and the version manifest.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Receives the installation's progress messages.</summary>
        private readonly ILogger<ReleaseInstaller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseInstaller"/> class.
        /// </summary>
        /// <param name="fetcher">Downloads the release's inputs.</param>
        /// <param name="resolver">Resolves the tag to its upstream commits.</param>
        /// <param name="layout">Resolves the <c>sources/</c> folder and the version manifest.</param>
        /// <param name="logger">Receives the progress messages.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public ReleaseInstaller(
            IReleaseFetcher fetcher,
            ICommitResolver resolver,
            IKnowledgeLayout layout,
            ILogger<ReleaseInstaller> logger)
        {
            ArgumentNullException.ThrowIfNull(fetcher);
            ArgumentNullException.ThrowIfNull(resolver);
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.fetcher = fetcher;
            this.resolver = resolver;
            this.layout = layout;
            this.logger = logger;
        }

        /// <summary>
        /// Installs one release: fetches its metamodel and textual inputs (and its specification PDFs when
        /// the request asks for them) into <c>sources/&lt;tag&gt;/</c>, resolves the commits the tag points
        /// at, and only then records it in the version manifest, replacing any earlier record of the same
        /// tag.
        /// </summary>
        /// <param name="request">Which release to install, and how.</param>
        /// <param name="cancellationToken">Cancels the fetch and the commit resolution.</param>
        /// <param name="progress">
        /// Reported once per fetched file, as a <see cref="KnowledgeProgress.Fetch"/> step per kind of
        /// input: <c>metamodel</c>, <c>textual</c> and, when requested, <c>specifications</c>. <c>null</c>
        /// when nobody needs to show progress.
        /// </param>
        /// <returns>What was fetched, the version recorded, and the manifest it was recorded in.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The tag is not an offerable release tag. Refusing early beats writing a
        /// <c>sources/2026-05-pre/</c> that nothing downstream will ever look at.
        /// </exception>
        /// <remarks>
        /// The release becomes the default when the request asks for it, and also when there was no manifest
        /// yet: the first release installed is always the default.
        /// </remarks>
        public async Task<ReleaseInstallation> InstallAsync(
            ReleaseInstallRequest request,
            CancellationToken cancellationToken = default,
            IProgress<KnowledgeProgress>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(request);

            var tag = request.Tag;

            if (!ReleaseTag.IsRelease(tag))
            {
                throw new ArgumentException(
                    $"not an offerable release tag: {tag}", nameof(request));
            }

            StartedMessage(this.logger, tag, null);

            var sources = this.layout.Sources;
            sources.Create();

            var metamodel = await this.fetcher.FetchMetamodelAsync(
                tag, sources, request.SkipExisting, cancellationToken, progress);
            FetchedMessage(this.logger, "metamodel", metamodel.Count, null);

            var textual = await this.fetcher.FetchTextualAsync(
                tag, sources, request.SkipExisting, cancellationToken, progress);
            FetchedMessage(this.logger, "textual", textual.Count, null);

            IReadOnlyList<FileInfo> specifications = [];

            if (request.IncludeSpecifications)
            {
                specifications = await this.fetcher.FetchSpecificationsAsync(
                    tag, sources, request.SkipExisting, cancellationToken, progress);
                FetchedMessage(this.logger, "specifications", specifications.Count, null);
            }

            var version = await this.resolver.ResolveVersionAsync(tag, cancellationToken);
            var manifest = this.Record(version, request.MakeDefault);

            RecordedMessage(this.logger, tag, manifest.FullName, null);

            return new ReleaseInstallation(tag, metamodel, textual, specifications, version, manifest);
        }

        /// <summary>
        /// Merges the release into the manifest, replacing any earlier record of the same tag.
        /// </summary>
        /// <param name="version">The release's record.</param>
        /// <param name="makeDefault">Makes the release the default; it also becomes the default when there is no manifest yet.</param>
        /// <returns>The manifest file written.</returns>
        private FileInfo Record(InstalledVersion version, bool makeDefault)
        {
            var path = this.layout.VersionManifest.FullName;
            var existing = VersionManifestFile.ReadIfPresent(path);

            var versions = existing?.Versions
                .Where(installed => !string.Equals(installed.Tag, version.Tag, StringComparison.Ordinal))
                .ToList() ?? [];

            versions.Add(version);

            // The first release installed is the default whether or not it was asked for: a manifest
            // whose default is not installed is one VersionManifest.Build refuses to assemble.
            var defaultTag = makeDefault || existing is null ? version.Tag : existing.Default;

            return VersionManifestFile.Write(VersionManifest.Build(defaultTag, versions), path);
        }
    }
}
