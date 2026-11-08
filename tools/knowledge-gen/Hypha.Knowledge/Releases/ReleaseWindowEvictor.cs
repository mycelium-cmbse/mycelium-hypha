// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseWindowEvictor.cs" company="Starion Group S.A.">
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
    /// Rewrites <c>knowledge/versions.json</c> to the newest <c>keep</c> releases and deletes the
    /// <c>sources/&lt;tag&gt;</c> and <c>knowledge/&lt;tag&gt;</c> folders of whichever releases no
    /// longer fit.
    /// </summary>
    /// <remarks>
    /// Nothing here commits anything: every change is still a working-tree edit until the caller
    /// commits, so an eviction that turns out to be premature is a <c>git checkout</c> away from
    /// undone.
    /// </remarks>
    public sealed class ReleaseWindowEvictor : IReleaseWindowEvictor
    {
        /// <summary>The compiled <c>Evicted {Tag}</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, Exception?> EvictedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Evicted"), "Evicted {Tag}");

        /// <summary>The compiled message that the window is not over size (event 2, information).</summary>
        private static readonly Action<ILogger, int, Exception?> NothingToEvictMessage =
            LoggerMessage.Define<int>(
                LogLevel.Information, new EventId(2, "NothingToEvict"),
                "The window already holds no more than {Keep} release(s); nothing evicted");

        /// <summary>Resolves the version manifest and the evicted releases' folders.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Receives a message per evicted release, or one that nothing was evicted.</summary>
        private readonly ILogger<ReleaseWindowEvictor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseWindowEvictor"/> class.
        /// </summary>
        /// <param name="layout">Resolves the version manifest and the releases' folders.</param>
        /// <param name="logger">Receives the eviction messages.</param>
        /// <exception cref="ArgumentNullException"><paramref name="layout"/> or <paramref name="logger"/> is <c>null</c>.</exception>
        public ReleaseWindowEvictor(IKnowledgeLayout layout, ILogger<ReleaseWindowEvictor> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <summary>
        /// Evicts every installed release beyond the newest <paramref name="keep"/>: rewrites the version
        /// manifest with the kept releases first, then deletes each evicted release's
        /// <c>sources/&lt;tag&gt;/</c> and <c>knowledge/&lt;tag&gt;/</c> folders. The work is synchronous;
        /// the returned task is already complete.
        /// </summary>
        /// <param name="keep">How many releases the window holds.</param>
        /// <param name="cancellationToken">
        /// Checked before each evicted release's folders are deleted; the manifest has already been rewritten
        /// by then.
        /// </param>
        /// <returns>The tags evicted, oldest first; empty when the window was not over size.</returns>
        /// <exception cref="InvalidOperationException">
        /// There is no manifest to evict from, or the release the manifest answers by default would
        /// itself be evicted. That is unreachable in practice, since the default always tracks the newest
        /// installed tag, which can never sort outside the kept window; asserted anyway, because a
        /// silent eviction of the default would leave every skill pointing at a folder that no longer
        /// exists.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="keep"/> is less than one.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public Task<IReadOnlyList<string>> EvictAsync(int keep, CancellationToken cancellationToken = default)
        {
            var path = this.layout.VersionManifest.FullName;
            var manifest = VersionManifestFile.ReadIfPresent(path)
                ?? throw new InvalidOperationException(
                    $"no version manifest at {path}; fetch a release before evicting from its window");

            var evicted = ReleaseWindow.Evicted(manifest.Versions, keep);

            if (evicted.Count == 0)
            {
                NothingToEvictMessage(this.logger, keep, null);
                return Task.FromResult<IReadOnlyList<string>>([]);
            }

            if (evicted.Contains(manifest.Default, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"'{manifest.Default}' is both the default release and outside the newest {keep} - "
                    + "refusing to evict the release every skill answers from");
            }

            var kept = manifest.Versions
                .Where(version => !evicted.Contains(version.Tag, StringComparer.Ordinal))
                .ToList();

            VersionManifestFile.Write(VersionManifest.Build(manifest.Default, kept), path);

            foreach (var tag in evicted)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Delete(this.layout.ReleaseSources(tag));
                Delete(this.layout.Knowledge(tag));

                EvictedMessage(this.logger, tag, null);
            }

            return Task.FromResult(evicted);
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
