// ------------------------------------------------------------------------------------------------
// <copyright file="IKnowledgeReader.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.CrossReferences
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Reads the generated knowledge trees the cross-references are assembled from.
    /// </summary>
    public interface IKnowledgeReader
    {
        /// <summary>Reads a release's metamodel index: its elements and the model URI it declares.</summary>
        (IReadOnlyList<ElementRecord> Elements, string ModelVersionUri) ReadElements(FileInfo index);

        /// <summary>
        /// Reads a specification clause catalog: <c>{clause: title}</c> and the document it describes.
        /// </summary>
        (IReadOnlyDictionary<string, string> Titles, SpecificationDocument Document) ReadClauseTitles(
            FileInfo index);

        /// <summary>
        /// Reads the <c>elements:</c> front matter of every worked example, keyed by
        /// <paramref name="prefix"/> plus the page's file name.
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<string>> ReadExamples(
            DirectoryInfo examples, string prefix);
    }
}
