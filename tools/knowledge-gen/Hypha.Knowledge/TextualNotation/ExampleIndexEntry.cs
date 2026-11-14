// ------------------------------------------------------------------------------------------------
// <copyright file="ExampleIndexEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    using System.Collections.Generic;

    /// <summary>One worked example in <c>examples/index.json</c>.</summary>
    /// <param name="Name">The model's own name: its file name without the extension, such as <c>Camera</c>.</param>
    /// <param name="Language"><c>KerML</c> or <c>SysML</c>.</param>
    /// <param name="Source">The model's path relative to the release's textual sources, <c>/</c>-separated.</param>
    /// <param name="File">Its page, relative to <c>examples/</c>.</param>
    /// <param name="Elements">The metaclasses the model declares, ordered ordinally, as its page lists them.</param>
    public sealed record ExampleIndexEntry(
        string Name,
        string Language,
        string Source,
        string File,
        IReadOnlyList<string> Elements);
}
