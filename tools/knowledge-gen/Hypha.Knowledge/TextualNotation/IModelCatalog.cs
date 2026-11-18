// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Hypha.Knowledge.TextualNotation
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Finds the models a release ships.
    /// </summary>
    public interface IModelCatalog
    {
        /// <summary>
        /// Every model under a release's textual sources, as <c>/</c>-separated paths relative to that
        /// root, in a stable order.
        /// </summary>
        /// <param name="textualRoot">The folder to search, e.g. the release's textual sources.</param>
        /// <returns>The models' relative paths; empty when the folder does not exist.</returns>
        IReadOnlyList<string> Discover(DirectoryInfo textualRoot);
    }
}
