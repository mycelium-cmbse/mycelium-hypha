// ------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeFile.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Generation
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Layout;

    /// <summary>
    /// Writes a generated file the way the committed knowledge base requires.
    /// </summary>
    /// <remarks>
    /// UTF-8 without a byte-order mark, through <see cref="ReaderSafeFile"/>, so a reader in another process
    /// never sees half a file. The content is expected to carry LF endings already:
    /// <c>.gitattributes</c> pins <c>knowledge/**</c> to LF, and a CRLF here would rewrite every line
    /// of the file on a Windows checkout.
    /// </remarks>
    internal static class KnowledgeFile
    {
        /// <summary>
        /// Writes <paramref name="content"/> to <paramref name="path"/>, creating its folder first if needed.
        /// </summary>
        /// <param name="path">The file to write.</param>
        /// <param name="content">The content, already with LF line endings.</param>
        /// <param name="cancellationToken">Cancels the write.</param>
        /// <returns>The written file, refreshed so its length and timestamps are current.</returns>
        public static async Task<FileInfo> WriteAsync(
            string path, string content, CancellationToken cancellationToken = default)
        {
            var file = new FileInfo(path);
            await ReaderSafeFile.WriteAllTextAsync(file.FullName, content, cancellationToken);
            file.Refresh();

            return file;
        }
    }
}
