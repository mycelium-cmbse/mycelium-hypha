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
        /// <param name="sourcePath">The file's path, starting with <c>sysml.library/</c>.</param>
        /// <param name="modelText">The file's content.</param>
        /// <param name="declarations">The declarations found in it, in the order the file declares them.</param>
        /// <param name="language"><c>KerML</c> or <c>SysML</c>.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string RenderPackage(
            string sourcePath, string modelText, IReadOnlyList<LibraryDeclaration> declarations, string language);

        /// <summary>Renders the per-release index page: every standard-library file, grouped by folder.</summary>
        /// <param name="tag">The release the pages were generated for.</param>
        /// <param name="packages">The generated package pages.</param>
        /// <returns>The page's Markdown, with LF line endings.</returns>
        string RenderIndex(string tag, IReadOnlyList<ModelLibraryPackageEntry> packages);
    }
}
