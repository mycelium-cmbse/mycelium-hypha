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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Writes a generated file the way the committed knowledge base requires.
    /// </summary>
    /// <remarks>
    /// UTF-8 without a byte-order mark. The content is expected to carry LF endings already -
    /// <c>.gitattributes</c> pins <c>knowledge/**</c> to LF, and a CRLF here would rewrite every line
    /// of the file on a Windows checkout.
    /// </remarks>
    internal static class KnowledgeFile
    {
        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        /// <summary>Writes <paramref name="content"/> to <paramref name="path"/>.</summary>
        public static async Task<FileInfo> WriteAsync(
            string path, string content, CancellationToken cancellationToken = default)
        {
            var file = new FileInfo(path);
            file.Directory?.Create();

            await File.WriteAllTextAsync(file.FullName, content, Utf8NoBom, cancellationToken);
            file.Refresh();

            return file;
        }
    }
}
