// ------------------------------------------------------------------------------------------------
// <copyright file="ApiRemover.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Removes one installed API version, under the same rules as a language release: the default is
    /// not removed while another could take its place, and the last one only with <c>--force</c>.
    /// </summary>
    public sealed class ApiRemover : IApiRemover
    {
        /// <summary>Logs that one API version was removed.</summary>
        private static readonly Action<ILogger, string, Exception?> RemovedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Removed"), "Removed API version {Tag}");

        /// <summary>Where the version manifest and each version's folders are.</summary>
        private readonly IApiLayout layout;

        /// <summary>The logger the removal is reported to.</summary>
        private readonly ILogger<ApiRemover> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRemover"/> class.
        /// </summary>
        /// <param name="layout">Where the version manifest and each version's folders are.</param>
        /// <param name="logger">The logger the removal is reported to.</param>
        public ApiRemover(IApiLayout layout, ILogger<ApiRemover> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <summary>
        /// Removes <paramref name="tag"/>: drops it from <c>knowledge/api/versions.json</c> (deleting the
        /// manifest when it was the last version) and deletes its <c>sources/api/&lt;tag&gt;/</c> and
        /// <c>knowledge/api/&lt;tag&gt;/</c> folders.
        /// </summary>
        /// <param name="tag">The API version to remove.</param>
        /// <param name="force">
        /// Removes the last installed API version too. Without it, removing the only remaining version is
        /// refused.
        /// </param>
        /// <param name="cancellationToken">Cancels the removal before anything is changed.</param>
        /// <returns>A completed task: the removal runs synchronously.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, is not version-shaped, or is not
        /// installed.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// There is no API version manifest; or <paramref name="tag"/> is the default while another
        /// version is installed; or it is the only one and <paramref name="force"/> was not given.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// <paramref name="cancellationToken"/> was cancelled before anything was changed.
        /// </exception>
        public Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            // The tag becomes a folder that is deleted recursively. Nothing but a version-shaped tag
            // may get that far, whatever a hand-edited manifest says is installed.
            if (!ReleaseTag.IsRelease(tag))
            {
                throw new ArgumentException($"not an API version: {tag}", nameof(tag));
            }

            var path = this.layout.VersionManifest.FullName;
            var manifest = ApiVersionManifestFile.ReadIfPresent(path)
                ?? throw new InvalidOperationException(
                    $"no API version manifest at {path}; nothing is installed to remove");

            if (!manifest.Versions.Any(version => string.Equals(version.Tag, tag, StringComparison.Ordinal)))
            {
                throw new ArgumentException($"API version '{tag}' is not installed", nameof(tag));
            }

            if (manifest.Versions.Count == 1)
            {
                if (!force)
                {
                    throw new InvalidOperationException(
                        $"'{tag}' is the only installed API version - pass --force to remove it anyway");
                }
            }
            else if (string.Equals(manifest.Default, tag, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"'{tag}' is the default API version - run 'hypha api use --tag <other>' before removing it");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var kept = manifest.Versions
                .Where(version => !string.Equals(version.Tag, tag, StringComparison.Ordinal))
                .ToList();

            if (kept.Count > 0)
            {
                ApiVersionManifestFile.Write(ApiVersionManifest.Build(manifest.Default, kept), path);
            }
            else
            {
                File.Delete(path);
            }

            Delete(this.layout.Sources(tag));
            Delete(this.layout.Knowledge(tag));

            RemovedMessage(this.logger, tag, null);

            return Task.CompletedTask;
        }

        /// <summary>Deletes <paramref name="directory"/> recursively, if it exists.</summary>
        /// <param name="directory">The folder to delete.</param>
        private static void Delete(DirectoryInfo directory)
        {
            directory.Refresh();

            if (directory.Exists)
            {
                directory.Delete(recursive: true);
            }
        }
    }
}
