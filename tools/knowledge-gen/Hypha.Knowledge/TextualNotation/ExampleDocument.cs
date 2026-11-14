// ------------------------------------------------------------------------------------------------
// <copyright file="ExampleDocument.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System.Collections.Generic;

    /// <summary>
    /// The whole of <c>knowledge/&lt;tag&gt;/textual-notation/examples/index.json</c>: every worked example
    /// the release ships, by the name its page has.
    /// </summary>
    /// <remarks>
    /// Written by the textual-notation generator next to the pages it describes, so a reader finds an
    /// example by name without opening every page. <see cref="Examples"/> is keyed and ordered ordinally, so
    /// two runs over the same inputs produce the same bytes.
    /// </remarks>
    /// <param name="SchemaVersion">The version of <c>examples.schema.json</c> this satisfies.</param>
    /// <param name="Tag">The release the models were read from.</param>
    /// <param name="Examples">
    /// One entry per example page, keyed by the page's file name without <c>.md</c>, such as
    /// <c>kerml-src-examples-behavior-examples-camera</c>: the name in its <c>hypha://</c> address.
    /// </param>
    public sealed record ExampleDocument(
        string SchemaVersion,
        string Tag,
        IReadOnlyDictionary<string, ExampleIndexEntry> Examples)
    {
        /// <summary>The schema version this generator emits.</summary>
        public const string CurrentSchemaVersion = "1.0.0";

        /// <summary>The file name, relative to <c>knowledge/&lt;tag&gt;/textual-notation/examples/</c>.</summary>
        public const string FileName = "index.json";

        /// <summary>The extension every example page has, which an example's name leaves out.</summary>
        public const string PageExtension = ".md";
    }
}
