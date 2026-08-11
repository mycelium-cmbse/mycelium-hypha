// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryDeclarationEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    /// <summary>One <c>knowledge/&lt;tag&gt;/model-library/index.json</c> entry.</summary>
    /// <param name="Kind">The literal keyword(s) that introduced it, e.g. <c>attribute def</c>.</param>
    /// <param name="File">The package page, relative to <c>knowledge/&lt;tag&gt;/model-library/</c>.</param>
    /// <param name="Source">The upstream source path, relative to the release's textual sources.</param>
    public sealed record ModelLibraryDeclarationEntry(string Kind, string File, string Source);
}
