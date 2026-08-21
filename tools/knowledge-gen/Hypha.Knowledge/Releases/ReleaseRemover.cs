// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseRemover.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Releases
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Removes one release's <c>sources/</c> and <c>knowledge/</c> folders and drops it from the
    /// version manifest.
    /// </summary>
    /// <remarks>
    /// Nothing here commits anything: every change is still a working-tree edit until the caller
    /// commits, the same property <see cref="ReleaseWindowEvictor"/> already has.
    /// </remarks>
    public sealed class ReleaseRemover : IReleaseRemover
    {
        private static readonly Action<ILogger, string, Exception?> RemovedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Removed"), "Removed {Tag}");

        private readonly IKnowledgeLayout layout;
        private readonly ILogger<ReleaseRemover> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseRemover"/> class.
        /// </summary>
        public ReleaseRemover(IKnowledgeLayout layout, ILogger<ReleaseRemover> logger)
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
            var manifest = VersionManifestFile.ReadIfPresent(path)
                ?? throw new InvalidOperationException(
                    $"no version manifest at {path}; nothing is installed to remove");

            if (!manifest.Versions.Any(version => string.Equals(version.Tag, tag, StringComparison.Ordinal)))
            {
                throw new ArgumentException($"'{tag}' is not installed", nameof(tag));
            }

            // The only installed release is always the default (VersionManifest.Build requires it),
            // so that case is decided by --force alone - checking "is it the default" first would
            // make --force unreachable, since the last release could never pass that check.
            if (manifest.Versions.Count == 1)
            {
                if (!force)
                {
                    throw new InvalidOperationException(
                        $"'{tag}' is the only installed release - pass --force to remove it anyway");
                }
            }
            else if (string.Equals(manifest.Default, tag, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"'{tag}' is the default release - run 'hypha use --tag <other>' before removing it");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var kept = manifest.Versions
                .Where(version => !string.Equals(version.Tag, tag, StringComparison.Ordinal))
                .ToList();

            if (kept.Count > 0)
            {
                VersionManifestFile.Write(VersionManifest.Build(manifest.Default, kept), path);
            }
            else
            {
                File.Delete(path);
            }

            Delete(this.layout.ReleaseSources(tag));
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
