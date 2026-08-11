// ------------------------------------------------------------------------------------------------
// <copyright file="ModelLibraryPackageEntry.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    /// <summary>One generated package page, for the index.</summary>
    /// <param name="FileName">The page's file name under <c>model-library/packages/</c>.</param>
    /// <param name="SourcePath">The upstream path, e.g. <c>sysml.library/Systems Library/Parts.sysml</c>.</param>
    public sealed record ModelLibraryPackageEntry(string FileName, string SourcePath);
}
