// ------------------------------------------------------------------------------------------------
// <copyright file="SyncCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Generation;
    using Hypha.Knowledge.Layout;
    using Hypha.Knowledge.Releases;
    using Hypha.Tools.Sync;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Brings the installed knowledge base up to the newest offerable release, when there is one -
    /// the one verb meant to be launched unattended, from the plugin's <c>SessionStart</c> hook.
    /// </summary>
    /// <remarks>
    /// Unlike <c>move-window</c>, this never shells out to <c>dotnet test</c>/<c>pytest</c>: it works
    /// from the downloaded self-contained binary, which is what a plugin install actually has. It
    /// also never fetches specification PDFs (a plugin install has no OMG PDFs to include) and never
    /// calls <see cref="IReleaseWindowEvictor"/> - see <see cref="SyncTagPruner"/> for why.
    /// </remarks>
    public sealed class SyncCommand : Command
    {
        /// <summary>Where progress is written as JSON.</summary>
        public static readonly Option<FileInfo> StatusFile =
            new("--status-file")
            {
                Description = "Where progress is written as JSON, for a caller that is not waiting.",
                Required = true,
            };

        /// <summary>Where diagnostics go, instead of the console.</summary>
        public static readonly Option<FileInfo?> LogFile =
            new("--log-file")
            {
                Description = "Log to this file instead of the console - meant for an unattended run.",
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncCommand"/> class.
        /// </summary>
        public SyncCommand()
            : base("sync", "Fetch and generate the newest offerable release, if it is not installed already")
        {
            this.Options.Add(StatusFile);
            this.Options.Add(LogFile);
        }

        /// <summary>Runs the <see cref="SyncCommand"/>.</summary>
        public sealed class Handler
        {
            private readonly IReleaseDiscovery discovery;
            private readonly IReleaseInstaller installer;
            private readonly IReadOnlyList<IKnowledgeGenerator> generators;
            private readonly IKnowledgeLayout layout;
            private readonly ISyncStatusWriter statusWriter;
            private readonly ISyncLock syncLock;
            private readonly ILogger<Handler> logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            public Handler(
                IReleaseDiscovery discovery,
                IReleaseInstaller installer,
                IEnumerable<IKnowledgeGenerator> generators,
                IKnowledgeLayout layout,
                ISyncStatusWriter statusWriter,
                ISyncLock syncLock,
                ILogger<Handler> logger)
            {
                ArgumentNullException.ThrowIfNull(discovery);
                ArgumentNullException.ThrowIfNull(installer);
                ArgumentNullException.ThrowIfNull(generators);
                ArgumentNullException.ThrowIfNull(layout);
                ArgumentNullException.ThrowIfNull(statusWriter);
                ArgumentNullException.ThrowIfNull(syncLock);
                ArgumentNullException.ThrowIfNull(logger);

                this.discovery = discovery;
                this.installer = installer;
                this.generators = [.. generators.OrderBy(generator => generator.Order)];
                this.layout = layout;
                this.statusWriter = statusWriter;
                this.syncLock = syncLock;
                this.logger = logger;
            }

            /// <summary>Runs the sync.</summary>
            /// <returns>
            /// 0 when the release was already current or is now installed; 1 when the run failed; 3
            /// when another run already holds the lock.
            /// </returns>
            public async Task<int> InvokeAsync(
                ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);

                if (!this.syncLock.TryAcquire())
                {
                    this.logger.LogInformation("Another sync is already running; nothing more to do here.");

                    return 3;
                }

                try
                {
                    return await this.RunAsync(cancellationToken);
                }
                finally
                {
                    this.syncLock.Release();
                }
            }

            private async Task<int> RunAsync(CancellationToken cancellationToken)
            {
                var status = this.statusWriter.Load() ?? new SyncStatus { Phase = SyncPhase.Checking };

                status = status with
                {
                    Pid = Environment.ProcessId,
                    StartedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    FinishedAt = null,
                    Phase = SyncPhase.Checking,
                    Fetch = null,
                    Generate = null,
                    Error = null,

                    // Captured exactly once, ever - never recomputed from the current InstalledTags,
                    // or a release sync itself added earlier would silently promote itself into the
                    // permanent baseline the moment a later sync replaces it.
                    CommittedBaselineTags = status.CommittedBaselineTags ?? this.layout.InstalledTags,
                };
                this.statusWriter.Write(status);

                try
                {
                    var available = await this.discovery.AvailableAsync(cancellationToken);
                    var newest = available.Count > 0 ? available[0] : null;

                    if (newest is null
                        || this.layout.InstalledTags.Contains(newest, StringComparer.Ordinal))
                    {
                        this.statusWriter.Write(status with
                        {
                            UpdatedAt = DateTimeOffset.UtcNow,
                            FinishedAt = DateTimeOffset.UtcNow,
                            Phase = SyncPhase.UpToDate,
                            TargetTag = newest,
                        });

                        return 0;
                    }

                    status = status with { TargetTag = newest };

                    status = await this.FetchAsync(status, newest, cancellationToken);
                    status = await this.GenerateAsync(status, newest, cancellationToken);
                    status = this.Prune(status, newest);

                    this.statusWriter.Write(status with
                    {
                        UpdatedAt = DateTimeOffset.UtcNow,
                        FinishedAt = DateTimeOffset.UtcNow,
                        Phase = SyncPhase.Done,
                    });

                    return 0;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    this.logger.LogError(exception, "sync failed");

                    this.statusWriter.Write(status with
                    {
                        UpdatedAt = DateTimeOffset.UtcNow,
                        FinishedAt = DateTimeOffset.UtcNow,
                        Phase = SyncPhase.Failed,
                        Error = new SyncError(ErrorKind(exception), exception.Message),
                    });

                    return 1;
                }
            }

            private async Task<SyncStatus> FetchAsync(
                SyncStatus status, string tag, CancellationToken cancellationToken)
            {
                status = status with { UpdatedAt = DateTimeOffset.UtcNow, Phase = SyncPhase.Fetching };
                this.statusWriter.Write(status);

                // Fetches run up to MaxDownloadConcurrency at once (see ReleaseFetcher), so this
                // callback can be invoked from several threads at once - Progress<T> only guarantees
                // marshalling onto one SynchronizationContext when the caller set one up, which a
                // console app has not. Without the lock, concurrent read-modify-write of the captured
                // `status` local (and concurrent atomic-rename writes to the same file) would race.
                var reportLock = new object();

                var progress = new Progress<FetchProgress>(update =>
                {
                    lock (reportLock)
                    {
                        status = status with
                        {
                            UpdatedAt = DateTimeOffset.UtcNow,
                            Fetch = new SyncFetchProgress(update.Kind, update.Done, update.Total),
                        };
                        this.statusWriter.Write(status);
                    }
                });

                var request = new ReleaseInstallRequest
                {
                    Tag = tag,
                    IncludeSpecifications = false,
                    SkipExisting = true,
                    MakeDefault = true,
                };

                await this.installer.InstallAsync(request, cancellationToken, progress);

                return status with { UpdatedAt = DateTimeOffset.UtcNow, Fetch = null };
            }

            private async Task<SyncStatus> GenerateAsync(
                SyncStatus status, string tag, CancellationToken cancellationToken)
            {
                status = status with { UpdatedAt = DateTimeOffset.UtcNow, Phase = SyncPhase.Generating };
                this.statusWriter.Write(status);

                for (var index = 0; index < this.generators.Count; index++)
                {
                    var generator = this.generators[index];

                    status = status with
                    {
                        UpdatedAt = DateTimeOffset.UtcNow,
                        Generate = new SyncGenerateProgress(generator.Artifact, index + 1, this.generators.Count),
                    };
                    this.statusWriter.Write(status);

                    await generator.GenerateAsync(tag, cancellationToken);
                }

                return status with { UpdatedAt = DateTimeOffset.UtcNow, Generate = null };
            }

            private SyncStatus Prune(SyncStatus status, string newTag)
            {
                status = status with { UpdatedAt = DateTimeOffset.UtcNow, Phase = SyncPhase.Pruning };
                this.statusWriter.Write(status);

                var previous = status.LocallyAddedTag;

                if (previous is not null && !string.Equals(previous, newTag, StringComparison.Ordinal))
                {
                    SyncTagPruner.Prune(this.layout, previous, status.CommittedBaselineTags);
                }

                return status with { UpdatedAt = DateTimeOffset.UtcNow, LocallyAddedTag = newTag };
            }

            private static string ErrorKind(Exception exception) => exception switch
            {
                System.Net.Http.HttpRequestException => "network",
                IOException => "disk",
                UnauthorizedAccessException => "disk",
                _ => "unexpected",
            };
        }
    }
}
