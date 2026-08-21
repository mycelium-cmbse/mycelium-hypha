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
        private static readonly Action<ILogger, string, Exception?> StartedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Started"), "Fetching the inputs for {Tag}");

        private static readonly Action<ILogger, string, int, Exception?> FetchedMessage =
            LoggerMessage.Define<string, int>(
                LogLevel.Information, new EventId(2, "Fetched"), "Fetched {Kind}: {Files} files");

        private static readonly Action<ILogger, string, string, Exception?> RecordedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(3, "Recorded"), "Recorded {Tag} in {Manifest}");

        private readonly IReleaseFetcher fetcher;
        private readonly ICommitResolver resolver;
        private readonly IKnowledgeLayout layout;
        private readonly ILogger<ReleaseInstaller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseInstaller"/> class.
        /// </summary>
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

        /// <inheritdoc/>
        public async Task<ReleaseInstallation> InstallAsync(
            ReleaseInstallRequest request,
            CancellationToken cancellationToken = default,
            IProgress<FetchProgress>? progress = null)
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
                    tag, sources, request.SkipExisting, cancellationToken);
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
