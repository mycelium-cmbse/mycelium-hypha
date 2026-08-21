// ------------------------------------------------------------------------------------------------
// <copyright file="FileSyncLock.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A lock file at a fixed path: held by whichever process's <c>pid</c> is recorded inside it.
    /// </summary>
    /// <remarks>
    /// Exclusive file creation (<see cref="FileMode.CreateNew"/>) is the actual mutex - it either
    /// succeeds atomically or fails because the file is already there. The recorded pid and timestamp
    /// exist only to recognise and clear a <b>stale</b> lock left behind by a run that crashed instead
    /// of reaching its own <see cref="Release"/>.
    /// </remarks>
    public sealed class FileSyncLock : ISyncLock
    {
        private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(30);

        private readonly FileInfo path;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSyncLock"/> class.
        /// </summary>
        public FileSyncLock(FileInfo path)
        {
            ArgumentNullException.ThrowIfNull(path);

            this.path = path;
        }

        /// <inheritdoc/>
        public bool TryAcquire()
        {
            this.path.Directory?.Create();

            if (this.TryCreate())
            {
                return true;
            }

            if (!this.IsStale())
            {
                return false;
            }

            // Best effort: another process may already be clearing the same stale lock. Whichever of
            // us wins the next TryCreate genuinely holds it; the other correctly reports "held".
            try
            {
                File.Delete(this.path.FullName);
            }
            catch (IOException)
            {
                return false;
            }

            return this.TryCreate();
        }

        /// <inheritdoc/>
        public void Release()
        {
            try
            {
                File.Delete(this.path.FullName);
            }
            catch (IOException)
            {
                // Nothing left to do: the lock will be reclaimed as stale once it ages out.
            }
        }

        private bool TryCreate()
        {
            try
            {
                using var stream = new FileStream(this.path.FullName, FileMode.CreateNew, FileAccess.Write);
                using var writer = new StreamWriter(stream);

                writer.Write(JsonSerializer.Serialize(
                    new Contents(Environment.ProcessId, DateTimeOffset.UtcNow)));

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private bool IsStale()
        {
            Contents? contents;

            try
            {
                contents = JsonSerializer.Deserialize<Contents>(File.ReadAllText(this.path.FullName));
            }
            catch (IOException)
            {
                return false;
            }
            catch (JsonException)
            {
                // Unreadable lock content is as good as no useful information: treat it as stale
                // rather than jamming the lock forever.
                return true;
            }

            if (contents is null)
            {
                return true;
            }

            if (DateTimeOffset.UtcNow - contents.StartedAt > StaleAfter)
            {
                return true;
            }

            try
            {
                using var process = Process.GetProcessById(contents.Pid);

                return false;
            }
            catch (ArgumentException)
            {
                // No such process: whatever held the lock is gone.
                return true;
            }
        }

        private sealed record Contents(
            [property: JsonPropertyName("pid")] int Pid,
            [property: JsonPropertyName("startedAt")] DateTimeOffset StartedAt);
    }
}
