// ------------------------------------------------------------------------------------------------
// <copyright file="ReleaseOperationLease.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A held release-operation lock; disposing it releases the lock.
    /// </summary>
    /// <remarks>
    /// Releasing deletes the lock file only while it still holds this owner's content (the whole
    /// content, token included), so a lock that was recovered and taken by someone else in the
    /// meantime is left alone, even by a later lease of the same process. Disposing more than once
    /// does nothing, and releasing never throws: a lock that cannot be removed is logged and left
    /// behind, to be recovered once this process is gone, rather than hiding the outcome of the
    /// operation it protected.
    /// </remarks>
    public sealed class ReleaseOperationLease : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// The compiled warning that the lock file could not be removed and is left to be recovered (event 2,
        /// warning).
        /// </summary>
        private static readonly Action<ILogger, string, Exception?> NotReleasedMessage =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(2, "LockNotReleased"),
                "Could not remove the release-operation lock {LockFile}; it is recovered once this process has exited");

        /// <summary>The lock file this lease holds.</summary>
        private readonly FileInfo lockFile;

        /// <summary>The exact content this owner wrote into the lock file, token included.</summary>
        private readonly string owner;

        /// <summary>Where a lock that could not be removed is reported.</summary>
        private readonly ILogger logger;

        /// <summary>Whether the lease has been released already, which makes a further dispose do nothing.</summary>
        private bool released;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseOperationLease"/> class.
        /// </summary>
        /// <param name="lockFile">The lock file this lease holds.</param>
        /// <param name="owner">The exact content this owner wrote into it.</param>
        /// <param name="logger">Where a lock that could not be removed is reported.</param>
        internal ReleaseOperationLease(FileInfo lockFile, string owner, ILogger logger)
        {
            this.lockFile = lockFile;
            this.owner = owner;
            this.logger = logger;
        }

        /// <summary>The lock file this lease holds.</summary>
        public FileInfo LockFile => this.lockFile;

        /// <summary>
        /// Releases the lock: deletes the lock file when it still holds exactly this owner's content, and
        /// leaves it alone otherwise. Only the first call does anything.
        /// </summary>
        /// <remarks>
        /// Never throws: a lock file that is already gone, or whose folder is, needs nothing more, and one that
        /// cannot be removed is logged as a warning and left to be recovered once this process has exited.
        /// </remarks>
        public void Dispose()
        {
            if (this.released)
            {
                return;
            }

            this.released = true;

            try
            {
                if (string.Equals(ReleaseOperationLock.Read(this.lockFile), this.owner, StringComparison.Ordinal))
                {
                    this.lockFile.Delete();
                }
            }
            catch (FileNotFoundException)
            {
                // Already gone.
            }
            catch (DirectoryNotFoundException)
            {
                // The whole root is gone.
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                NotReleasedMessage(this.logger, this.lockFile.FullName, exception);
            }
        }

        /// <summary>
        /// Releases the lock, exactly as <see cref="Dispose"/> does; the work is synchronous.
        /// </summary>
        /// <returns>A completed task.</returns>
        public ValueTask DisposeAsync()
        {
            this.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
