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
    /// Pilot-Implementation repository (all three at the same tag), and the PIM model itself from the
    /// OMG document server, because no repository carries it.
    /// </remarks>
    public sealed class ApiInstaller : IApiInstaller
    {
        /// <summary>Logs that fetching the inputs of one API version has started.</summary>
        private static readonly Action<ILogger, string, Exception?> StartedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Started"), "Fetching the API inputs for {Tag}");

        /// <summary>Logs that one API version was recorded in the version manifest.</summary>
        private static readonly Action<ILogger, string, string, Exception?> RecordedMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(2, "Recorded"), "Recorded API version {Tag} in {Manifest}");

        /// <summary>Downloads the inputs that live in a repository at a tag.</summary>
        private readonly IReleaseFetcher fetcher;

        /// <summary>Downloads the PIM model from the OMG document server.</summary>
        private readonly IDocumentDownloader documents;

        /// <summary>Resolves each upstream's tag to the commit recorded in the manifest.</summary>
        private readonly ICommitResolver resolver;

        /// <summary>Where the inputs and the version manifest are written.</summary>
        private readonly IApiLayout layout;

        /// <summary>The options that name the PIM's address.</summary>
        private readonly HyphaKnowledgeOptions options;

        /// <summary>The logger progress is reported to.</summary>
        private readonly ILogger<ApiInstaller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiInstaller"/> class.
        /// </summary>
        /// <param name="fetcher">Downloads the inputs that live in a repository at a tag.</param>
        /// <param name="documents">Downloads the PIM model from the OMG document server.</param>
        /// <param name="resolver">Resolves each upstream's tag to a commit.</param>
        /// <param name="layout">Where the inputs and the version manifest are written.</param>
        /// <param name="options">
        /// The options whose <see cref="HyphaKnowledgeOptions.PimAddress"/> the PIM model is downloaded from.
        /// </param>
        /// <param name="logger">The logger progress is reported to.</param>
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

        /// <summary>
        /// Installs the requested API version: downloads the OpenAPI document, the KerML model and the
        /// PIM model (and the specification PDF when <see cref="ApiInstallRequest.IncludeSpecification"/>
        /// is set), then resolves each upstream's tag to a commit and records the version in
        /// <c>knowledge/api/versions.json</c>.
        /// </summary>
        /// <param name="request">The API version to install and how.</param>
        /// <param name="cancellationToken">Cancels the downloads and the commit resolution.</param>
        /// <returns>The files fetched, what the tag resolved to and the manifest it was recorded in.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The tag is not an offerable API version: it is not release-shaped, or is older than
        /// <see cref="ApiCatalog.EarliestVersion"/>.
        /// </exception>
        public async Task<ApiInstallation> InstallAsync(
            ApiInstallRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var tag = request.Tag;

            if (!ApiCatalog.IsOfferable(tag))
            {
                throw new ArgumentException(
                    $"not an offerable API version: {tag} (the earliest is {ApiCatalog.EarliestVersion})",
                    nameof(request));
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

        /// <summary>
        /// Resolves <paramref name="tag"/> in <paramref name="repository"/> to the commit it points at.
        /// </summary>
        /// <param name="repository">The upstream repository, e.g. <see cref="ApiUpstream.ServicesRepository"/>.</param>
        /// <param name="tag">The tag to resolve.</param>
        /// <param name="cancellationToken">Cancels the resolution.</param>
        /// <returns>The repository and the commit, ready to record.</returns>
        private async Task<UpstreamReference> ResolveAsync(
            string repository, string tag, CancellationToken cancellationToken) =>
            new(repository, await this.resolver.ResolveAsync(repository, tag, cancellationToken));

        /// <summary>
        /// Records <paramref name="version"/> in the version manifest, replacing any earlier entry for
        /// the same tag.
        /// </summary>
        /// <param name="version">The installed version to record.</param>
        /// <param name="makeDefault">
        /// Makes it the default. The first version recorded becomes the default regardless.
        /// </param>
        /// <returns>The manifest written.</returns>
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
