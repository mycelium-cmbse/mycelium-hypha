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
        /// <summary>The compiled <c>Removed {Tag}</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, Exception?> RemovedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Removed"), "Removed {Tag}");

        /// <summary>Resolves the version manifest and the release's folders.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Receives the message that a release was removed.</summary>
        private readonly ILogger<ReleaseRemover> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseRemover"/> class.
        /// </summary>
        /// <param name="layout">Resolves the version manifest and the release's folders.</param>
        /// <param name="logger">Receives the message that a release was removed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> or <paramref name="logger"/> is <c>null</c>.</exception>
        public ReleaseRemover(IKnowledgeLayout layout, ILogger<ReleaseRemover> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <summary>
        /// Removes <paramref name="tag"/>: rewrites the version manifest without it (or deletes the manifest
        /// when it was the last release), then deletes its <c>sources/&lt;tag&gt;/</c> and
        /// <c>knowledge/&lt;tag&gt;/</c> folders. The work is synchronous; the returned task is already
        /// complete.
        /// </summary>
        /// <param name="tag">The installed release to remove.</param>
        /// <param name="force">
        /// Removes the last installed release too. Without it, removing the only remaining release is
        /// refused: it would leave nothing for any skill to answer from.
        /// </param>
        /// <param name="cancellationToken">Checked once, before anything is changed.</param>
        /// <returns>A completed task.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is <c>null</c>, empty or white space, is not a release tag, or is not
        /// installed.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// There is no version manifest, <paramref name="tag"/> is the manifest's current default while
        /// other releases are installed (switch with <c>hypha use</c> first), or it is the only installed
        /// release and <paramref name="force"/> was not given.
        /// </exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public Task RemoveAsync(string tag, bool force = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            // The tag becomes a folder that is deleted recursively, beside sources/api/ and
            // knowledge/api/. Nothing but a release-shaped tag may get that far, whatever a
            // hand-edited manifest says is installed.
            if (!ReleaseTag.IsRelease(tag))
            {
                throw new ArgumentException($"not a release: {tag}", nameof(tag));
            }

            var path = this.layout.VersionManifest.FullName;
            var manifest = VersionManifestFile.ReadIfPresent(path)
                ?? throw new InvalidOperationException(
                    $"no version manifest at {path}; nothing is installed to remove");

            if (!manifest.Versions.Any(version => string.Equals(version.Tag, tag, StringComparison.Ordinal)))
            {
                throw new ArgumentException($"'{tag}' is not installed", nameof(tag));
            }

            // The only installed release is always the default (VersionManifest.Build requires it),
            // so that case is decided by --force alone; checking "is it the default" first would
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

        /// <summary>Deletes a folder and everything in it, when it exists.</summary>
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
