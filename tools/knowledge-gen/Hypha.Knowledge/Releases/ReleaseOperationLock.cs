// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperationLock.cs" company="Starion Group S.A.">
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
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Hosting;
    using Hypha.Knowledge.Layout;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The release-operation lock, as a file in the repository root shared with the VS Code extension.
    /// </summary>
    /// <remarks>
    /// See <see cref="IReleaseOperationLock"/> for the protocol. Every step mirrors the extension's
    /// <c>acquireReleaseLock</c> and <c>recoverReleaseLock</c>: two attempts, each recovering a stale
    /// lock first; the lock file is written and closed, never held open, so the other side can always
    /// read it; and a lock is stale only when its owner process is known to be gone.
    /// </remarks>
    public sealed class ReleaseOperationLock : IReleaseOperationLock
    {
        /// <summary>The lock file's name, in the repository root.</summary>
        public const string FileName = ".release-operation.lock";

        /// <summary>The file that serialises stale-lock recovery, beside the lock file.</summary>
        public const string RecoveryFileName = FileName + ".recovery";

        private const int Attempts = 2;

        private const FileShare Shared = FileShare.ReadWrite | FileShare.Delete;

        private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

        private static readonly Action<ILogger, int, Exception?> RecoveredMessage =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, "RecoveredStaleLock"),
                "Recovered a stale release-operation lock left by process {ProcessId}");

        private readonly IKnowledgeLayout layout;
        private readonly IProcessProbe processes;
        private readonly ILogger<ReleaseOperationLock> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseOperationLock"/> class.
        /// </summary>
        public ReleaseOperationLock(IKnowledgeLayout layout, IProcessProbe processes, ILogger<ReleaseOperationLock> logger)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(processes);
            ArgumentNullException.ThrowIfNull(logger);

            this.layout = layout;
            this.processes = processes;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<ReleaseOperationLease> AcquireAsync(CancellationToken cancellationToken = default)
        {
            var root = this.layout.Root;
            Directory.CreateDirectory(root.FullName);

            var lockFile = new FileInfo(Path.Combine(root.FullName, FileName));
            var owner = JsonSerializer.Serialize(new { pid = Environment.ProcessId, token = Guid.NewGuid().ToString() });

            for (var attempt = 0; attempt < Attempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryCreate(lockFile.FullName, owner))
                {
                    return Task.FromResult(new ReleaseOperationLease(lockFile, owner));
                }

                this.Recover(root, lockFile);
            }

            throw new ReleaseOperationLockException("Could not acquire the hypha release-operation lock. Try again.");
        }

        /// <summary>
        /// Reads a lock file without holding it, sharing it with every other reader and writer.
        /// </summary>
        /// <exception cref="FileNotFoundException">The lock file does not exist.</exception>
        internal static string Read(FileInfo lockFile)
        {
            using var stream = new FileStream(lockFile.FullName, FileMode.Open, FileAccess.Read, Shared);
            using var reader = new StreamReader(stream, Utf8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// The owner's process id, when <paramref name="content"/> is a lock both sides would accept:
        /// a JSON object whose <c>pid</c> is a positive integer.
        /// </summary>
        internal static int? OwnerProcessId(string content)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var lockObject = document.RootElement;

                if (lockObject.ValueKind == JsonValueKind.Object
                    && lockObject.TryGetProperty("pid", out var pid)
                    && pid.ValueKind == JsonValueKind.Number
                    && pid.TryGetInt32(out var processId)
                    && processId > 0)
                {
                    return processId;
                }

                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool TryCreate(string path, string content)
        {
            FileStream stream;
            try
            {
                stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, Shared);
            }
            catch (IOException) when (File.Exists(path))
            {
                return false;
            }

            try
            {
                using (stream)
                {
                    var bytes = Utf8.GetBytes(content);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch
            {
                File.Delete(path);
                throw;
            }

            return true;
        }

        private void Recover(DirectoryInfo root, FileInfo lockFile)
        {
            var recoveryPath = Path.Combine(root.FullName, RecoveryFileName);

            FileStream recovery;
            try
            {
                recovery = new FileStream(recoveryPath, FileMode.CreateNew, FileAccess.Write, Shared);
            }
            catch (IOException) when (File.Exists(recoveryPath))
            {
                throw new ReleaseOperationLockException(
                    "Another hypha process may be recovering a release-operation lock. Try again; if recovery was "
                    + "interrupted, make sure no hypha process or VS Code window is changing releases, then remove "
                    + $"{RecoveryFileName} from {root.FullName}.");
            }

            try
            {
                string existing;
                try
                {
                    existing = Read(lockFile);
                }
                catch (FileNotFoundException)
                {
                    // Released in the meantime: the next attempt can take it.
                    return;
                }

                var owner = OwnerProcessId(existing)
                    ?? throw new ReleaseOperationLockException(
                        "A hypha release-operation lock is unreadable or still being created. Try again; if it "
                        + "remains unreadable, make sure no hypha process or VS Code window is changing releases, "
                        + $"then remove {FileName} from {root.FullName}.");

                if (this.processes.IsRunning(owner))
                {
                    throw new ReleaseOperationLockException(
                        "A hypha release operation is already running in another process or VS Code window. "
                        + "Wait for it to finish before trying again.");
                }

                File.Delete(lockFile.FullName);
                RecoveredMessage(this.logger, owner, null);
            }
            finally
            {
                recovery.Dispose();
                File.Delete(recoveryPath);
            }
        }
    }
}
