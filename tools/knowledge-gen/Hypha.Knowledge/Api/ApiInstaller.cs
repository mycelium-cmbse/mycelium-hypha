// ------------------------------------------------------------------------------------------------
// <copyright file="ApiInstaller.cs" company="Starion Group S.A.">
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
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Fetches one API version's inputs and records it in <c>knowledge/api/versions.json</c>.
    /// </summary>
    /// <remarks>
    /// Four files from four places: the OpenAPI document from the API-Services repository, the
    /// specification PDF from the Release repository, the KerML model the PIM refers to from the
    /// Pilot-Implementation repository - all three at the same tag - and the PIM model itself from the
    /// OMG document server, because no repository carries it.
    /// </remarks>
    public sealed class ApiInstaller : IApiInstaller
    {
        private static readonly Action<ILogger, string, Exception?> StartedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Started"), "Fetching the API inputs for {Tag}");

        private static readonly Action<ILogger, string, string, Exception?> RecordedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(2, "Recorded"), "Recorded API version {Tag} in {Manifest}");

        private readonly IReleaseFetcher fetcher;
        private readonly IDocumentDownloader documents;
        private readonly ICommitResolver resolver;
        private readonly IApiLayout layout;
        private readonly HyphaKnowledgeOptions options;
        private readonly ILogger<ApiInstaller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiInstaller"/> class.
        /// </summary>
        public ApiInstaller(
            IReleaseFetcher fetcher,
            IDocumentDownloader documents,
            ICommitResolver resolver,
            IApiLayout layout,
            HyphaKnowledgeOptions options,
            ILogger<ApiInstaller> logger)
        {
            ArgumentNullException.ThrowIfNull(fetcher);
            ArgumentNullException.ThrowIfNull(documents);
            ArgumentNullException.ThrowIfNull(resolver);
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);

            this.fetcher = fetcher;
            this.documents = documents;
            this.resolver = resolver;
            this.layout = layout;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ApiInstallation> InstallAsync(
            ApiInstallRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var tag = request.Tag;

            if (!ReleaseTag.IsRelease(tag))
            {
                throw new ArgumentException($"not an offerable API version: {tag}", nameof(request));
            }

            StartedMessage(this.logger, tag, null);

            var skip = request.SkipExisting;
            var xmi = this.layout.Xmi(tag).FullName;

            var files = new List<FileInfo>
            {
                await this.fetcher.DownloadAsync(
                    ApiUpstream.ServicesRepository, tag, ApiUpstream.OpenApiPath, this.layout.OpenApi(tag), skip,
                    cancellationToken),
                await this.fetcher.DownloadAsync(
                    Upstream.PilotRepository, tag, ApiUpstream.KermlXmiPath,
                    new FileInfo(Path.Combine(xmi, ApiInputs.KermlXmi)), skip, cancellationToken),
                await this.documents.DownloadAsync(
                    this.options.PimAddress, this.layout.PimModel(tag), skip, cancellationToken),
            };

            if (request.IncludeSpecification)
            {
                files.Add(await this.fetcher.DownloadAsync(
                    Upstream.ReleaseRepository, tag, ApiUpstream.SpecificationPath,
                    new FileInfo(Path.Combine(this.layout.Specifications(tag).FullName, ApiInputs.Specification)),
                    skip, cancellationToken));
            }

            var version = new InstalledApiVersion(
                tag,
                await this.ResolveAsync(ApiUpstream.ServicesRepository, tag, cancellationToken),
                await this.ResolveAsync(Upstream.ReleaseRepository, tag, cancellationToken),
                await this.ResolveAsync(Upstream.PilotRepository, tag, cancellationToken),
                ApiUpstream.PimDocument);

            var manifest = this.Record(version, request.MakeDefault);
            RecordedMessage(this.logger, tag, manifest.FullName, null);

            return new ApiInstallation(tag, files, version, manifest);
        }

        private async Task<UpstreamReference> ResolveAsync(
            string repository, string tag, CancellationToken cancellationToken) =>
            new(repository, await this.resolver.ResolveAsync(repository, tag, cancellationToken));

        private FileInfo Record(InstalledApiVersion version, bool makeDefault)
        {
            var path = this.layout.VersionManifest.FullName;
            var existing = ApiVersionManifestFile.ReadIfPresent(path);

            var versions = existing?.Versions
                .Where(installed => !string.Equals(installed.Tag, version.Tag, StringComparison.Ordinal))
                .ToList() ?? [];

            versions.Add(version);

            // The first version installed is the default whether or not it was asked for: a manifest
            // whose default is not installed is one ApiVersionManifest.Build refuses to assemble.
            var defaultTag = makeDefault || existing is null ? version.Tag : existing.Default;

            return ApiVersionManifestFile.Write(ApiVersionManifest.Build(defaultTag, versions), path);
        }
    }
}
