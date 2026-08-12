// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System.Collections.Generic;

    /// <summary>The whole of <c>knowledge/&lt;tag&gt;/model-library/index.json</c>.</summary>
    /// <remarks>
    /// Property order is the file's key order, and <see cref="Declarations"/> is inserted in scan
    /// order (files sorted, then each file's declarations in source order), so two runs over the same
    /// inputs produce the same bytes.
    /// </remarks>
    /// <param name="SchemaVersion">The version of <c>model-library.schema.json</c> this satisfies.</param>
    /// <param name="Tag">The release these declarations were indexed from.</param>
    /// <param name="Counts">Coverage.</param>
    /// <param name="Declarations">One entry per qualified name a standard-library file declares.</param>
    public sealed record ModelLibraryDocument(
        string SchemaVersion,
        string Tag,
        ModelLibraryCounts Counts,
        IReadOnlyDictionary<string, ModelLibraryDeclarationEntry> Declarations)
    {
        /// <summary>The schema version this generator emits.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to <c>knowledge/&lt;tag&gt;/model-library/</c>.</summary>
        public const string FileName = "index.json";
    }
}
