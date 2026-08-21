// ------------------------------------------------------------------------------------------------
// <copyright file="SyncStatusWriter.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Sync
{
    using System;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// Reads and writes the status file at a fixed path, given once at construction.
    /// </summary>
    public sealed class SyncStatusWriter : ISyncStatusWriter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
        };

        private readonly FileInfo path;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncStatusWriter"/> class.
        /// </summary>
        public SyncStatusWriter(FileInfo path)
        {
            ArgumentNullException.ThrowIfNull(path);

            this.path = path;
        }

        /// <inheritdoc/>
        public SyncStatus? Load()
        {
            this.path.Refresh();
            if (!this.path.Exists)
            {
                return null;
            }

            var content = File.ReadAllText(this.path.FullName);

            return JsonSerializer.Deserialize<SyncStatus>(content, SerializerOptions);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// A concurrent reader (the hook binary, on a later session) must never see a half-written
        /// file: write to a temporary file in the same directory, then rename over the real one - a
        /// same-filesystem rename is atomic, a write-in-place is not.
        /// </remarks>
        public void Write(SyncStatus status)
        {
            ArgumentNullException.ThrowIfNull(status);

            this.path.Directory?.Create();

            var temporary = this.path.FullName + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(status, SerializerOptions));
            File.Move(temporary, this.path.FullName, overwrite: true);
        }
    }
}
