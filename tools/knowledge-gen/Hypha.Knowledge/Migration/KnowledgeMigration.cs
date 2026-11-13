// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeMigration.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Migration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Api;
    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Copies the releases and API versions the clients installed in their own folders into <c>HYPHA_HOME</c>.
    /// </summary>
    /// <remarks>
    /// It holds the release-operation lock of <c>HYPHA_HOME</c> throughout, and the lock of each client
    /// folder while it copies from it, without waiting: a client installing a release at that moment has
    /// listed it in its manifest before generating it, so its folder is half built, and the migration
    /// stops rather than copy it, recording nothing, so the next run tries again.
    /// </remarks>
    public sealed class KnowledgeMigration : IKnowledgeMigration
    {
        /// <summary>The file in <c>HYPHA_HOME</c> that records that a migration ran, and from where.</summary>
        public const string MarkerFileName = ".migrated-from-clients";

        /// <summary>The suffix of a folder being copied, before it is renamed into place.</summary>
        public const string StagingSuffix = ".migrating";

        /// <summary>The compiled <c>Copied {Kind} {Tag} from {From}</c> message (event 1, information).</summary>
        private static readonly Action<ILogger, string, string, string, Exception?> CopiedMessage =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Information, new EventId(1, "Copied"), "Copied {Kind} {Tag} from {From} into HYPHA_HOME");

        /// <summary>The compiled <c>Skipped {From}</c> message (event 2, warning).</summary>
        private static readonly Action<ILogger, string, string, Exception?> UnreadableMessage =
            LoggerMessage.Define<string, string>(
                LogLevel.Warning, new EventId(2, "Unreadable"), "Skipped {From}: its version manifest cannot be read ({Reason})");

        /// <summary>Where the knowledge is read from now: <c>HYPHA_HOME</c>, a checkout or a folder given explicitly.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Resolves the <c>HYPHA_HOME</c> folder, which may not exist yet.</summary>
        private readonly Func<DirectoryInfo> resolveHome;

        /// <summary>Finds the client folders.</summary>
        private readonly ILegacyInstallLocator locator;

        /// <summary>Tells whether a lock's owner is alive.</summary>
        private readonly IProcessProbe processes;

        /// <summary>Creates the locks' loggers.</summary>
        private readonly ILoggerFactory loggers;

        /// <summary>Logs what was copied.</summary>
        private readonly ILogger logger;

        /// <summary>How long to wait for the lock on <c>HYPHA_HOME</c>.</summary>
        private readonly TimeSpan lockWait;

        /// <summary>Initializes a new instance of the <see cref="KnowledgeMigration"/> class.</summary>
        /// <param name="layout">Where the knowledge is read from now.</param>
        /// <param name="home">
        /// Resolves the <c>HYPHA_HOME</c> folder. It is asked only when a migration needs it, and an automatic
        /// one does not when it fails: a caller that reads its knowledge from elsewhere is never refused over
        /// a <c>HYPHA_HOME</c> it does not use.
        /// </param>
        /// <param name="locator">Finds the client folders.</param>
        /// <param name="processes">Tells whether a lock's owner is alive.</param>
        /// <param name="loggers">Creates the migration's logger, and the logger of each lock it takes.</param>
        /// <param name="lockWait">
        /// How long to wait for the lock on <c>HYPHA_HOME</c>, as <see cref="HyphaKnowledgeOptions.ReleaseLockWait"/>.
        /// A client folder's lock is never waited for.
        /// </param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lockWait"/> is negative.</exception>
        public KnowledgeMigration(
            IKnowledgeLayout layout,
            Func<DirectoryInfo> home,
            ILegacyInstallLocator locator,
            IProcessProbe processes,
            ILoggerFactory loggers,
            TimeSpan lockWait = default)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(home);
            ArgumentNullException.ThrowIfNull(locator);
            ArgumentNullException.ThrowIfNull(processes);
            ArgumentNullException.ThrowIfNull(loggers);
            ArgumentOutOfRangeException.ThrowIfLessThan(lockWait, TimeSpan.Zero);

            this.layout = layout;
            this.resolveHome = home;
            this.locator = locator;
            this.processes = processes;
            this.loggers = loggers;
            this.logger = loggers.CreateLogger<KnowledgeMigration>();
            this.lockWait = lockWait;
        }

        /// <summary>
        /// Migrates once, on its own: only when the knowledge is read from <c>HYPHA_HOME</c>, it has no release
        /// installed yet, and no migration ran before.
        /// </summary>
        /// <param name="cancellationToken">Cancels the migration between files.</param>
        /// <returns>What it copied, or why it did not look.</returns>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="IOException">A folder could not be copied; nothing is recorded, so the next run tries again.</exception>
        public async Task<MigrationResult> MigrateOnceAsync(CancellationToken cancellationToken = default)
        {
            DirectoryInfo home;
            try
            {
                home = this.resolveHome();
            }
            catch (InvalidOperationException exception)
            {
                // A HYPHA_HOME that cannot be used is the business of whoever uses it, not of every verb.
                return new MigrationResult(this.layout.Root, [], [], $"HYPHA_HOME cannot be used ({exception.Message})");
            }

            if (!SameFolder(this.layout.Root, home))
            {
                return Skip(home, "the knowledge is not read from HYPHA_HOME");
            }

            var target = new KnowledgeLayout(home);

            if (Done(home, target) is { } done)
            {
                return Skip(home, done);
            }

            // Nothing found records nothing: a client installed later is still found on a later run.
            var installs = this.Found(home, null);
            if (installs.Count == 0)
            {
                return new MigrationResult(home, [], []);
            }

            ReleaseOperationLease lease;
            try
            {
                lease = await this.Lock(home, this.lockWait).AcquireAsync(cancellationToken);
            }
            catch (ReleaseOperationLockException exception)
            {
                return Skip(home, $"another hypha is changing releases ({exception.Message})");
            }

            await using (lease)
            {
                // Checked again under the lock: another hypha may have installed or migrated meanwhile.
                if (Done(home, target) is { } meanwhile)
                {
                    return Skip(home, meanwhile);
                }

                try
                {
                    return new MigrationResult(home, installs, await this.CopyAsync(home, installs, target, cancellationToken));
                }
                catch (ReleaseOperationLockException exception)
                {
                    return Skip(home, $"a client is changing its releases ({exception.Message})");
                }
            }
        }

        /// <summary>
        /// Migrates on request: copies every release and API version <c>HYPHA_HOME</c> does not have yet.
        /// </summary>
        /// <param name="from">The client folders to copy from; <c>null</c> or empty for the ones found.</param>
        /// <param name="cancellationToken">Cancels the migration between files.</param>
        /// <returns>What it copied, and the client folders it looked in.</returns>
        /// <exception cref="ArgumentException">A folder named in <paramref name="from"/> holds no <c>knowledge/versions.json</c>.</exception>
        /// <exception cref="InvalidOperationException"><c>HYPHA_HOME</c> cannot be used.</exception>
        /// <exception cref="ReleaseOperationLockException">Another hypha, or a client, is changing releases.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        /// <exception cref="IOException">A folder could not be copied.</exception>
        public async Task<MigrationResult> MigrateAsync(IReadOnlyList<DirectoryInfo>? from = null, CancellationToken cancellationToken = default)
        {
            var home = this.resolveHome();

            foreach (var folder in from ?? [])
            {
                if (!new KnowledgeLayout(folder).VersionManifest.Exists)
                {
                    throw new ArgumentException($"'{folder.FullName}' holds no {Path.Combine("knowledge", VersionManifest.FileName)}, so it has nothing to copy");
                }
            }

            var installs = this.Found(home, from);

            await using var lease = await this.Lock(home, this.lockWait).AcquireAsync(cancellationToken);

            return new MigrationResult(home, installs, await this.CopyAsync(home, installs, new KnowledgeLayout(home), cancellationToken));
        }

        /// <summary>Whether two paths name the same folder.</summary>
        /// <param name="left">One folder.</param>
        /// <param name="right">The other.</param>
        /// <returns><c>true</c> when they are the same folder.</returns>
        private static bool SameFolder(DirectoryInfo left, DirectoryInfo right) =>
            string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(left.FullName)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(right.FullName)),
                OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        /// <summary>The marker that records a migration ran.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <returns>The marker file.</returns>
        private static FileInfo Marker(DirectoryInfo home) => new(Path.Combine(home.FullName, MarkerFileName));

        /// <summary>A migration that did not look, and why.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="reason">Why.</param>
        /// <returns>The result.</returns>
        private static MigrationResult Skip(DirectoryInfo home, string reason) => new(home, [], [], reason);

        /// <summary>
        /// Copies a folder to a temporary name beside its target, then renames it into place, replacing
        /// whatever the target held.
        /// </summary>
        /// <param name="from">The folder to copy; nothing happens when it does not exist.</param>
        /// <param name="to">Where it goes.</param>
        /// <param name="cancellationToken">Cancels the copy between files; the temporary folder is left for the next run to clear.</param>
        private static void CopyFolder(DirectoryInfo from, DirectoryInfo to, CancellationToken cancellationToken)
        {
            from.Refresh();
            if (!from.Exists)
            {
                return;
            }

            var staging = new DirectoryInfo(to.FullName + StagingSuffix);
            if (staging.Exists)
            {
                staging.Delete(recursive: true);
            }

            staging.Create();

            foreach (var file in from.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var copy = Path.Combine(staging.FullName, Path.GetRelativePath(from.FullName, file.FullName));
                Directory.CreateDirectory(Path.GetDirectoryName(copy)!);
                file.CopyTo(copy);
            }

            to.Refresh();
            if (to.Exists)
            {
                to.Delete(recursive: true);
            }

            Directory.CreateDirectory(to.Parent!.FullName);
            staging.MoveTo(to.FullName);
        }

        /// <summary>Why a migration on its own need not run any more, or <c>null</c> when it should.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="target">The <c>HYPHA_HOME</c> layout.</param>
        /// <returns>The reason, or <c>null</c>.</returns>
        private static string? Done(DirectoryInfo home, KnowledgeLayout target)
        {
            if (Marker(home).Exists)
            {
                return "a migration already ran";
            }

            if (target.InstalledTags.Count > 0)
            {
                // Recorded, so that removing every release later does not bring the clients' back.
                Record(home, []);

                return "HYPHA_HOME already has a release";
            }

            return null;
        }

        /// <summary>Writes the marker, naming the client folders looked through.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="installs">The client folders.</param>
        private static void Record(DirectoryInfo home, IReadOnlyList<LegacyInstall> installs)
        {
            home.Create();

            ReaderSafeFile.WriteAllText(
                Marker(home).FullName,
                "hypha copied the releases these clients had installed into this folder, once:\n"
                + string.Concat(installs.Select(install => $"{install.Origin}: {install.Root.FullName}\n")));
        }

        /// <summary>The client folders to copy from, without <c>HYPHA_HOME</c> itself.</summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="from">The folders asked for; <c>null</c> or empty for the ones found.</param>
        /// <returns>The folders.</returns>
        private List<LegacyInstall> Found(DirectoryInfo home, IReadOnlyList<DirectoryInfo>? from) =>
            (from is { Count: > 0 } ? from.Select(folder => new LegacyInstall(folder, folder.FullName)) : this.locator.Locate())
                .Where(install => !SameFolder(install.Root, home))
                .ToList();

        /// <summary>The release-operation lock of a knowledge folder.</summary>
        /// <param name="root">The folder.</param>
        /// <param name="wait">How long to wait for it.</param>
        /// <returns>The lock.</returns>
        private ReleaseOperationLock Lock(DirectoryInfo root, TimeSpan wait) =>
            new(new KnowledgeLayout(root), this.processes, this.loggers.CreateLogger<ReleaseOperationLock>(), wait);

        /// <summary>
        /// Copies every release and API version <paramref name="target"/> lacks, the most recently changed
        /// client folder first, each under that folder's own lock, then writes the manifests and the marker.
        /// </summary>
        /// <param name="home">The <c>HYPHA_HOME</c> folder.</param>
        /// <param name="installs">The client folders.</param>
        /// <param name="target">The <c>HYPHA_HOME</c> layout.</param>
        /// <param name="cancellationToken">Cancels the copy between files.</param>
        /// <returns>What was copied.</returns>
        /// <exception cref="ReleaseOperationLockException">
        /// A client is changing its releases; nothing is recorded, and what was copied so far is not installed.
        /// </exception>
        private async Task<List<MigratedVersion>> CopyAsync(
            DirectoryInfo home, IReadOnlyList<LegacyInstall> installs, KnowledgeLayout target, CancellationToken cancellationToken)
        {
            var apiTarget = new ApiLayout(target);
            var existing = VersionManifestFile.ReadIfPresent(target.VersionManifest.FullName);
            var existingApi = ApiVersionManifestFile.ReadIfPresent(apiTarget.VersionManifest.FullName);

            var releases = existing?.Versions.ToList() ?? [];
            var apis = existingApi?.Versions.ToList() ?? [];

            // The default stays what HYPHA_HOME had; otherwise it is the one the most recent client used.
            var releaseDefault = existing?.Default;
            var apiDefault = existingApi?.Default;
            var copied = new List<MigratedVersion>();

            var ordered = installs
                .Select(install => (install, changed: File.GetLastWriteTimeUtc(new KnowledgeLayout(install.Root).VersionManifest.FullName)))
                .OrderByDescending(candidate => candidate.changed)
                .Select(candidate => candidate.install);

            foreach (var install in ordered)
            {
                // A client installing a release lists it before generating it, so its folder is not copied
                // while the client holds its lock.
                await using var clientLease = await this.Lock(install.Root, TimeSpan.Zero).AcquireAsync(cancellationToken);

                var source = new KnowledgeLayout(install.Root);
                var apiSource = new ApiLayout(source);

                VersionManifest? manifest;
                ApiVersionManifest? apiManifest;
                try
                {
                    manifest = VersionManifestFile.ReadIfPresent(source.VersionManifest.FullName);
                    apiManifest = ApiVersionManifestFile.ReadIfPresent(apiSource.VersionManifest.FullName);
                }
                catch (Exception exception) when (exception is InvalidOperationException or JsonException or IOException)
                {
                    UnreadableMessage(this.logger, install.Origin, exception.Message, null);
                    continue;
                }

                foreach (var version in manifest?.Versions ?? [])
                {
                    if (!ReleaseTag.IsRelease(version.Tag)
                        || releases.Any(installed => installed.Tag == version.Tag)
                        || !source.Knowledge(version.Tag).Exists)
                    {
                        continue;
                    }

                    CopyFolder(source.Knowledge(version.Tag), target.Knowledge(version.Tag), cancellationToken);
                    CopyFolder(source.ReleaseSources(version.Tag), target.ReleaseSources(version.Tag), cancellationToken);
                    releases.Add(version);
                    copied.Add(new MigratedVersion(MigratedVersion.Release, version.Tag, install.Origin));
                    CopiedMessage(this.logger, MigratedVersion.Release, version.Tag, install.Origin, null);

                    releaseDefault ??= version.Tag == manifest!.Default ? version.Tag : null;
                }

                foreach (var version in apiManifest?.Versions ?? [])
                {
                    if (!ReleaseTag.IsRelease(version.Tag)
                        || apis.Any(installed => installed.Tag == version.Tag)
                        || !apiSource.Knowledge(version.Tag).Exists)
                    {
                        continue;
                    }

                    CopyFolder(apiSource.Knowledge(version.Tag), apiTarget.Knowledge(version.Tag), cancellationToken);
                    CopyFolder(apiSource.Sources(version.Tag), apiTarget.Sources(version.Tag), cancellationToken);
                    apis.Add(version);
                    copied.Add(new MigratedVersion(MigratedVersion.Api, version.Tag, install.Origin));
                    CopiedMessage(this.logger, MigratedVersion.Api, version.Tag, install.Origin, null);

                    apiDefault ??= version.Tag == apiManifest!.Default ? version.Tag : null;
                }

                if (!target.SharedPrimitiveTypes.Exists && source.SharedPrimitiveTypes.Exists)
                {
                    Directory.CreateDirectory(target.Sources.FullName);
                    source.SharedPrimitiveTypes.CopyTo(target.SharedPrimitiveTypes.FullName);
                }
            }

            // The manifests last: until they are written, nothing copied is installed.
            if (releases.Count > 0 && releases.Count != (existing?.Versions.Count ?? 0))
            {
                Directory.CreateDirectory(target.VersionManifest.DirectoryName!);
                VersionManifestFile.Write(
                    VersionManifest.Build(releaseDefault ?? releases.MaxBy(version => ReleaseTag.SortKey(version.Tag))!.Tag, releases),
                    target.VersionManifest.FullName);
            }

            if (apis.Count > 0 && apis.Count != (existingApi?.Versions.Count ?? 0))
            {
                Directory.CreateDirectory(apiTarget.VersionManifest.DirectoryName!);
                ApiVersionManifestFile.Write(
                    ApiVersionManifest.Build(apiDefault ?? apis.MaxBy(version => ReleaseTag.SortKey(version.Tag))!.Tag, apis),
                    apiTarget.VersionManifest.FullName);
            }

            Record(home, installs);

            return copied;
        }
    }
}
