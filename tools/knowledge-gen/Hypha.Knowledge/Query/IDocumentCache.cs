// ------------------------------------------------------------------------------------------------
// <copyright file="IDocumentCache.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Query
{
    using System;
    using System.IO;
    using System.Text.Json.Serialization.Metadata;

    using Hypha.Knowledge.Lookup;

    /// <summary>
    /// Reads generated documents for the queries, keeping each one until the file on disk changes.
    /// </summary>
    /// <remarks>
    /// One instance is shared by every query, so a document read by one is not read again by another.
    /// Every way a document can fail to read ends in a <see cref="KnowledgeLookupException"/>.
    /// Implementations must be safe to call from several threads at once.
    /// </remarks>
    public interface IDocumentCache
    {
        /// <summary>Returns the document at a path, reading it when it is not held or has changed.</summary>
        /// <typeparam name="T">The document's type.</typeparam>
        /// <param name="file">The generated file.</param>
        /// <param name="contract">The contract to read it through.</param>
        /// <param name="isWhole">
        /// Whether a document that read without error has every top-level member the queries rely on.
        /// </param>
        /// <param name="missing">The message for a file that does not exist.</param>
        /// <param name="regenerate">The command that regenerates the file, for the messages.</param>
        /// <returns>The document.</returns>
        /// <exception cref="KnowledgeLookupException">
        /// The file does not exist, is too large, cannot be read, is empty, or is not a valid document of
        /// its kind.
        /// </exception>
        T Get<T>(FileInfo file, JsonTypeInfo<T> contract, Func<T, bool> isWhole, string missing, string regenerate)
            where T : class;

        /// <summary>Reads a generated page as UTF-8 text, without keeping it.</summary>
        /// <param name="file">The page.</param>
        /// <returns>The page's text.</returns>
        /// <exception cref="KnowledgeLookupException">The page cannot be read.</exception>
        string ReadText(FileInfo file);
    }
}
