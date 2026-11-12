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
    using System.Diagnostics;
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
    /// read it; and a lock is stale only when its owner process is known to be gone. The one addition is
    /// the wait (<see cref="HyphaKnowledgeOptions.ReleaseLockWait"/>): while it lasts, a refusal is
    /// not thrown, so a lock that is held, or that another process is still creating or recovering, is
    /// taken once it is free. Between tries the waiter only reads the lock, every
    /// <see cref="PollInterval"/>, and runs the protocol again once the lock is gone or its owner is
    /// gone. The protocol creates the recovery file; running it on every poll would keep that file in
    /// place most of the time, and a waiter killed while it holds it would block every later
    /// acquisition until someone removed it by hand.
    /// </remarks>
    public sealed class ReleaseOperationLock : IReleaseOperationLock
    {
        /// <summary>The lock file's name, in the repository root.</summary>
        public const string FileName = ".release-operation.lock";

        /// <summary>The file that serialises stale-lock recovery, beside the lock file.</summary>
        public const string RecoveryFileName = FileName + ".recovery";

        /// <summary>How many times the lock is tried, each failed try recovering a stale lock before the next.</summary>
        private const int Attempts = 2;

        /// <summary>How long a waiting acquisition pauses between two tries.</summary>
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// How the lock and recovery files are opened: shared with every other reader, writer and deleter, so
        /// the other side can always read or remove them.
        /// </summary>
        private const FileShare Shared = FileShare.ReadWrite | FileShare.Delete;

        /// <summary>The Windows error that a file to be created exclusively exists (<c>ERROR_FILE_EXISTS</c>).</summary>
        private const int WindowsFileExists = 80;

        /// <summary>The Windows error that a file or folder to be created exists (<c>ERROR_ALREADY_EXISTS</c>).</summary>
        private const int WindowsAlreadyExists = 183;

        /// <summary>The Linux and macOS error that a file to be created exclusively exists (<c>EEXIST</c>).</summary>
        private const int UnixFileExists = 17;

        /// <summary>UTF-8 without a byte-order mark, the encoding of the lock file.</summary>
        private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// The compiled message that a stale lock left by a process that is gone was recovered (event 1,
        /// information).
        /// </summary>
        private static readonly Action<ILogger, int, Exception?> RecoveredMessage =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, "RecoveredStaleLock"),
                "Recovered a stale release-operation lock left by process {ProcessId}");

        /// <summary>Resolves the repository root the lock file lives in.</summary>
        private readonly IKnowledgeLayout layout;

        /// <summary>Tells whether a lock's owner process is still running.</summary>
        private readonly IProcessProbe processes;

        /// <summary>Receives the recovery message, and is handed to each lease.</summary>
        private readonly ILogger<ReleaseOperationLock> logger;

        /// <summary>How long an acquisition keeps trying while the lock is refused.</summary>
        private readonly TimeSpan wait;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseOperationLock"/> class.
        /// </summary>
        /// <param name="layout">Resolves the repository root the lock file lives in.</param>
        /// <param name="processes">Tells whether a lock's owner process is still running.</param>
        /// <param name="logger">Receives the recovery message, and is handed to each lease.</param>
        /// <param name="wait">
        /// How long an acquisition keeps trying while the lock is refused; zero, the default, refuses at once.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="layout"/>, <paramref name="processes"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="wait"/> is negative.</exception>
        public ReleaseOperationLock(
            IKnowledgeLayout layout, IProcessProbe processes, ILogger<ReleaseOperationLock> logger, TimeSpan wait = default)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(processes);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentOutOfRangeException.ThrowIfLessThan(wait, TimeSpan.Zero);

            this.layout = layout;
            this.processes = processes;
            this.logger = logger;
            this.wait = wait;
        }

        /// <summary>
        /// Takes the lock by creating <see cref="FileName"/> exclusively in the repository root with this
        /// process's id and a fresh token, recovering a stale lock and trying once more when it already exists.
        /// While the wait given to the constructor lasts, a refusal is tried again instead of thrown.
        /// </summary>
        /// <param name="cancellationToken">Checked before each attempt, and cancels the pause between them.</param>
        /// <returns>The held lock; disposing it releases the lock.</returns>
        /// <exception cref="ReleaseOperationLockException">
        /// Once the wait is over: another process or VS Code window holds the lock (or this process does,
        /// through a lease it has not released yet, since the lock is not re-entrant), another recovery is
        /// in progress or was interrupted, the existing lock cannot be read or created, or both attempts
        /// failed. The refusal of the last try is the one thrown.
        /// </exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public async Task<ReleaseOperationLease> AcquireAsync(CancellationToken cancellationToken = default)
        {
            var root = this.layout.Root;
            Directory.CreateDirectory(root.FullName);

            var lockFile = new FileInfo(Path.Combine(root.FullName, FileName));
            var owner = JsonSerializer.Serialize(new { pid = Environment.ProcessId, token = Guid.NewGuid().ToString() });
            var started = Stopwatch.GetTimestamp();

            while (true)
            {
                try
                {
                    return this.TryAcquire(root, lockFile, owner, cancellationToken);
                }
                catch (ReleaseOperationLockException) when (this.HasTimeLeft(started))
                {
                    // Waited out below; once the wait is over, the next try's refusal is thrown.
                }

                do
                {
                    await Task.Delay(PollInterval, cancellationToken);
                }
                while (this.HasTimeLeft(started) && this.IsStillTaken(root, lockFile));
            }
        }

        /// <summary>
        /// Reads a lock file without holding it, sharing it with every other reader and writer.
        /// </summary>
        /// <param name="lockFile">The lock file.</param>
        /// <returns>Its content.</returns>
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
        /// <param name="content">The lock file's content.</param>
        /// <returns>The process id, or <c>null</c> when the content is not such a lock.</returns>
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

        /// <summary>
        /// Whether a waiting acquisition has time for another pause and try.
        /// </summary>
        /// <param name="started">When the acquisition started, as a <see cref="Stopwatch"/> timestamp.</param>
        /// <returns><c>true</c> when one more <see cref="PollInterval"/> still fits in the wait.</returns>
        private bool HasTimeLeft(long started) => Stopwatch.GetElapsedTime(started) + PollInterval <= this.wait;

        /// <summary>
        /// Whether the lock is plainly still taken, judged by reading it only: a recovery is in progress,
        /// or the lock names this process or a process that is running, or cannot be read as a lock yet
        /// because it is still being written.
        /// </summary>
        /// <remarks>
        /// Creates nothing, unlike the protocol, so a waiter can call it on every poll. When it returns
        /// <c>false</c> the protocol runs again and decides: it takes a free lock, and recovers a stale one.
        /// </remarks>
        /// <param name="root">The repository root.</param>
        /// <param name="lockFile">The lock file.</param>
        /// <returns><c>true</c> while trying again would be refused.</returns>
        private bool IsStillTaken(DirectoryInfo root, FileInfo lockFile)
        {
            if (File.Exists(Path.Combine(root.FullName, RecoveryFileName)))
            {
                return true;
            }

            string content;
            try
            {
                content = Read(lockFile);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Gone (or being removed this instant): let the protocol try.
                return false;
            }

            var owner = OwnerProcessId(content);

            return owner is not { } processId
                || processId == Environment.ProcessId
                || this.processes.IsRunning(processId);
        }

        /// <summary>
        /// The two attempts of the extension's protocol: create the lock, and when it exists, recover it
        /// if it is stale and create it once more.
        /// </summary>
        /// <param name="root">The repository root.</param>
        /// <param name="lockFile">The lock file.</param>
        /// <param name="owner">The owner record to write.</param>
        /// <param name="cancellationToken">Checked before each attempt.</param>
        /// <returns>The held lock.</returns>
        /// <exception cref="ReleaseOperationLockException">The lock was refused.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        private ReleaseOperationLease TryAcquire(
            DirectoryInfo root, FileInfo lockFile, string owner, CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < Attempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (TryCreate(root, lockFile.FullName, owner))
                {
                    return new ReleaseOperationLease(lockFile, owner, this.logger);
                }

                this.Recover(root, lockFile);
            }

            throw new ReleaseOperationLockException("Could not acquire the hypha release-operation lock. Try again.");
        }

        /// <summary>
        /// Creates <paramref name="path"/> exclusively and writes <paramref name="content"/> to it, closing it
        /// straight away; a file left half-written by a failed write is deleted again.
        /// </summary>
        /// <param name="root">The repository root, named in the error when the file cannot be created.</param>
        /// <param name="path">The lock file's path.</param>
        /// <param name="content">The owner record to write.</param>
        /// <returns><c>true</c> when the file was created; <c>false</c> when it already exists.</returns>
        /// <exception cref="ReleaseOperationLockException">The file cannot be created right now.</exception>
        private static bool TryCreate(DirectoryInfo root, string path, string content)
        {
            FileStream stream;
            try
            {
                stream = CreateExclusively(root, path);
            }
            catch (IOException exception) when (AlreadyExists(exception, path))
            {
                // Held, or released again in the instant since: either way the caller recovers or tries again.
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

        /// <summary>
        /// Whether creating <paramref name="path"/> exclusively failed because the file exists.
        /// </summary>
        /// <remarks>
        /// Judged by the error itself first, not only by looking for the file again: the other side may
        /// have removed it in the instant since, which would otherwise turn a held lock into a raw
        /// <see cref="IOException"/>.
        /// </remarks>
        /// <param name="exception">The error creating the file.</param>
        /// <param name="path">The file.</param>
        /// <returns><c>true</c> when the file existed.</returns>
        private static bool AlreadyExists(IOException exception, string path)
        {
            var code = OperatingSystem.IsWindows() ? exception.HResult & 0xFFFF : exception.HResult;

            return (OperatingSystem.IsWindows() ? code is WindowsFileExists or WindowsAlreadyExists : code == UnixFileExists)
                || File.Exists(path);
        }

        /// <summary>
        /// Creates <paramref name="path"/>, failing if it exists.
        /// </summary>
        /// <param name="root">The repository root, named in the error when the file cannot be created.</param>
        /// <param name="path">The file to create.</param>
        /// <returns>The new file, open for writing.</returns>
        /// <exception cref="IOException">The file already exists.</exception>
        /// <exception cref="ReleaseOperationLockException">Creating the file was refused with access denied.</exception>
        /// <remarks>
        /// On Windows a file that is still being deleted refuses creation with access denied rather
        /// than "exists"; that is reported as a lock that cannot be taken right now, not as a raw
        /// exception.
        /// </remarks>
        private static FileStream CreateExclusively(DirectoryInfo root, string path)
        {
            try
            {
                return new FileStream(path, FileMode.CreateNew, FileAccess.Write, Shared);
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new ReleaseOperationLockException(
                    $"{Path.GetFileName(path)} in {root.FullName} cannot be created right now; it may still be "
                    + "being removed by another hypha process or VS Code window. Try again.",
                    exception);
            }
        }

        /// <summary>
        /// Removes the lock when its owner process is known to be gone, under the exclusively created
        /// <see cref="RecoveryFileName"/>, which is deleted again afterwards. A lock released in the meantime
        /// is left for the next attempt.
        /// </summary>
        /// <param name="root">The repository root.</param>
        /// <param name="lockFile">The lock file.</param>
        /// <exception cref="ReleaseOperationLockException">
        /// Another recovery is in progress or was interrupted, the lock cannot be read, or its owner is this
        /// process or another process that is still running.
        /// </exception>
        private void Recover(DirectoryInfo root, FileInfo lockFile)
        {
            var recoveryPath = Path.Combine(root.FullName, RecoveryFileName);

            FileStream recovery;
            try
            {
                recovery = CreateExclusively(root, recoveryPath);
            }
            catch (IOException exception) when (AlreadyExists(exception, recoveryPath))
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

                if (owner == Environment.ProcessId)
                {
                    throw new ReleaseOperationLockException(
                        "A hypha release operation is already running in this process. Wait for it to finish "
                        + "before starting another.");
                }

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
