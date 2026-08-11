// ------------------------------------------------------------------------------------------------
// <copyright file="IModelLibraryRenderer.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Knowledge.ModelLibrary
{
    using System.Collections.Generic;

    /// <summary>Renders <c>knowledge/&lt;tag&gt;/model-library/</c> from one release's standard libraries.</summary>
    public interface IModelLibraryRenderer
    {
        /// <summary>
        /// Renders one standard-library file as a page: front matter, the verbatim source, and the
        /// qualified names it declares.
        /// </summary>
        string RenderPackage(
            string sourcePath, string modelText, IReadOnlyList<LibraryDeclaration> declarations, string language);

        /// <summary>Renders the per-release index page: every standard-library file, grouped by folder.</summary>
        string RenderIndex(string tag, IReadOnlyList<ModelLibraryPackageEntry> packages);
    }
}
