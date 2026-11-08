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

    /// <summary>
    /// A held release-operation lock; disposing it releases the lock.
    /// </summary>
    /// <remarks>
    /// Releasing deletes the lock file only while it still holds this owner's content, so a lock that
    /// was recovered and taken by someone else in the meantime is left alone. Disposing more than once
    /// does nothing.
    /// </remarks>
    public sealed class ReleaseOperationLease : IAsyncDisposable, IDisposable
    {
        private readonly FileInfo lockFile;
        private readonly string owner;
        private bool released;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseOperationLease"/> class.
        /// </summary>
        /// <param name="lockFile">The lock file this lease holds.</param>
        /// <param name="owner">The exact content this owner wrote into it.</param>
        internal ReleaseOperationLease(FileInfo lockFile, string owner)
        {
            this.lockFile = lockFile;
            this.owner = owner;
        }

        /// <summary>The lock file this lease holds.</summary>
        public FileInfo LockFile => this.lockFile;

        /// <inheritdoc/>
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
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            this.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
