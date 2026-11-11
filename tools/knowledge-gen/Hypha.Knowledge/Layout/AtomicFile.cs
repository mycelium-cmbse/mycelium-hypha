// ------------------------------------------------------------------------------------------------
// <copyright file="AtomicFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Layout
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Writes a text file so that a reader in another process sees either the old content or the new
    /// content, never part of one: the content goes to a temporary file beside it, which is then
    /// renamed over the file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every manifest and generated knowledge file is written through it, since several MCP servers,
    /// the CLI and the VS Code extension can read a knowledge base while one of them changes it.
    /// </para>
    /// <para>
    /// The content is written as UTF-8 without a byte-order mark, as every knowledge file is. The
    /// temporary file is named <c>&lt;name&gt;.&lt;guid&gt;.tmp</c>, so no reader looking for the
    /// file's own extension picks it up, and it is deleted again when the write fails. On Windows a
    /// reader that holds the old file open, or a virus scanner looking at the new one, makes the rename
    /// fail; it is tried again, after short pauses of random length, for up to two seconds before the
    /// error is passed on.
    /// </para>
    /// </remarks>
    public static class AtomicFile
    {
        /// <summary>How long the rename is tried again while it is refused.</summary>
        private static readonly TimeSpan ReplaceBudget = TimeSpan.FromSeconds(2);

        /// <summary>UTF-8 without a byte-order mark.</summary>
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Replaces <paramref name="path"/> with <paramref name="content"/>, creating its folder first if needed.
        /// </summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The content.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty or blank.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">The file could not be written or replaced.</exception>
        /// <exception cref="UnauthorizedAccessException">The file could not be replaced.</exception>
        public static void WriteAllText(string path, string content)
        {
            var temporary = Prepare(path, content);

            try
            {
                File.WriteAllText(temporary, content, Utf8NoBom);

                var started = Stopwatch.GetTimestamp();

                while (!TryReplace(temporary, path, started))
                {
                    Thread.Sleep(Pause());
                }
            }
            catch
            {
                DeleteQuietly(temporary);
                throw;
            }
        }

        /// <summary>
        /// Replaces <paramref name="path"/> with <paramref name="content"/>, creating its folder first if needed.
        /// </summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The content.</param>
        /// <param name="cancellationToken">Cancels the write, leaving the old file as it was.</param>
        /// <returns>A task that completes once the file holds the new content.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty or blank.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">The file could not be written or replaced.</exception>
        /// <exception cref="UnauthorizedAccessException">The file could not be replaced.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        public static async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
        {
            var temporary = Prepare(path, content);

            try
            {
                await File.WriteAllTextAsync(temporary, content, Utf8NoBom, cancellationToken);

                var started = Stopwatch.GetTimestamp();

                while (!TryReplace(temporary, path, started))
                {
                    await Task.Delay(Pause(), cancellationToken);
                }
            }
            catch
            {
                DeleteQuietly(temporary);
                throw;
            }
        }

        /// <summary>Checks the arguments, creates the folder and names the temporary file.</summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The content.</param>
        /// <returns>The temporary file's path, beside <paramref name="path"/>.</returns>
        private static string Prepare(string path, string content)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(content);

            var file = new FileInfo(path);
            file.Directory?.Create();

            return Path.Combine(file.DirectoryName ?? string.Empty, $"{file.Name}.{Guid.NewGuid():N}.tmp");
        }

        /// <summary>Renames the temporary file over the target.</summary>
        /// <param name="temporary">The temporary file.</param>
        /// <param name="path">The file it replaces.</param>
        /// <param name="started">When the first try was made, as a <see cref="Stopwatch"/> timestamp.</param>
        /// <returns><c>true</c> when it was renamed; <c>false</c> when it was refused and there is time left to try again.</returns>
        private static bool TryReplace(string temporary, string path, long started)
        {
            try
            {
                File.Move(temporary, path, overwrite: true);

                return true;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                && Stopwatch.GetElapsedTime(started) < ReplaceBudget)
            {
                return false;
            }
        }

        /// <summary>
        /// A short pause of a random length, so the tries do not fall into step with a reader that opens
        /// the file at a steady rate and keep meeting it.
        /// </summary>
        /// <returns>Between 2 and 20 milliseconds.</returns>
        private static TimeSpan Pause() => TimeSpan.FromMilliseconds(Random.Shared.Next(2, 21));

        /// <summary>Deletes the temporary file left by a failed write, ignoring a failure to.</summary>
        /// <param name="temporary">The temporary file.</param>
        private static void DeleteQuietly(string temporary)
        {
            try
            {
                File.Delete(temporary);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Left behind: it is named so that nothing reads it.
            }
        }
    }
}
