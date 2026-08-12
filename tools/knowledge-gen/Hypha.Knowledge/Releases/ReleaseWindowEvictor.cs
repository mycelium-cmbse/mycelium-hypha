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
        private static readonly Action<ILogger, string, Exception?> EvictedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Information, new EventId(1, "Evicted"), "Evicted {Tag}");

        private static readonly Action<ILogger, int, Exception?> NothingToEvictMessage =
            LoggerMessage.Define<int>(
                LogLevel.Information, new EventId(2, "NothingToEvict"),
                "The window already holds no more than {Keep} release(s); nothing evicted");

        private readonly IKnowledgeLayout layout;
        private readonly ILogger<ReleaseWindowEvictor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseWindowEvictor"/> class.
        /// </summary>
        public ReleaseWindowEvictor(IKnowledgeLayout layout, ILogger<ReleaseWindowEvictor> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.logger = logger;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// There is no manifest to evict from, or the release the manifest answers by default would
        /// itself be evicted - unreachable in practice, since the default always tracks the newest
        /// installed tag, which can never sort outside the kept window; asserted anyway, because a
        /// silent eviction of the default would leave every skill pointing at a folder that no longer
        /// exists.
        /// </exception>
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
