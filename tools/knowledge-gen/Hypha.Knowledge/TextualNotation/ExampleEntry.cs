// ------------------------------------------------------------------------------------------------
// <copyright file="ExampleEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.TextualNotation
{
    /// <summary>
    /// One generated example page, and the model it was generated from.
    /// </summary>
    /// <param name="FileName">The flat file name under <c>examples/</c>.</param>
    /// <param name="SourcePath">The model's path relative to the release's textual sources, <c>/</c>-separated.</param>
    public sealed record ExampleEntry(string FileName, string SourcePath);
}
