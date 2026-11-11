// ------------------------------------------------------------------------------------------------
// <copyright file="KeywordDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.Keywords
{
    using System.Collections.Generic;

    /// <summary>
    /// The whole of <c>knowledge/&lt;tag&gt;/textual-notation/keywords.json</c>: each textual grammar's
    /// keywords and operators, with the productions that use them and the metaclasses they introduce.
    /// </summary>
    /// <remarks>
    /// Read from the release's own grammars, so nothing is hand-written. Property order is the file's key
    /// order and every collection is ordered, so two runs over the same inputs produce the same bytes.
    /// </remarks>
    /// <param name="SchemaVersion">The version of <c>keywords.schema.json</c> this satisfies.</param>
    /// <param name="Tag">The release the grammars were read from.</param>
    /// <param name="Grammars">
    /// Each grammar the release ships, keyed <c>kerml</c> or <c>sysml</c> as in <c>cross-references.json</c>.
    /// A grammar the release does not ship is left out.
    /// </param>
    public sealed record KeywordDocument(
        string SchemaVersion,
        string Tag,
        IReadOnlyDictionary<string, KeywordGrammar> Grammars)
    {
        /// <summary>The schema version this generator emits.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to a release's <c>textual-notation</c> directory.</summary>
        public const string FileName = "keywords.json";
    }
}
