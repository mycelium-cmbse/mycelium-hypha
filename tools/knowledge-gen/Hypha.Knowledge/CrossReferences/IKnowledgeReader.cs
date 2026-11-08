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
        /// <param name="index">The release's <c>metamodel/index.json</c>.</param>
        /// <returns>The elements, in index order, and the index's <c>modelVersionUri</c>.</returns>
        (IReadOnlyList<ElementRecord> Elements, string ModelVersionUri) ReadElements(FileInfo index);

        /// <summary>
        /// Reads a specification clause catalog: <c>{clause: title}</c> and the document it describes.
        /// </summary>
        /// <param name="index">A specification's <c>spec/&lt;document&gt;/index.json</c>.</param>
        /// <returns>The clause titles by clause number, and the document's name and version.</returns>
        (IReadOnlyDictionary<string, string> Titles, SpecificationDocument Document) ReadClauseTitles(
            FileInfo index);

        /// <summary>
        /// Reads the <c>elements:</c> front matter of every worked example, keyed by
        /// <paramref name="prefix"/> plus the page's file name.
        /// </summary>
        /// <param name="examples">The release's <c>textual-notation/examples</c> folder.</param>
        /// <param name="prefix">Prepended to each page's file name to form its key.</param>
        /// <returns>The metaclasses each example declares, by page path.</returns>
        IReadOnlyDictionary<string, IReadOnlyList<string>> ReadExamples(
            DirectoryInfo examples, string prefix);
    }
}
