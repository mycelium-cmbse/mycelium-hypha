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

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Removes one installed API version, under the same rules as a language release: the default is
    /// not removed while another could take its place, and the last one only with <c>--force</c>.
    /// </summary>
    public sealed class ApiRemover : IApiRemover
    {
        private static readonly Action<ILogger, string, Exception?> RemovedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Removed"), "Removed API version {Tag}");

        private readonly IApiLayout layout;
        private readonly ILogger<ApiRemover> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRemover"/> class.
        /// </summary>
        public ApiRemover(IApiLayout layout, ILogger<ApiRemover> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

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
